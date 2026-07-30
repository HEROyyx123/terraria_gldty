using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 甲虫礼包 - 用甲虫外壳合成
    /// </summary>
    public class BeetlePack : ModItem
    {
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

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.BeetleHusk, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.TurtleShell, 1, 3, 3));
            itemLoot.Add(ItemDropRule.Common(ItemID.FrozenTurtleShell));
            itemLoot.Add(ItemDropRule.Common(ItemID.Picksaw));
            itemLoot.Add(ItemDropRule.Common(ItemID.TruffleWorm, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.EmpressButterfly, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.GoldenFishingRod));
            itemLoot.Add(ItemDropRule.Common(ItemID.GoldenBugNet));
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedBeetlePack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.BeetleHusk)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}