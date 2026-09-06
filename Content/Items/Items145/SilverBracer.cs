using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Content.Items.Items145
{
    public class SilverBracer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 75);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<SilverBracerPlayer>();
            modPlayer.hasSilverBracer = true;
            modPlayer.maxWhipTags += 1; // 标记上限 +1
            modPlayer.whipTagDurationMult *= 2.0f; // 标记持续时间翻倍
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SilverBar, 1)
                .AddTile(TileID.WorkBenches) 
                .Register();
        }
    }
}