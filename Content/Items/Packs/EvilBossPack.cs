using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 邪恶 Boss 礼包 - 击败世界吞噬者/克苏鲁之脑后合成
    /// </summary>
    public class EvilBossPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.WormScarf));
            itemLoot.Add(ItemDropRule.Common(ItemID.BrainOfConfusion));
            itemLoot.Add(ItemDropRule.Common(ItemID.Hellstone, 1, 300, 300));
            itemLoot.Add(ItemDropRule.Common(ItemID.Obsidian, 1, 100, 100));
            itemLoot.Add(ItemDropRule.Common(ItemID.LavaCharm));
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedEvilBossPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.TissueSample, 20)
                .AddTile(TileID.WorkBenches)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedEvilBoss)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddTile(TileID.WorkBenches)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedEvilBoss)
                .Register();
        }
    }
}