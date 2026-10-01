// 每存档独立存储
//
// 路径：<GamePath>/UserData/<ModId>/save_<runID>.txt
// 格式：每行 key=value；# 开头为注释

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace NewbieGift;

internal sealed class SaveStore
{
    private readonly string _modId;
    private readonly Dictionary<string, string> _mem = new Dictionary<string, string>(StringComparer.Ordinal);
    private string _currentRun = "";
    private bool _loadPending;
    private int _pendingFrames;
    private bool _dirty;

    public int LoadDelayFrames { get; set; } = 30;

    /// <summary>true 表示存档已就绪。构造后默认 false，必须经过一次 ReloadNow/Tick 才会变 true。</summary>
    public bool Loaded => !_loadPending;

    public SaveStore(string modId)
    {
        _modId = string.IsNullOrEmpty(modId) ? "NewbieGiftMod" : modId;
        TryMigrateLegacyDir();

        // ★ 关键修复：构造后处于「未加载」状态
        // 避免主菜单阶段误判存档已就绪
        MarkLoadPending();
    }

    // ---------- 路径 ----------

    private string UserDataDir
    {
        get
        {
            try
            {
                string gameDir = Directory.GetParent(Application.dataPath)?.FullName ?? "";
                return Path.Combine(gameDir, "UserData", _modId);
            }
            catch { return Path.Combine("UserData", _modId); }
        }
    }

    private void TryMigrateLegacyDir()
    {
        try
        {
            string gameDir = Directory.GetParent(Application.dataPath)?.FullName ?? "";
            if (string.IsNullOrEmpty(gameDir)) return;

            string legacy = Path.Combine(gameDir, "UserData", "良心商人");
            string current = Path.Combine(gameDir, "UserData", _modId);

            if (Directory.Exists(legacy) && !Directory.Exists(current))
            {
                Directory.Move(legacy, current);
                MelonLogger.Msg($"[SaveStore] 旧目录已迁移：良心商人 → {_modId}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning("[SaveStore] 旧目录迁移失败（不影响新数据）: " + ex.Message);
        }
    }

    private string PathForRun(string runId)
        => Path.Combine(UserDataDir,
            "save_" + (string.IsNullOrEmpty(runId) ? "_pending" : runId) + ".txt");

    private string ResolveRunId()
    {
        try
        {
            var s = PlayerStore.Instance;
            if (s != null)
            {
                string r = s.runID;
                if (!string.IsNullOrEmpty(r)) return r;
            }
        }
        catch { }
        return _currentRun ?? "";
    }

    // ---------- 读写 API ----------

    public void Set(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        _mem[key] = value ?? "";
        _dirty = true;
    }

    public void Set(string key, int v) => Set(key, v.ToString(CultureInfo.InvariantCulture));
    public void Set(string key, long v) => Set(key, v.ToString(CultureInfo.InvariantCulture));
    public void Set(string key, float v) => Set(key, v.ToString("R", CultureInfo.InvariantCulture));
    public void Set(string key, double v) => Set(key, v.ToString("R", CultureInfo.InvariantCulture));
    public void Set(string key, bool v) => Set(key, v ? "1" : "0");

    public string GetString(string key, string fallback = "")
        => _mem.TryGetValue(key, out var v) ? v : fallback;

    public int GetInt(string key, int fallback = 0)
        => int.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)
            ? r : fallback;

    public long GetLong(string key, long fallback = 0)
        => long.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)
            ? r : fallback;

    public float GetFloat(string key, float fallback = 0f)
        => float.TryParse(GetString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
            ? r : fallback;

    public double GetDouble(string key, double fallback = 0.0)
        => double.TryParse(GetString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
            ? r : fallback;

    public bool GetBool(string key, bool fallback = false)
    {
        string s = GetString(key, "");
        if (s == "1") return true;
        if (s == "0") return false;
        return bool.TryParse(s, out var r) ? r : fallback;
    }

    public bool Has(string key) => _mem.ContainsKey(key);

    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (_mem.Remove(key)) _dirty = true;
    }

    // ---------- 生命周期 ----------

    public void MarkLoadPending()
    {
        _mem.Clear();
        _loadPending = true;
        _pendingFrames = Math.Max(1, LoadDelayFrames);
    }

    /// <summary>立即重新加载（不依赖每帧 Tick）</summary>
    public void ReloadNow()
    {
        _loadPending = false;

        string realRun = ResolveRunId();
        if (string.IsNullOrEmpty(realRun))
        {
            // runID 还没就绪 → 先读 pending 文件
            LoadFromDisk(PathForRun(""));
            return;
        }

        if (string.IsNullOrEmpty(_currentRun))
        {
            string pendingPath = PathForRun("");
            string realPath = PathForRun(realRun);
            try
            {
                if (File.Exists(pendingPath) && !File.Exists(realPath))
                {
                    Directory.CreateDirectory(UserDataDir);
                    File.Move(pendingPath, realPath);
                }
            }
            catch { }
        }

        _currentRun = realRun;
        LoadFromDisk(PathForRun(_currentRun));
    }

    /// <summary>可选：仅在事件点调用，不做每帧轮询</summary>
    public void Tick()
    {
        if (!_loadPending) return;
        if (_pendingFrames > 0) { _pendingFrames--; return; }
        ReloadNow();
    }

    public void Flush(bool force = false)
    {
        if (!_dirty && !force) return;
        try
        {
            Directory.CreateDirectory(UserDataDir);
            string path = PathForRun(_currentRun);
            using var w = new StreamWriter(path, false, new UTF8Encoding(false));
            w.WriteLine("#runID=" + (_currentRun ?? ""));
            w.WriteLine("#saved=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            foreach (var kv in _mem)
                w.WriteLine(kv.Key + "=" + (kv.Value ?? ""));
            _dirty = false;
        }
        catch (Exception ex) { MelonLogger.Warning("[SaveStore] Flush 失败: " + ex.Message); }
    }

    private void LoadFromDisk(string path)
    {
        try
        {
            _mem.Clear();
            if (!File.Exists(path)) return;
            foreach (var raw in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(raw)) continue;
                if (raw[0] == '#') continue;
                int eq = raw.IndexOf('=');
                if (eq <= 0) continue;
                string k = raw.Substring(0, eq);
                string v = raw.Substring(eq + 1);
                _mem[k] = v;
            }
        }
        catch (Exception ex) { MelonLogger.Warning("[SaveStore] Load 失败: " + ex.Message); }
    }
}