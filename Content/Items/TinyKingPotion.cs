using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class TinyKingPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
             Item.ResearchUnlockCount = 20; // 旅途模式解锁数量
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightRed; // 稀有度
            Item.value = Item.sellPrice(0, 0, 50, 0);

            // 给玩家赋予的 Buff 类型和持续时间（例如 5 分钟 = 18000 帧）
            Item.buffType = ModContent.BuffType<Buff.TinyKingBuff>();
            Item.buffTime = 18000;
        }

        // 配方合成
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Deathweed, 1)
                .AddIngredient(ItemID.SoulofFright, 1) 
                .AddTile(TileID.Bottles) // 放置的瓶子或炼药台
                .Register();
        }
    }
}