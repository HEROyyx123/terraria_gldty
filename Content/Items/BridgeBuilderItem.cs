using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Systems;

namespace terraria_gldty.Content.Items
{
    public class BridgeBuilderItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = false;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override void AddRecipes(){
            CreateRecipe()
            .AddIngredient(ItemID.Amber, 15)
            .AddIngredient(ItemID.JungleSpores, 10)
            .AddIngredient(ItemID.Wood, 10)
            .AddTile(TileID.Anvils)
            .Register();                               
        }
        

        public override bool CanUseItem(Player player)
        {
            // 修复1：如果 UI 面板打开，或者鼠标正位于 UI 界面上，禁止左键触发摆放
            if (BridgeBuilderSystem.Instance.IsUIOpen() || player.mouseInterface)
            {
                // 如果是右键点击，依然允许切换 UI 面板
                if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
                {
                    BridgeBuilderSystem.Instance.ToggleUI();
                }
                return false;
            }

            if (player.altFunctionUse == 2)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    BridgeBuilderSystem.Instance.ToggleUI();
                }
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                int startTileX = (int)(Main.MouseWorld.X / 16f);
                int startTileY = (int)(Main.MouseWorld.Y / 16f);

                BridgeBuilderSettings.ExecuteBuild(startTileX, startTileY);
            }
            return true;
        }
    }
}