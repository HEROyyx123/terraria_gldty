using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    /// <summary>
    /// 礼包掉落注册中心，集中管理所有礼包的内容和概率。
    /// 同时提供 Call 接口供其他 Mod 覆盖礼包内容。
    /// </summary>
    public class PackLootRegistry
    {
        /// <summary> 礼包 Key → 默认掉落规则（在 PostSetupContent 中由其他 Mod 覆盖） </summary>
        private static readonly Dictionary<string, Action<ItemLoot>> DefaultLoot = new();
        /// <summary> 礼包 Key → 其他 Mod 注册的覆盖规则（优先于 DefaultLoot） </summary>
        internal static readonly Dictionary<string, Action<ItemLoot>> OverrideLoot = new();

        /// <summary> 所有礼包的 Key 列表（用于遍历） </summary>
        public static readonly Dictionary<string, string> PackKeys = new()
        {
            { "starter", "StarterPack" },
            { "eyeofcthulhu", "EyeOfCthulhuPack" },
            { "evilboss", "EvilBossPack" },
            { "kingslime", "KingSlimePack" },
            { "hardmode", "HardmodePack" },
            { "mechboss", "MechBossPack" },
            { "jungle", "JunglePack" },
            { "spookywood", "SpookyWoodPack" },
            { "beetle", "BeetlePack" },
            { "cultist", "CultistPack" },
            { "moonlord", "MoonLordPack" },
        };

        /// <summary> 注册默认掉落（在 Mod.Load 中由各礼包调用） </summary>
        public static void RegisterDefault(string packKey, Action<ItemLoot> setup) {
            DefaultLoot[packKey] = setup;
        }

        /// <summary> 根据 Key 获取礼包的 ClassName（供其他 Mod 查询） </summary>
        public static string GetPackClassName(string packKey) {
            return PackKeys.TryGetValue(packKey, out var name) ? name : null;
        }

        /// <summary> 检查是否有覆盖规则 </summary>
        internal static bool HasOverride(string packKey) {
            return OverrideLoot.ContainsKey(packKey);
        }

        /// <summary> 应用掉落（优先使用覆盖规则，其次使用默认规则） </summary>
        public static void Apply(string packKey, ItemLoot itemLoot) {
            if (OverrideLoot.TryGetValue(packKey, out var overrideAction)) {
                overrideAction(itemLoot);
            }
            else if (DefaultLoot.TryGetValue(packKey, out var defaultAction)) {
                defaultAction(itemLoot);
            }
        }
    }
}
