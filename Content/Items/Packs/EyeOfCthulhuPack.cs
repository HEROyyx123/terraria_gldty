using Terraria;
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

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            // 100 个猩红锭
            player.QuickSpawnItem(source, ItemID.CrimtaneBar, 100);

            // 100 个猩红草种
            player.QuickSpawnItem(source, ItemID.CrimsonSeeds, 100);

            // 30 瓶铁皮药水
            player.QuickSpawnItem(source, ItemID.IronskinPotion, 30);

            // 99 个生命水晶
            player.QuickSpawnItem(source, ItemID.LifeCrystal, 99);

            // 99 个魔力水晶
            player.QuickSpawnItem(source, ItemID.ManaCrystal, 99);

            // 99 个黑曜石
            player.QuickSpawnItem(source, ItemID.Obsidian, 99);

            // 10 个铂金币
            player.QuickSpawnItem(source, ItemID.PlatinumCoin, 10);

            // 20 个憎恶之蜂（蜂后召唤物）
            player.QuickSpawnItem(source, ItemID.Abeemination, 20);

            // 标记已获得
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