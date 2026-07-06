using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.ModIntegration.CalamityIntegration;

namespace terraria_gldty.Content.Items.Packs.Calamity
{
    public class AquaticScourgePack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            AddItem(itemLoot, "IronBoots", 1);
            AddItem(itemLoot, "AnechoicPlating", 1);
            AddItem(itemLoot, "DepthsCharm", 1);
            AddItem(itemLoot, "EffigyOfDecay", 1);
            itemLoot.Add(ItemDropRule.Common(ItemID.ArcticDivingGear));
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.ModIntegration.ModIntegrationPlayer>().SetFlag("Calamity_AquaticScourge");
        }

        public override void AddRecipes() {
            int ingr = CalamityItemHelper.GetItemType("AquaticEmblem");
            if (ingr > 0) {
                CreateRecipe()
                    .AddIngredient(ingr, 1)
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