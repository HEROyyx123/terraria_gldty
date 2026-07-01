using Terraria;
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

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            // 1 个蠕虫围巾
            player.QuickSpawnItem(source, ItemID.WormScarf);
            // 1 个混乱之脑
            player.QuickSpawnItem(source, ItemID.BrainOfConfusion);

            // 300 个狱石
            player.QuickSpawnItem(source, ItemID.Hellstone, 300);

            // 100 个黑曜石
            player.QuickSpawnItem(source, ItemID.Obsidian, 100);

            // 1 个熔岩护身符
            player.QuickSpawnItem(source, ItemID.LavaCharm);

            // 标记已获得
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedEvilBossPack = true;
        }

        public override void AddRecipes() {
            // 配方一：20个组织样本合成
            CreateRecipe()
                .AddIngredient(ItemID.TissueSample, 20)
                .AddTile(TileID.WorkBenches)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedEvilBoss)
                .Register();

            // 配方二：20个暗影鳞片合成
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddTile(TileID.WorkBenches)
                .AddCondition(Common.Systems.PackRecipeConditions.DownedEvilBoss)
                .Register();
        }
    }
}