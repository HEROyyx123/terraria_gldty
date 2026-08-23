using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;


namespace terraria_gldty.Content.Items
{
    public class SoulMark : ModItem
    {
        public override void SetStaticDefaults() {
            // 注册为生命水晶类消耗品的动画逻辑（消耗后提示）
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults() {
            Item.width = 26;
            Item.height = 26;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.UseSound = SoundID.Item4; // 类似于使用生命水晶的声音
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EffectTip1", "[c/C77SOO:永久强化效果：]"));
            tooltips.Add(new TooltipLine(Mod, "EffectTip2", "[c/BE93D4:✦ 同种怪物召唤上限 +1]"));
        }

        public override bool CanUseItem(Player player) {
            var modPlayer = player.GetModPlayer<TamedPlayer>();
            // 如果尚未消耗过该物品，则可以使用
            return !modPlayer.usedSoulMark;
        }

        public override bool? UseItem(Player player) {
            var modPlayer = player.GetModPlayer<TamedPlayer>();
            modPlayer.usedSoulMark = true;

            // 生成特效
            for (int i = 0; i < 25; i++) {
                Dust.NewDust(player.position, player.width, player.height, DustID.Enchanted_Pink, 0, -2f);
            }

            if (player.whoAmI == Main.myPlayer) {
                Main.NewText("你的灵魂得到了拓展，同种怪物的召唤上限 +1！", Color.MediumSpringGreen);
            }

            return true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.Bass, 1)
                .AddIngredient(ItemID.WaterCandle, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}