using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Items.Items145;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 145尝鲜礼包 - 用金钥匙外壳合成
    /// </summary>
    public class Pack145 : ModItem
    {
        public override void SetDefaults() {
            Item.width = 100;
            Item.height = 131;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Cyan;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        // public override void ModifyItemLoot(ItemLoot itemLoot) {
        //     itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StormDecree>(), 2, 0, 1));
        // }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 使用 SequentialRules（顺序判定，满足一个就停止后续判定）
            itemLoot.Add(ItemDropRule.SequentialRules(
                1, // 整体规则链的概率分母（1 代表 100% 触发这个判定链）

                // 1. 先判定 50% 概率（1/2）掉落物品 A
                ItemDropRule.Common(ItemID.IronBar, 2, 1, 1),

                // 2. 如果物品 A 没中，再判定 20% 概率掉落物品 B
                // 注意：因为第一步有 50% 失败率，要在剩余的 50% 里占 20% 的绝对概率，
                // 相对概率为 20% / 50% = 40% (即 1/2.5，也可以用 2/5)
                ItemDropRule.Common(ModContent.ItemType<StormDecree>(), 5, 1, 1),
                ItemDropRule.Common(ModContent.ItemType<SerpentBrace>(), 5, 1, 1),
                ItemDropRule.Common(ModContent.ItemType<SilverBracer>(), 5, 1, 1),
                ItemDropRule.Common(ModContent.ItemType<SilverShield>(), 5, 1, 1)
            ));
        }

        public override void RightClick(Player player) {
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.GoldenKey)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}