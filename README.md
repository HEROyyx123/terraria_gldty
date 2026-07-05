# 更懒的体验 — Terraria tModLoader Mod

一个让你从开荒到毕业都能偷懒的物资礼包模组。告别无聊的挖矿找材料，把时间花在更有趣的事情上！

## 📦 礼包总览

| 礼包 | 解锁条件 | 合成材料 | 合成台 | 内容亮点 |
|:----|:--------|:--------|:------:|:--------|
| 新手大礼包 | 开局自动获得 | — | — | 铂金锭、宝石、方块资源、饰品 + 计划书 |
| 克苏鲁之眼礼包 | 击败克苏鲁之眼 | 10 晶状体 | 工作台 | 猩红锭、魔矿锭、双种子、铁皮药水、憎恶之蜂 |
| 邪恶 Boss 礼包 | 击败世吞/克脑 | 20 组织样本或暗影鳞片 | 工作台 | 蠕虫围巾、混乱之脑、狱石、黑曜石 |
| 史莱姆王礼包 | 击败史莱姆王 | 10 凝胶 | 工作台 | 凝胶、坠落之星、丛林孢子、蛛网、丝绸等海量材料 |
| 困难模式礼包 | 击败血肉墙 | 任意职业徽章 | 秘银砧 | 四种职业徽章、光明/黑暗/飞翔之魂、灵液、诅咒焰等 |
| 机械礼包 | — | 视域魂+力量魂+恐惧魂 | 秘银砧 | 99×三种机械魂、100 神圣锭、生命果 |
| 丛林礼包 | 击败世纪之花 | 神庙钥匙 | 秘银砧 | 神庙钥匙、100 叶绿锭、自动锤炼机、断裂英雄剑 |
| 阴森木礼包 | — | 250 阴森木 | 秘银砧 | 随机阴森套部件 + 随机阴森木家具 |
| 甲虫礼包 | — | 甲虫外壳 | 秘银砧 | 甲虫外壳、海龟壳、锯刃镐、金钓竿等 |
| 教徒礼包 | — | 远古操纵机 | 秘银砧 | 远古操纵机、月钩、无底微光桶 |
| 月总礼包 | — | 夜明锭 | 远古操纵机 | 100 夜明锭、四种月亮碎片、混沌传送杖 + 一个新手礼包 |

## 📖 计划书

新手礼包中附赠 **计划书**，使用后会在聊天栏提示下一个未获得礼包的合成条件，全程指引不迷路！

## 🧪 药剂包

包含 29 种药剂包，每种打开获得 30 瓶对应药水。所有药水包直接引用原版药水图标，无需额外贴图。

## 🎁 摸彩袋系统

所有礼包均采用摸彩袋（GrabBag）机制：
- 鼠标悬浮自动显示包含物品列表
- 支持概率掉落（阴森木礼包中的套装部件各 33%、家具各 ~5.26%）
- 其余礼包所有物品 100% 必得

## 🔌 模组联动（Call API）

本模组开放了 `Call` 接口，其他模组无需依赖即可调用：

```csharp
var mod = ModLoader.GetMod("terraria_gldty");

// 获取所有礼包 Key
mod.Call("GetPackKeys");

// 根据 Key 获取类名
mod.Call("GetPackClassName", "hardmode");  // → "HardmodePack"

// 覆盖指定礼包的掉落内容（完全替换）
mod.Call("OverridePackContents", "hardmode", (ItemLoot loot) => {
    loot.Add(ItemDropRule.Common(ItemID.SoulofLight, 1, 50, 50));
    loot.Add(ItemDropRule.Common(YourModItem, 1, 10, 10));
});
```

支持的礼包 Key：`starter`, `eyeofcthulhu`, `evilboss`, `kingslime`, `hardmode`, `mechboss`, `jungle`, `spookywood`, `beetle`, `cultist`, `moonlord`

## 🌐 本地化支持

- [x] English (en-US)
- [x] 简体中文 (zh-Hans)

## 🔧 安装方法

1. 确保已安装 [tModLoader](https://store.steampowered.com/app/1281930/)（Steam 版）
2. 在 tModLoader 的**模组浏览器**中搜索"更懒的体验"或"terraria_gldty"
3. 下载并启用
4. 创建新世界，新手大礼包自动获得！

## 🛠️ 开发

### 环境要求

- [tModLoader](https://github.com/tModLoader/tModLoader) 1.4.4+
- .NET 8.0 SDK
- Visual Studio 2022 或更高版本

### 构建

```bash
git clone https://github.com/xuebao/terraria_gldty.git
# 用 Visual Studio 打开 .csproj 文件，按 Ctrl+B 构建
```

### 项目结构

```
terraria_gldty/
├── Common/
│   ├── Players/
│   │   └── PackPlayer.cs              # 玩家数据（礼包领取状态）
│   └── Systems/
│       ├── PackRecipeConditions.cs     # 自定义合成条件
│       ├── PackRecipeGroups.cs         # 合成配方组（职业徽章组）
│       └── PackLootRegistry.cs         # 掉落注册中心 + Call 接口
├── Content/
│   └── Items/
│       └── Packs/                     # 所有礼包物品
│           ├── PotionPacks.cs          # 29 种药剂包（基类继承）
│           ├── StarterPack.cs          # 新手大礼包
│           ├── GuideBook.cs            # 计划书
│           ├── EyeOfCthulhuPack.cs     # 克苏鲁之眼礼包
│           ├── EvilBossPack.cs         # 邪恶 Boss 礼包
│           ├── KingSlimePack.cs        # 史莱姆王礼包
│           ├── HardmodePack.cs         # 困难模式礼包
│           ├── MechBossPack.cs         # 机械礼包
│           ├── JunglePack.cs           # 丛林礼包
│           ├── SpookyWoodPack.cs        # 阴森木礼包
│           ├── BeetlePack.cs           # 甲虫礼包
│           ├── CultistPack.cs          # 教徒礼包
│           └── MoonLordPack.cs         # 月总礼包
├── Localization/
│   ├── en-US_Mods.terraria_gldty.hjson
│   └── zh-Hans_Mods.terraria_gldty.hjson
├── build.txt
├── description.txt
└── terraria_gldty.csproj
```

## 📄 许可证

[MIT](LICENSE)

## 🙏 致谢

- [tModLoader](https://github.com/tModLoader/tModLoader) 团队
- 所有贡献者和玩家