using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Content.Items.Items145
{
    public class SilverShield : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 36;
            Item.accessory = true;
            Item.defense = 2;
            Item.rare = ItemRarityID.LightRed; // 进阶饰品稀有度设为粉/浅红
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 1. 黑曜石护盾效果：防击退 & 免疫火块伤害（陨石、狱卒石等）
            player.noKnockback = true;
            player.fireWalk = true;

            // 2. 银护腕继承效果：标记上限 +1，标记持续时间翻倍
            var modPlayer = player.GetModPlayer<SilverBracerPlayer>();
            modPlayer.hasSilverBracer = true;
            modPlayer.maxWhipTags += 1;
            modPlayer.whipTagDurationMult *= 2.0f;
        }

        // 配方定义：用 银护腕 + 黑曜石护盾 在 工匠作坊 合成
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SilverBracer>() // 银护腕
                .AddIngredient(ItemID.ObsidianShield) // 黑曜石护盾
                .AddTile(TileID.TinkerersWorkbench) // 工匠作坊
                .Register();
        }
    }
}