using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Items.Weapons;
using Terraria.Audio; 
namespace terraria_gldty.Content.Items
{
    public class BambooCicada : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.Yoyo[Item.type] = true;
            ItemID.Sets.GamepadExtraRange[Item.type] = 15;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.width = 30;
            Item.height = 30;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.knockBack = 4.5f;
            Item.damage = 38;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.DamageType = DamageClass.MeleeNoSpeed; 

            //原始
            //Item.UseSound = SoundID.Item1;
            
            // Item.UseSound = new SoundStyle("terraria_gldty/Assets/Sounds/BambooCicada") {
            //                 PitchVariance = 0.2f, // 每次播放时音调会有 ±20% 的随机微调，避免连续听起来机械单调
            //                 Volume = 0.8f         // 调整音量 (0.0 ~ 1.0)
            //                 };

            Item.shoot = ModContent.ProjectileType<BambooCicadaProjectile>();
            Item.shootSpeed = 16f;
        }

        public override void HoldItem(Player player)
        {
            // 手持该武器时获得【鱼音缭绕】Buff
            player.AddBuff(ModContent.BuffType<Buff.FishyResonanceBuff>(), 2);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BambooBlock, 15)
                .AddIngredient(ItemID.Cobweb, 20)
                .AddIngredient(ItemID.Goldfish, 1)
                .AddIngredient(ItemID.WoodYoyo, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
