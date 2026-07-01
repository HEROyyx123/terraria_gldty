using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 计划书 - 使用后显示下一个未获得礼包的合成条件
    /// </summary>
    public class GuideBook : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Book;

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
            Item.UseSound = SoundID.Item37; // 翻书声
        }

        public override bool CanUseItem(Player player) {
            var packPlayer = player.GetModPlayer<Common.Players.PackPlayer>();

            // 按顺序检查每个礼包
            string message = GetNextUnreceivedMessage(packPlayer);

            if (message != null) {
                // 在聊天栏显示信息
                Main.NewText(message, 255, 255, 0);
            }

            return true; // 不可消耗，始终返回 true
        }

        /// <summary>
        /// 获取下一个未获得礼包的提示信息
        /// </summary>
        private static string GetNextUnreceivedMessage(Common.Players.PackPlayer packPlayer) {
            // 按流程顺序检查
            // 1. 克苏鲁之眼礼包
            if (!packPlayer.ReceivedEyeOfCthulhuPack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintEyeOfCthulhuPack");
            }
            // 2. 邪恶 Boss 礼包
            if (!packPlayer.ReceivedEvilBossPack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintEvilBossPack");
            }
            // 3. 史莱姆王礼包
            if (!packPlayer.ReceivedKingSlimePack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintKingSlimePack");
            }
            // 4. 困难模式礼包
            if (!packPlayer.ReceivedHardmodePack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintHardmodePack");
            }
            // 5. 机械礼包
            if (!packPlayer.ReceivedMechBossPack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintMechBossPack");
            }
            // 6. 丛林礼包
            if (!packPlayer.ReceivedJunglePack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintJunglePack");
            }
            // 7. 阴森木礼包
            if (!packPlayer.ReceivedSpookyWoodPack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintSpookyWoodPack");
            }
            // 8. 甲虫礼包
            if (!packPlayer.ReceivedBeetlePack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintBeetlePack");
            }
            // 9. 教徒礼包
            if (!packPlayer.ReceivedCultistPack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintCultistPack");
            }
            // 10. 月总礼包
            if (!packPlayer.ReceivedMoonLordPack) {
                return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.HintMoonLordPack");
            }

            // 全部已获得
            return Language.GetTextValue("Mods.terraria_gldty.Items.GuideBook.AllDone");
        }

        public override void AddRecipes() {
            // 计划书不可合成，仅从新手礼包开出
        }
    }
}