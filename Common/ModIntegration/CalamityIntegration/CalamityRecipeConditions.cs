using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration.CalamityIntegration
{
    /// <summary>
    /// 灾厄 Boss 击败条件的反射读取工具
    /// 通过反射读取 CalamityMod.Systems.DownedBossSystem 的静态字段来判断 Boss 是否被击败
    /// </summary>
    public static class CalamityRecipeConditions
    {
        private static Type _downedBossType;
        private static Mod _calamityMod;

        // === 各个 Boss 的击败条件 ===
        public static Condition DownedDesertScourge { get; private set; }
        public static Condition DownedSlimeGod { get; private set; }
        public static Condition DownedCrabulon { get; private set; }
        public static Condition DownedHiveMind { get; private set; }
        public static Condition DownedPerforators { get; private set; }
        public static Condition DownedAquaticScourge { get; private set; }
        public static Condition DownedCalamitasClone { get; private set; }
        public static Condition DownedLeviathan { get; private set; }
        public static Condition DownedRavager { get; private set; }
        public static Condition DownedProfanedGuardians { get; private set; }
        public static Condition DownedProvidence { get; private set; }
        public static Condition DownedPolterghast { get; private set; }
        public static Condition DownedDevourerOfGods { get; private set; }
        public static Condition DownedExoMechs { get; private set; }
        public static Condition DownedYharon { get; private set; }
        public static Condition DownedCalamitas { get; private set; }

        public static void Initialize(Mod calamityMod) {
            _calamityMod = calamityMod;
            if (_calamityMod == null) return;

            try {
                // 尝试获取 DownedBossSystem 类型（名称可能随版本变化）
                _downedBossType = _calamityMod.Code.GetType("CalamityMod.Systems.DownedBossSystem")
                    ?? _calamityMod.Code.GetType("CalamityMod.Events.DownedBoss")
                    ?? _calamityMod.Code.GetType("CalamityMod.NPCs.DownedBoss");
            }
            catch (Exception) {
                ModContent.GetInstance<terraria_gldty>().Logger.Warn("无法反射获取 Calamity 的 DownedBossSystem 类型");
            }

            // 注册条件（每个条件 try-catch，单个失败不影响其他）
            DownedDesertScourge = CreateCondition("downedDesertScourge",
                "Mods.terraria_gldty.Conditions.Calamity.DownedDesertScourge");

            DownedSlimeGod = CreateCondition("downedSlimeGod",
                "Mods.terraria_gldty.Conditions.Calamity.DownedSlimeGod");

            DownedCrabulon = CreateCondition("downedCrabulon",
                "Mods.terraria_gldty.Conditions.Calamity.DownedCrabulon");

            DownedHiveMind = CreateCondition("downedHiveMind",
                "Mods.terraria_gldty.Conditions.Calamity.DownedHiveMind");

            DownedPerforators = CreateCondition("downedPerforators",
                "Mods.terraria_gldty.Conditions.Calamity.DownedPerforators");

            DownedAquaticScourge = CreateCondition("downedAquaticScourge",
                "Mods.terraria_gldty.Conditions.Calamity.DownedAquaticScourge");

            DownedCalamitasClone = CreateCondition("downedCalamitasClone",
                "Mods.terraria_gldty.Conditions.Calamity.DownedCalamitasClone");

            DownedLeviathan = CreateCondition("downedLeviathan",
                "Mods.terraria_gldty.Conditions.Calamity.DownedLeviathan");

            DownedRavager = CreateCondition("downedRavager",
                "Mods.terraria_gldty.Conditions.Calamity.DownedRavager");

            DownedProfanedGuardians = CreateCondition("downedProfanedGuardians",
                "Mods.terraria_gldty.Conditions.Calamity.DownedProfanedGuardians");

            DownedProvidence = CreateCondition("downedProvidence",
                "Mods.terraria_gldty.Conditions.Calamity.DownedProvidence");

            DownedPolterghast = CreateCondition("downedPolterghast",
                "Mods.terraria_gldty.Conditions.Calamity.DownedPolterghast");

            DownedDevourerOfGods = CreateCondition("downedDevourerOfGods",
                "Mods.terraria_gldty.Conditions.Calamity.DownedDevourerOfGods");

            DownedExoMechs = CreateCondition("downedExoMechs",
                "Mods.terraria_gldty.Conditions.Calamity.DownedExoMechs");

            DownedYharon = CreateCondition("downedYharon",
                "Mods.terraria_gldty.Conditions.Calamity.DownedYharon");

            DownedCalamitas = CreateCondition("downedCalamitas",
                "Mods.terraria_gldty.Conditions.Calamity.DownedCalamitas");
        }

        private static Condition CreateCondition(string fieldName, string localizationKey) {
            try {
                var field = _downedBossType?.GetField(fieldName,
                    BindingFlags.Public | BindingFlags.Static);
                if (field != null && field.FieldType == typeof(bool)) {
                    return new Condition(localizationKey, () => {
                        try { return (bool)field.GetValue(null); }
                        catch { return false; }
                    });
                }
            }
            catch (Exception) { }

            // 退一步：用 Main.hardMode 作为通用替代（仅反射失败时）
            return new Condition(localizationKey, () => Main.hardMode);
        }
    }
}