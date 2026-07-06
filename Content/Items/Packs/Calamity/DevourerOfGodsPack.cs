using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.ModIntegration.CalamityIntegration;

namespace terraria_gldty.Content.Items.Packs.Calamity
{
    public class DevourerOfGodsPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.Cyan;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            AddItem(itemLoot, "CosmiliteBar", 100);
            AddItem(itemLoot, "EndothermicEnergy", 100);
            AddItem(itemLoot, "NightmareFuel", 100);
            AddItem(itemLoot, "DarksunFragment", 100);
            AddItem(itemLoot, "AscendantSpiritEssence", 50);
            AddItem(itemLoot, "CosmicAnvil", 1);
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.ModIntegration.ModIntegrationPlayer>().SetFlag("Calamity_DevourerOfGods");
        }

        public override void AddRecipes() {
            int ingr = CalamityItemHelper.GetItemType("CosmiliteBar");
            if (ingr > 0) {
                CreateRecipe()
                    .AddIngredient(ingr, 1)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
            }
        }

        private static void AddItem(ItemLoot itemLoot, string name, int stack) {
            int type = CalamityItemHelper.GetItemType(name);
            if (type > 0) itemLoot.Add(ItemDropRule.Common(type, 1, stack, stack));
        }
    }
}