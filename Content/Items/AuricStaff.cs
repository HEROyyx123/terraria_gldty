using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Buff;

namespace terraria_gldty.Content.Items.Weapons
{
    public class AuricStaff : ModItem
    {
        // 替换为原版铜短剑贴图
        public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;

        public override void SetDefaults() {
            Item.damage = 220;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;

            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.autoReuse = true;

            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AuricMinionProj>();
            Item.buffType = ModContent.BuffType<AuricMinionBuff>();
            Item.shootSpeed = 15f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            if (ModLoader.HasMod("CalamityMod")) {
                TooltipLine nameLine = tooltips.Find(l => l.Name == "ItemName" && l.Mod == "Terraria");
                if (nameLine != null) {
                    float pulse = (float)(System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f) + 1f) * 0.5f;
                    nameLine.OverrideColor = Color.Lerp(new Color(255, 215, 0), new Color(255, 255, 200), pulse);
                }
            }

            TooltipLine line1 = new TooltipLine(Mod, "AuricCustomTooltip1", "召唤飞行的铜短剑为你而战");
            TooltipLine line2 = new TooltipLine(Mod, "AuricCustomTooltip2", "[c/FFD700:『蕴含着神威与纪元的铜短剑』]");
            
            tooltips.Add(line1);
            tooltips.Add(line2);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            player.AddBuff(Item.buffType, 2);
            var projectile = Projectile.NewProjectileDirect(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            projectile.originalDamage = Item.damage;
            return false;
        }

        public override void AddRecipes() {
            if (ModLoader.HasMod("CalamityMod") && ModContent.TryFind("CalamityMod/AuricBar", out ModItem auricBar)) {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(auricBar.Type, 5);
                recipe.AddIngredient(ItemID.CopperShortsword, 1);
                recipe.AddTile(TileID.LunarCraftingStation);
                recipe.Register();
            }
        }
    }
}