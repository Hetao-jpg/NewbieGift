// 良心商人派送 + 上架（首访 / 回头客 分离版）
//
// 设计要点：
//   1. 两个客户端 ID 独立：首访用 First，回头客用 Repeat。
//   2. 派发时即确定角色，不再依赖「到达时重新判定是不是首次」。
//   3. 到达回调接收 StoreClient 实例，不再回读 currentClientInstance。
//   4. 首访逻辑全权负责：放箱子、发钱、写 first_visit_day / last_visit_day。
//   5. 回头客逻辑只负责：写 last_visit_day。
//   6. mod 送出的 storage_bay 打 NEWBIE_GIFT_DELIVERED tag。

using System;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace NewbieGift;

internal enum CourierKind { First, Repeat }

internal static class Courier
{
    public static int DispatchedDay = -1;

    private static bool _delivered;   // 本日是否已处理到访
    private static bool _enqueued;    // 本日是否已排入队列

    public static void ResetForNewDay()
    {
        _delivered = false;
        _enqueued = false;
    }

    public static bool IsCourier(string id)
        => id == Ids.CourierFirstClientId || id == Ids.CourierRepeatClientId;

    // ---------------- 派发入口 ----------------

    public static bool SendFirst(string giftItemId, int cash)
        => SendInternal(cash, CourierKind.First, giftItemId);

    public static bool SendRepeat(int cash)
        => SendInternal(cash, CourierKind.Repeat, null);

    private static bool SendInternal(int cash, CourierKind kind, string itemId)
    {
        PlayerStore store = PlayerStore.Instance;
        if (store == null) return false;

        StoreClientManager manager = store.storeClientManager;
        if (manager == null) return false;

        if (_enqueued) return true;

        StoreClient client = Build(cash, kind);
        if (client == null) return false;
        if (!Enqueue(manager, client)) return false;

        _enqueued = true;

        if (kind == CourierKind.First)
            ModLog.Info($"【首访】良心商人已派出，礼品={itemId}，预算={cash}");
        else
            ModLog.Info($"【回头客】良心商人已派出，预算={cash}");

        return true;
    }

    private static bool Enqueue(StoreClientManager manager, StoreClient client)
    {
        try { manager.AddNextClient(client); return true; }
        catch (Exception ex) { ModLog.Debug("AddNextClient 失败，尝试 TryAddClient: " + ex.Message); }

        try { manager.TryAddClient(client); return true; }
        catch (Exception ex) { ModLog.Warn("排入顾客失败: " + ex.Message); return false; }
    }

    // ---------------- 构建 StoreClient ----------------

    private static StoreClient Build(int cash, CourierKind kind)
    {
        StoreClient template = ResolveTemplate();
        StoreClient client = CreateClient(template, cash, kind);
        ApplyBuyPrice(client);
        ApplyVariety(client);
        ApplyBuyingIds(client);
        EnsureDialogue(client, template, kind);
        return client;
    }

    private static StoreClient ResolveTemplate()
    {
        StoreClient t = null;
        try { t = StoreClientList.CreateMerchant(); }
        catch (Exception ex) { ModLog.Debug("CreateMerchant 失败: " + ex.Message); }

        if (t == null)
        {
            try { t = StoreClientListStory.CreateMentorClient(); }
            catch (Exception ex) { ModLog.Debug("CreateMentorClient 失败: " + ex.Message); }
        }

        if (t == null)
        {
            try { t = StoreClientManager.PickShopperClient(); }
            catch (Exception ex) { ModLog.Debug("PickShopperClient 失败: " + ex.Message); }
        }
        return t;
    }

    private static string IdentifierOf(CourierKind kind)
        => kind == CourierKind.First ? Ids.CourierFirstClientId : Ids.CourierRepeatClientId;

