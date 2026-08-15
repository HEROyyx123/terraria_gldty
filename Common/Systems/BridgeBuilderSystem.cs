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
            if (player.HeldItem != null && !player.HeldItem.IsAir && player.HeldItem.type == ModContent.ItemType<BridgeBuilderItem>() && BridgeBuilderSettings.ShowPreview)
            {
                int startX = Player.tileTargetX;
                int startY = Player.tileTargetY;

                int length = BridgeBuilderSettings.Length;
                BuildDirection dir = BridgeBuilderSettings.Direction;

                // 使用 ZoomMatrix (仅处理画面缩放，坐标由 screenPosition 管理)
                Main.spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullCounterClockwise,
                    null,
                    Main.GameViewMatrix.ZoomMatrix
                );

                Color lineTileColor = new Color(0, 255, 120) * 0.6f;

                if (dir == BuildDirection.Both)
                {
                    DrawPreviewPath(startX, startY, 1, length / 2, lineTileColor);
                    DrawPreviewPath(startX, startY, -1, length / 2, lineTileColor);
                }
                else
                {
                    int dirVal = dir == BuildDirection.Left ? -1 : 1;
                    DrawPreviewPath(startX, startY, dirVal, length, lineTileColor);
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

            Color torchPreviewColor = new Color(255, 200, 40) * 0.7f;
            Color lanternPreviewColor = new Color(80, 200, 255) * 0.7f;
            Color destroyPreviewColor = new Color(255, 60, 60) * 0.4f;

            for (int i = 0; i < length; i++)
            {
                int x = startX + (i * dir);
                int y = startY;

                if (x < 10 || x >= Main.maxTilesX - 10) break;

                // 计算屏幕像素坐标
                int screenX = (int)(x * 16 - Main.screenPosition.X);

                // 1. 绘制摧毁范围预览
                for (int dy = -BridgeBuilderSettings.ClearUp; dy <= BridgeBuilderSettings.ClearDown; dy++)
                {
                    if (dy == 0) continue;
                    int targetY = y + dy;
                    int screenDestroyY = (int)(targetY * 16 - Main.screenPosition.Y);

                    Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenDestroyY, 16, 16), destroyPreviewColor);
                }

                // 2. 绘制平台预览框 (Rectangle 强制将图片剪裁限制为 16x16)
                int screenY = (int)(y * 16 - Main.screenPosition.Y);
                Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenY, 16, 16), tileColor);

                // 3. 绘制光源预览框
                if (isLightActive && i % BridgeBuilderSettings.LightSpacing == 0 && i > 0)
                {
                    if (isTorch)
                    {
                        int screenTorchY = (int)((y - 1) * 16 - Main.screenPosition.Y);
                        Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenTorchY, 16, 16), torchPreviewColor);
                    }
                    else
                    {
                        int screenLanternY = (int)((y + 1) * 16 - Main.screenPosition.Y);
                        Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenLanternY, 16, 32), lanternPreviewColor);
                    }
                }
            }
        }
        // 添加坐标转换辅助方法
        private Vector2 WorldToScreen(Vector2 worldPos)
        {
            // 使用Terraria的矩阵转换
            return Vector2.Transform(worldPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
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