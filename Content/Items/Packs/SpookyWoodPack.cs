using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Packs
{
    /// <summary>
    /// 阴森木礼包 - 用阴森木合成，随机给一件阴森套装备和一件阴森木家具
    /// </summary>
    public class SpookyWoodPack : ModItem
    {
        // // TODO: 替换为自定义占位 PNG 后删除此行
        // public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;

        /// <summary> 所有阴森木家具的 ID 列表 </summary>
        private static readonly List<int> SpookyFurniture = new()
        {
            ItemID.SpookyDoor,
            ItemID.SpookyChair,
            ItemID.SpookyTable,
            ItemID.SpookyBed,
            ItemID.SpookyDresser,
            ItemID.SpookySofa,
            ItemID.SpookyBathtub,
            ItemID.SpookyClock,
            ItemID.SpookyCandelabra,
            ItemID.SpookyCandle,
            ItemID.SpookyChandelier,
            ItemID.SpookyLamp,
            ItemID.SpookyLantern,
            ItemID.SpookyWorkBench,
            ItemID.SpookyPiano,
            ItemID.SpookyBookcase,
            ItemID.ToiletSpooky,
            ItemID.SpookySink,
            ItemID.SpookyPlatform,
        };

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            var source = player.GetSource_OpenItem(Type);

            // 随机给一件阴森套装
            int armorChoice = Main.rand.Next(3);
            if (armorChoice == 0) player.QuickSpawnItem(source, ItemID.SpookyHelmet);
            else if (armorChoice == 1) player.QuickSpawnItem(source, ItemID.SpookyBreastplate);
            else player.QuickSpawnItem(source, ItemID.SpookyLeggings);

            // 随机给一件阴森木家具
            int furnitureItem = SpookyFurniture[Main.rand.Next(SpookyFurniture.Count)];
            player.QuickSpawnItem(source, furnitureItem);

            player.GetModPlayer<Common.Players.PackPlayer>().ReceivedSpookyWoodPack = true;
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.SpookyWood, 250)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}