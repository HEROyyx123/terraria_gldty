using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    public class GuideBook : ModItem
    {
        public override void SetDefaults() {
            Item.width = 28;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Orange;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.consumable = false;
            Item.UseSound = SoundID.Item37;
        }

        public override bool? UseItem(Player player) {
            // 只在客户端执行
            if (Main.netMode == NetmodeID.Server)
                return false;

            var packPlayer = player.GetModPlayer<Common.Players.PackPlayer>();
            var msg = GetNextHint(packPlayer, player);

            if (msg != null) {
                Main.NewText(msg, 255, 255, 0);
            }

            return true;
        }

        private static string GetNextHint(Common.Players.PackPlayer packPlayer, Player player) {
            bool hasCalamity = ModLoader.TryGetMod("CalamityMod", out _);
            var intPlayer = player.GetModPlayer<Common.ModIntegration.ModIntegrationPlayer>();

            // 1. 史莱姆王礼包
            if (!packPlayer.ReceivedKingSlimePack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintKingSlimePack");

            // 2. （灾厄）荒漠灾虫礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_DesertScourge"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintDesertScourgePack");    

            // 2. 克苏鲁之眼礼包
            if (!packPlayer.ReceivedEyeOfCthulhuPack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintEyeOfCthulhuPack");

            // 3. 邪恶 Boss 礼包
            if (!packPlayer.ReceivedEvilBossPack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintEvilBossPack");

            // 5. （灾厄）史莱姆之神礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_SlimeGod"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintSlimeGodPack");

            // 6. 困难模式礼包
            if (!packPlayer.ReceivedHardmodePack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintHardmodePack");

            // 7. 机械礼包
            if (!packPlayer.ReceivedMechBossPack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintMechBossPack");

            // 8. （灾厄）渊海灾虫礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_AquaticScourge"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintAquaticScourgePack");

            // 9. （灾厄）灾厄之影礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_CalamitasClone"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintCalamitasClonePack");

            // 10. （灾厄）利维坦礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_Leviathan"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintLeviathanPack");

            // 11. 丛林礼包
            if (!packPlayer.ReceivedJunglePack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintJunglePack");

            // 12. 阴森木礼包
            if (!packPlayer.ReceivedSpookyWoodPack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintSpookyWoodPack");

            // 13. 甲虫礼包
            if (!packPlayer.ReceivedBeetlePack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintBeetlePack");

            // 14. （灾厄）毁灭魔像礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_Ravager"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintRavagerPack");    
            
            // 15. 教徒礼包
            if (!packPlayer.ReceivedCultistPack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintCultistPack");

            // 16. 月总礼包
            if (!packPlayer.ReceivedMoonLordPack)
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintMoonLordPack");

            // 17. （灾厄）亵渎守卫礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_ProfanedGuardians"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintProfanedGuardiansPack");

            // 18. （灾厄）亵渎天神礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_Providence"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintProvidencePack");

            // 19. （灾厄）噬魂幽花礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_Polterghast"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintPolterghastPack");

            // 20. （灾厄）神明吞噬者礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_DevourerOfGods"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintDevourerOfGodsPack");

            // 21. （灾厄）犽戎礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_Yharon"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintYharonPack");

            // 22. （灾厄）星流巨械礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_ExoMechs"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintExoMechsPack");

            // 23. （灾厄）至尊灾厄女巫礼包
            if (hasCalamity && !intPlayer.GetFlag("Calamity_Calamitas"))
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.Calamity.HintCalamitasPack");

            return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.AllDone");
        }

        public override void AddRecipes() { }
    }
}