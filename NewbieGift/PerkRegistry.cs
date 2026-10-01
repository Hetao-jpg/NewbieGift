using System;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NewbieGift;

public static class PerkRegistry
{
    public static readonly CustomPerk[] All = new CustomPerk[]
    {
        new HonestMerchantPerk()
    };

    // ★ 图标静态缓存，避免每次 OpenUI 都新建 Texture2D
    private static Sprite _cachedIcon;
    private static bool _iconLoaded;

    private static Sprite GetIcon()
    {
        if (!_iconLoaded)
        {
            _iconLoaded = true;
            try
            {
                _cachedIcon = Embedded.LoadSprite(typeof(NewbieGiftMod).Assembly, "newbie_gift.png");
            }
            catch (Exception ex) { ModLog.Warn("加载图标失败: " + ex.Message); }
        }
        return _cachedIcon;
    }

    public static CustomPerk Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var perk in All)
            if (perk.Id == id) return perk;
        return null;
    }

    public static bool TryGetLoc(string id, out string displayName, out string description)
    {
        displayName = null;
        description = null;
        var perk = Find(id);
        if (perk == null) return false;
        displayName = perk.DisplayName;
        description = perk.Description;
        return true;
    }

    public static void EnsureRegistered()
    {
        try
        {
            var perks = StartingPerkList.Perks;
            if (perks == null) return;

            foreach (var perk in All)
            {
                bool exists = false;
                for (int i = 0; i < perks.Count; i++)
                {
                    if (perks[i] != null && perks[i].id == perk.Id)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    perks.Add(perk.Create());
                    ModLog.Info($"注册自定义特性：{perk.Id}（消耗 {perk.Cost} 点）");
                }
            }
        }
        catch (Exception ex) { ModLog.Error($"注册特性失败：{ex.Message}"); }
    }

    public static StartingPerk GetOrCreate(CustomPerk perk)
    {
        var perks = StartingPerkList.Perks;
        if (perks != null)
        {
            for (int i = 0; i < perks.Count; i++)
                if (perks[i] != null && perks[i].id == perk.Id) return perks[i];
        }
        return perk.Create();
    }

    public static void EnsurePickerElements(PerkUIController ui)
    {
        try
        {
            if (ui == null || ui.availablePerks == null) return;
            EnsureRegistered();

            var mySprite = GetIcon();

            foreach (var perk in All)
            {
                var existing = FindElement(ui.availablePerks, perk.Id);
                if (existing != null)
                {
                    if (existing.perk == null) existing.perk = GetOrCreate(perk);
                    if (existing.icon != null && mySprite != null)
                        existing.icon.sprite = mySprite;
                    continue;
                }

                if (ui.perkElementPrefab == null) continue;

                var obj = UnityEngine.Object.Instantiate(ui.perkElementPrefab, ui.availablePerks.transform);
                var element = obj.GetComponent<StartingPerkElement>();
                if (element == null) { UnityEngine.Object.Destroy(obj); continue; }

                element.id = perk.Id;
                element.isSelected = false;
                element.perk = GetOrCreate(perk);
                if (element.icon != null && mySprite != null)
                    element.icon.sprite = mySprite;
                obj.SetActive(true);
                ModLog.Info($"UI 元素已注入：{perk.Id}");
            }

            ui.SortPerkContainer(ui.availablePerks);
        }
        catch (Exception ex) { ModLog.Error($"注入特性界面失败：{ex.Message}"); }
    }

    public static StartingPerkElement FindElement(GameObject parent, string id)
    {
        if (parent == null || string.IsNullOrEmpty(id)) return null;
        foreach (var e in parent.GetComponentsInChildren<StartingPerkElement>(true))
            if (e != null && e.id == id) return e;
        return null;
    }

    public static void EnsureElement(StartingPerkElement element)
    {
        if (element == null) return;
        var perk = Find(element.id);
        if (perk != null)
        {
            if (element.perk == null) element.perk = GetOrCreate(perk);
            if (element.icon != null && element.icon.sprite == null)
                element.icon.sprite = GetIcon();
        }
    }

    /// <summary>对外入口：加载图标 + 推送到游戏 perkIcons 字典</summary>
    public static void SetupIcon()
    {
        var sprite = GetIcon();
        if (sprite == null)
        {
            ModLog.Warn("自定义图标为空，跳过图标注入");
            return;
        }
        InjectIcon();
    }

    /// <summary>注入图标到 perkIcons 字典</summary>
    public static void InjectIcon()
    {
        try
        {
            var dict = GetIconDict();
            if (dict == null) { ModLog.Warn("拿不到 perkIcons 字典"); return; }

            var sprite = GetIcon();
            // 强制覆盖：无论字典里是否已有 newbie_gift，都覆盖为我们的图标
            foreach (var perk in All)
            {
                if (sprite == null)
                {
                    // 自定义图标为空 → 用原版任意图标兜底
                    foreach (var kv in dict) if (kv.Value != null) { sprite = kv.Value; break; }
                    if (sprite == null) { ModLog.Warn("无可用图标，跳过"); continue; }
                    ModLog.Warn("自定义图标为空，用原版兜底");
                }

                dict[perk.Id] = sprite;
                ModLog.Info($"图标已注入：{perk.Id}");
            }
        }
        catch (Exception ex) { ModLog.Error($"图标注入失败：{ex.Message}"); }
    }

    private static Il2CppSystem.Collections.Generic.Dictionary<string, Sprite> GetIconDict()
    {
        try
        {
            object raw = null;
            var prop = AccessTools.Property(typeof(StartingPerkIconLoader), "perkIcons");
            if (prop != null) { try { raw = prop.GetValue(null); } catch { } }
            if (raw == null)
            {
                var f = AccessTools.Field(typeof(StartingPerkIconLoader), "_perkIcons_k__BackingField")
                     ?? AccessTools.Field(typeof(StartingPerkIconLoader), "<perkIcons>k__BackingField");
                if (f != null) { try { raw = f.GetValue(null); } catch { } }
            }
            return raw as Il2CppSystem.Collections.Generic.Dictionary<string, Sprite>;
        }
        catch { return null; }
    }
}