    private static StoreClient CreateClient(StoreClient template, int cash, CourierKind kind)
    {
        StoreClient c = new StoreClient();
        c.identifier = IdentifierOf(kind);
        c.displayName = Lang.T("良心商人", "The Honest Merchant");
        c.spriteName = template?.spriteName;
        c.possibleSprites = template?.possibleSprites;
        c.clientFaction = template?.clientFaction;
        c.bubbleColor = template?.bubbleColor;
        c.realName = template?.realName;
        c.dismissable = true;
        c.arrestable = false;
        c.nonDismissableArrestable = false;
        c.nonShootable = true;
        c.isIntroduced = true;
        c.clientIntent = StoreClient.ClientIntent.SELLNBUY;
        c.clientCash = cash;
        c.clientBudget = cash;
        c.useClientBudget = true;
        c.eventSourceId = Ids.EventSourceId;
        c.clientBuyingTagList = new List<string>();
        c.clientBuyingIdList = new List<string>();
        c.clientBlackTagList = new List<string>();
        c.clientBlackIdList = new List<string>();
        c.clientNegociationCheck = new List<string>();
        c.tag = new List<string>();
        c.clientItemFeatureBuying = new List<string>();
        c.varietyTrackingList = new List<string>();
        c.CompleteClientCreation(IgnoreExposeFunc: true);

        // ★ 从模板继承鉴定/揭穿策略（必须在 CompleteClientCreation 之后，否则可能被重置）
        try
        {
            c.canClientExposeFeature = template?.canClientExposeFeature;
            c.CanExposeChem = template?.CanExposeChem ?? false;
            c.isLazyInspector = false;   // 良心商人：不偷懒，全查
        }
        catch (Exception ex) { ModLog.Warn("挂载鉴定策略失败: " + ex.Message); }

        return c;
    }

    private static void ApplyBuyPrice(StoreClient c)
    {
        try { c.buyPriceModifier = NewbieGiftMod.BuyPriceBonus(); }
        catch (Exception ex) { ModLog.Warn("设置收购价加成失败: " + ex.Message); }
    }

    private static void ApplyVariety(StoreClient c)
    {
        if (!NewbieGiftMod.VarietyBuy()) return;
        try { c.SetVariety(); }
        catch (Exception ex) { ModLog.Warn("SetVariety 失败: " + ex.Message); }
    }

    // 永久黑名单：良心商人不收的物品
    private static readonly string[] BlacklistedItemIds =
    {
        "empty_nuclear_waste_barrel", // 空的核废料桶
        "nuclear_waste",              // 放射性废料
    };

