using System;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Preferences;
using HarmonyLib;

namespace NewbieGift;

public sealed class NewbieGiftMod : MelonMod
{
    public static class Defaults
    {
        public const int FirstDay = 1;
        public static readonly int GiftCash = 100;
        public const string GiftItemId = "storage_bay";
        public const int ContrabandCrimePercentDefault = 50;
    }

    private MelonPreferences_Category _cat;
    internal MelonPreferences_Entry<bool> CfgEnabled;
    internal MelonPreferences_Entry<bool> CfgFreeGift;
    internal MelonPreferences_Entry<bool> CfgDirectToInventory;
    internal MelonPreferences_Entry<bool> CfgVarietyBuy;
    internal MelonPreferences_Entry<int> CfgBuyPriceBonus;
    internal MelonPreferences_Entry<int> CfgContrabandCrimePercent;
    internal MelonPreferences_Entry<string> CfgLanguage;

    internal SaveStore Save;
    public static NewbieGiftMod Instance { get; private set; }

    // ★ 只依赖事件钩子，不做每帧轮询
    private static int _lastHandledDay = -1;
    private static string _lastRunId = "";

    public override void OnInitializeMelon()
    {
        Instance = this;

        // ── 配置 ──
        _cat = MelonPreferences.CreateCategory(Lang.CategoryId, Lang.CategoryId);
        CfgEnabled = _cat.CreateEntry("Enabled", true, "启用 mod / Enable mod");
        CfgFreeGift = _cat.CreateEntry("FreeGift", true, "白送=true / 要付款=false | Free=true / Paid=false");
        CfgDirectToInventory = _cat.CreateEntry("DirectToInventory", false, "直接入库（排障用）/ Direct to inventory (debug)");
        CfgVarietyBuy = _cat.CreateEntry("VarietyBuy", true, "每种只收 1 件 / One per kind");
        CfgBuyPriceBonus = _cat.CreateEntry("BuyPriceBonus", 15, "收购价加成 % / Buy price bonus %");
        CfgContrabandCrimePercent = _cat.CreateEntry("ContrabandCrimePercent", Defaults.ContrabandCrimePercentDefault, "违禁值 % / Contraband crime %");
        CfgLanguage = _cat.CreateEntry("Language", "zh", "语言 / Language (zh / en)");
        _cat.SaveToFile(false);

        Lang.Refresh();
        Save = new SaveStore("NewbieGift");

        // ── Harmony 补丁 ──
        try
        {
            HarmonyInstance.PatchAll();
            ModLog.Info("Harmony 补丁已挂（特性 / 良心商人 / 违禁值）");
        }
        catch (Exception e) { ModLog.Error("Harmony 挂载异常: " + e.Message); }

        // ★ 提前加载图标（此时字典可能还没就绪，由补丁兜底）
        try { PerkRegistry.SetupIcon(); }
        catch (Exception e) { ModLog.Warn("SetupIcon: " + e.Message); }

        // ★ 事件钩子：LoadGame（读档）
        try
        {
            var m = AccessTools.Method(typeof(PlayerStore), "LoadGame");
            if (m != null)
            {
                HarmonyInstance.Patch(m,
                    postfix: new HarmonyMethod(typeof(NewbieGiftMod), nameof(OnLoadGamePostfix)));
                ModLog.Info("读档钩子已挂（PlayerStore.LoadGame）");
            }
            else ModLog.Warn("未找到 PlayerStore.LoadGame");
        }
        catch (Exception e) { ModLog.Warn("读档钩子异常: " + e.Message); }

        // ★ 事件钩子：OpenShutter（开门/开始营业）
        try
        {
            var m = AccessTools.Method(typeof(PlayerStore), "OpenShutter");
            if (m != null)
            {
                HarmonyInstance.Patch(m,
                    postfix: new HarmonyMethod(typeof(NewbieGiftMod), nameof(OnOpenShutterPostfix)));
                ModLog.Info("开门钩子已挂（PlayerStore.OpenShutter）");
            }
            else ModLog.Warn("未找到 PlayerStore.OpenShutter");
        }
        catch (Exception e) { ModLog.Warn("开门钩子异常: " + e.Message); }

        ModLog.Info("良心商人已加载（v1.7.0，反作弊 + 容器递归）");
    }
    public override void OnUpdate()
    {
        try { AntiCheat.Tick(); } catch { }
    }
    public override void OnApplicationQuit()
    {
        try { Save?.Flush(true); } catch { }
    }

