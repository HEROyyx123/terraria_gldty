using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class ResonanceTransmuterItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; 
            Item.consumable = true;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;
            
            // 指向放置后的家具 Tile
            Item.createTile = ModContent.TileType<Tiles.ResonanceTransmuterTile>(); 
        }

        public override void AddRecipes()
        {
            if (ModLoader.HasMod("FargowiltasSouls") )
            {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 10) // 任意铁/铅锭
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
            }
        }
    }
}