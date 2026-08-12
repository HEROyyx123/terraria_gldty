using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Buff;

namespace terraria_gldty.Content.Items.Weapons
{
    public class LuminiteStaff : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarBar;

        public override void SetDefaults() {
            Item.damage = 80; 
            Item.DamageType = DamageClass.Summon; // 召唤伤害
            Item.mana = 10;                        // 消耗 10 点魔力
            Item.width = 40;
            Item.height = 40;

            Item.useTime = 20; 
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp; // 举起法杖召唤
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red; 
            Item.UseSound = SoundID.Item44; // 标准召唤音效

            Item.noMelee = true;

            // 绑定的召唤物弹幕与 Buff
            Item.shoot = ModContent.ProjectileType<LuminiteMinionProj>();
            Item.buffType = ModContent.BuffType<LuminiteMinionBuff>();
            Item.shootSpeed = 10f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            // 给玩家赋予召唤 Buff
            player.AddBuff(Item.buffType, 2);

            // 在鼠标位置生成召唤物
            var projectile = Projectile.NewProjectileDirect(
                source,
                Main.MouseWorld,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI
            );
            projectile.originalDamage = Item.damage;

            return false; // 阻止默认发射
        }

        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();    
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}