using Terraria;
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

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            // 999 个凝胶
            player.QuickSpawnItem(source, ItemID.Gel, 999);

            // 10 个哥布林战旗
            player.QuickSpawnItem(source, ItemID.GoblinBattleStandard, 10);

            // 999 个坠落之星
            player.QuickSpawnItem(source, ItemID.FallenStar, 999);

            // 99 个丛林孢子
            player.QuickSpawnItem(source, ItemID.JungleSpores, 99);

            // 99 个藤蔓
            player.QuickSpawnItem(source, ItemID.Vine, 99);

            // 99 个毒刺
            player.QuickSpawnItem(source, ItemID.Stinger, 99);

            // 999 个蛛网
            player.QuickSpawnItem(source, ItemID.Cobweb, 999);

            // 99 个小雪怪皮毛
            player.QuickSpawnItem(source, ItemID.FlinxFur, 99);

            // 99 个丝绸
            player.QuickSpawnItem(source, ItemID.Silk, 99);

            // 标记已获得
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