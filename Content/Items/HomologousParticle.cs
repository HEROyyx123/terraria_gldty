using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items 
{
    public class HomologousParticle : ModItem
    {
        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 0, 50, 0); // 售价 50 银币
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            // 尝试获取 Fargo Souls 中的黛薇安能量
            if (ModLoader.HasMod("FargowiltasSouls") && ModContent.TryFind("FargowiltasSouls", "DeviatingEnergy", out ModItem devEnergy))
            {
                CreateRecipe(1)
                    .AddIngredient(devEnergy.Type, 1) // 1 个黛薇安能量
                    .AddTile(TileID.Anvils)
                    .Register();
            }
            else
            {
                // // 保留备用/降级配方（防止未开启 Fargo Mod 时报错）
                // CreateRecipe(1)
                //     .AddIngredient(ItemID.FallenStar, 1)
                //     .AddTile(TileID.Anvils)
                //     .Register();
            }
        }
    }
}