using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class EnchantingWorkshop : ModItem
    {
        public override void SetStaticDefaults() {
            // 微光转换：工匠作坊 -> 附魔作坊
            ItemID.Sets.ShimmerTransformToItem[ItemID.TinkerersWorkshop] = Type;
        }

        public override void SetDefaults() {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.EnchantingWorkshopTile>());
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes() {
            // 也可以反向合成回工匠作坊
            CreateRecipe()
                .AddIngredient(ItemID.TinkerersWorkshop)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
