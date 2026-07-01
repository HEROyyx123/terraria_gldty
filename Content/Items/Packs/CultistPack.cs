using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 教徒礼包 - 用远古操纵机合成
    /// </summary>
    public class CultistPack : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Cyan;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            player.QuickSpawnItem(source, ItemID.LunarCraftingStation);
            player.QuickSpawnItem(source, ItemID.LunarHook);
            player.QuickSpawnItem(source, ItemID.BottomlessShimmerBucket);

            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedCultistPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.LunarCraftingStation)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}