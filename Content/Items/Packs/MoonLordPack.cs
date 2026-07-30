using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 月总礼包 - 用夜明锭在远古操纵机处合成
    /// </summary>
    public class MoonLordPack : ModItem
    {
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

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.LunarBar, 1, 100, 100));
            itemLoot.Add(ItemDropRule.Common(ItemID.FragmentSolar, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.FragmentVortex, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.FragmentNebula, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.FragmentStardust, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.RodofDiscord));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarterPack>()));

            // 联动模组增强（如有灾厄，追加灾厄材料）
            Common.ModIntegration.ModIntegrationSystem.ModifyAllExistingPacks("moonlord", itemLoot);
        }

        public override void RightClick(Player player) {
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