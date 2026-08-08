using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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

        // 标识当前是否开启删除模式
        public bool IsDeleteMode { get; private set; } = false;

        public override void OnInitialize() {
            mainPanel = new UIPanel();
            mainPanel.SetPadding(10);
            mainPanel.Width.Set(480f, 0f);
            mainPanel.Height.Set(380f, 0f);
            mainPanel.HAlign = 0.5f;
            mainPanel.VAlign = 0.5f;
            mainPanel.BackgroundColor = new Color(33, 43, 79) * 0.92f;

            titleText = new UIText("灵魂手册 - 已驯服小怪", 0.85f, true);
            titleText.Top.Set(10f, 0f);
            titleText.Left.Set(15f, 0f);
            mainPanel.Append(titleText);

            // 关闭按钮 (X)
            UITextPanel<string> closeButton = new UITextPanel<string>("X");
            closeButton.SetPadding(7);
            closeButton.Width.Set(30f, 0f);
            closeButton.Height.Set(30f, 0f);
            closeButton.Left.Set(-40f, 1f);
            closeButton.Top.Set(10f, 0f);
            closeButton.BackgroundColor = Color.Red * 0.7f;
            closeButton.OnMouseOver += (evt, listeningElement) => closeButton.BackgroundColor = Color.Red;
            closeButton.OnMouseOut += (evt, listeningElement) => closeButton.BackgroundColor = Color.Red * 0.7f;
            closeButton.OnLeftClick += (evt, listeningElement) => {
                SoundEngine.PlaySound(SoundID.MenuClose);
                Common.Systems.TamedSystem.ToggleUI();
            };
            mainPanel.Append(closeButton);

            // 【新增】：删除模式切换按钮 (位于关闭按钮左侧)
            deleteModeButton = new UITextPanel<string>("删除模式");
            deleteModeButton.SetPadding(5);
            deleteModeButton.Width.Set(90f, 0f);
            deleteModeButton.Height.Set(30f, 0f);
            deleteModeButton.Left.Set(-140f, 1f);
            deleteModeButton.Top.Set(10f, 0f);
            deleteModeButton.BackgroundColor = Color.Gray * 0.6f;

            deleteModeButton.OnLeftClick += (evt, listeningElement) => {
                IsDeleteMode = !IsDeleteMode; // 切换删除模式状态
                SoundEngine.PlaySound(IsDeleteMode ? SoundID.Item14 : SoundID.MenuTick);
                
                // 动态更改按钮外观
                if (IsDeleteMode) {
                    deleteModeButton.SetText("[c/FF3333:删除模式中]");
                    deleteModeButton.BackgroundColor = Color.DarkRed * 0.8f;
                } else {
                    deleteModeButton.SetText("删除模式");
                    deleteModeButton.BackgroundColor = Color.Gray * 0.6f;
                }
                
                RefreshGrid(); // 刷新网格应用样式
            };
            mainPanel.Append(deleteModeButton);

            // 滚动列表面板容器
            UIPanel gridContainer = new UIPanel();
            gridContainer.Top.Set(50f, 0f);
            gridContainer.Width.Set(0f, 1f);
            gridContainer.Height.Set(-60f, 1f);
            gridContainer.BackgroundColor = Color.Transparent;
            gridContainer.BorderColor = Color.Transparent;
            mainPanel.Append(gridContainer);

            npcList = new UIList();
            npcList.Width.Set(-25f, 1f);
            npcList.Height.Set(0f, 1f);
            npcList.ListPadding = 6f;
            gridContainer.Append(npcList);

            scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(0f, 1f);
            scrollbar.Left.Set(-20f, 1f);
            scrollbar.HAlign = 1f;
            gridContainer.Append(scrollbar);

            npcList.SetScrollbar(scrollbar);

            Append(mainPanel);
        }

        public void RefreshGrid() {
            if (npcList == null) return;
            
            npcList.Clear();

            var modPlayer = Main.LocalPlayer.GetModPlayer<Common.Players.TamedPlayer>();
            var unlockedList = modPlayer.UnlockedNPCTypes;

            if (unlockedList.Count == 0) {
                UIText hint = new UIText("尚未捕获任何怪物...\n(使用灵魂手册右键敌怪)", 0.8f);
                hint.HAlign = 0.5f;
                hint.TextColor = Color.Gray;
                npcList.Add(hint);
                titleText.SetText($"灵魂手册 (0/???)");
                return;
            }

            titleText.SetText($"灵魂手册 ({unlockedList.Count}/{NPCLoader.NPCCount})");

            UIElement rowPanel = null;
            int itemsInRow = 0;
            int maxItemsPerRow = 7;

            foreach (int npcType in unlockedList) {
                if (itemsInRow == 0) {
                    rowPanel = new UIElement();
                    rowPanel.Width.Set(0f, 1f);
                    rowPanel.Height.Set(48f, 0f);
                    npcList.Add(rowPanel);
                }

                UINpcSlot slot = new UINpcSlot(npcType, 0.85f);
                slot.Left.Set(itemsInRow * 52f, 0f);

                int typeToHandle = npcType;
                // 在 TamedUI.cs 的 RefreshGrid() 方法内修改删除点击事件：
                slot.OnLeftClick += (evt, listeningElement) =>
                {
                    if (IsDeleteMode)
                    {
                        if (modPlayer.RemoveUnlockedNPC(typeToHandle))
                        {
                            SoundEngine.PlaySound(SoundID.Item14);

                            // 安全方式获取名称，不使用 new NPC()[cite: 7]
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
    }
}