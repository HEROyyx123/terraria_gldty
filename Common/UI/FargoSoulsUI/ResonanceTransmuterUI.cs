using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using terraria_gldty.Common.UI.BuilderUI; // 引用 VanillaItemSlotWrapper 辅助槽
using terraria_gldty.Common.Systems;
using terraria_gldty.Common.ModIntegration.FargoSoulsHelper;

namespace terraria_gldty.Common.UI.FargoSoulsUI
{
    public class ResonanceTransmuterUI : UIState
    {
        private UIPanel _mainPanel;
        private UIList _targetList;
        private UIScrollbar _scrollbar;
        private UIText _selectedText;
        private UIPanel _transmuteButton;
        private UIText _btnText;

        // 原生物品槽包装
        private VanillaItemSlotWrapper _inputSlot;
        private VanillaItemSlotWrapper _catalystSlot;

        public Item InputItem => _inputSlot.Item;
        public Item CatalystItem => _catalystSlot.Item;
        public int SelectedTargetType = -1;

        private bool _pendingConfirmation = false;
        private Vector2 _dragOffset;
        private bool _isDragging;

        public override void OnInitialize()
        {
            // 1. 主面板设计 (深蓝科技风)
            _mainPanel = new UIPanel();
            _mainPanel.SetPadding(10);
            _mainPanel.Width.Set(420f, 0f);
            _mainPanel.Height.Set(320f, 0f);
            _mainPanel.Left.Set(Main.screenWidth / 2f - 210f, 0f);
            _mainPanel.Top.Set(Main.screenHeight / 2f - 160f, 0f);
            _mainPanel.BackgroundColor = new Color(16, 20, 36, 240);
            _mainPanel.BorderColor = new Color(0, 190, 230, 220);

            _mainPanel.OnLeftMouseDown += DragStart;
            _mainPanel.OnLeftMouseUp += DragEnd;
            Append(_mainPanel);

            // 2. 标题
            UIText title = new UIText("魂石兑换机", 0.65f, true);
            title.Left.Set(8f, 0f);
            title.Top.Set(8f, 0f);
            title.TextColor = new Color(255, 215, 0);
            _mainPanel.Append(title);

            // 3. 右上角关闭按钮
            UIPanel closeBtn = CreateButton("X", 0, 6, 26, 26, (evt, el) => UISystem.Instance.CloseUI(), new Color(190, 45, 45, 220));
            closeBtn.HAlign = 1f;
            closeBtn.Left.Set(-4, 0);
            _mainPanel.Append(closeBtn);

            // 4. 左侧物品槽区域
            _inputSlot = new VanillaItemSlotWrapper(scale: 0.9f);
            _inputSlot.Left.Set(15f, 0f);
            _inputSlot.Top.Set(45f, 0f);
            _mainPanel.Append(_inputSlot);

            UIText inputLabel = new UIText("魔石 / 力/ 魂", 0.75f);
            inputLabel.Left.Set(68f, 0f);
            inputLabel.Top.Set(58f, 0f);
            inputLabel.TextColor = Color.LightSkyBlue;
            _mainPanel.Append(inputLabel);

            _catalystSlot = new VanillaItemSlotWrapper(scale: 0.9f);
            _catalystSlot.Left.Set(15f, 0f);
            _catalystSlot.Top.Set(110f, 0f);
            _mainPanel.Append(_catalystSlot);

            UIText catalystLabel = new UIText("纯净魔石", 0.75f);
            catalystLabel.Left.Set(68f, 0f);
            catalystLabel.Top.Set(123f, 0f);
            catalystLabel.TextColor = Color.LightSkyBlue;
            _mainPanel.Append(catalystLabel);

            // 5. 目标魔石选择列表面板
            UIPanel listPanel = new UIPanel();
            listPanel.Width.Set(215f, 0f);
            listPanel.Height.Set(210f, 0f);
            listPanel.Left.Set(185f, 0f);
            listPanel.Top.Set(40f, 0f);
            listPanel.BackgroundColor = new Color(10, 14, 26, 200);
            listPanel.BorderColor = new Color(0, 120, 180, 180);
            _mainPanel.Append(listPanel);

            _targetList = new UIList();
            _targetList.Width.Set(180f, 0f);
            _targetList.Height.Set(198f, 0f);
            listPanel.Append(_targetList);

            _scrollbar = new UIScrollbar();
            _scrollbar.SetView(100f, 1000f);
            _scrollbar.Height.Set(198f, 0f);
            _scrollbar.Left.Set(182f, 0f);
            listPanel.Append(_scrollbar);
            _targetList.SetScrollbar(_scrollbar);

            // 6. 底部提示文本
            _selectedText = new UIText("请先放入魂石/力", 0.75f);
            _selectedText.Left.Set(15f, 0f);
            _selectedText.Top.Set(180f, 0f);
            _selectedText.TextColor = Color.Gray;
            _mainPanel.Append(_selectedText);

            // 7. 转换操作按钮
            _transmuteButton = CreateButton("", 15, 215, 155, 36, OnTransmuteClicked);
            _mainPanel.Append(_transmuteButton);

            _btnText = new UIText("转换 / 还原", 0.85f) { HAlign = 0.5f, VAlign = 0.5f };
            _transmuteButton.Append(_btnText);
        }

