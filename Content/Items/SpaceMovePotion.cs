using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class SpaceMovePotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20; // 旅途模式解锁数量
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(0, 0, 50, 0);

            // 绑定生成的 Buff 与持续时间
            Item.buffType = ModContent.BuffType<Buff.SpaceMoveBuff>();
            Item.buffTime = 18000;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.PixieDust, 1)
                .AddIngredient(ItemID.CrystalShard, 1)
                .AddTile(TileID.Bottles) // 在放置的瓶子旁合成
                .Register();
        }
    }
}