    internal static bool IsBlacklisted(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        for (int i = 0; i < BlacklistedItemIds.Length; i++)
        {
            if (string.Equals(BlacklistedItemIds[i], id, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static void ApplyBuyingIds(StoreClient c)
    {
        try
        {
            var ids = CollectAllItemIds();
            if (ids == null || ids.Count == 0) return;

            // 收货白名单：过滤掉黑名单
            c.clientBuyingIdList.Clear();
            int skipped = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                if (IsBlacklisted(id)) { skipped++; continue; }
                c.clientBuyingIdList.Add(id);
            }

            // 显式写入游戏侧黑名单，防止其它路径买入
            if (c.clientBlackIdList != null)
            {
                c.clientBlackIdList.Clear();
                for (int i = 0; i < BlacklistedItemIds.Length; i++)
                    c.clientBlackIdList.Add(BlacklistedItemIds[i]);
            }

            ModLog.Info($"良心商人收货 ID 已装填：{c.clientBuyingIdList.Count} 个（黑名单 {BlacklistedItemIds.Length} 个，已剔除 {skipped}）");
        }
        catch (Exception ex) { ModLog.Warn("装填收货 ID 失败: " + ex.Message); }
    }

    // ---------------- 对话 ----------------

    private static void EnsureDialogue(StoreClient c, StoreClient template, CourierKind kind)
    {
        try
        {
            string title = string.IsNullOrEmpty(c.displayName) ? "???" : c.displayName;

            Dialogue root = kind == CourierKind.First
                ? BuildFirstDialogue(title)
                : BuildRepeatDialogue(title);

            c.mainDialogue = root;
            c.isMainDialogueStarted = false;
            return;
        }
        catch (Exception ex) { ModLog.Warn("自建对话失败: " + ex.Message); }

        try
        {
            c.AddBasicDialog();
            if (c.mainDialogue != null) { c.isMainDialogueStarted = false; return; }
        }
        catch (Exception ex) { ModLog.Debug("AddBasicDialog 回退失败: " + ex.Message); }

        try
        {
            if (template != null && template.mainDialogue != null)
            {
                c.mainDialogue = template.mainDialogue;
                try { c.mainDialogue.title = c.displayName; } catch { }
                c.isMainDialogueStarted = false;
            }
        }
        catch (Exception ex) { ModLog.Debug("模板对话回退失败: " + ex.Message); }
    }

    private static Dialogue BuildFirstDialogue(string title)
    {
        Dialogue root = new Dialogue().SetText(title,
            Lang.T("你好啊，同行。欢迎来到下层区。", "Hey there, pal. Welcome to the Lower District."));
        Dialogue cur = root;

        cur = cur.NextDialogue();
        cur.SetText(title, Lang.T("听说你今天刚开张，我特地带了份见面礼。",
                                  "Heard it's your opening day, so I brought you a housewarming gift."));

        cur = cur.NextDialogue();
        cur.SetText(title, Lang.T("柜台上的这个储藏区是白送的，直接搬走就行。",
                                  "The storage bay on the counter is free - just take it."));

        int giftCash = NewbieGiftMod.Defaults.GiftCash;
        if (giftCash > 0)
        {
            cur = cur.NextDialogue();
            cur.SetText(title, Lang.T(
                $"另外，还有 {giftCash} 现金已经打进了你的账上——小小意思，不成敬意。",
                $"Also, {giftCash} cash has been wired to you - a small token, please accept it."));
        }

        cur = cur.NextDialogue();
        cur.SetText(title, Lang.T("我这人什么都收，不过每种只收一件，价格上我会给你额外优惠。",
                                  "I buy just about anything, but only one of each kind. You'll get a better price from me."));
        return root;
    }

    private static Dialogue BuildRepeatDialogue(string title)
    {
        Dialogue root = new Dialogue().SetText(title,
            Lang.T("又见面了，最近生意怎么样？", "Good to see you again. How's business?"));
        Dialogue cur = root.NextDialogue();
        cur.SetText(title, Lang.T("有什么好东西要出手的吗？我出价比别人高。",
                                  "Got anything good to sell? I pay better than the others."));
        return root;
    }

    // ---------------- 到达回调：接收补丁传入的实例 ----------------

    /// <summary>由 StartMainDialogue Postfix 传入实际 StoreClient 实例。</summary>
    public static void OnClientArrived(StoreClient client)
    {
        if (client == null) return;

        // ★ 每次良心商人到店，都启动反作弊监视（幂等）
        if (client.identifier == Ids.CourierFirstClientId ||
            client.identifier == Ids.CourierRepeatClientId)
        {
            AntiCheat.Begin(client.identifier);
        }

        if (_delivered) return;

        if (client.identifier == Ids.CourierFirstClientId)
            HandleFirstArrival();
        else if (client.identifier == Ids.CourierRepeatClientId)
            HandleRepeatArrival();
    }

    /// <summary>由 OnNextClientArrived 兜底调用；内部从 currentClientInstance 取。</summary>
    public static void OnClientArrivedFromCurrent()
    {
        if (_delivered) return;

        PlayerStore store = PlayerStore.Instance;
        if (store == null) return;

        StoreClientInstance instance = store.currentClientInstance;
        if (instance == null) return;

        StoreClient client = GetClientFromInstance(instance);
        if (client == null) return;

        OnClientArrived(client);
    }

    internal static StoreClient GetClientFromInstance(StoreClientInstance instance)
    {
        try { return instance.GetClientBlueprint(); }
        catch (Exception ex) { ModLog.Debug("GetClientBlueprint 失败: " + ex.Message); }

        try { return instance.storeClient; }
        catch (Exception ex) { ModLog.Debug("读取 storeClient 失败: " + ex.Message); }

        return null;
    }

    /// <summary>首访：放箱子 + 发钱 + 写 first/last visit</summary>
    private static void HandleFirstArrival()
    {
        if (!PutOnCounter(NewbieGiftMod.Defaults.GiftItemId)) return;
        _delivered = true;

        int day = GetCurrentDay();
        NewbieGiftMod.SetFirstVisitDay(day);
        NewbieGiftMod.SetLastVisitDay(day);

        // 幂等礼金入口
        NewbieGiftMod.Instance?.TryGrantGiftCash();

        try
        {
            StoreUIManager.Instance?.Notify(
                Lang.T("良心商人把储藏箱放上了柜台", "The honest merchant dropped a storage bay on the counter"),
                "white");
        }
        catch { }

        // 关键写入后主动刷盘
        try { NewbieGiftMod.Instance?.Save?.Flush(); } catch { }

        ModLog.Info($"【首访】完成：日={day}");
    }

    /// <summary>回头客：什么都不放，只更新 last_visit_day</summary>
    private static void HandleRepeatArrival()
    {
        _delivered = true;
        int day = GetCurrentDay();
        NewbieGiftMod.SetLastVisitDay(day);
        ModLog.Info($"【回头客】到访完成：日={day}");
    }

    private static int GetCurrentDay()
    {
        try { return StoreStation.GetDayCounter(); } catch { return 0; }
    }

    // ---------------- 上架 / 检测 ----------------

    public static bool PutOnCounter(string itemId)
    {
        try
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null) return false;

            if (AlreadyOnCounter(itemId)) { ModLog.Debug("柜台已有 mod 送出的该物品，跳过上架"); return true; }

            GameItem item = ResolveGiftItem(itemId);
            if (item == null) return false;

            bool freeGift = NewbieGiftMod.FreeGift();

            ClearBadTags(item);
            if (freeGift) MarkOwned(item);

            store.AddDirectSellingItemToTable(item, freeGift);

            // ★ 上架后，从柜台实际物品上打 tag（防止 API 内部克隆）
            TagCounterItems(itemId);

            ClearBadTags(item);
            if (freeGift) MarkOwned(item);

            ModLog.Info($"已上架到柜台: {itemId}（{(freeGift ? "白送" : "需付款")}，tag={Tags.NewbieGiftDelivered}）");
            return true;
        }
        catch (Exception ex) { ModLog.Warn("上架柜台失败: " + ex.Message); return false; }
    }

    /// <summary>遍历柜台，给指定 id 的实际物品打 NEWBIE_GIFT_DELIVERED tag。</summary>
    private static void TagCounterItems(string itemId)
    {
        try
        {
            EmporiumEntry emporium = EmporiumEntry.Instance;
            if ((UnityEngine.Object)(object)emporium == (UnityEngine.Object)null) return;

            List<GameItem> counters = emporium.GetCountersItems();
            if (counters == null) return;

            for (int i = 0; i < counters.Count; i++)
            {
                var it = counters[i];
                if (it == null) continue;

                string id = null;
                try { id = it.identifier; } catch { }
                if (!string.Equals(id, itemId, StringComparison.OrdinalIgnoreCase)) continue;

                TryEnableTag(it, Tags.NewbieGiftDelivered);
            }
        }
        catch (Exception ex) { ModLog.Debug("TagCounterItems 失败: " + ex.Message); }
    }

    private static GameItem ResolveGiftItem(string itemId)
    {
        if (!ItemExists(itemId)) { ModLog.Warn("物品 id 无效: " + itemId); return null; }
        GameItem item = DirectoryMaster.Item(itemId);
        if (item == null) { ModLog.Warn("创建物品失败: " + itemId); return null; }
        return item;
    }

    private static void ClearBadTags(GameItem item)
    {
        if (item == null) return;
        TryDisableTag(item, Tags.Stolen);
        TryDisableTag(item, Tags.TagStolen);
        TryDisableTag(item, Tags.StolenValueInt);
    }

    private static void MarkOwned(GameItem item)
    {
        if (item == null) return;
        TryDisableTag(item, Tags.NotPurchased);
        TryDisableTag(item, Tags.TagNotPurchased);
        TryEnableTag(item, Tags.IsOwned);
    }

    private static void TryDisableTag(GameItem item, string tag)
    {
        try { item.DisableTag(tag, skipValidate: true); }
        catch (Exception ex) { ModLog.Debug($"DisableTag({tag}) 失败: " + ex.Message); }
    }

    private static void TryEnableTag(GameItem item, string tag)
    {
        try { item.EnableTag(tag, skipValidate: true); }
        catch (Exception ex) { ModLog.Debug($"EnableTag({tag}) 失败: " + ex.Message); }
    }

    private static bool HasTag(GameItem item, string tag)
    {
        try { return item.IsTag(tag); }
        catch (Exception ex) { ModLog.Debug($"IsTag({tag}) 失败: " + ex.Message); return false; }
    }

    /// <summary>
    /// 只认「mod 送出且带 NEWBIE_GIFT_DELIVERED tag」的箱子。
    /// 玩家自己造的 storage_bay 不会命中。
    /// </summary>
    public static bool AlreadyOnCounter(string itemId)
    {
        try
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            EmporiumEntry emporium = EmporiumEntry.Instance;
            if ((UnityEngine.Object)(object)emporium == (UnityEngine.Object)null) return false;

            List<GameItem> counters = emporium.GetCountersItems();
            if (counters == null) return false;

            for (int i = 0; i < counters.Count; i++)
            {
                var item = counters[i];
                if (item == null) continue;

                string id = null;
                try { id = item.identifier; } catch { }
                if (!string.Equals(id, itemId, StringComparison.OrdinalIgnoreCase)) continue;

                if (HasTag(item, Tags.NewbieGiftDelivered)) return true;
            }
            return false;
        }
        catch (Exception ex) { ModLog.Debug("检查柜台物品失败: " + ex.Message); return false; }
    }