        private void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            if (evt.Target == _mainPanel)
            {
                _dragOffset = new Vector2(evt.MousePosition.X - _mainPanel.Left.Pixels, evt.MousePosition.Y - _mainPanel.Top.Pixels);
                _isDragging = true;
            }
        }

        private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            _isDragging = false;
        }

        private int _lastInputType = 0;
        private int _lastInputStack = 0;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_mainPanel != null && _mainPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (_isDragging)
            {
                _mainPanel.Left.Set(Main.mouseX - _dragOffset.X, 0f);
                _mainPanel.Top.Set(Main.mouseY - _dragOffset.Y, 0f);
                _mainPanel.Recalculate();
            }

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                UISystem.Instance.CloseUI();
            }

            // 监听物品槽变动，自动刷新列表
            if (InputItem.type != _lastInputType || InputItem.stack != _lastInputStack)
            {
                _lastInputType = InputItem.type;
                _lastInputStack = InputItem.stack;
                RefreshTargetList();
            }
        }

        public void RefreshTargetList()
        {
            _targetList.Clear();
            SelectedTargetType = -1; // 重置当前选中的魔石ID
            ResetConfirmation();

            if (InputItem.IsAir)
            {
                _selectedText.SetText("请先放入魂石/力");
                _selectedText.TextColor = Color.Gray;
                return;
            }

            var global = InputItem.GetGlobalItem<BoundGlobalItem>();
            if (global.BoundSourceType > 0)
            {
                _selectedText.SetText($"可还原: {Lang.GetItemNameValue(global.BoundSourceType)}");
                _selectedText.TextColor = Color.LightSkyBlue;
                _btnText.SetText("还原魂石");
                return;
            }

            List<int> targets = TransmutationLogic.GetAvailableTargets(InputItem);

            foreach (int targetType in targets)
            {
                UIItemTargetSlot slot = new UIItemTargetSlot(targetType, this);
                _targetList.Add(slot);
            }

            _targetList.Recalculate();
        }

        public void SelectTarget(int targetType)
        {
            SelectedTargetType = targetType;
            ResetConfirmation();
            SoundEngine.PlaySound(SoundID.MenuTick);
            _selectedText.SetText($"目标: {Lang.GetItemNameValue(targetType)}");
            _selectedText.TextColor = Color.Gold;
            _btnText.SetText("确认转换");
        }

        private void ResetConfirmation()
        {
            _pendingConfirmation = false;
            _transmuteButton.BackgroundColor = new Color(28, 40, 70, 220);
            _transmuteButton.BorderColor = new Color(0, 150, 200, 180);

            if (InputItem.IsAir)
            {
                _btnText.SetText("锁定 / 还原");
            }
            else
            {
                var global = InputItem.GetGlobalItem<BoundGlobalItem>();
                if (global.BoundSourceType > 0)
                {
                    _btnText.SetText("确认还原");
                }
                else
                {
                    _btnText.SetText(SelectedTargetType > 0 ? "确认转换" : "选择目标");
                }
            }
        }

        private void OnTransmuteClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            if (InputItem.IsAir) return;

            var global = InputItem.GetGlobalItem<BoundGlobalItem>();

            // 还原模式不需校验同源媒介与二次确认
            if (global.BoundSourceType > 0)
            {
                ExecuteTransmute();
                return;
            }

            if (SelectedTargetType <= 0)
            {
                _selectedText.SetText("请先选择转换目标！");
                _selectedText.TextColor = Color.Orange;
                SoundEngine.PlaySound(SoundID.MenuClose);
                return;
            }

            // 【校验】：同源媒介不足提示
            if (CatalystItem.IsAir || CatalystItem.stack < 1)
            {
                _selectedText.SetText("转换失败: 纯净魔石不足！");
                _selectedText.TextColor = Color.Red;
                SoundEngine.PlaySound(SoundID.MenuClose);
                return;
            }

            // 防误触逻辑：二次确认
            if (!_pendingConfirmation)
            {
                _pendingConfirmation = true;
                _transmuteButton.BackgroundColor = new Color(180, 40, 40, 230);
                _transmuteButton.BorderColor = new Color(255, 60, 60, 255);
                _btnText.SetText("再次点击以确认");
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else
            {
                ExecuteTransmute();
            }
        }

        private void ExecuteTransmute()
        {
            Item catalyst = CatalystItem;
            Item result = TransmutationLogic.Transmute(InputItem, ref catalyst, SelectedTargetType);
            
            if (!result.IsAir)
            {
                SoundEngine.PlaySound(SoundID.Item4); // 转化成功音效
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("Transmutation"), result, result.stack);
                RefreshTargetList();
            }
            else
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
            }

            ResetConfirmation();
        }

        // 通用按钮封装
        private UIPanel CreateButton(string text, float x, float y, float w, float h, UIElement.MouseEvent clickEvent, Color? customBgColor = null)
        {
            UIPanel btn = new UIPanel();
            btn.Left.Set(x, 0);
            btn.Top.Set(y, 0);
            btn.Width.Set(w, 0);
            btn.Height.Set(h, 0);
            btn.SetPadding(0);

            Color defaultBg = customBgColor ?? new Color(28, 40, 70, 220);
            Color defaultBorder = new Color(0, 150, 200, 180);

            btn.BackgroundColor = defaultBg;
            btn.BorderColor = defaultBorder;

            btn.OnLeftClick += (evt, el) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
                clickEvent?.Invoke(evt, el);
            };

            btn.OnMouseOver += (evt, el) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
                btn.BackgroundColor = new Color(45, 80, 130, 240);
                btn.BorderColor = new Color(0, 230, 255, 255);
            };

            btn.OnMouseOut += (evt, el) => {
                if (!_pendingConfirmation || btn != _transmuteButton)
                {
                    btn.BackgroundColor = defaultBg;
                    btn.BorderColor = defaultBorder;
                }
            };

            if (!string.IsNullOrEmpty(text))
            {
                UIText txt = new UIText(text, 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
                btn.Append(txt);
            }

            return btn;
        }
    }

    // 目标魔石槽位项
    public class UIItemTargetSlot : UIPanel
    {
        public readonly int ItemType;
        private readonly Item _dummyItem;
        private readonly ResonanceTransmuterUI _parentUI;

        public UIItemTargetSlot(int itemType, ResonanceTransmuterUI parentUI)
        {
            ItemType = itemType;
            _parentUI = parentUI;

            _dummyItem = new Item();
            _dummyItem.SetDefaults(ItemType);

            Width.Set(160f, 0f);
            Height.Set(36f, 0f);
            PaddingLeft = PaddingRight = PaddingTop = PaddingBottom = 2f;

            OnLeftClick += (evt, el) => _parentUI.SelectTarget(ItemType);

            OnMouseOver += (evt, el) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // 唯一选中状态判定：防止多个框出现高亮
            bool isSelected = _parentUI.SelectedTargetType == ItemType;

            if (isSelected)
            {
                BackgroundColor = new Color(50, 90, 140, 240);
                BorderColor = new Color(255, 215, 0, 255); // 唯一金色选中边框
            }
            else if (IsMouseHovering)
            {
                BackgroundColor = new Color(35, 60, 95, 220);
                BorderColor = new Color(0, 180, 220, 220); // 鼠标悬停天蓝边框
            }
            else
            {
                BackgroundColor = new Color(20, 28, 48, 200);
                BorderColor = new Color(0, 100, 150, 160); // 默认边框
            }

            base.DrawSelf(spriteBatch);

            Vector2 pos = GetDimensions().Position();
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[ItemType].Value;

            // 绘制物品图标
            spriteBatch.Draw(tex, pos + new Vector2(6, 4), Color.White);

            // 绘制物品名称
            string name = Lang.GetItemNameValue(ItemType);
            Color nameColor = isSelected ? Color.Gold : Color.White;
            Utils.DrawBorderString(spriteBatch, name, pos + new Vector2(40, 6), nameColor, 0.72f);

            // 悬浮显示原生物品预览 Tooltip
            if (IsMouseHovering)
            {
                Main.HoverItem = _dummyItem.Clone();
                Main.hoverItemName = _dummyItem.Name;
            }
        }
    }
}