    // ---------- 事件钩子 ----------

    private static void OnLoadGamePostfix()
    {
        try
        {
            var inst = Instance;
            if (inst == null) return;

            _lastHandledDay = -1;
            Courier.DispatchedDay = -1;
            Courier.ResetForNewDay();

            ResetStaticsOnRunChange();

            // 读档时立即重新加载存档（runID 通常已就绪）
            inst.Save?.ReloadNow();

            ModLog.Info("读档完成，存档已同步加载");
        }
        catch (Exception e) { ModLog.Warn("OnLoadGamePostfix: " + e.Message); }
    }

    private static void OnOpenShutterPostfix()
    {
        try
        {
            var inst = Instance;
            if (inst == null) return;

            // runID 变化检测（仅在事件点，不每帧）
            ResetStaticsOnRunChange();

            int day;
            try { day = StoreStation.GetDayCounter(); } catch { return; }
            if (day <= 0) return;

            if (_lastHandledDay == day) return;
            _lastHandledDay = day;

            // 存档未就绪 → 此时再加载一次
            if (inst.Save != null && !inst.Save.Loaded)
            {
                inst.Save.ReloadNow();
            }

            inst.OnDayStarted(day);
        }
        catch (Exception e) { ModLog.Error("OnOpenShutterPostfix: " + e.Message); }
    }

    /// <summary>仅在事件点检查 runID 变化，避免每帧 interop。</summary>
    private static void ResetStaticsOnRunChange()
    {
        try
        {
            string runId = "";
            try { runId = PlayerStore.Instance?.runID ?? ""; } catch { }

            if (runId != _lastRunId)
            {
                _lastRunId = runId;
                _lastHandledDay = -1;
                Courier.DispatchedDay = -1;
                Courier.ResetForNewDay();
                ModLog.Info($"runID 变化：'{runId}'，static 状态已重置");
            }
        }
        catch { }
    }

    // ---------- 日循环 ----------

    private void OnDayStarted(int day)
    {
        try
        {
            Courier.ResetForNewDay();

            if (!CfgEnabled.Value) return;
            if (!HonestMerchantPerk.IsActive) return;
            if (!IsVisitDay(day)) return;
            if (Courier.DispatchedDay == day) return;

            int slot = CurrentSlotId();
            if (slot < 0)
            {
                ModLog.Warn("无法获取存档槽位，跳过本日派发");
                return;
            }

            string grantedKey = SaveKeys.GrantedDay(slot);
            if (Save.GetInt(grantedKey) == day) return;

            int cash = CalcCourierCash(day);

            if (GetFirstVisitDay() == -1)
                DispatchFirstMerchant(day, grantedKey, cash);
            else
                DispatchRepeatMerchant(day, grantedKey, cash);
        }
        catch (Exception ex) { ModLog.Error("OnDayStarted: " + ex.Message); }
        finally
        {
            try { Save?.Flush(); } catch (Exception e) { ModLog.Warn("Save.Flush: " + e.Message); }
        }
    }