    private static bool ItemExists(string itemId)
    {
        try { if (DirectoryMaster.Has<GameItem>(itemId)) return true; }
        catch (Exception ex) { ModLog.Debug("DirectoryMaster.Has 失败: " + ex.Message); }

        try { return DirectoryMaster.Item(itemId) != null; }
        catch (Exception ex) { ModLog.Debug("DirectoryMaster.Item 失败: " + ex.Message); return false; }
    }

    public static bool HandToPlayer(string itemId)
    {
        try
        {
            EmporiumEntry emporium = EmporiumEntry.Instance;
            if ((UnityEngine.Object)(object)emporium == (UnityEngine.Object)null || emporium.invElement == null)
                return false;

            GameItem item = DirectoryMaster.Item(itemId);
            if (item == null) return false;

            SlotMarker slot = emporium.invElement.TryFindOneValidInventorySlot(item);
            if (slot != null)
            {
                slot.AcceptUnchecked();
                TagCounterItems(itemId);   // 兜底：若 API 克隆，找到实际物品再打 tag
                return true;
            }
            if (emporium.invElement.UncheckedAccept(item))
            {
                TagCounterItems(itemId);   // 兜底
                return true;
            }
            return false;
        }
        catch (Exception ex) { ModLog.Warn("直接入库失败: " + ex.Message); return false; }
    }

