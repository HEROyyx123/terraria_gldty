using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace terraria_gldty.Common.UI.BlackHoleStoneUI
{
    internal class BlackHoleStoneUI : UIState
    {
        private UIPanel panel;
        private UIText rangeLabel;
        private UIText rangeValue;
        private UIImageButton closeButton;
        private UIImageButton sliderThumb;

        private bool _pickUpAll = true;
        private bool _previousPickUpAll = true;
        private CheckboxElement checkbox;

        private SliderTrack sliderTrack;
        private float currentRange;

        private FilterSlot[] filterSlots = new FilterSlot[10];

        private bool _dragging;
        private Vector2 _dragOffset;

        public override void OnInitialize() {
            panel = new UIPanel();
            panel.SetPadding(12);
            panel.Width.Set(440, 0f);
            panel.Height.Set(400, 0f);
            panel.Left.Set(Main.screenWidth / 2 - 220, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 200, 0f);
            panel.BackgroundColor = new Color(30, 30, 50, 230);
            panel.BorderColor = new Color(100, 80, 180, 255);
            Append(panel);

            var titleText = new UIText(Language.GetTextValue("Mods.terraria_gldty.BlackHoleStone.DisplayName"), 1.1f);
            titleText.Left.Set(10, 0f);
            titleText.Top.Set(8, 0f);
            titleText.TextColor = Color.LightSkyBlue;
            panel.Append(titleText);

            closeButton = new UIImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonDelete"));
            closeButton.Width.Set(18, 0f);
            closeButton.Height.Set(18, 0f);
            closeButton.Left.Set(410, 0f);
            closeButton.Top.Set(8, 0f);
            closeButton.OnLeftClick += (evt, _) => CloseUI();
            panel.Append(closeButton);

            checkbox = new CheckboxElement();
            checkbox.Width.Set(24, 0f);
            checkbox.Height.Set(24, 0f);
            checkbox.Left.Set(10, 0f);
            checkbox.Top.Set(40, 0f);
            checkbox.OnLeftClick += (_, _) => TogglePickUpAll();
            panel.Append(checkbox);

            var checkLabel = new UIText(Language.GetTextValue("Mods.terraria_gldty.BlackHoleStone.PickUpAll"), 0.9f);
            checkLabel.Left.Set(34, 0f);
            checkLabel.Top.Set(42, 0f);
            panel.Append(checkLabel);

            var filterLabel = new UIText(Language.GetTextValue("Mods.terraria_gldty.BlackHoleStone.FilterLabel"), 0.9f);
            filterLabel.Left.Set(10, 0f);
            filterLabel.Top.Set(72, 0f);
            panel.Append(filterLabel);

            for (int i = 0; i < 10; i++) {
                int row = i / 5;
                int col = i % 5;
                var slot = new FilterSlot(i);
                slot.Left.Set(10 + col * 82, 0f);
                slot.Top.Set(96 + row * 58, 0f);
                panel.Append(slot);
                filterSlots[i] = slot;
            }

            rangeLabel = new UIText(Language.GetTextValue("Mods.terraria_gldty.BlackHoleStone.RangeLabel"), 0.9f);
            rangeLabel.Left.Set(10, 0f);
            rangeLabel.Top.Set(220, 0f);
            panel.Append(rangeLabel);

            rangeValue = new UIText("0", 0.9f);
            rangeValue.Left.Set(300, 0f);
            rangeValue.Top.Set(220, 0f);
            rangeValue.TextColor = Color.Gold;
            panel.Append(rangeValue);

            sliderTrack = new SliderTrack();
            sliderTrack.Width.Set(270, 0f);
            sliderTrack.Height.Set(16, 0f);
            sliderTrack.Left.Set(10, 0f);
            sliderTrack.Top.Set(248, 0f);
            panel.Append(sliderTrack);

            sliderThumb = new UIImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonPlay"));
            sliderThumb.Width.Set(16, 0f);
            sliderThumb.Height.Set(24, 0f);
            sliderThumb.Left.Set(10, 0f);
            sliderThumb.Top.Set(244, 0f);
            panel.Append(sliderThumb);

            var hintText = new UIText(Language.GetTextValue("Mods.terraria_gldty.BlackHoleStone.Hint"), 0.8f);
            hintText.Left.Set(10, 0f);
            hintText.Top.Set(360, 0f);
            hintText.TextColor = Color.Gray;
            panel.Append(hintText);
        }

        private void TogglePickUpAll() {
            _pickUpAll = !_pickUpAll;
            checkbox.Checked = _pickUpAll;
            var player = Main.LocalPlayer.GetModPlayer<Players.BlackHoleStonePlayer>();
            player.PickUpAll = _pickUpAll;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if (Main.LocalPlayer == null) return;

            var player = Main.LocalPlayer.GetModPlayer<Players.BlackHoleStonePlayer>();

            _pickUpAll = player.PickUpAll;
            if (_pickUpAll != _previousPickUpAll) {
                checkbox.Checked = _pickUpAll;
                _previousPickUpAll = _pickUpAll;
            }

            for (int i = 0; i < 10; i++) {
                filterSlots[i].StoredType = player.FilterItems[i];
            }

            currentRange = player.PickupRange;
            UpdateSliderPosition();
            sliderTrack.Value = currentRange / 2000f;

            if (Main.mouseLeft) {
                float minX = 10f;
                float maxX = minX + 270f - 16f;
                CalculatedStyle panelDims = panel.GetDimensions();
                float sliderAreaY = panelDims.Y + 244f;
                float sliderAreaHeight = 24f;
                if (Main.mouseY >= sliderAreaY && Main.mouseY <= sliderAreaY + sliderAreaHeight) {
                    float mouseX = Main.mouseX - panelDims.X;
                    float newX = MathHelper.Clamp(mouseX - 8, minX, maxX);
                    sliderThumb.Left.Set(newX, 0f);

                    float t = (newX - minX) / (maxX - minX);
                    int range = (int)(t * 2000f);
                    range = (int)(range / 10f) * 10;
                    currentRange = range;
                    player.PickupRange = range;
                    rangeValue.SetText(range.ToString());
                }
            }

            if (panel.ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            CalculatedStyle dims = panel.GetDimensions();
            Rectangle titleBar = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, 30);
            if (!_dragging && Main.mouseLeft && titleBar.Contains(Main.MouseScreen.ToPoint()) && !closeButton.ContainsPoint(Main.MouseScreen)) {
                _dragging = true;
                _dragOffset = Main.MouseScreen - new Vector2(dims.X, dims.Y);
            }

            if (_dragging) {
                if (!Main.mouseLeft) {
                    _dragging = false;
                }
                else {
                    panel.Left.Set(Main.mouseX - _dragOffset.X, 0f);
                    panel.Top.Set(Main.mouseY - _dragOffset.Y, 0f);
                    panel.Recalculate();
                }
            }
        }

        private void UpdateSliderPosition() {
            float minX = 10f;
            float maxX = minX + 270f - 16f;
            float t = currentRange / 2000f;
            sliderThumb.Left.Set(minX + t * (maxX - minX), 0f);
            rangeValue.SetText(((int)currentRange).ToString());
        }

        public void OpenUI() {
            if (Main.LocalPlayer != null) {
                var player = Main.LocalPlayer.GetModPlayer<Players.BlackHoleStonePlayer>();
                _pickUpAll = player.PickUpAll;
                currentRange = player.PickupRange;
                for (int i = 0; i < 10; i++) {
                    filterSlots[i].StoredType = player.FilterItems[i];
                }

                panel.Left.Set(Main.screenWidth / 2 - 220, 0f);
                panel.Top.Set(Main.screenHeight / 2 - 200, 0f);
            }
            checkbox.Checked = _pickUpAll;
            _previousPickUpAll = _pickUpAll;
            UpdateSliderPosition();
            sliderTrack.Value = currentRange / 2000f;
        }

        private void CloseUI() {
            SoundEngine.PlaySound(SoundID.MenuClose);
            ModContent.GetInstance<BlackHoleStoneUISystem>().HideUI();
        }
    }

    internal class CheckboxElement : UIElement
    {
        public bool Checked;

        public CheckboxElement() {
            Width.Set(16, 0f);
            Height.Set(16, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle dims = GetDimensions();
            var rect = new Rectangle((int)dims.X, (int)dims.Y, 16, 16);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Checked ? Color.LightSkyBlue : Color.Gray * 0.4f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2), new Color(30, 30, 50));
            if (Checked) {
                Utils.DrawBorderString(spriteBatch, "X", new Vector2(dims.X + 3, dims.Y - 1), Color.White, 0.7f);
            }
        }
    }

    internal class SliderTrack : UIElement
    {
        public float Value;

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            CalculatedStyle dims = GetDimensions();
            var bgRect = new Rectangle((int)dims.X, (int)dims.Y + 6, (int)dims.Width, 4);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, bgRect, Color.Gray * 0.4f);
            var fillRect = new Rectangle((int)dims.X, (int)dims.Y + 6, (int)(dims.Width * Value), 4);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, Color.LightSkyBlue * 0.8f);
        }
    }

    internal class FilterSlot : UIElement
    {
        private readonly int _index;
        public int StoredType;

        public FilterSlot(int index) {
            _index = index;
            Width.Set(48, 0f);
            Height.Set(48, 0f);
            OnLeftClick += (_, _) => HandleClick();
        }

        private void HandleClick() {
            var player = Main.LocalPlayer.GetModPlayer<Players.BlackHoleStonePlayer>();
            Item cursorItem = Main.mouseItem;

            if (!cursorItem.IsAir) {
                player.FilterItems[_index] = cursorItem.type;
                StoredType = cursorItem.type;
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else {
                player.FilterItems[_index] = 0;
                StoredType = 0;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            CalculatedStyle dims = GetDimensions();

            Texture2D backTex = TextureAssets.InventoryBack.Value;
            spriteBatch.Draw(backTex, dims.Position(), Color.White * 0.7f);

            if (StoredType > 0) {
                Main.instance.LoadItem(StoredType);
                if (ContentSamples.ItemsByType.TryGetValue(StoredType, out Item item) && !item.IsAir) {
                    Texture2D itemTex = TextureAssets.Item[StoredType].Value;
                    if (itemTex != null) {
                        float scale = Math.Min(36f / itemTex.Width, 36f / itemTex.Height);
                        spriteBatch.Draw(itemTex, dims.Center(), null, Color.White, 0f, itemTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                    }
                }
            }
        }
    }
}