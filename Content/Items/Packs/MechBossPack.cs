using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 机械礼包 - 用三种机械魂合成
    /// </summary>
    public class MechBossPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Pink;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.SoulofSight, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.SoulofMight, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.SoulofFright, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.HallowedBar, 1, 100, 100));
            itemLoot.Add(ItemDropRule.Common(ItemID.LifeFruit));

            // 联动模组增强（如有灾厄，追加灾厄材料）
            Common.ModIntegration.ModIntegrationSystem.ModifyAllExistingPacks("mechboss", itemLoot);
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedMechBossPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofFright)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}