using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 月总礼包 - 用夜明锭在远古操纵机处合成
    /// </summary>
    public class MoonLordPack : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = ItemRarityID.Red;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            player.QuickSpawnItem(source, ItemID.LunarBar, 100);
            player.QuickSpawnItem(source, ItemID.FragmentSolar, 99);
            player.QuickSpawnItem(source, ItemID.FragmentVortex, 99);
            player.QuickSpawnItem(source, ItemID.FragmentNebula, 99);
            player.QuickSpawnItem(source, ItemID.FragmentStardust, 99);
            player.QuickSpawnItem(source, ItemID.RodofDiscord);
            // 送一个新手礼包物品（不是内容，玩家自己右键打开）
            player.QuickSpawnItem(source, ModContent.ItemType<StarterPack>());

            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedMoonLordPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.LunarBar)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}