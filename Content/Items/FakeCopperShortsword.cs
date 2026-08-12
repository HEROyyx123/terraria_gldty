using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Items.Packs;

namespace terraria_gldty.Content.Items 
{
    public class FakeCopperShortsword : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.HallowedBar;

        public override void SetDefaults() {
            Item.damage = 100; 
            Item.DamageType = DamageClass.Melee;
            Item.width = 32;
            Item.height = 32;

            Item.useTime = 6; 
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true; // 自动连续挥舞
            Item.channel = true;

            Item.knockBack = 4f;
            Item.value = Item.sellPrice(copper: 1);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item73; 

            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.shoot = ModContent.ProjectileType<Weapons.FakeCopperShortswordProj>();
            Item.shootSpeed = 16f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            // 每次发射带有微微的随机偏移，形成“万剑归宗/飞剑雨”的效果
            Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(15));
            
            Projectile.NewProjectile(
                source,
                player.Center,
                perturbedSpeed,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            return false; // 阻止默认发射
        }

        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();    
            //recipe.AddIngredient<GuideBook>(1);    
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}