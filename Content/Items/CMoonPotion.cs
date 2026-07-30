using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;


namespace terraria_gldty.Content.Items // 注意：替换为你的 Mod 命名空间
{
    public class CMoonPotion : ModItem
    {
        public override void SetStaticDefaults() {
            // 在 tModLoader 1.4.4 中，物品名称和描述通常在 hjson 语言包中设置，
            // 也可以在这里直接设置展示文本：
            Item.ResearchUnlockCount = 20; // 旅途模式解锁所需数量
        }

        public override void SetDefaults() {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3; // 喝药水音效
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange; // 橙色稀有度
            Item.value = Item.sellPrice(0, 0, 10, 0);

            // 绑定对应的 Buff，持续时间设置为 3 分钟 (3 * 60 * 60 帧)
            Item.buffType = ModContent.BuffType<Buff.CMoonBuff>();
            Item.buffTime = 10800; 
            // 使用原版重力药水 (GravitationPotion) 的贴图纹理作为底层资源基底
            // 这样如果你没有放入自带图片，它会自动去读取原版重力药水贴图
            TextureAssets.Item[Type] = TextureAssets.Item[ItemID.GravitationPotion];
        }
        // 1. 在背包/快捷栏/UI中绘制时旋转 90°
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            // 获取原版重力药水的贴图
            Texture2D texture = TextureAssets.Item[ItemID.GravitationPotion].Value;

            // 旋转 90 度 (MathHelper.PiOver2)
            float rotation = MathHelper.PiOver2;

            // 重新指定旋转中心为贴图正中心
            Vector2 textureOrigin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            // 自定义绘制旋转后的物品
            spriteBatch.Draw(
                texture,
                position,
                frame,
                drawColor,
                rotation,
                textureOrigin,
                scale,
                SpriteEffects.None,
                0f
            );

            return false; // 返回 false，阻止游戏再用默认方式绘制一遍
        }

        // 2. 在掉落到游戏世界中（地上）时旋转 90°
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            Texture2D texture = TextureAssets.Item[ItemID.GravitationPotion].Value;

            // 在原本地面旋转的基础上额外旋转 90 度
            float customRotation = rotation + MathHelper.PiOver2;

            Vector2 textureOrigin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            spriteBatch.Draw(
                texture,
                Item.Center - Main.screenPosition,
                null,
                Item.GetAlpha(lightColor),
                customRotation,
                textureOrigin,
                scale,
                SpriteEffects.None,
                0f
            );

            return false; // 返回 false，阻止游戏绘制默认贴图
        }
        // 合成配方：1个重力药水 + 10个坠落之星
        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.GravitationPotion, 1)
                .AddIngredient(ItemID.FallenStar, 10)
                .AddTile(TileID.Bottles) // 在放置的瓶子/炼金台上合成
                .Register();
        }
    }
}