using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class AnomalyAccessory3 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 12, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.endurance += 0.25f; // 受到的所有伤害降低 25%
            player.lifeRegen += 12; // 大幅生命再生（与饰品 2 数值一致）
            player.GetDamage(DamageClass.Generic) += 0.05f; // 造成的伤害提升 5%
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<AnomalyAccessory1>()
                .AddIngredient<AnomalyAccessory2>()
                .AddIngredient(ItemID.LunarBar, 10) // 夜明锭 x10
                .AddTile(TileID.LunarCraftingStation) // 远古操纵台合成
                .Register();
        }
    }
}