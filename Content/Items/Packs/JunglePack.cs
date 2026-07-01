using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 丛林礼包 - 击败世纪之花后解锁
    /// </summary>
    public class JunglePack : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            player.QuickSpawnItem(source, ItemID.TempleKey);
            player.QuickSpawnItem(source, ItemID.SolarTablet, 10);
            player.QuickSpawnItem(source, ItemID.LihzahrdPowerCell, 10);
            player.QuickSpawnItem(source, ItemID.LifeFruit, 99);
            player.QuickSpawnItem(source, ItemID.BrokenHeroSword);
            player.QuickSpawnItem(source, ItemID.Ectoplasm, 99);
            player.QuickSpawnItem(source, ItemID.Autohammer);
            player.QuickSpawnItem(source, ItemID.ChlorophyteBar, 100);

            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedJunglePack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.TempleKey)
                .AddTile(TileID.MythrilAnvil)
                .AddCondition(Condition.DownedPlantera)
                .Register();
        }
    }
}