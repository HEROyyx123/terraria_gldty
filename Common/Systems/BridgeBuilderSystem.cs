using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using terraria_gldty.Content.Items;
using terraria_gldty.Common.UI.BuilderUI;

namespace terraria_gldty.Common.Systems
{
    public class BridgeBuilderSystem : ModSystem
    {
        public static BridgeBuilderSystem Instance => ModContent.GetInstance<BridgeBuilderSystem>();

        public UserInterface BridgeUIInterface;
        public BridgeBuilderUI BridgeUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                BridgeUIInterface = new UserInterface();
                BridgeUI = new BridgeBuilderUI();
                BridgeUI.Activate();
            }
        }

        public void ToggleUI()
        {
            if (BridgeUIInterface.CurrentState == null)
                BridgeUIInterface.SetState(BridgeUI);
            else
                BridgeUIInterface.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            BridgeUIInterface?.Update(gameTime);
        }

        public override void PostDrawTiles()
        {
            Player player = Main.LocalPlayer;
            if (player.HeldItem != null && player.HeldItem.type == ModContent.ItemType<BridgeBuilderItem>() && BridgeBuilderSettings.ShowPreview)
            {
                int startX = (int)(Main.MouseWorld.X / 16f);
                int startY = (int)(Main.MouseWorld.Y / 16f);

                int length = BridgeBuilderSettings.Length;
                BuildDirection dir = BridgeBuilderSettings.Direction;

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                Color lineTileColor = new Color(0, 255, 120, 150); // 平台/方块：半透明亮绿框

                if (dir == BuildDirection.Both)
                {
                    DrawPreviewPath(startX, startY, 1, length / 2, lineTileColor);
                    DrawPreviewPath(startX, startY, -1, length / 2, lineTileColor);
                }
                else
                {
                    DrawPreviewPath(startX, startY, (int)dir, length, lineTileColor);
                }

                Main.spriteBatch.End();
            }
        }

        private void DrawPreviewPath(int startX, int startY, int dir, int length, Color tileColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            int lightTile = BridgeBuilderSettings.LightItem.IsAir ? -1 : BridgeBuilderSettings.LightItem.createTile;
            bool isLightActive = lightTile >= 0 && BridgeBuilderSettings.LightSpacing > 0;
            bool isTorch = BridgeBuilderSettings.IsTorch(lightTile);

            Color torchPreviewColor = new Color(255, 200, 40, 180);
            Color lanternPreviewColor = new Color(80, 200, 255, 180);
            Color destroyPreviewColor = new Color(255, 60, 60, 120); // 摧毁区域：半透明红框

            for (int i = 0; i < length; i++)
            {
                int x = startX + (i * dir);
                int y = startY;

                if (x < 10 || x >= Main.maxTilesX - 10) break;

                // 1. 绘制摧毁范围预览
                for (int dy = -BridgeBuilderSettings.ClearUp; dy <= BridgeBuilderSettings.ClearDown; dy++)
                {
                    if (dy == 0) continue;
                    int targetY = y + dy;
                    Vector2 destroyPos = new Vector2(x * 16, targetY * 16) - Main.screenPosition;
                    Main.spriteBatch.Draw(pixel, new Rectangle((int)destroyPos.X, (int)destroyPos.Y, 16, 16), destroyPreviewColor);
                }

                // 2. 绘制平台预览框 (16x16 像素)
                Vector2 worldPos = new Vector2(x * 16, y * 16) - Main.screenPosition;
                Main.spriteBatch.Draw(pixel, new Rectangle((int)worldPos.X, (int)worldPos.Y, 16, 16), tileColor);

                // 3. 绘制光源预览框
                if (isLightActive && i % BridgeBuilderSettings.LightSpacing == 0 && i > 0)
                {
                    if (isTorch)
                    {
                        Vector2 lightPos = new Vector2(x * 16, (y - 1) * 16) - Main.screenPosition;
                        Main.spriteBatch.Draw(pixel, new Rectangle((int)lightPos.X, (int)lightPos.Y, 16, 16), torchPreviewColor);
                    }
                    else
                    {
                        Vector2 lightPos = new Vector2(x * 16, (y + 1) * 16) - Main.screenPosition;
                        Main.spriteBatch.Draw(pixel, new Rectangle((int)lightPos.X, (int)lightPos.Y, 16, 32), lanternPreviewColor);
                    }
                }
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "YourMod: Bridge Builder UI",
                    delegate
                    {
                        if (BridgeUIInterface?.CurrentState != null)
                        {
                            BridgeUIInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public bool IsUIOpen()
        {
            return BridgeUIInterface?.CurrentState != null;
        }
    }
}