using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 克苏鲁之眼礼包 - 击败克眼后合成，打开获得前期实用物资
    /// </summary>
    public class EyeOfCthulhuPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.CrimtaneBar, 1, 50, 50));
            itemLoot.Add(ItemDropRule.Common(ItemID.DemoniteBar, 1, 50, 50));
            itemLoot.Add(ItemDropRule.Common(ItemID.CrimsonSeeds, 1, 100, 100));
            itemLoot.Add(ItemDropRule.Common(ItemID.CorruptSeeds, 1, 100, 100));
            itemLoot.Add(ItemDropRule.Common(ItemID.IronskinPotion, 1, 30, 30));
            itemLoot.Add(ItemDropRule.Common(ItemID.LifeCrystal, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.ManaCrystal, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Obsidian, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.PlatinumCoin, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.Abeemination, 1, 20, 20));
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedEyeOfCthulhuPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Lens, 10)
                .AddTile(TileID.WorkBenches)
                .AddCondition(Condition.DownedEyeOfCthulhu)
                .Register();
        }
    }
}