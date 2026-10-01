using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace NewbieGift;

// ── 注册 + 图标 ──
[HarmonyPatch(typeof(StartingPerkList), "InitStartingPerk")]
internal static class InitStartingPerkPatch
{
    static void Postfix()
    {
        PerkRegistry.EnsureRegistered();
        PerkRegistry.SetupIcon();       // ★ 新增
    }
}

// ★ 新增：游戏加载 perkIcons 字典后，立刻推入自定义图标
[HarmonyPatch(typeof(StartingPerkIconLoader), "Start")]
internal static class StartingPerkIconLoaderStartPatch
{
    static void Postfix()
    {
        try { PerkRegistry.SetupIcon(); }
        catch (Exception ex) { ModLog.Warn("IconLoader.Start.Post: " + ex.Message); }
    }
}

// ── 本地化 ──
[HarmonyPatch(typeof(StartingPerk), "GetLocalizedDisplayName")]
internal static class PerkDisplayNamePatch
{
    static bool Prefix(StartingPerk __instance, ref string __result)
    {
        if (__instance != null && PerkRegistry.TryGetLoc(__instance.id, out var name, out _))
        {
            __result = name;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(StartingPerk), "GetLocalizedDescription")]
internal static class PerkDescriptionPatch
{
    static bool Prefix(StartingPerk __instance, ref string __result)
    {
        if (__instance != null && PerkRegistry.TryGetLoc(__instance.id, out _, out var desc))
        {
            __result = desc;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(LocHelper), "GetLocalizedPerkTable",
    new Type[] { typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
internal static class PerkTableLocPatch
{
    static bool Prefix(string key, ref string __result)
    {
        if (string.IsNullOrEmpty(key) || !key.StartsWith("perk_")) return true;
        bool isName = key.EndsWith("_name");
        bool isDesc = key.EndsWith("_desc");
        if (!isName && !isDesc) return true;

        string id = key.Substring(5, key.Length - 10);
        if (PerkRegistry.TryGetLoc(id, out var name, out var desc))
        {
            __result = isName ? name : desc;
            return false;
        }
        return true;
    }
}

// ── UI 注入 ──
[HarmonyPatch(typeof(PerkUIController), "OpenUI")]
internal static class PerkUiOpenPatch
{
    static void Postfix(PerkUIController __instance) => PerkRegistry.EnsurePickerElements(__instance);
}

[HarmonyPatch(typeof(StartingPerkElement), "Start")]
internal static class StartingPerkElementStartPatch
{
    static void Postfix(StartingPerkElement __instance) => PerkRegistry.EnsureElement(__instance);
}

// ★ 额外到达检测点：无实例参数，从 currentClientInstance 取
[HarmonyPatch(typeof(StoreUIManager), "OnNextClientArrived")]
internal static class Patch_OnNextClientArrived
{
    private static void Postfix()
    {
        try { Courier.OnClientArrivedFromCurrent(); }
        catch (Exception ex) { ModLog.Warn("OnNextClientArrived.Post: " + ex.Message); }
    }
}