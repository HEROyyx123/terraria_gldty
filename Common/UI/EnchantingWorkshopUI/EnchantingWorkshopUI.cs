using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using terraria_gldty.Common.UI.Globals;

namespace terraria_gldty.Common.UI.EnchantingWorkshopUI
{
    internal class EnchantingWorkshopUI : UIState
    {
        private UIPanel panel;
        private UIImageButton closeButton;
        private CustomWorkshopItemSlot slot;
        private UIList prefixList;
        private UIScrollbar scrollbar;
        private UIText statusText;

        private UIText currentDamageTypeText;
        private UIPanel changeDamageTypeBtn;
        private UIPanel resetDamageTypeBtn;
        private int selectedDamageTypeIndex = -1;

        private int storedItemType;
        private int storedItemStack;
        private int storedItemPrefix;

        private bool _dragging;
        private Vector2 _dragOffset;

        public override void OnInitialize() {
            // 主面板：科技感深蓝底色 + 霓虹青色微光边框
            panel = new UIPanel();
            panel.SetPadding(14);
            panel.Width.Set(460, 0f);
            panel.Height.Set(540, 0f);
            panel.Left.Set(Main.screenWidth / 2 - 230, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 270, 0f);
            panel.BackgroundColor = new Color(16, 20, 36, 240);
            panel.BorderColor = new Color(0, 190, 230, 220);
            Append(panel);

            // 标题：优雅天青配色
            var titleText = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.DisplayName"), 0.6f, true);
            titleText.Left.Set(8, 0f);
            titleText.Top.Set(8, 0f);
            titleText.TextColor = new Color(110, 225, 255);
            panel.Append(titleText);

            // 1. 关闭按钮：利用 HAlign 右对齐，锁定在右上角
            closeButton = new UIImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/SearchCancel"));
            closeButton.HAlign = 1f;
            closeButton.Left.Set(-4, 0f);
            closeButton.Top.Set(8, 0f);
            closeButton.OnLeftClick += (evt, _) => CloseUI();
            panel.Append(closeButton);

            // 存入物品槽
            slot = new CustomWorkshopItemSlot();
            slot.Left.Set(8, 0f);
            slot.Top.Set(48, 0f);
            panel.Append(slot);

            // 状态 / 成本信息
            statusText = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.PlaceItem"), 0.82f);
            statusText.Left.Set(66, 0f);
            statusText.Top.Set(48, 0f);
            statusText.TextColor = new Color(255, 215, 100);
            panel.Append(statusText);

            // 伤害类型状态
            currentDamageTypeText = new UIText("伤害类型: 未放入物品", 0.78f);
            currentDamageTypeText.Left.Set(66, 0f);
            currentDamageTypeText.Top.Set(72, 0f);
            currentDamageTypeText.TextColor = new Color(160, 210, 230);
            panel.Append(currentDamageTypeText);

            // 美化后的操作按钮
            changeDamageTypeBtn = CreateSciFiButton("切换类型", 240, 48, 90, 28, OnChangeDamageTypeClick);
            panel.Append(changeDamageTypeBtn);

            resetDamageTypeBtn = CreateSciFiButton("恢复默认", 338, 48, 90, 28, OnResetDamageTypeClick);
            panel.Append(resetDamageTypeBtn);

            // 滚动条与前缀列表
            scrollbar = new UIScrollbar();
            scrollbar.Height.Set(410, 0f);
            scrollbar.Left.Set(424, 0f);
            scrollbar.Top.Set(110, 0f);
            panel.Append(scrollbar);

            prefixList = new UIList();
            prefixList.Height.Set(410, 0f);
            prefixList.Width.Set(406, 0f);
            prefixList.Left.Set(8, 0f);
            prefixList.Top.Set(110, 0f);
            prefixList.SetScrollbar(scrollbar);
            panel.Append(prefixList);
        }

        private UIPanel CreateSciFiButton(string text, float x, float y, float w, float h, UIElement.MouseEvent onClick) {
            UIPanel btn = new UIPanel();
            btn.Left.Set(x, 0f);
            btn.Top.Set(y, 0f);
            btn.Width.Set(w, 0f);
            btn.Height.Set(h, 0f);
            btn.SetPadding(0);

            Color defaultBg = new Color(28, 40, 70, 220);
            Color defaultBorder = new Color(0, 150, 200, 180);

            btn.BackgroundColor = defaultBg;
            btn.BorderColor = defaultBorder;

            UIText txt = new UIText(text, 0.78f) { HAlign = 0.5f, VAlign = 0.5f };
            txt.TextColor = new Color(210, 240, 255);
            btn.Append(txt);

            btn.OnLeftClick += (evt, el) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
                onClick?.Invoke(evt, el);
            };

