using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 困难模式礼包 - 击败血肉墙后解锁
    /// </summary>
    public class HardmodePack : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;

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

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            player.QuickSpawnItem(source, ItemID.SoulofLight, 99);
            player.QuickSpawnItem(source, ItemID.SoulofNight, 99);
            player.QuickSpawnItem(source, ItemID.SoulofFlight, 99);
            player.QuickSpawnItem(source, ItemID.PlatinumCoin, 10);
            player.QuickSpawnItem(source, ItemID.WarriorEmblem);
            player.QuickSpawnItem(source, ItemID.RangerEmblem);
            player.QuickSpawnItem(source, ItemID.SorcererEmblem);
            player.QuickSpawnItem(source, ItemID.SummonerEmblem);
            player.QuickSpawnItem(source, ItemID.PixieDust, 99);
            player.QuickSpawnItem(source, ItemID.Ichor, 99);
            player.QuickSpawnItem(source, ItemID.CursedFlame, 99);
            player.QuickSpawnItem(source, ItemID.CrystalShard, 99);
            player.QuickSpawnItem(source, ItemID.UnicornHorn, 10);
            player.QuickSpawnItem(source, ItemID.FrostCore, 4);
            // 禁戒碎片
            player.QuickSpawnItem(source, ItemID.AncientBattleArmorMaterial, 4);
            player.QuickSpawnItem(source, ItemID.PirateMap, 10);
            // 明胶水晶
            player.QuickSpawnItem(source, ItemID.QueenSlimeCrystal, 10);

            // 标记已获得
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedHardmodePack = true;
        }

        public override void AddRecipes() {
            // 使用 RecipeGroup 让四种职业徽章任意一种都能合成
            CreateRecipe()
                .AddRecipeGroup("terraria_gldty:AnyEmblem", 1)
                .AddTile(TileID.MythrilAnvil)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedWallOfFlesh)
                .Register();
        }
    }
}