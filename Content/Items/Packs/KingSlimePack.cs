using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 史莱姆王礼包 - 击败史莱姆王后合成
    /// </summary>
    public class KingSlimePack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.GoblinBattleStandard, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.FallenStar, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.JungleSpores, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Vine, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Stinger, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Cobweb, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.FlinxFur, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Silk, 1, 99, 99));
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedKingSlimePack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Gel, 10)
                .AddTile(TileID.WorkBenches)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedSlimeKing)
                .Register();
        }
    }
}