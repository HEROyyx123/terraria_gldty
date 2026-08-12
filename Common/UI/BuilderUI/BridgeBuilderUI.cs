using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
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
            _mainPanel.SetPadding(10);
            _mainPanel.Width.Set(360, 0);
            _mainPanel.Height.Set(350, 0); // 增加高度适配新选项
            _mainPanel.Left.Set(400, 0);
            _mainPanel.Top.Set(200, 0);

            _mainPanel.OnLeftMouseDown += DragStart;
            _mainPanel.OnLeftMouseUp += DragEnd;
            Append(_mainPanel);

            // 标题
            UIText title = new UIText("平台建造器", 1.0f);
            title.Left.Set(10, 0);
            title.Top.Set(10, 0);
            _mainPanel.Append(title);

            // 关闭按钮
            UIText closeBtn = new UIText("X", 0.9f);
            closeBtn.Left.Set(330, 0);
            closeBtn.Top.Set(8, 0);
            closeBtn.OnLeftClick += (evt, el) => BridgeBuilderSystem.Instance.ToggleUI();
            _mainPanel.Append(closeBtn);

            // 槽位
            _platformSlot = new VanillaItemSlotWrapper(scale: 0.85f);
            _platformSlot.Left.Set(10, 0);
            _platformSlot.Top.Set(40, 0);
            _mainPanel.Append(_platformSlot);

            UIText platformSlotLabel = new UIText("放入平台/方块", 0.8f);
            platformSlotLabel.Left.Set(60, 0);
            platformSlotLabel.Top.Set(52, 0);
            _mainPanel.Append(platformSlotLabel);

            _lightSlot = new VanillaItemSlotWrapper(scale: 0.85f);
            _lightSlot.Left.Set(10, 0);
            _lightSlot.Top.Set(90, 0);
            _mainPanel.Append(_lightSlot);

            UIText lightSlotLabel = new UIText("放入火把/灯笼(可选)", 0.8f);
            lightSlotLabel.Left.Set(60, 0);
            lightSlotLabel.Top.Set(102, 0);
            _mainPanel.Append(lightSlotLabel);

            // 长度调节
            _lengthText = new UIText("", 0.85f);
            _lengthText.Left.Set(10, 0);
            _lengthText.Top.Set(145, 0);
            _mainPanel.Append(_lengthText);

            UIPanel lenSub = CreateButton("-", 160, 140, 55, 25, (a, b) => AdjustLength(-BridgeBuilderSettings.DynamicStep));
            UIPanel lenAdd = CreateButton("+", 220, 140, 55, 25, (a, b) => AdjustLength(BridgeBuilderSettings.DynamicStep));
            UIPanel lenMax = CreateButton("最大", 280, 140, 45, 25, (a, b) => BridgeBuilderSettings.Length = Main.maxTilesX);
            _mainPanel.Append(lenSub);
            _mainPanel.Append(lenAdd);
            _mainPanel.Append(lenMax);

            // 光源间隔
            _spacingText = new UIText("", 0.85f);
            _spacingText.Left.Set(10, 0);
            _spacingText.Top.Set(175, 0);
            _mainPanel.Append(_spacingText);

            UIPanel spcSub = CreateButton("-1", 220, 170, 45, 25, (a, b) => AdjustSpacing(-1));
            UIPanel spcAdd = CreateButton("+1", 270, 170, 45, 25, (a, b) => AdjustSpacing(1));
            _mainPanel.Append(spcSub);
            _mainPanel.Append(spcAdd);

            // ===== 新增：向上/向下摧毁范围控件 =====
            _clearUpText = new UIText("", 0.85f);
            _clearUpText.Left.Set(10, 0);
            _clearUpText.Top.Set(205, 0);
            _mainPanel.Append(_clearUpText);

            UIPanel upSub = CreateButton("-1", 220, 200, 45, 25, (a, b) => AdjustClearUp(-1));
            UIPanel upAdd = CreateButton("+1", 270, 200, 45, 25, (a, b) => AdjustClearUp(1));
            _mainPanel.Append(upSub);
            _mainPanel.Append(upAdd);

            _clearDownText = new UIText("", 0.85f);
            _clearDownText.Left.Set(10, 0);
            _clearDownText.Top.Set(235, 0);
            _mainPanel.Append(_clearDownText);

            UIPanel downSub = CreateButton("-1", 220, 230, 45, 25, (a, b) => AdjustClearDown(-1));
            UIPanel downAdd = CreateButton("+1", 270, 230, 45, 25, (a, b) => AdjustClearDown(1));
            _mainPanel.Append(downSub);
            _mainPanel.Append(downAdd);

            // 形状 / 方向 / 预览
            _shapeBtnText = new UIText("", 0.8f);
            UIPanel shapeBtn = CreateButton("", 10, 265, 120, 30, ToggleShape);
            shapeBtn.Append(_shapeBtnText);
            _mainPanel.Append(shapeBtn);

            _dirBtnText = new UIText("", 0.8f);
            UIPanel dirBtn = CreateButton("", 140, 265, 100, 30, ToggleDirection);
            dirBtn.Append(_dirBtnText);
            _mainPanel.Append(dirBtn);

            _previewBtnText = new UIText("", 0.8f);
            UIPanel previewBtn = CreateButton("", 250, 265, 95, 30, (a, b) => BridgeBuilderSettings.ShowPreview = !BridgeBuilderSettings.ShowPreview);
            previewBtn.Append(_previewBtnText);
            _mainPanel.Append(previewBtn);

            // 恢复默认 & 撤销搭建
            UIPanel resetBtn = CreateButton("恢复默认", 10, 305, 160, 25, (a, b) => BridgeBuilderSettings.ResetToDefaults());
            _mainPanel.Append(resetBtn);

            _undoBtnText = new UIText("撤销搭建", 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
            UIPanel undoBtn = CreateButton("", 180, 305, 165, 25, (a, b) => BridgeBuilderSettings.UndoLastBuild());
            undoBtn.Append(_undoBtnText);
            _mainPanel.Append(undoBtn);
            undoBtn.BackgroundColor = Color.DarkRed * 0.8f;

            UpdateTextDisplays();
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

            BridgeBuilderSettings.PlatformItem = _platformSlot.Item;
            BridgeBuilderSettings.LightItem = _lightSlot.Item;

            UpdateTextDisplays();
        }

        private void AdjustLength(int delta) => BridgeBuilderSettings.Length = System.Math.Clamp(BridgeBuilderSettings.Length + delta, 10, Main.maxTilesX);
        private void AdjustSpacing(int delta) => BridgeBuilderSettings.LightSpacing = System.Math.Clamp(BridgeBuilderSettings.LightSpacing + delta, 2, 100);
        
        // 限额 0 ~ 10 格
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
        }

        private UIPanel CreateButton(string text, float x, float y, float w, float h, UIElement.MouseEvent clickEvent)
        {
            UIPanel btn = new UIPanel();
            btn.Left.Set(x, 0);
            btn.Top.Set(y, 0);
            btn.Width.Set(w, 0);
            btn.Height.Set(h, 0);
            btn.OnLeftClick += clickEvent;

            if (!string.IsNullOrEmpty(text))
            {
                UIText txt = new UIText(text, 0.8f) { HAlign = 0.5f, VAlign = 0.5f };
                btn.Append(txt);
            }
            return btn;
        }
    }
}