    // ---------------- 物品 ID 枚举 ----------------

    private static readonly string[] DirectoryClassNames =
    {
        "MiscItemDirectory", "ToolDirectory", "MedsItemDirectory", "FoodItemDirectory",
        "WineDirectory", "HydroponicDirectory", "HusbandryDirectory", "MaterialDirectory",
        "GunsItemDirectory", "GunModDirectory", "MeleeWeaponItemDirectory", "ExplosiveItemDirectory",
        "ArmorItemDirectory", "ContainerItemDirectory", "FurnitureItemDirectory", "ModuleDirectory",
        "ModItemDirectory", "StationMachinery", "RuinedMachineDirectory", "KeyItemDirectory",
        "OrganDirectory", "AmenitiesItemDirectory", "ConstructionItemDirectory", "EquipmentDirectory",
        "ShipItemDirectory", "ShipSystemDirectory", "TechnicianBackpackDirectory",
        "PlayerAbilityItemDirectory", "UnusedDirectory"
    };

    private static System.Collections.Generic.List<string> _cachedAllIds;

    private static System.Collections.Generic.List<string> CollectAllItemIds()
    {
        if (_cachedAllIds != null) return _cachedAllIds;

        var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (CatalogIds.All != null)
        {
            for (int i = 0; i < CatalogIds.All.Length; i++)
            {
                string id = CatalogIds.All[i];
                if (!string.IsNullOrWhiteSpace(id)) seen.Add(id);
            }
        }

        int dirOk = 0, dirFail = 0;
        for (int d = 0; d < DirectoryClassNames.Length; d++)
        {
            string dirName = DirectoryClassNames[d];
            try
            {
                var idList = DirectoryMaster.GetIdentifierList<GameItem>(dirName);
                if (idList == null) { dirFail++; continue; }
                for (int i = 0; i < idList.Count; i++)
                {
                    string id = idList[i];
                    if (!string.IsNullOrWhiteSpace(id)) seen.Add(id);
                }
                dirOk++;
            }
            catch (Exception ex)
            {
                dirFail++;
                ModLog.Debug($"枚举目录 {dirName} 失败: " + ex.Message);
            }
        }

        var result = new System.Collections.Generic.List<string>(seen.Count);
        foreach (var id in seen) result.Add(id);

        // 即使运行时枚举不完整，也先缓存 CatalogIds 的结果，避免重复全量枚举
        if (result.Count > 0)
        {
            if (dirOk >= 20)
            {
                _cachedAllIds = result;
                ModLog.Info($"良心商人：运行时枚举共 {result.Count} 个物品 ID（目录 {dirOk} 成功 / {dirFail} 失败）");
            }
            else
            {
                // 运行时枚举不完整 → 只缓存 CatalogIds 的兜底结果
                _cachedAllIds = new System.Collections.Generic.List<string>(CatalogIds.All);
                ModLog.Warn($"良心商人：运行时枚举不完整（{dirOk}/{dirFail}），使用 CatalogIds 兜底 {_cachedAllIds.Count} 项");
            }
        }
        return _cachedAllIds ?? result;
    }
}