    private void DispatchFirstMerchant(int day, string grantedKey, int cash)
    {
        if (CfgDirectToInventory.Value)
        {
            if (!Courier.HandToPlayer(Defaults.GiftItemId))
            {
                ModLog.Warn("直接入库失败，下个周期重试");
                return;
            }
            Courier.DispatchedDay = day;
            Save.Set(grantedKey, day);
            SetFirstVisitDay(day);
            SetLastVisitDay(day);
            TryGrantGiftCash();
            Notify(Lang.T("储藏箱已直接塞进仓库", "Storage bay placed into your inventory"));
            ModLog.Info($"【首访·直接入库】完成：日={day}");
            return;
        }

        if (Courier.SendFirst(Defaults.GiftItemId, cash))
        {
            Courier.DispatchedDay = day;
            Save.Set(grantedKey, day);
            ModLog.Info($"第 {day} 天，【首访】良心商人已派出（预算 {cash}）");
        }
    }

    private void DispatchRepeatMerchant(int day, string grantedKey, int cash)
    {
        if (Courier.SendRepeat(cash))
        {
            Courier.DispatchedDay = day;
            Save.Set(grantedKey, day);
            ModLog.Info($"第 {day} 天，【回头客】良心商人已派出（预算 {cash}）");
        }
    }

    // ---------- 工具方法 ----------

    internal void TryGrantGiftCash()
    {
        if (Defaults.GiftCash <= 0) return;
        if (Save == null || !Save.Loaded)
        {
            ModLog.Debug("TryGrantGiftCash: 存档未就绪，跳过");
            return;
        }

        int slot = CurrentSlotId();
        if (slot < 0) return;

        string key = SaveKeys.CashGranted(slot);
        if (Save.GetInt(key) == 1)
        {
            ModLog.Debug($"TryGrantGiftCash: slot {slot} 礼金已发过，跳过");
            return;
        }

        try { PlayerStore.Instance?.ModCash(Defaults.GiftCash, true); }
        catch (Exception ex) { ModLog.Warn("加现金失败: " + ex.Message); return; }

        Save.Set(key, 1);
        Save.Flush();
        ModLog.Info($"礼金已发：+{Defaults.GiftCash}（slot {slot}）");
    }

    private void Notify(string text)
    {
        try { StoreUIManager.Instance?.Notify(text, "white"); } catch { }
    }

    private static bool IsVisitDay(int day)
        => day == Defaults.FirstDay || (day > Defaults.FirstDay && day % 7 == 0);

    private static int CalcCourierCash(int day) => 15 * (10 + day);

    private static int CurrentSlotId()
    {
        try
        {
            var s = PlayerStore.Instance;
            if (s != null) return s.saveSlotId;
        }
        catch { }
        return -1;
    }

    internal static int GetFirstVisitDay()
        => Instance?.Save?.GetInt(SaveKeys.FirstVisitDay(CurrentSlotId()), -1) ?? -1;

    internal static void SetFirstVisitDay(int day)
        => Instance?.Save?.Set(SaveKeys.FirstVisitDay(CurrentSlotId()), day);

    internal static void SetLastVisitDay(int day)
        => Instance?.Save?.Set(SaveKeys.LastVisitDay(CurrentSlotId()), day);

    internal static bool FreeGift() => Instance?.CfgFreeGift?.Value ?? true;
    internal static bool VarietyBuy() => Instance?.CfgVarietyBuy?.Value ?? true;
    internal static int BuyPriceBonus() => Instance?.CfgBuyPriceBonus?.Value ?? 15;

    internal static int ContrabandCrimePercent()
    {
        int v = Instance?.CfgContrabandCrimePercent?.Value ?? Defaults.ContrabandCrimePercentDefault;
        if (v < 0) v = 0;
        if (v > 100) v = 100;
        return v;
    }

    internal static int ApplyContrabandCrimeRate(int baseValue)
    {
        if (baseValue <= 0) return baseValue;
        int percent = ContrabandCrimePercent();
        long result = (long)baseValue * percent / 100L;
        if (result < 0) result = 0;
        if (result > int.MaxValue) result = int.MaxValue;
        return (int)result;
    }
}