            btn.OnMouseOver += (evt, el) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
                btn.BackgroundColor = new Color(45, 80, 130, 240);
                btn.BorderColor = new Color(0, 230, 255, 255);
                txt.TextColor = Color.White;
            };

            btn.OnMouseOut += (evt, el) => {
                btn.BackgroundColor = defaultBg;
                btn.BorderColor = defaultBorder;
                txt.TextColor = new Color(210, 240, 255);
            };

            return btn;
        }

        private void OnChangeDamageTypeClick(UIMouseEvent evt, UIElement listeningElement) {
            if (storedItemType <= 0) return;
            selectedDamageTypeIndex++;
            if (selectedDamageTypeIndex >= CustomDamageTypeItem.DamageClasses.Count) {
                selectedDamageTypeIndex = 0;
            }
            UpdateDamageTypeDisplay();
        }

        private void OnResetDamageTypeClick(UIMouseEvent evt, UIElement listeningElement) {
            if (storedItemType <= 0) return;
            selectedDamageTypeIndex = -1;
            UpdateDamageTypeDisplay();
        }

        private void UpdateDamageTypeDisplay() {
            if (storedItemType <= 0) {
                currentDamageTypeText.SetText("伤害类型: 未放入物品");
                return;
            }

            if (selectedDamageTypeIndex == -1) {
                currentDamageTypeText.SetText("伤害类型: 保持原版");
            } else {
                string name = CustomDamageTypeItem.DamageClassNames[selectedDamageTypeIndex];
                currentDamageTypeText.SetText("目标类型: " + name);
            }
        }

        public void OpenUI() {
            slot.StoredType = storedItemType;
            slot.StoredStack = storedItemStack;
            slot.StoredPrefix = storedItemPrefix;
            selectedDamageTypeIndex = slot.StoredDamageTypeIndex;
            UpdatePrefixList();
            UpdateDamageTypeDisplay();
            panel.Left.Set(Main.screenWidth / 2 - 230, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 270, 0f);
        }

        private void CloseUI() {
            if (Main.LocalPlayer != null && storedItemType > 0 && !Main.LocalPlayer.dead) {
                Item item = new Item();
                item.SetDefaults(storedItemType);
                item.stack = storedItemStack;
                item.Prefix(storedItemPrefix);

                if (selectedDamageTypeIndex >= 0) {
                    var globalItem = item.GetGlobalItem<CustomDamageTypeItem>();
                    globalItem.OverrideDamageTypeIndex = selectedDamageTypeIndex;
                    item.DamageType = CustomDamageTypeItem.DamageClasses[selectedDamageTypeIndex];
                }

                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), item);
            }
            storedItemType = 0;
            storedItemStack = 0;
            storedItemPrefix = 0;
            selectedDamageTypeIndex = -1;
            SoundEngine.PlaySound(SoundID.MenuClose);
            ModContent.GetInstance<EnchantingWorkshopUISystem>().HideUI();
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if (Main.LocalPlayer == null) return;
            if (Main.LocalPlayer.dead || !Main.LocalPlayer.active) {
                CloseUI();
                return;
            }

            if (panel.ContainsPoint(Main.MouseScreen)) {
                Main.LocalPlayer.mouseInterface = true;
            }

            CalculatedStyle dims = panel.GetDimensions();
            Rectangle titleBar = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, 35);
            if (!_dragging && Main.mouseLeft && titleBar.Contains(Main.MouseScreen.ToPoint()) && !closeButton.ContainsPoint(Main.MouseScreen)) {
                _dragging = true;
                _dragOffset = Main.MouseScreen - new Vector2(dims.X, dims.Y);
            }

            if (_dragging) {
                if (!Main.mouseLeft) {
                    _dragging = false;
                } else {
                    panel.Left.Set(Main.mouseX - _dragOffset.X, 0f);
                    panel.Top.Set(Main.mouseY - _dragOffset.Y, 0f);
                    panel.Recalculate();
                }
            }

            if (slot.StoredType != storedItemType) {
                storedItemType = slot.StoredType;
                storedItemStack = slot.StoredStack;
                storedItemPrefix = slot.StoredPrefix;
                selectedDamageTypeIndex = slot.StoredDamageTypeIndex;
                UpdatePrefixList();
                UpdateDamageTypeDisplay();
            }

            if (storedItemType > 0) {
                Item item = ContentSamples.ItemsByType[storedItemType];
                string name = Lang.GetItemNameValue(storedItemType);
                long cost = (long)(item.value * 3f);
                statusText.SetText(name + " | 消耗 " + FormatCoins(cost));
            } else {
                statusText.SetText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.PlaceItem"));
            }
        }

        private void UpdatePrefixList() {
            prefixList.Clear();
            if (storedItemType <= 0) return;

            Item baseItem = ContentSamples.ItemsByType[storedItemType];
            if (baseItem == null || baseItem.IsAir) return;

            List<int> applicablePrefixes = new List<int>();
            for (int p = 1; p < PrefixLoader.PrefixCount; p++) {
                Item testItem = baseItem.Clone();
                testItem.Prefix(p);
                if (testItem.prefix == p) {
                    applicablePrefixes.Add(p);
                }
            }

            if (applicablePrefixes.Count == 0) {
                var noPrefix = new UIText("该物品无法强化词条", 0.8f);
                noPrefix.TextColor = new Color(255, 100, 100);
                prefixList.Add(noPrefix);
                return;
            }

            foreach (int prefixId in applicablePrefixes) {
                var entry = new PrefixEntry(storedItemType, prefixId);
                prefixList.Add(entry);
            }
        }

        internal void OnCraft(int prefixId) {
            if (storedItemType <= 0) return;

            Player player = Main.LocalPlayer;
            Item baseItem = ContentSamples.ItemsByType[storedItemType];
            long cost = (long)(baseItem.value * 3f);

            if (!TryRemoveCoins(player, cost)) {
                Main.NewText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.NotEnoughCoins"), Color.Red);
                return;
            }

            Item newItem = new Item();
            newItem.SetDefaults(storedItemType);
            newItem.Prefix(prefixId);

            if (selectedDamageTypeIndex >= 0 && selectedDamageTypeIndex < CustomDamageTypeItem.DamageClasses.Count) {
                var customData = newItem.GetGlobalItem<CustomDamageTypeItem>();
                customData.OverrideDamageTypeIndex = selectedDamageTypeIndex;
                newItem.DamageType = CustomDamageTypeItem.DamageClasses[selectedDamageTypeIndex];
                customData.ApplyDamageType(newItem);
            }

            storedItemType = 0;
            storedItemStack = 0;
            selectedDamageTypeIndex = -1;
            slot.StoredType = 0;
            slot.StoredStack = 0;
            slot.StoredPrefix = 0;
            slot.StoredDamageTypeIndex = -1;

            bool placed = false;
            for (int i = 0; i < 50; i++) {
                if (player.inventory[i].IsAir) {
                    player.inventory[i] = newItem;
                    placed = true;
                    break;
                }
            }
            if (!placed) {
                player.QuickSpawnItem(player.GetSource_DropAsItem(), newItem);
            }

            SoundEngine.PlaySound(SoundID.Item37);
            Main.NewText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.Crafted"), Color.LightSkyBlue);
            UpdatePrefixList();
            UpdateDamageTypeDisplay();
        }

        private bool TryRemoveCoins(Player player, long amount) {
            if (amount <= 0) return true;
            if (!player.CanAfford(amount)) return false;
            return player.BuyItem(amount);
        }

        internal static string FormatCoins(long value) {
            int platinum = (int)(value / 1000000);
            value %= 1000000;
            int gold = (int)(value / 10000);
            value %= 10000;
            int silver = (int)(value / 100);
            int copper = (int)(value % 100);

            string result = "";
            if (platinum > 0) result += platinum + Language.GetTextValue("LegacyInterface.15") + " ";
            if (gold > 0) result += gold + Language.GetTextValue("LegacyInterface.16") + " ";
            if (silver > 0) result += silver + Language.GetTextValue("LegacyInterface.17") + " ";
            if (copper > 0 || result == "") result += copper + Language.GetTextValue("LegacyInterface.18");
            return result.Trim();
        }
    }

    internal class CustomWorkshopItemSlot : UIElement
    {
        public int StoredType;
        public int StoredStack = 1;
        public int StoredPrefix;
        public int StoredDamageTypeIndex = -1;

        public CustomWorkshopItemSlot() {
            this.Width.Set(48, 0f);
            this.Height.Set(48, 0f);
            OnLeftClick += (_, _) => HandleClick();
        }

        private void HandleClick() {
            Item cursorItem = Main.mouseItem;

            if (StoredType > 0 && cursorItem.IsAir) {
                Item item = new Item();
                item.SetDefaults(StoredType);
                item.stack = StoredStack;
                item.Prefix(StoredPrefix);
                
                if (StoredDamageTypeIndex >= 0) {
                    item.GetGlobalItem<CustomDamageTypeItem>().OverrideDamageTypeIndex = StoredDamageTypeIndex;
                    item.DamageType = CustomDamageTypeItem.DamageClasses[StoredDamageTypeIndex];
                }

                Main.mouseItem = item;
                StoredType = 0;
                StoredStack = 0;
                StoredPrefix = 0;
                StoredDamageTypeIndex = -1;
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (StoredType <= 0 && !cursorItem.IsAir) {
                if (ContentSamples.ItemsByType.TryGetValue(cursorItem.type, out Item testItem) && !testItem.IsAir) {
                    StoredType = cursorItem.type;
                    StoredStack = cursorItem.stack;
                    StoredPrefix = cursorItem.prefix;
                    StoredDamageTypeIndex = cursorItem.GetGlobalItem<CustomDamageTypeItem>().OverrideDamageTypeIndex;
                    
                    Main.mouseItem = new Item();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            CalculatedStyle dims = GetDimensions();

            Texture2D backTex = TextureAssets.InventoryBack.Value;
            Color slotColor = IsMouseHovering ? new Color(0, 200, 255) : new Color(180, 220, 255);
            spriteBatch.Draw(backTex, dims.Position(), slotColor);

            if (StoredType > 0) {
                Main.instance.LoadItem(StoredType);
                if (ContentSamples.ItemsByType.TryGetValue(StoredType, out Item item) && !item.IsAir) {
                    Texture2D itemTex = TextureAssets.Item[StoredType].Value;
                    if (itemTex != null) {
                        float scale = Math.Min(36f / itemTex.Width, 36f / itemTex.Height);
                        spriteBatch.Draw(itemTex, dims.Center(), null, Color.White, 0f, itemTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                    }
                }

                if (ContainsPoint(Main.MouseScreen)) {
                    Item hoverItem = new Item();
                    hoverItem.SetDefaults(StoredType);
                    hoverItem.stack = StoredStack;
                    hoverItem.Prefix(StoredPrefix);
                    if (StoredDamageTypeIndex >= 0) {
                        hoverItem.GetGlobalItem<CustomDamageTypeItem>().OverrideDamageTypeIndex = StoredDamageTypeIndex;
                        hoverItem.DamageType = CustomDamageTypeItem.DamageClasses[StoredDamageTypeIndex];
                    }
                    Main.HoverItem = hoverItem;
                    Main.hoverItemName = hoverItem.Name;
                }
            }
        }
    }

    internal class PrefixEntry : UIPanel
    {
        private readonly int _itemType;
        private readonly int _prefixId;
        private readonly Item _previewItem;

        public PrefixEntry(int itemType, int prefixId) {
            _itemType = itemType;
            _prefixId = prefixId;

            _previewItem = new Item();
            _previewItem.SetDefaults(_itemType);
            _previewItem.Prefix(_prefixId);

            this.Width.Set(400, 0f);
            this.Height.Set(34, 0f);
            BackgroundColor = new Color(20, 28, 48, 200);
            BorderColor = new Color(0, 120, 160, 180);
            SetPadding(4);
            OnLeftClick += (_, _) => Craft();
        }

        private void Craft() {
            var ui = ModContent.GetInstance<EnchantingWorkshopUISystem>()._ui;
            ui?.OnCraft(_prefixId);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            base.DrawSelf(spriteBatch);
            CalculatedStyle dims = GetDimensions();

            if (IsMouseHovering && Main.mouseItem.IsAir) {
                BackgroundColor = new Color(35, 60, 95, 230);
                BorderColor = new Color(0, 220, 255);
                Main.HoverItem = _previewItem.Clone();
                Main.hoverItemName = _previewItem.Name;
            } else {
                BackgroundColor = new Color(20, 28, 48, 200);
                BorderColor = new Color(0, 120, 160, 180);
            }

            string prefixName = Lang.prefix[_prefixId].Value;
            Terraria.Utils.DrawBorderString(spriteBatch, prefixName, new Vector2(dims.X + 10, dims.Y + 6), Color.White, 0.8f);

            Item baseItem = ContentSamples.ItemsByType[_itemType];
            long cost = (long)(baseItem.value * 3f);
            string costText = "消耗 " + EnchantingWorkshopUI.FormatCoins(cost);
            
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(costText) * 0.75f;
            Terraria.Utils.DrawBorderString(spriteBatch, costText, new Vector2(dims.X + dims.Width - textSize.X - 10, dims.Y + 6), new Color(255, 215, 100), 0.75f);
        }
    }
}