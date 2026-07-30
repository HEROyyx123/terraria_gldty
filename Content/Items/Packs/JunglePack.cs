using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 丛林礼包 - 击败世纪之花后解锁
    /// </summary>
    public class JunglePack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.TempleKey));
            itemLoot.Add(ItemDropRule.Common(ItemID.SolarTablet, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.LihzahrdPowerCell, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.LifeFruit, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.BrokenHeroSword));
            itemLoot.Add(ItemDropRule.Common(ItemID.Ectoplasm, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Autohammer));
            itemLoot.Add(ItemDropRule.Common(ItemID.ChlorophyteBar, 1, 100, 100));
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedJunglePack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.TempleKey)
                .AddTile(TileID.MythrilAnvil)
                .AddCondition(Condition.DownedPlantera)
                .Register();
        }
    }
}