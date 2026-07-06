using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.ModIntegration.CalamityIntegration;

namespace terraria_gldty.Content.Items.Packs.Calamity
{
    public class ExoMechsPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 50);
            Item.rare = ItemRarityID.Red;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            AddItem(itemLoot, "DraedonsForge", 1);
            AddItem(itemLoot, "ExoPrism", 100);
            AddItem(itemLoot, "MiracleMatter", 10);
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.ModIntegration.ModIntegrationPlayer>().SetFlag("Calamity_ExoMechs");
        }

        public override void AddRecipes() {
            int ingr = CalamityItemHelper.GetItemType("ExoPrism");
            int tile = CalamityItemHelper.GetTileType("CosmicAnvil");
            if (ingr > 0 && tile > 0) {
                CreateRecipe()
                    .AddIngredient(ingr, 1)
                    .AddTile(tile)
                    .Register();
            }
        }

        private static void AddItem(ItemLoot itemLoot, string name, int stack) {
            int type = CalamityItemHelper.GetItemType(name);
            if (type > 0) itemLoot.Add(ItemDropRule.Common(type, 1, stack, stack));
        }
    }
}