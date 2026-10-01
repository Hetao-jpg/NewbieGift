// 嵌入资源 → Sprite
// 用于加载 mod 内嵌的图标 PNG

using System;
using System.IO;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace NewbieGift;

internal static class Embedded
{
    public static Sprite LoadSprite(string resourceSuffix)
        => LoadSprite(Assembly.GetExecutingAssembly(), resourceSuffix);

    public static Sprite LoadSprite(Assembly asm, string resourceSuffix)
    {
        try
        {
            if (asm == null) return null;
            string name = FindResource(asm, resourceSuffix);
            if (name == null)
            {
                MelonLogger.Warning("[Embedded] 找不到嵌入资源 “" + resourceSuffix + "”");
                return null;
            }

            using Stream stream = asm.GetManifestResourceStream(name);
            if (stream == null) return null;

            byte[] bytes = new byte[stream.Length];
            int offset = 0;
            while (offset < bytes.Length)
            {
                int read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read <= 0) break;
                offset += read;
            }
            if (offset == 0) return null;

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };

            Il2CppStructArray<byte> il2cppBytes = bytes;
            if (!ImageConversion.LoadImage(tex, il2cppBytes))
            {
                UnityEngine.Object.Destroy(tex);
                MelonLogger.Warning("[Embedded] 解码失败 “" + resourceSuffix + "”");
                return null;
            }

            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning("[Embedded] 加载 “" + resourceSuffix + "” 异常: " + ex.Message);
            return null;
        }
    }

    private static string FindResource(Assembly asm, string suffix)
    {
        if (asm == null || string.IsNullOrEmpty(suffix)) return null;
        foreach (var n in asm.GetManifestResourceNames())
        {
            if (n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return n;
        }
        return null;
    }
}