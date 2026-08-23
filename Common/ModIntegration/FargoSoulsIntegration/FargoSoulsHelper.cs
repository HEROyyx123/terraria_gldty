using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration.FargoSoulsHelper
{
    public static class FargoSoulsHelper
    {
        public const string FargoModName = "FargowiltasSouls";

        public static List<int> PreHMEncList = new();
        public static List<int> HMEncList = new();
        public static List<int> ForcesList = new();
        public static List<int> SoulsList = new();

        public static bool IsFargoLoaded => ModLoader.HasMod(FargoModName);

        public static void Initialize()
        {
            if (!ModLoader.TryGetMod(FargoModName, out Mod fargoMod))
            {
                Main.NewText("[魂石兑换机调试] 未检测到 Fargo 模组 (FargowiltasSouls)！", Color.Red);
                return;
            }

            PreHMEncList.Clear();
            HMEncList.Clear();
            ForcesList.Clear();
            SoulsList.Clear();

            // 注册魂石/力/魂 
            RegisterGroup(fargoMod, new[] { 
                "CopperEnchant", "TimberEnchant", "CrimsonEnchant", "CactusEnchant", "WoodEnchant", "WizardEnchant","TungstenEnchant", "TinEnchant", 
                "SnowEnchant", "SilverEnchant", "ShadowEnchant", "ShadewoodEnchant","RichMahoganyEnchant", "RainEnchant", "PumpkinEnchant", "PlatinumEnchant",
                "PalmWoodEnchant", "MinerEnchant", "LeadEnchant", "JungleEnchant", "IronEnchant", "GoldEnchant", "GladiatorEnchant","FossilEnchant",
                "EbonwoodEnchant", "CactusEnchant", "BorealWoodEnchant", "AshWoodEnchant", "AnglerEnchant", "AncientCobaltEnchant", "ObsidianEnchant", 
                "NinjaEnchant", "NecroEnchant", "MoltenEnchant", "MeteorEnchant", "BeeEnchant"}, PreHMEncList);//肉前
            RegisterGroup(fargoMod, new[] { "AdamantiteEnchant", "ChlorophyteEnchant", "HallowedEnchant", "OrichalcumEnchant", "TitaniumEnchant", "SpiderEnchant", "PalladiumEnchant",
                "OrichalcumEnchant", "MythrilEnchant", "FrostEnchant", "ForbiddenEnchant", "CobaltEnchant", "AncientShadowEnchant", "AdamantiteEnchant" }, HMEncList);//肉后
            RegisterGroup(fargoMod, new[] { "TimberForce", "EarthForce", "SpiritForce", "ShadowForce", "WillForce", "LifeForce", "NatureForce", "TerraForce" }, ForcesList);//力
            RegisterGroup(fargoMod, new[] {"MasochistSoul", "ArchWizardsSoul", "BerserkerSoul", "ColossusSoul", "ConjuristsSoul", "MasochistSoul", "SnipersSoul", "TrawlerSoul", "WorldShaperSoul" }, SoulsList);//魂

            Main.NewText($"[魂石兑换机调试] Fargo数据已载入！肉前:{PreHMEncList.Count} | 肉后:{HMEncList.Count} | 力:{ForcesList.Count} | 魂:{SoulsList.Count}", Color.Yellow);
        }

        private static void RegisterGroup(Mod fargoMod, string[] itemNames, List<int> targetList)
        {
            foreach (var name in itemNames)
            {
                if (fargoMod.TryFind<ModItem>(name, out var modItem))
                {
                    targetList.Add(modItem.Type);
                }
            }
        }

        public static int GetGroupType(int itemType)
        {
            if (PreHMEncList.Contains(itemType)) return 1;
            if (HMEncList.Contains(itemType)) return 2;
            if (ForcesList.Contains(itemType)) return 3;
            if (SoulsList.Contains(itemType)) return 4;
            return 0;
        }
    }
}