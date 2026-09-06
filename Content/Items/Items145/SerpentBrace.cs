using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Content.Items.Items145
{
    public class SerpentBrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 22;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 激活玩家身上的效果标记
            player.GetModPlayer<SerpentBracePlayer>().hasSerpentBrace = true;
        }

         public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 15)
                .AddIngredient(ItemID.ManaRegenerationBand, 1)
                .AddTile(TileID.TinkerersWorkbench) 
                .Register();
        }
    }
}