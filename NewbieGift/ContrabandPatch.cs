using System;
using HarmonyLib;
using Il2Cpp;

namespace NewbieGift;

[HarmonyPatch(typeof(SecData), "ApplyCrimeModifier",
    new Type[] { typeof(StoreClient), typeof(int) })]
internal static class Patch_ApplyCrimeModifier
{
    private static void Postfix(StoreClient client, int value, ref int __result)
    {
        try
        {
            if (client != null && Courier.IsCourier(client.identifier))
            {
                int old = __result;
                __result = NewbieGiftMod.ApplyContrabandCrimeRate(old);
                ModLog.Debug($"良心商人违禁值修正: {old} -> {__result}（{NewbieGiftMod.ContrabandCrimePercent()}%）");
            }
        }
        catch (Exception ex) { ModLog.Warn("ApplyCrimeModifier.Post: " + ex.Message); }
    }
}