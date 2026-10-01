using System;
using HarmonyLib;
using Il2Cpp;

namespace NewbieGift;

/// <summary>StartMainDialogue 携带 StoreClient 实例，直接传给 Courier。</summary>
[HarmonyPatch(typeof(StoreClient), "StartMainDialogue")]
internal static class Patch_StartMainDialogue
{
    private static void Postfix(StoreClient __instance)
    {
        try
        {
            if (__instance != null && Courier.IsCourier(__instance.identifier))
                Courier.OnClientArrived(__instance);
        }
        catch (Exception ex) { ModLog.Warn("StartMainDialogue.Post: " + ex.Message); }
    }
}

/// <summary>商人离场（含成交后自动离场）→ 停止反作弊监视。</summary>
[HarmonyPatch(typeof(PlayerStore), "DismissCurrentClient")]
internal static class Patch_DismissCurrentClient
{
    private static void Postfix()
    {
        try { AntiCheat.End(); }
        catch (Exception ex) { ModLog.Warn("DismissCurrentClient.Post: " + ex.Message); }
    }
}