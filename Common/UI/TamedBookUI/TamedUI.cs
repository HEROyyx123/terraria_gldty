using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using terraria_gldty.Common.Systems;

namespace terraria_gldty.Common.UI
{
    public class TamedUI : UIState
    {
        private UIPanel mainPanel;
        private UIList npcList;
        private UIScrollbar scrollbar;
        private UIText titleText;
        private UITextPanel<string> deleteModeButton;
        private UITextPanel<string> closeButton; // 【修复】：声明为类成员字段

        // 拖拽相关
        private Vector2 offset;
        private bool dragging;

        // 标识当前是否开启删除模式
        public bool IsDeleteMode { get; private set; } = false;

        public override void OnInitialize() {
            // 主面板 setup
            mainPanel = new UIPanel();
            mainPanel.SetPadding(12);
            mainPanel.Width.Set(500f, 0f);
            mainPanel.Height.Set(400f, 0f);
            mainPanel.HAlign = 0.5f;
            mainPanel.VAlign = 0.5f;
            
            // 边缘样式与深色深蓝背景
            mainPanel.BackgroundColor = new Color(20, 26, 48) * 0.95f;
            mainPanel.BorderColor = new Color(80, 110, 180);

            // 全局监听面板按下事件以启动拖拽
            mainPanel.OnLeftMouseDown += DragStart;

            // 标题
            titleText = new UIText("灵魂手册 - 已驯服小怪", 0.9f, true);
            titleText.Top.Set(6f, 0f);
            titleText.Left.Set(10f, 0f);
            titleText.TextColor = Color.Gold;
            mainPanel.Append(titleText);

            // 关闭按钮 (X)
            closeButton = new UITextPanel<string>("X"); // 【修复】：去掉前缀类型声明，改用已声明的字段
            closeButton.SetPadding(0);
            closeButton.Width.Set(28f, 0f);
            closeButton.Height.Set(28f, 0f);
            closeButton.Left.Set(-32f, 1f);
            closeButton.Top.Set(6f, 0f);
            closeButton.BackgroundColor = new Color(180, 40, 40) * 0.8f;
            closeButton.BorderColor = Color.Red;
            
            closeButton.OnMouseOver += (evt, listeningElement) => {
                closeButton.BackgroundColor = Color.Red;
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            closeButton.OnMouseOut += (evt, listeningElement) => closeButton.BackgroundColor = new Color(180, 40, 40) * 0.8f;
            closeButton.OnLeftClick += (evt, listeningElement) => {
                SoundEngine.PlaySound(SoundID.MenuClose);
                TamedSystem.ToggleUI();
            };
            mainPanel.Append(closeButton);

            // 删除模式切换按钮
            deleteModeButton = new UITextPanel<string>("删除模式");
            deleteModeButton.SetPadding(4);
            deleteModeButton.Width.Set(95f, 0f);
            deleteModeButton.Height.Set(28f, 0f);
            deleteModeButton.Left.Set(-135f, 1f);
            deleteModeButton.Top.Set(6f, 0f);
            deleteModeButton.BackgroundColor = new Color(60, 65, 85) * 0.8f;
            deleteModeButton.BorderColor = new Color(100, 105, 125);

            deleteModeButton.OnMouseOver += (evt, listeningElement) => SoundEngine.PlaySound(SoundID.MenuTick);
            deleteModeButton.OnLeftClick += (evt, listeningElement) => {
                IsDeleteMode = !IsDeleteMode;
                SoundEngine.PlaySound(IsDeleteMode ? SoundID.Item14 : SoundID.MenuTick);
                
                if (IsDeleteMode) {
                    deleteModeButton.SetText("[c/FF4444:删除模式中]");
                    deleteModeButton.BackgroundColor = new Color(140, 30, 30) * 0.9f;
                    deleteModeButton.BorderColor = Color.Red;
                } else {
                    deleteModeButton.SetText("删除模式");
                    deleteModeButton.BackgroundColor = new Color(60, 65, 85) * 0.8f;
                    deleteModeButton.BorderColor = new Color(100, 105, 125);
                }
                
                RefreshGrid();
            };
            mainPanel.Append(deleteModeButton);

            // 列表内容容器面板
            UIPanel gridContainer = new UIPanel();
            gridContainer.Top.Set(42f, 0f);
            gridContainer.Width.Set(0f, 1f);
            gridContainer.Height.Set(-48f, 1f);
            gridContainer.BackgroundColor = new Color(12, 16, 30) * 0.8f;
            gridContainer.BorderColor = new Color(50, 65, 100);
            gridContainer.SetPadding(6);
            mainPanel.Append(gridContainer);

            // 滚动列表 UIList
            npcList = new UIList();
            npcList.Width.Set(-22f, 1f);
            npcList.Height.Set(0f, 1f);
            npcList.ListPadding = 6f;
            gridContainer.Append(npcList);

            // 滚动条 UIScrollbar
            scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(0f, 1f);
            scrollbar.Left.Set(-16f, 1f);
            scrollbar.Width.Set(14f, 0f);
            gridContainer.Append(scrollbar);

            npcList.SetScrollbar(scrollbar);

            Append(mainPanel);
        }

        private void DragStart(UIMouseEvent evt, UIElement listeningElement) {
            // 点击位置在顶部标题栏区域或主面板空白处均可触发拖拽，并排除交互按钮
            if (evt.Target == mainPanel || evt.Target == titleText || evt.MousePosition.Y - mainPanel.GetDimensions().Y <= 40f) {
                if (evt.Target == deleteModeButton || evt.Target == closeButton) return;

                CalculatedStyle dimensions = mainPanel.GetDimensions();
                mainPanel.Left.Set(dimensions.X, 0f);
                mainPanel.Top.Set(dimensions.Y, 0f);
                mainPanel.HAlign = 0f;
                mainPanel.VAlign = 0f;

                offset = evt.MousePosition - new Vector2(dimensions.X, dimensions.Y);
                dragging = true;
            }
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (!Main.mouseLeft) {
                dragging = false;
            }

            if (dragging) {
                float targetX = Main.mouseX - offset.X;
                float targetY = Main.mouseY - offset.Y;

                float minX = 10f;
                float maxX = Main.screenWidth - mainPanel.Width.Pixels - 10f;
                float minY = 10f;
                float maxY = Main.screenHeight - mainPanel.Height.Pixels - 10f;

                mainPanel.Left.Set(MathHelper.Clamp(targetX, minX, maxX), 0f);
                mainPanel.Top.Set(MathHelper.Clamp(targetY, minY, maxY), 0f);

                mainPanel.Recalculate();
            }

            if (mainPanel.IsMouseHovering) {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public void RefreshGrid() {
            if (npcList == null) return;
            
            npcList.Clear();

            var modPlayer = Main.LocalPlayer.GetModPlayer<Players.TamedPlayer>();
            var unlockedList = modPlayer.UnlockedNPCTypes;

            if (unlockedList.Count == 0) {
                UIText hint = new UIText("尚未捕获任何怪物...\n(使用灵魂手册右键敌怪)", 0.85f);
                hint.HAlign = 0.5f;
                hint.Top.Set(40f, 0f);
                hint.TextColor = Color.LightGray;
                npcList.Add(hint);
                titleText.SetText($"灵魂手册 (0/???)");
                return;
            }

            titleText.SetText($"灵魂手册 ({unlockedList.Count}/{NPCLoader.NPCCount})");

            UIElement rowPanel = null;
            int itemsInRow = 0;
            int maxItemsPerRow = 8;

            foreach (int npcType in unlockedList) {
                if (itemsInRow == 0) {
                    rowPanel = new UIElement();
                    rowPanel.Width.Set(0f, 1f);
                    rowPanel.Height.Set(52f, 0f);
                    npcList.Add(rowPanel);
                }

                UINpcSlot slot = new UINpcSlot(npcType, 0.9f);
                slot.Left.Set(itemsInRow * 54f, 0f);

                int typeToHandle = npcType;
                slot.OnLeftClick += (evt, listeningElement) =>
                {
                    if (IsDeleteMode)
                    {
                        if (modPlayer.RemoveUnlockedNPC(typeToHandle))
                        {
                            SoundEngine.PlaySound(SoundID.Item14);

                            string npcName = ContentSamples.NpcsByNetId.TryGetValue(typeToHandle, out NPC sample) ? sample.GivenOrTypeName : "未知怪物";
                            Main.NewText($"已从手册中删除: {npcName}", Color.Orange);

                            RefreshGrid();
                        }
                    }
                    else
                    {
                        if (modPlayer.TrySummonNPC(typeToHandle))
                        {
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.MenuClose);
                        }
                    }
                };

                rowPanel.Append(slot);
                itemsInRow++;

                if (itemsInRow >= maxItemsPerRow) {
                    itemsInRow = 0;
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);

            Rectangle dim = mainPanel.GetDimensions().ToRectangle();
            float pulse = (float)(System.Math.Sin(Main.GameUpdateCount * 0.08f) + 1f) * 0.5f;
            Color topGlow = Color.Lerp(new Color(255, 215, 0), new Color(100, 200, 255), pulse) * 0.6f;

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(dim.X + 2, dim.Y + 2, dim.Width - 4, 2), topGlow);
        }
    }
}