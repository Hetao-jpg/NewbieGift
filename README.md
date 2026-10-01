# NewbieGift · 良心商人

**Probably Stolen** mod — adds "The Honest Merchant" starting perk.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---
## 📥 下载 / Download

**最新版本 v1.7.0**：[点击下载 NewbieGift.dll](https://github.com/Hetao-jpg/NewbieGift/releases/latest)
---

## 中文说明

### 做什么

为游戏添加「良心商人」起始特性。选中后，第 1、7、14、21……天（每 7 天）会有一位商人上门：

- **首次来访**：送一个储藏区（直接放柜台）+ 100 现金
- **后续来访**：只当买家，收购价高于市价 15%
- **自带鉴定能力**：到店期间识破柜台上（含容器内部）被标签打印机伪装过的假货，不扣声望

### 反作弊鉴定

商人到店期间，每秒扫描一次柜台（含所有嵌套容器的内部），一旦发现被伪装的假货立刻识破并还原真实状态。

**识破范围**：化学品纯度、注射器真伪、水纯度、酒品质等所有 `isExposable = true` 的假货特征。

**不会误伤**：冰箱冷冻/冰镇、系统买家加成、违禁品加价、赃物收购加成等正当状态。

**游戏内通知**：`哎哟，这小手段我见多了。（识破 N 件）`

### 核废料桶黑名单

任何形态的核废料桶一律拒收：
- `empty_nuclear_waste_barrel`（空桶 / 装水 / 装在容器里都拒收）
- `nuclear_waste`（放射性废料）

### 安装

1. 把 `NewbieGift.dll` 放进游戏目录的 `Mods/` 文件夹
2. 启动游戏即可

⚠️ 从 1.5.1 或更早版本升级，请删除旧的 `良心商人.dll`。

### 配置项

在 `UserData/MelonPreferences.cfg` 的 `[良心商人]` 段：

| 项 | 默认 | 说明 |
|---|---|---|
| `Enabled` | 开 | 总开关 |
| `FreeGift` | 开 | 白送 / 要花钱买 |
| `DirectToInventory` | 关 | 跳过顾客直接塞仓库（排障用） |
| `VarietyBuy` | 开 | 每种物品只收 1 件 |
| `BuyPriceBonus` | 15 | 收购价加成（%） |
| `ContrabandCrimePercent` | 50 | 违禁值百分比 |
| `Language` | zh | 语言（zh / en） |

---

## English

### What it does

Adds "The Honest Merchant" as a starting perk. A friendly merchant visits every 7 days (Day 1, 7, 14, 21, ...):

- **First visit**: brings a free storage bay + 100 cash
- **Repeat visits**: buys your items at above-market prices (+15%)
- **Built-in counter-detection**: identifies label-printer-disguised fakes on the counter (including items inside containers), without deducting reputation

### Anti-cheat detection

The merchant scans the counter (including all nested containers) every second while present. Any disguised fake is exposed and reverted to its real state.

**Exposes**: chemical purity, injector authenticity, water purity, wine quality, and all other `isExposable = true` fakes.

**Does NOT false-positive**: frozen / chilled food, system buyer bonuses, contraband markups, stolen bonuses, and other legitimate states.

### Nuclear waste barrel blacklist

Any form of nuclear waste barrel is refused:
- `empty_nuclear_waste_barrel` (empty / water-filled / inside another container)
- `nuclear_waste`

### Installation

1. Drop `NewbieGift.dll` into your `Mods/` folder
2. Launch the game

⚠️ Upgrading from 1.5.1 or older? Delete the old `良心商人.dll`.

### Configuration

In `UserData/MelonPreferences.cfg`, section `[良心商人]`:

| Key | Default | Description |
|---|---|---|
| `Enabled` | on | Master switch |
| `FreeGift` | on | Free / paid |
| `DirectToInventory` | off | Skip merchant, direct to inventory (debug) |
| `VarietyBuy` | on | Only one per item kind |
| `BuyPriceBonus` | 15 | Buy price bonus (%) |
| `ContrabandCrimePercent` | 50 | Contraband crime % |
| `Language` | zh | Language (zh / en) |

---

## 兼容性 / Compatibility

- ✅ NotEnoughItems / ProbablyStolenItemManager
- ✅ SaveBagExpand (自定义仓库)
- ✅ More Device Upgrades (更多设备升级)
- ✅ XIAOWO series / 钓鱼 / 酿酒 / 料理
- ✅ 冰箱相关 mod（已识别 `frozen` / `chilled`）

所有其他 mod 加的物品会自动进入收购范围。

All items added by other mods are automatically included in the merchant's buying range.

---

## 反馈 / Feedback

- QQ 群：@夏月的核桃
- Nexus Mods: [[link to your mod page]](https://www.nexusmods.com/probablystolen/mods/368)
- Discord: @hetaoxd

---

## License

MIT License. See [LICENSE](LICENSE).
