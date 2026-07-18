using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class BlackHoleStone : ModItem
    {
        public override void SetStaticDefaults() {
            // 微光转换：负重石/不加重石 -> 黑洞石
            ItemID.Sets.ShimmerTransformToItem[ItemID.EncumberingStone] = Type;
            // ItemID.Sets.ShimmerTransformToItem[ItemID.UnencumberingStone] = Type;
        }

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            if (Main.netMode == NetmodeID.Server)
                return;

            ModContent.GetInstance<Common.UI.BlackHoleStoneUI.BlackHoleStoneUISystem>().ShowUI();

            // 右键不会消耗物品，放回物品栏
            Item clone = Item.Clone();
            for (int i = 0; i < 50; i++) {
                if (player.inventory[i].IsAir) {
                    player.inventory[i] = clone;
                    return;
                }
            }
            player.QuickSpawnItem(player.GetSource_DropAsItem(), clone);
        }
    }
}
