using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.ModIntegration.CalamityIntegration;

namespace terraria_gldty.Content.Items.Packs.Calamity
{
    public class SlimeGodPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 3);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            AddItem(itemLoot, "PurifiedGel", 100);
            AddItem(itemLoot, "BlightedGel", 100);
            AddItem(itemLoot, "RadiantOoze", 1);
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.ModIntegration.ModIntegrationPlayer>().SetFlag("Calamity_SlimeGod");
        }

        public override void AddRecipes() {
            int ingr = CalamityItemHelper.GetItemType("PurifiedGel");
            if (ingr > 0) {
                CreateRecipe()
                    .AddIngredient(ingr, 10)
                    .AddTile(TileID.WorkBenches)
                    .Register();
            }
        }

        private static void AddItem(ItemLoot itemLoot, string name, int stack) {
            int type = CalamityItemHelper.GetItemType(name);
            if (type > 0) itemLoot.Add(ItemDropRule.Common(type, 1, stack, stack));
        }
    }
}