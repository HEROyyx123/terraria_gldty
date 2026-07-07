using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration.CalamityIntegration
{
    /// <summary>
    /// 灾厄模组联动实现
    /// 实现 IModIntegration 接口，管理13个灾厄礼包
    /// </summary>
    public class CalamityIntegration : IModIntegration
    {
        public string TargetModName => "CalamityMod";
        public bool IsLoaded { get; set; }

        private Mod _calamityMod;

        // === 13 个灾厄礼包的 Key → ClassName ===
        public Dictionary<string, string> GetPackEntries() => new()
        {
            { "desertscourge", "DesertScourgePack" },
            { "slimgod", "SlimeGodPack" },
            { "aquaticscourge", "AquaticScourgePack" },
            { "calamitasclone", "CalamitasClonePack" },
            { "leviathan", "LeviathanPack" },
            { "ravager", "RavagerPack" },
            { "profanedguardians", "ProfanedGuardiansPack" },
            { "providence", "ProvidencePack" },
            { "polterghast", "PolterghastPack" },
            { "devourerofgods", "DevourerOfGodsPack" },
            { "exomechs", "ExoMechsPack" },
            { "yharon", "YharonPack" },
            { "calamitas", "CalamitasPack" },
        };

        // === 13 个存档标记 ===
        public string[] GetPlayerFlagKeys() => new[]
        {
            "Calamity_DesertScourge",
            "Calamity_SlimeGod",
            "Calamity_AquaticScourge",
            "Calamity_CalamitasClone",
            "Calamity_Leviathan",
            "Calamity_Ravager",
            "Calamity_ProfanedGuardians",
            "Calamity_Providence",
            "Calamity_Polterghast",
            "Calamity_DevourerOfGods",
            "Calamity_ExoMechs",
            "Calamity_Yharon",
            "Calamity_Calamitas",
        };

        public void Load() {
            if (!IsLoaded) return;

            // 记录灾厄 Mod 实例供内部使用
            ModLoader.TryGetMod("CalamityMod", out _calamityMod);

            // 初始化物品/物块引用
            CalamityItemHelper.Initialize(_calamityMod);

            // 初始化合成条件
            CalamityRecipeConditions.Initialize(_calamityMod);
        }

        /// <summary>
        /// 增强原版礼包——预留接口
        /// </summary>
        public void ModifyExistingPackLoot(string packKey, ItemLoot itemLoot) {
            // 在这里为原版礼包添加额外物品
            if(packKey=="hardmode"){
            AddItem(itemLoot, "RogueEmblem", 1);}
        }

        /// <summary>
        /// 向 GuideBook 的计划书提示中插入灾厄礼包（按流程顺序）
        /// </summary>
        public void AppendGuideHints(List<(string action, string flagKey, string hintKey)> hints) {
            // 在邪恶Boss之后、困难模式之前：荒漠灾虫、史莱姆之神
            hints.Insert(3, ("integration", "Calamity_SlimeGod", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintSlimeGodPack"));
            hints.Insert(3, ("integration", "Calamity_DesertScourge", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintDesertScourgePack"));

            // 在困难模式之后、机械之前
            hints.Insert(5, ("integration", "Calamity_CalamitasClone", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintCalamitasClonePack"));
            hints.Insert(5, ("integration", "Calamity_AquaticScourge", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintAquaticScourgePack"));

            // 在机械之后、丛林之前
            hints.Insert(8, ("integration", "Calamity_Leviathan", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintLeviathanPack"));

            // 在丛林之后、阴森木之前
            hints.Insert(11, ("integration", "Calamity_Ravager", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintRavagerPack"));

            // 在甲虫之后、教徒之前
            hints.Insert(15, ("integration", "Calamity_Providence", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintProvidencePack"));
            hints.Insert(14, ("integration", "Calamity_ProfanedGuardians", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintProfanedGuardiansPack"));

            // 在教徒之后、月总之前
            hints.Insert(18, ("integration", "Calamity_Polterghast", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintPolterghastPack"));

            // 在月总之后
            hints.Insert(21, ("integration", "Calamity_Calamitas", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintCalamitasPack"));
            hints.Insert(21, ("integration", "Calamity_Yharon", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintYharonPack"));
            hints.Insert(21, ("integration", "Calamity_ExoMechs", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintExoMechsPack"));
            hints.Insert(21, ("integration", "Calamity_DevourerOfGods", "Mods.terraria_gldty.Items.GuideBook.Calamity.HintDevourerOfGodsPack"));
        }

        // 辅助方法
        private void AddItem(ItemLoot itemLoot, string name, int stack) {
            int type = CalamityItemHelper.GetItemType(name);
            if (type > 0) {
                itemLoot.Add(ItemDropRule.Common(type, 1, stack, stack));
            }
        }
    }
}