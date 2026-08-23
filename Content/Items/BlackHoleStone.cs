using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class BlackHoleStone : ModItem
    {
        public override void SetStaticDefaults() {
            // 微光嬗变
            ItemID.Sets.ShimmerTransformToItem[ItemID.EncumberingStone] = Type;
            ItemID.Sets.ShimmerTransformToItem[ItemID.UncumberingStone] = Type;
        }

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes(){
            CreateRecipe()
            .AddIngredient(ItemID.EncumberingStone, 1) 
            .Register();                               
        }

        public override bool CanRightClick() => true;

        // 重写此方法，防止右键时物品被消耗（不再需要手动重新插入背包）
        public override bool ConsumeItem(Player player) => false;

        public override void RightClick(Player player) {
            if (Main.netMode == NetmodeID.Server)
                return;

            ModContent.GetInstance<Common.UI.BlackHoleStoneUI.BlackHoleStoneUISystem>().ShowUI();
        }
    }
}