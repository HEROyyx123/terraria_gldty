using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 机械礼包 - 用三种机械魂合成
    /// </summary>
    public class MechBossPack : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Pink;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            player.QuickSpawnItem(source, ItemID.SoulofSight, 99);
            player.QuickSpawnItem(source, ItemID.SoulofMight, 99);
            player.QuickSpawnItem(source, ItemID.SoulofFright, 99);
            player.QuickSpawnItem(source, ItemID.HallowedBar, 100);
            player.QuickSpawnItem(source, ItemID.LifeFruit);

            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedMechBossPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofFright)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}