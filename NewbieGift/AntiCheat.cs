// 良心商人反作弊：商人到店期间扫描柜台物品（含容器内容），
// 揭穿玩家用标签打印机伪装过的假货特征。
//
// 机制（复用原版，跳过扣声望）：
//   判定：ItemFeature.CanExposeFeature() && !IsExposed()
//   执行：ItemFeature.ExposeFeature(false, false)   ← RVA 0x6A0B30
//   不用 StoreClient.ClientExposeFeature（那个会连带扣声望）
//
// 触发：商人到店时 Begin，每秒 Tick 一次，商人离场时 End。

using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using Il2CppGameItemList = Il2CppSystem.Collections.Generic.List<Il2Cpp.GameItem>;
using Il2CppFeatureList = Il2CppSystem.Collections.Generic.List<Il2Cpp.ItemFeature>;
using Il2CppInterop.Runtime.InteropTypes;

namespace NewbieGift;

internal static class AntiCheat
{
    private const float TickInterval = 1.0f;

    private static bool _active;
    private static float _nextTickTime;
    // ★ 系统加成黑名单：这些 feature 是原版系统给的正向加成，
    //   不是玩家用标签打印机伪造的假货，不能识破。
    private static readonly string[] SystemBonusBlacklist =
    {
        // 多样化买家加成（商人给的正向加成 +15%）
        "varietyBuyerBonus",

        // 违禁品加价（系统对违禁品的收购加价）
        "contrabandCriticalMarkUp",
        "contrabandHighMarkUp",
        "contrabandMidMarkUp",
        "contrabandLowMarkUp",
        "DiscountedCriticalContraband",
        "DiscountedHighContraband",
        "DiscountedMidContraband",
        "DiscountedLowContraband",

        // 赃物收购加成
        "StolenBonusBuying",
        "StolenBonusBuyingHigh",
        "StolenBonusBuyingMid",
        "StolenBonusBuyingLow",
    };
    /// <summary>去掉显示文案末尾的 " (+XX%)" / " (-XX%)" 值修正后缀。</summary>
    private static string StripValueSuffix(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        int idx = s.LastIndexOf(" (", StringComparison.Ordinal);
        if (idx < 0) return s;
        string suffix = s.Substring(idx);
        if (suffix.EndsWith("%)") || suffix.EndsWith("％）"))
            return s.Substring(0, idx);
        return s;
    }
    private static bool IsSystemBonus(ItemFeature f)
    {
        try
        {
            string id = f.identifier;
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < SystemBonusBlacklist.Length; i++)
                if (string.Equals(SystemBonusBlacklist[i], id, StringComparison.OrdinalIgnoreCase))
                    return true;

            // 兜底：名字里含这些关键字的一律视为系统加成
            if (id.IndexOf("MarkUp", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (id.IndexOf("BonusBuying", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (id.IndexOf("BuyerBonus", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        catch { }
        return false;
    }

    /// <summary>商人到场时调用（幂等）。</summary>
    public static void Begin(string clientId)
    {
        if (_active) return;
        _active = true;
        _nextTickTime = 0f;
        ModLog.Info($"[反作弊] 开始监视（{clientId}）");
    }

    /// <summary>商人离场时调用。</summary>
    public static void End()
    {
        if (!_active) return;
        _active = false;
        ModLog.Info("[反作弊] 停止监视");
    }

    /// <summary>每帧调用（内部 1 秒节流）。</summary>
    public static void Tick()
    {
        if (!_active) return;
        try
        {
            float now = Time.unscaledTime;
            if (now < _nextTickTime) return;
            _nextTickTime = now + TickInterval;
            ScanOnce();
        }
        catch (Exception ex) { ModLog.Warn("[反作弊] Tick 异常: " + ex.Message); }
    }
    private static void RecursiveScan(GameItem item, HashSet<IntPtr> visited, ref int exposed)
    {
        if (item == null) return;
        try { if (!visited.Add(item.Pointer)) return; } catch { return; }

        TryExposeFeatures(item, ref exposed);

        var children = TryGetContainerContents(item);
        if (children == null) return;
        for (int i = 0; i < children.Count; i++)
            RecursiveScan(children[i], visited, ref exposed);
    }
    private static void ScanOnce()
    {
        Il2CppGameItemList counters;
        try
        {
            var emp = EmporiumEntry.Instance;
            if ((UnityEngine.Object)(object)emp == (UnityEngine.Object)null) return;
            counters = emp.GetCountersItems();
        }
        catch (Exception ex) { ModLog.Debug("[反作弊] 取柜台物品失败: " + ex.Message); return; }

        if (counters == null || counters.Count == 0) return;

        ModLog.Debug($"[反作弊] 本轮扫描 {counters.Count} 件柜台物品");

        int exposed = 0;
        var visited = new HashSet<IntPtr>();
        for (int i = 0; i < counters.Count; i++)
            RecursiveScan(counters[i], visited, ref exposed);

        if (exposed > 0)
        {
            string text = Lang.T(
                $"哎哟，这小手段我见多了。（识破 {exposed} 件）",
                $"Nice try, pal. (Busted {exposed} item(s))");
            Notify(text);
            ModLog.Info($"[反作弊] 本轮识破 {exposed} 件");
        }
    }

   
    private static void TryExposeFeatures(GameItem item, ref int exposed)
    {
        Il2CppFeatureList features;
        try { features = item.itemFeatures; }
        catch { return; }
        if (features == null) return;

        for (int i = 0; i < features.Count; i++)
        {
            var f = features[i];
            if (f == null) continue;

            bool shouldExpose = false;
            string reason = "";

            try
            {
                if (!f.CanExposeFeature()) continue;
                if (f.IsExposed()) continue;
                if (!f.isExposable) continue;   // ★ 本质不可伪装 → 跳过（冷冻/冰镇/系统加成等）
                if (IsSystemBonus(f)) continue;

                if (!f.useCondition)
                {
                    // 情况 A：值修正型假货（化学品纯度、酒品质、水量…）
                    int cur = f.GetCurrentValueMod();
                    int act = f.GetActualValueModifier();
                    if (cur > act)
                    {
                        shouldExpose = true;
                        reason = $"value {cur}%→{act}%";
                    }
                }
                else
                {
                    // 情况 B：显示型假货（注射器真伪、香烟真假、邮票…）
                    string pub = null, real = null;
                    try { pub = f.GetPublicDisplay(); } catch { }
                    try { real = f.GetActualDisplay(); } catch { }

                    if (!string.IsNullOrEmpty(pub) && !string.IsNullOrEmpty(real) && pub != real)
                    {
                        // 去掉 pub 末尾的 " (+XX%)" / " (-XX%)" 值后缀后再比较
                        // 真酒："-琼浆玉液 (+75%)" → strip → "-琼浆玉液"，与 real 相等 → 不揭穿
                        // 假注射器："-正品 (0%)" → strip → "-正品"，与 real "-已过期" 不等 → 揭穿
                        string pubStripped = StripValueSuffix(pub);
                        if (pubStripped != real)
                        {
                            shouldExpose = true;
                            reason = $"display '{pub}'→'{real}'";
                        }
                    }
                }
            }
            catch { continue; }

            if (!shouldExpose) continue;

            try
            {
                f.ExposeFeature(false, false);
                exposed++;
                ModLog.Info($"[反作弊] 识破: item={SafeId(item)} feature={SafeFeatureId(f)} {reason}");
            }
            catch (Exception ex) { ModLog.Warn("[反作弊] ExposeFeature 失败: " + ex.Message); }
        }
    }

    private static string SafeId(GameItem it) { try { return it?.identifier ?? "?"; } catch { return "?"; } }
    private static string SafeFeatureId(ItemFeature f) { try { return f?.identifier ?? "?"; } catch { return "?"; } }

    private static void Notify(string text)
    {
        try { StoreUIManager.Instance?.Notify(text, "default"); } catch { }
    }

    // ---------- 容器内容获取 ----------

    private static List<GameItem> TryGetContainerContents(GameItem item)
    {
        try
        {
            var win = item.contentWindow;
            if (win == null) return null;

            // childElement 运行时类型是 GameGridInventory，虽然 .GetType().Name 报 PixelElement
            var child = win.childElement;
            if (child == null) return null;

            var grid = child.TryCast<GameGridInventory>();
            if (grid == null) return null;

            var raw = grid.items;
            if (raw == null || raw.Count == 0) return null;

            var result = new List<GameItem>();
            for (int i = 0; i < raw.Count; i++)
                if (raw[i] != null) result.Add(raw[i]);
            return result;
        }
        catch (Exception ex)
        {
            ModLog.Debug("[反作弊] 容器内容读取失败: " + ex.Message);
            return null;
        }
    }
}
/// <summary>良心商人拒收核废料桶（含装了水的空桶）。</summary>
[HarmonyPatch(typeof(StoreClient), "IsClientBuyingThisItem")]
internal static class Patch_IsClientBuyingThisItem
{
    private static float _nextLogTime;

    private static void Postfix(StoreClient __instance, GameItem gameItem, ref bool __result)
    {
        if (!__result) return;
        if (__instance == null || gameItem == null) return;
        if (!Courier.IsCourier(__instance.identifier)) return;

        try
        {
            if (Courier.IsBlacklisted(gameItem.identifier))
            {
                __result = false;

                float now = Time.unscaledTime;
                if (now >= _nextLogTime)
                {
                    _nextLogTime = now + 3f;
                    ModLog.Info($"[黑名单] 拒收: {gameItem.identifier}");
                }
            }
        }
        catch (Exception ex) { ModLog.Warn("IsClientBuyingThisItem.Post: " + ex.Message); }
    }
}