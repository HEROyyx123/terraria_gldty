using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using terraria_gldty.Content.Items.Items145.Proj;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;


namespace  terraria_gldty.Content.Items.Items145
{
    public class StormDecree : ModItem
    {
        // 存储当前装备的染料物品类型（0 表示没有染料）
        public int AppliedDyeType = 0;
        public override void SetDefaults()
        {
            Item.damage = 285;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.width = 40;
            Item.height = 38;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<StormLightningProj>();
            Item.shootSpeed = 16f;
        }


        public override bool CanRightClick() => true;

        // 右键逻辑：如果光标上拿着染料，则吸收染料；如果没拿，则退还染料
        public override void RightClick(Player player)
        {
            Item mouseItem = Main.mouseItem;

            // 1. 如果玩家手里拿着染料（dye > 0）
            if (!mouseItem.IsAir && mouseItem.dye > 0)
            {
                // 如果武器里已有旧染料，退还给玩家
                if (AppliedDyeType > 0)
                {
                    player.QuickSpawnItem(player.GetSource_Misc("DyeUnequip"), AppliedDyeType, 1);
                }

                // 镶嵌新染料，并消耗手里的一颗
                AppliedDyeType = mouseItem.type;
                mouseItem.stack--;
                if (mouseItem.stack <= 0) mouseItem.TurnToAir();

                Main.NewText("已成功为武器涂装染料！", Color.Cyan);
            }
            // 2. 如果手里没拿东西，且武器上有染料，则取下染料
            else if (AppliedDyeType > 0)
            {
                player.QuickSpawnItem(player.GetSource_Misc("DyeUnequip"), AppliedDyeType, 1);
                AppliedDyeType = 0;
                Main.NewText("已卸下染料。", Color.Yellow);
            }
        }

        // 阻止右键时把武器给直接消耗掉了！
        public override bool ConsumeItem(Player player) => false;

        // 保证切换世界或存入箱子后染料数据不会丢失
        public override void SaveData(TagCompound tag)
        {
            tag["AppliedDyeType"] = AppliedDyeType;
        }

        public override void LoadData(TagCompound tag)
        {
            AppliedDyeType = tag.GetInt("AppliedDyeType");
        }

        // 发射弹幕时，将染料传递给弹幕
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 获取鼠标在世界中的真实点击坐标
            Vector2 targetPos = Main.MouseWorld;

            // 创建弹幕，初始位置设为鼠标点击处
            int projIndex = Projectile.NewProjectile(source, targetPos, Vector2.Zero, type, damage, knockback, player.whoAmI);

            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                // ai[1] 记录染料物品 ID
                Main.projectile[projIndex].ai[1] = AppliedDyeType;
            }
            return false;
        }

        // 在 Tooltip 中显示当前镶嵌的染料名称
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            if (AppliedDyeType > 0)
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(AppliedDyeType);
                tooltips.Add(new TooltipLine(Mod, "DyeInfo", $"[i:{AppliedDyeType}] 已绑定染料: {tempItem.Name}")
                {
                    OverrideColor = Color.LightSkyBlue
                });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "DyeInfo", "提示: 右键按住染料可为闪电染色")
                {
                    OverrideColor = Color.Gray
                });
            }
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 15)
                .AddIngredient(ItemID.LightningCarrot, 1)
                .AddTile(TileID.MythrilAnvil) 
                .Register();
        }

    }
}