using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using terraria_gldty.Common.ModIntegration;
using System.Linq;
using Terraria.ID;


namespace terraria_gldty.Content.Items.Packs.MagicStorage
{
    public class MagicStoragePack : ModItem
    {
        //public override string Texture => "Terraria/Images/Item_" + Terraria.ID.ItemID.EyeOfCthulhuBossBag;
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("MagicStorage", out Mod magicStorage))
            {
                CreateRecipe()
                    .AddIngredient(ItemID.SkywareChest, 1)
                    .AddTile(TileID.WorkBenches)
                    .Register();
            }
        }

        public override void ModifyItemLoot(ItemLoot itemLoot) {
            // 从 ModIntegrationSystem 中找到已加载的 MagicStorageIntegration
            var integration = ModIntegrationSystem.ActiveIntegrations.FirstOrDefault(i => i.TargetModName == "MagicStorage") as MagicStorageIntegration;

            if (integration == null) return;

            // 1个 存储核心
            if (integration.StorageHeartId > 0)
                itemLoot.Add(ItemDropRule.Common(integration.StorageHeartId, 1, 1, 1));

            // 1个 制作接口
            if (integration.CraftingAccessId > 0)
                itemLoot.Add(ItemDropRule.Common(integration.CraftingAccessId, 1, 1, 1));

            // 8个 存储单元
            if (integration.StorageUnitId > 0)
                itemLoot.Add(ItemDropRule.Common(integration.StorageUnitId, 1, 16, 16));

            // 16个 存储组件
            if (integration.StorageComponentId > 0)
                itemLoot.Add(ItemDropRule.Common(integration.StorageComponentId, 1, 8, 8));
                
            // 1个 环境访问器
            if (integration.EnvironmentAccessId > 0)
                itemLoot.Add(ItemDropRule.Common(integration.EnvironmentAccessId, 1, 1, 1));
        }
    }
}