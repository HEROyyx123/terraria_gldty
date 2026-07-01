# 更懒的体验 — Terraria tModLoader Mod

[![tModLoader](https://img.shields.io/badge/tModLoader-1.4.4+-blue)](https://github.com/tModLoader/tModLoader)

一个让你从开荒到毕业都能偷懒的物资礼包模组。告别无聊的挖矿找材料，把时间花在更有趣的事情上！

## 📦 礼包总览

| 礼包 | 解锁条件 | 合成材料 | 合成台 |
|------|---------|---------|:------:|
| 新手大礼包 | 开局自动获得 | — | — |
| 克苏鲁之眼礼包 | 击败克苏鲁之眼 | 10 晶状体 | 工作台 |
| 邪恶 Boss 礼包 | 击败世吞/克脑 | 20 组织样本或暗影鳞片 | 工作台 |
| 史莱姆王礼包 | 击败史莱姆王 | 10 凝胶 | 工作台 |
| 困难模式礼包 | 击败血肉墙 | 任意职业徽章 | 秘银砧 |
| 机械礼包 | — | 视域魂+力量魂+恐惧魂 | 秘银砧 |
| 丛林礼包 | 击败世纪之花 | 神庙钥匙 | 秘银砧 |
| 阴森木礼包 | — | 250 阴森木 | 秘银砧 |
| 甲虫礼包 | — | 甲虫外壳 | 秘银砧 |
| 教徒礼包 | — | 远古操纵机 | 秘银砧 |
| 月总礼包 | — | 夜明锭 | 远古操纵机 |

## 📖 计划书

新手礼包中附赠 **计划书**，使用后会在聊天栏提示下一个未获得礼包的合成条件，全程指引不迷路！

## 🧪 药剂包

包含 29 种药剂包，每种打开获得 30 瓶对应药水。所有药水包直接引用原版药水图标，无需额外贴图。

## 🌐 本地化支持

- [x] English (en-US)
- [x] 简体中文 (zh-Hans)

## 🔧 安装方法

1. 确保已安装 [tModLoader](https://store.steampowered.com/app/1281930/)（Steam 版）
2. 在 tModLoader 的**模组浏览器**中搜索 "更懒的体验" 或 "terraria_gldty"
3. 下载并启用
4. 创建新世界，新手大礼包自动获得！

### 手动安装

1. 从 [Releases](https://github.com/xuebao/terraria_gldty/releases) 下载 `.tmod` 文件
2. 放入 `Documents/My Games/Terraria/tModLoader/Mods/` 文件夹
3. 启动 tModLoader，在模组菜单中启用

## 🛠️ 开发

### 环境要求

- [tModLoader](https://github.com/tModLoader/tModLoader) 1.4.4+
- .NET 8.0 SDK
- Visual Studio 2022 或更高版本

### 构建

```bash
# 克隆仓库
git clone https://github.com/xuebao/terraria_gldty.git

# 用 Visual Studio 打开 .csproj 文件
# 按 Ctrl+B 构建
# 构建后的 .tmod 文件在 bin/Debug/net8.0/ 目录下
```

### 项目结构

```
terraria_gldty/
├── Common/
│   ├── Players/
│   │   └── PackPlayer.cs          # 玩家数据（礼包领取状态）
│   └── Systems/
│       ├── PackRecipeConditions.cs # 自定义合成条件
│       └── PackRecipeGroups.cs     # 合成配方组
├── Content/
│   └── Items/
│       └── Packs/                 # 所有礼包物品
│           ├── PotionPacks.cs      # 药剂包基类
│           ├── StarterPack.cs      # 新手大礼包
│           ├── GuideBook.cs        # 计划书
│           ├── EyeOfCthulhuPack.cs
│           ├── EvilBossPack.cs
│           ├── KingSlimePack.cs
│           ├── HardmodePack.cs
│           ├── MechBossPack.cs
│           ├── JunglePack.cs
│           ├── SpookyWoodPack.cs
│           ├── BeetlePack.cs
│           ├── CultistPack.cs
│           └── MoonLordPack.cs
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