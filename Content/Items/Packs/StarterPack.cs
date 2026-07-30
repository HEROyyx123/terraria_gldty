using Terraria;
using Terraria.GameContent.ItemDropRules;
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

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<GuideBook>()));

            itemLoot.Add(ItemDropRule.Common(ItemID.PlatinumBar, 1, 100, 100));

            itemLoot.Add(ItemDropRule.Common(ItemID.Diamond, 1, 20, 20));
            itemLoot.Add(ItemDropRule.Common(ItemID.Ruby, 1, 20, 20));
            itemLoot.Add(ItemDropRule.Common(ItemID.Emerald, 1, 20, 20));
            itemLoot.Add(ItemDropRule.Common(ItemID.Sapphire, 1, 20, 20));
            itemLoot.Add(ItemDropRule.Common(ItemID.Topaz, 1, 20, 20));
            itemLoot.Add(ItemDropRule.Common(ItemID.Amethyst, 1, 20, 20));
            itemLoot.Add(ItemDropRule.Common(ItemID.Amber, 1, 20, 20));

            itemLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.PlatinumCoin));
            itemLoot.Add(ItemDropRule.Common(ItemID.PiggyBank));

            itemLoot.Add(ItemDropRule.Common(ItemID.Wood, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.DirtBlock, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.StoneBlock, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.SnowBlock, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.MudBlock, 1, 999, 999));
            itemLoot.Add(ItemDropRule.Common(ItemID.SandBlock, 1, 999, 999));

            itemLoot.Add(ItemDropRule.Common(ItemID.Torch, 1, 999, 999));

            itemLoot.Add(ItemDropRule.Common(ItemID.LuckyHorseshoe));
            itemLoot.Add(ItemDropRule.Common(ItemID.CloudinaBottle));
            itemLoot.Add(ItemDropRule.Common(ItemID.GrapplingHook));
        }

        public override void RightClick(Player player) {
            // 仅做标记，物品由 ModifyItemLoot 自动生成
            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedStarterPack = true;
        }
    }
}