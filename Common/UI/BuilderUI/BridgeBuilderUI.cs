using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using terraria_gldty.Common.Systems;

namespace terraria_gldty.Common.UI.BuilderUI
{
    public class BridgeBuilderUI : UIState
    {
        private UIPanel _mainPanel;
        private VanillaItemSlotWrapper _platformSlot;
        private VanillaItemSlotWrapper _lightSlot;

        private UIText _lengthText;
        private UIText _spacingText;
        private UIText _clearUpText;
        private UIText _clearDownText;
        private UIText _shapeBtnText;
        private UIText _dirBtnText;
        private UIText _previewBtnText;
        private UIText _undoBtnText;

        private Vector2 _dragOffset;
        private bool _isDragging;

        public override void OnInitialize()
        {
            _mainPanel = new UIPanel();
            _mainPanel.SetPadding(12);
            _mainPanel.Width.Set(360, 0);
            _mainPanel.Height.Set(355, 0);
            _mainPanel.Left.Set(400, 0);
            _mainPanel.Top.Set(200, 0);
            _mainPanel.BackgroundColor = new Color(16, 20, 36, 240);
            _mainPanel.BorderColor = new Color(0, 190, 230, 220);

            _mainPanel.OnLeftMouseDown += DragStart;
            _mainPanel.OnLeftMouseUp += DragEnd;
            Append(_mainPanel);

            // 标题
            UIText title = new UIText("平台建造器", 0.6f, true);
            title.Left.Set(8, 0);
            title.Top.Set(8, 0);
            title.TextColor = new Color(255, 215, 0);
            _mainPanel.Append(title);

            // 1. 关闭按钮右顶对齐
            UIPanel closeBtn = CreateButton("X", 0, 6, 26, 26, (evt, el) => BridgeBuilderSystem.Instance.ToggleUI(), new Color(190, 45, 45, 220));
            closeBtn.HAlign = 1f;
            closeBtn.Left.Set(-4, 0);
            _mainPanel.Append(closeBtn);

            // 物品槽
            _platformSlot = new VanillaItemSlotWrapper(scale: 0.85f);
            _platformSlot.Left.Set(8, 0);
            _platformSlot.Top.Set(38, 0);
            _mainPanel.Append(_platformSlot);

            UIText platformSlotLabel = new UIText("放入平台 / 方块", 0.8f);
            platformSlotLabel.Left.Set(58, 0);
            platformSlotLabel.Top.Set(50, 0);
            platformSlotLabel.TextColor = Color.LightSkyBlue;
            _mainPanel.Append(platformSlotLabel);

            _lightSlot = new VanillaItemSlotWrapper(scale: 0.85f);
            _lightSlot.Left.Set(8, 0);
            _lightSlot.Top.Set(86, 0);
            _mainPanel.Append(_lightSlot);

            UIText lightSlotLabel = new UIText("放入火把 / 灯笼 (可选)", 0.8f);
            lightSlotLabel.Left.Set(58, 0);
            lightSlotLabel.Top.Set(98, 0);
            lightSlotLabel.TextColor = Color.LightSkyBlue;
            _mainPanel.Append(lightSlotLabel);

            // 控制按键右边缘对齐美化
            // 1. 长度调节
            _lengthText = new UIText("", 0.8f);
            _lengthText.Left.Set(8, 0);
            _lengthText.Top.Set(138, 0);
            _mainPanel.Append(_lengthText);

            _mainPanel.Append(CreateButton("-", 170, 134, 45, 24, (a, b) => AdjustLength(-BridgeBuilderSettings.DynamicStep)));
            _mainPanel.Append(CreateButton("+", 220, 134, 45, 24, (a, b) => AdjustLength(BridgeBuilderSettings.DynamicStep)));
            _mainPanel.Append(CreateButton("最大", 270, 134, 55, 24, (a, b) => BridgeBuilderSettings.Length = Main.maxTilesX));

            // 2. 光源间隔
            _spacingText = new UIText("", 0.8f);
            _spacingText.Left.Set(8, 0);
            _spacingText.Top.Set(168, 0);
            _mainPanel.Append(_spacingText);

            _mainPanel.Append(CreateButton("-1", 220, 164, 45, 24, (a, b) => AdjustSpacing(-1)));
            _mainPanel.Append(CreateButton("+1", 270, 164, 45, 24, (a, b) => AdjustSpacing(1)));

            // 3. 向上清理
            _clearUpText = new UIText("", 0.8f);
            _clearUpText.Left.Set(8, 0);
            _clearUpText.Top.Set(198, 0);
            _mainPanel.Append(_clearUpText);

            _mainPanel.Append(CreateButton("-1", 220, 194, 45, 24, (a, b) => AdjustClearUp(-1)));
            _mainPanel.Append(CreateButton("+1", 270, 194, 45, 24, (a, b) => AdjustClearUp(1)));

            // 4. 向下清理
            _clearDownText = new UIText("", 0.8f);
            _clearDownText.Left.Set(8, 0);
            _clearDownText.Top.Set(228, 0);
            _mainPanel.Append(_clearDownText);

            _mainPanel.Append(CreateButton("-1", 220, 224, 45, 24, (a, b) => AdjustClearDown(-1)));
            _mainPanel.Append(CreateButton("+1", 270, 224, 45, 24, (a, b) => AdjustClearDown(1)));

            // 5. 底部按钮组
            _shapeBtnText = new UIText("", 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
            UIPanel shapeBtn = CreateButton("", 8, 258, 100, 28, ToggleShape);
            shapeBtn.Append(_shapeBtnText);
            _mainPanel.Append(shapeBtn);

            _dirBtnText = new UIText("", 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
            UIPanel dirBtn = CreateButton("", 113, 258, 100, 28, ToggleDirection);
            dirBtn.Append(_dirBtnText);
            _mainPanel.Append(dirBtn);

            _previewBtnText = new UIText("", 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
            UIPanel previewBtn = CreateButton("", 218, 258, 108, 28, (a, b) => BridgeBuilderSettings.ShowPreview = !BridgeBuilderSettings.ShowPreview);
            previewBtn.Append(_previewBtnText);
            _mainPanel.Append(previewBtn);

            // 6. 重置与撤销
            UIPanel resetBtn = CreateButton("恢复默认", 8, 295, 140, 28, (a, b) => BridgeBuilderSettings.ResetToDefaults());
            _mainPanel.Append(resetBtn);

            _undoBtnText = new UIText("撤销搭建", 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
            UIPanel undoBtn = CreateButton("", 154, 295, 172, 28, (a, b) => BridgeBuilderSettings.UndoLastBuild(), new Color(140, 40, 40, 220));
            undoBtn.Append(_undoBtnText);
            _mainPanel.Append(undoBtn);
            UpdateTextDisplays();
            
        }

        // 1. 在 BridgeBuilderUI 类中新增一个同步数据的公开方法
        public void OnOpenUI()
        {
            if (Main.gameMenu || Main.LocalPlayer == null)
                return;

            BridgeBuilderPlayer modPlayer =
                Main.LocalPlayer.GetModPlayer<BridgeBuilderPlayer>();

            if (modPlayer.platformItem == null)
            {
                modPlayer.platformItem = new Item();
                modPlayer.platformItem.SetDefaults(ItemID.None);
            }

            if (modPlayer.lightItem == null)
            {
                modPlayer.lightItem = new Item();
                modPlayer.lightItem.SetDefaults(ItemID.None);
            }

            _platformSlot.Item = modPlayer.platformItem;
            _lightSlot.Item = modPlayer.lightItem;

            // 同时同步到实际建造设置
            BridgeBuilderSettings.PlatformItem =
                modPlayer.platformItem;

            BridgeBuilderSettings.LightItem =
                modPlayer.lightItem;
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_mainPanel != null && _mainPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (_isDragging)
            {
                _mainPanel.Left.Set(Main.mouseX - _dragOffset.X, 0);
                _mainPanel.Top.Set(Main.mouseY - _dragOffset.Y, 0);
                _mainPanel.Recalculate();
            }

            // 进游戏后同步给 ModPlayer 实时保存
            if (Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                var modPlayer = Main.LocalPlayer.GetModPlayer<BridgeBuilderPlayer>();
                modPlayer.platformItem = _platformSlot.Item;
                modPlayer.lightItem = _lightSlot.Item;

                BridgeBuilderSettings.PlatformItem = _platformSlot.Item;
                BridgeBuilderSettings.LightItem = _lightSlot.Item;
            }

            UpdateTextDisplays();
        }

        private void AdjustLength(int delta) => BridgeBuilderSettings.Length = System.Math.Clamp(BridgeBuilderSettings.Length + delta, 10, Main.maxTilesX);
        private void AdjustSpacing(int delta) => BridgeBuilderSettings.LightSpacing = System.Math.Clamp(BridgeBuilderSettings.LightSpacing + delta, 2, 100);
        private void AdjustClearUp(int delta) => BridgeBuilderSettings.ClearUp = System.Math.Clamp(BridgeBuilderSettings.ClearUp + delta, 0, 10);
        private void AdjustClearDown(int delta) => BridgeBuilderSettings.ClearDown = System.Math.Clamp(BridgeBuilderSettings.ClearDown + delta, 0, 10);

        private void ToggleShape(UIMouseEvent evt, UIElement listeningElement)
        {
            BridgeBuilderSettings.Shape = BridgeBuilderSettings.Shape switch
            {
                TileShape.Flat => TileShape.HalfBlock,
                TileShape.HalfBlock => TileShape.SlopeLeft,
                TileShape.SlopeLeft => TileShape.SlopeRight,
                _ => TileShape.Flat
            };
        }

        private void ToggleDirection(UIMouseEvent evt, UIElement listeningElement)
        {
            BridgeBuilderSettings.Direction = BridgeBuilderSettings.Direction switch
            {
                BuildDirection.Right => BuildDirection.Left,
                BuildDirection.Left => BuildDirection.Both,
                _ => BuildDirection.Right
            };
        }

        private void UpdateTextDisplays()
        {
            int step = BridgeBuilderSettings.DynamicStep;
            _lengthText?.SetText($"长度: {BridgeBuilderSettings.Length} 格 (±{step})");
            _spacingText?.SetText($"光源间隔: {BridgeBuilderSettings.LightSpacing} 格");

            _clearUpText?.SetText($"向上清理: {BridgeBuilderSettings.ClearUp} 格");
            _clearDownText?.SetText($"向下清理: {BridgeBuilderSettings.ClearDown} 格");

            _shapeBtnText?.SetText($"形状: {BridgeBuilderSettings.Shape switch { TileShape.Flat => "默认", TileShape.HalfBlock => "半砖", TileShape.SlopeLeft => "左斜坡", _ => "右斜坡" }}");
            _dirBtnText?.SetText($"方向: {BridgeBuilderSettings.Direction switch { BuildDirection.Right => "向右", BuildDirection.Left => "向左", _ => "两侧" }}");
            _previewBtnText?.SetText($"预览: {(BridgeBuilderSettings.ShowPreview ? "开启" : "关闭")}");

            bool canUndo = BridgeBuilderSettings.LastBuildHistory.Count > 0;
            _undoBtnText?.SetText(canUndo ? "撤销搭建 (可用)" : "撤销搭建 (无)");
            _undoBtnText.TextColor = canUndo ? Color.White : Color.Gray;
        }

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
                btn.BackgroundColor = defaultBg;
                btn.BorderColor = defaultBorder;
            };

            if (!string.IsNullOrEmpty(text))
            {
                UIText txt = new UIText(text, 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
                btn.Append(txt);
            }
            return btn;
        }
    }
}