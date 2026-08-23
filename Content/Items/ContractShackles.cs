using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Content.Items
{
    public class ContractShackles : ModItem
    {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults() {
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.buyPrice(0, 4, 0, 0);
            Item.UseSound = SoundID.Item29; 
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips.Add(new TooltipLine(Mod, "EffectTip1", "[c/C77SOO:永久强化效果：]"));
            tooltips.Add(new TooltipLine(Mod, "EffectTip2", "[c/BE93D4:✦ 同种怪物召唤上限 +1]"));
            tooltips.Add(new TooltipLine(Mod, "EffectTip3", "[c/BE93D4:✦ 召唤的怪物获得 135% 体型与碰撞箱]"));
            tooltips.Add(new TooltipLine(Mod, "EffectTip4", "[c/BE93D4:✦ 召唤的怪物移动速度提升]"));
        }

        public override bool CanUseItem(Player player) {
            var modPlayer = player.GetModPlayer<TamedPlayer>();
            return !modPlayer.usedContractShackles;
        }

        public override bool? UseItem(Player player) {
            var modPlayer = player.GetModPlayer<TamedPlayer>();
            modPlayer.usedContractShackles = true;

            for (int i = 0; i < 35; i++) {
                Dust.NewDust(player.position, player.width, player.height, DustID.PurpleCrystalShard, 0, -3f);
            }

            if (player.whoAmI == Main.myPlayer) {
                Main.NewText("契约枷锁已熔入你的灵魂，召唤兽获得了巨化体型与幽灵虚影！", Color.Violet);
            }

            return true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.MagicMirror, 1)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.HallowedBar, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}