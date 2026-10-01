using MelonLoader;

namespace NewbieGift;

internal static class ModLog
{
    public const string Prefix = "[良心商人] ";

    /// <summary>true 时输出 Debug 级日志。默认关闭，排查问题时手动打开。</summary>
    public static bool DebugEnabled = false;

    public static void Info(string message) => MelonLogger.Msg(Prefix + message);
    public static void Warn(string message) => MelonLogger.Warning(Prefix + message);
    public static void Error(string message) => MelonLogger.Error(Prefix + message);

    public static void Debug(string message)
    {
        if (!DebugEnabled) return;
        MelonLogger.Msg(Prefix + "[debug] " + message);
    }
}