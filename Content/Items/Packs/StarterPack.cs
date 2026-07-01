using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 新手大礼包 - 开局自动获得，打开获得基础物资
    /// </summary>
    public class StarterPack : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            // 计划书
            player.QuickSpawnItem(source, ModContent.ItemType<GuideBook>());

            // 100 个铂金锭
            player.QuickSpawnItem(source, ItemID.PlatinumBar, 100);

            // 20 个各种宝石
            player.QuickSpawnItem(source, ItemID.Diamond, 20);
            player.QuickSpawnItem(source, ItemID.Ruby, 20);
            player.QuickSpawnItem(source, ItemID.Emerald, 20);
            player.QuickSpawnItem(source, ItemID.Sapphire, 20);
            player.QuickSpawnItem(source, ItemID.Topaz, 20);
            player.QuickSpawnItem(source, ItemID.Amethyst, 20);
            player.QuickSpawnItem(source, ItemID.Amber, 20);

            // 999 个凝胶
            player.QuickSpawnItem(source, ItemID.Gel, 999);

            // 一个铂金币
            player.QuickSpawnItem(source, ItemID.PlatinumCoin);

            // 一个存钱罐
            player.QuickSpawnItem(source, ItemID.PiggyBank);

            // 方块资源各 999
            player.QuickSpawnItem(source, ItemID.Wood, 999);
            player.QuickSpawnItem(source, ItemID.DirtBlock, 999);
            player.QuickSpawnItem(source, ItemID.StoneBlock, 999);
            player.QuickSpawnItem(source, ItemID.SnowBlock, 999);
            player.QuickSpawnItem(source, ItemID.MudBlock, 999);
            player.QuickSpawnItem(source, ItemID.SandBlock, 999);

            // 999 个火把
            player.QuickSpawnItem(source, ItemID.Torch, 999);

            // 饰品
            player.QuickSpawnItem(source, ItemID.LuckyHorseshoe);     // 马蹄铁
            player.QuickSpawnItem(source, ItemID.CloudinaBottle);       // 云朵瓶
            player.QuickSpawnItem(source, ItemID.GrapplingHook);        // 抓钩
        }
    }
}