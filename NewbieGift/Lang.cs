// 双语字符串工具
// 读 MelonPreferences [良心商人] 段的 Language 选项："zh" / "en"

using System;
using MelonLoader;

namespace NewbieGift;

internal static class Lang
{
    internal const string CategoryId = "良心商人";
    internal const string DefaultLang = "zh";

    private static string _current;

    /// <summary>由 NewbieGiftMod 在启动 / 配置变更时调用</summary>
    internal static void Refresh()
    {
        try
        {
            var cat = MelonPreferences.GetCategory(CategoryId);
            var entry = cat?.GetEntry<string>("Language");
            string v = entry?.Value ?? DefaultLang;
            _current = string.Equals(v, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "zh";
        }
        catch { _current = DefaultLang; }
    }

    /// <summary>中英双语。en 参数为空 / 当前是中文 → 返回 zh</summary>
    public static string T(string zh, string en)
    {
        if (string.IsNullOrEmpty(_current)) Refresh();
        if (_current == "en" && !string.IsNullOrWhiteSpace(en)) return en;
        return zh ?? "";
    }
}