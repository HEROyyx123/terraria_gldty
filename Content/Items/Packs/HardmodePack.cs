using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 困难模式礼包 - 击败血肉墙后解锁
    /// </summary>
    public class HardmodePack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ItemID.SoulofLight, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.SoulofNight, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.SoulofFlight, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.PlatinumCoin, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.WarriorEmblem));
            itemLoot.Add(ItemDropRule.Common(ItemID.RangerEmblem));
            itemLoot.Add(ItemDropRule.Common(ItemID.SorcererEmblem));
            itemLoot.Add(ItemDropRule.Common(ItemID.SummonerEmblem));
            itemLoot.Add(ItemDropRule.Common(ItemID.PixieDust, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.Ichor, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.CursedFlame, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.CrystalShard, 1, 99, 99));
            itemLoot.Add(ItemDropRule.Common(ItemID.UnicornHorn, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.FrostCore, 1, 4, 4));
            itemLoot.Add(ItemDropRule.Common(ItemID.AncientBattleArmorMaterial, 1, 4, 4));
            itemLoot.Add(ItemDropRule.Common(ItemID.PirateMap, 1, 10, 10));
            itemLoot.Add(ItemDropRule.Common(ItemID.QueenSlimeCrystal, 1, 10, 10));

            // 联动模组增强（如有灾厄，追加灾厄材料）
            Common.ModIntegration.ModIntegrationSystem.ModifyAllExistingPacks("hardmode", itemLoot);
        }

        public override void RightClick(Player player) {
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedHardmodePack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddRecipeGroup("terraria_gldty:AnyEmblem")
                .AddTile(TileID.MythrilAnvil)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedWallOfFlesh)
                .Register();
        }
    }
}