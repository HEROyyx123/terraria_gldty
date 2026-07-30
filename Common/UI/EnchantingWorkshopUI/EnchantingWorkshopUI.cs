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
using terraria_gldty.Common.UI.Globals; // 引用上面的 GlobalItem 命名空间

namespace terraria_gldty.Common.UI.EnchantingWorkshopUI
{
    internal class EnchantingWorkshopUI : UIState
    {
        private UIPanel panel;
        private UIImageButton closeButton;
        private ItemSlot slot;
        private UIList prefixList;
        private UIScrollbar scrollbar;
        private UIText statusText;

        // --- 新增：伤害类型修改相关控件 ---
        private UIText damageTypeTitle;
        private UIText currentDamageTypeText;
        private UITextPanel<string> changeDamageTypeBtn;
        private UITextPanel<string> resetDamageTypeBtn; // 新增：重置按钮
        private int selectedDamageTypeIndex = -1;

        private int storedItemType;
        private int storedItemStack;
        private int storedItemPrefix;

        private bool _dragging;
        private Vector2 _dragOffset;

        public override void OnInitialize() {
            panel = new UIPanel();
            panel.SetPadding(12);
            panel.Width.Set(480, 0f);
            panel.Height.Set(560, 0f); // 稍微调高面板以容纳新功能
            panel.Left.Set(Main.screenWidth / 2 - 240, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 280, 0f);
            panel.BackgroundColor = new Color(30, 30, 50, 230);
            panel.BorderColor = new Color(100, 80, 180, 255);
            Append(panel);

            var titleText = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.DisplayName"), 1.1f);
            titleText.Left.Set(10, 0f);
            titleText.Top.Set(8, 0f);
            titleText.TextColor = Color.LightSkyBlue;
            panel.Append(titleText);

            closeButton = new UIImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/SearchCancel"));
            closeButton.Width.Set(64, 0f);
            closeButton.Height.Set(64, 0f);
            closeButton.Left.Set(430, 0f);
            closeButton.Top.Set(1, 0f);
            closeButton.OnLeftClick += (evt, _) => CloseUI();
            panel.Append(closeButton);

            var instructionText = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.Instruction"), 0.85f);
            instructionText.Left.Set(10, 0f);
            instructionText.Top.Set(40, 0f);
            instructionText.TextColor = Color.Gray;
            panel.Append(instructionText);

            slot = new ItemSlot();
            slot.Left.Set(10, 0f);
            slot.Top.Set(65, 0f);
            panel.Append(slot);

            statusText = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.PlaceItem"), 0.9f);
            statusText.Left.Set(70, 0f);
            statusText.Top.Set(68, 0f);
            statusText.TextColor = Color.Gold;
            panel.Append(statusText);

            // ================= 新增：伤害类型转换区域 =================
            currentDamageTypeText = new UIText("伤害类型: 原版默认", 0.85f);
            currentDamageTypeText.Left.Set(70, 0f);
            currentDamageTypeText.Top.Set(92, 0f);
            currentDamageTypeText.TextColor = Color.LightGreen;
            panel.Append(currentDamageTypeText);

            // 切换按钮
            changeDamageTypeBtn = new UITextPanel<string>("切换类型");
            changeDamageTypeBtn.Left.Set(260, 0f);
            changeDamageTypeBtn.Top.Set(65, 0f);
            changeDamageTypeBtn.Width.Set(90, 0f);
            changeDamageTypeBtn.Height.Set(35, 0f);
            changeDamageTypeBtn.OnLeftClick += OnChangeDamageTypeClick;
            panel.Append(changeDamageTypeBtn);

            // 复原/重置按钮
            resetDamageTypeBtn = new UITextPanel<string>("恢复默认");
            resetDamageTypeBtn.Left.Set(360, 0f);
            resetDamageTypeBtn.Top.Set(65, 0f);
            resetDamageTypeBtn.Width.Set(90, 0f);
            resetDamageTypeBtn.Height.Set(35, 0f);
            resetDamageTypeBtn.OnLeftClick += OnResetDamageTypeClick;
            panel.Append(resetDamageTypeBtn);
            // ==========================================================

            scrollbar = new UIScrollbar();
            scrollbar.Height.Set(380, 0f);
            scrollbar.Left.Set(455, 0f);
            scrollbar.Top.Set(150, 0f);
            panel.Append(scrollbar);

            prefixList = new UIList();
            prefixList.Height.Set(380, 0f);
            prefixList.Width.Set(430, 0f);
            prefixList.Left.Set(10, 0f);
            prefixList.Top.Set(150, 0f);
            prefixList.SetScrollbar(scrollbar);
            panel.Append(prefixList);
        }

        // 按钮点击事件：切换伤害类型
        private void OnChangeDamageTypeClick(UIMouseEvent evt, UIElement listeningElement) {
            if (storedItemType <= 0) return;

            selectedDamageTypeIndex++;
            if (selectedDamageTypeIndex >= CustomDamageTypeItem.DamageClasses.Count) {
                selectedDamageTypeIndex = 0; // 循环切换
            }

            SoundEngine.PlaySound(SoundID.MenuTick);
            UpdateDamageTypeDisplay();
        }

        // 新增：重置为默认原版伤害类型
        private void OnResetDamageTypeClick(UIMouseEvent evt, UIElement listeningElement) {
            if (storedItemType <= 0) return;

            selectedDamageTypeIndex = -1; // -1 代表还原原版类型
            SoundEngine.PlaySound(SoundID.MenuTick);
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
            selectedDamageTypeIndex = slot.StoredDamageTypeIndex; // 恢复选择
            UpdatePrefixList();
            UpdateDamageTypeDisplay();
            panel.Left.Set(Main.screenWidth / 2 - 240, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 280, 0f);
        }

        private void CloseUI() {
            if (Main.LocalPlayer != null && storedItemType > 0 && !Main.LocalPlayer.dead) {
                Item item = new Item();
                item.SetDefaults(storedItemType);
                item.stack = storedItemStack;
                item.Prefix(storedItemPrefix);

                // 还原修改后的伤害类型
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
                statusText.SetText(name + " | " + Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.Cost") + " " + FormatCoins(cost));
            }
            else {
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
                var noPrefix = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.NoPrefixes"), 0.8f);
                noPrefix.TextColor = Color.Red;
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

            // 应用选中的伤害类型信息到新生成的物品上
            if (selectedDamageTypeIndex >= 0 && selectedDamageTypeIndex < CustomDamageTypeItem.DamageClasses.Count) {
                var customData = newItem.GetGlobalItem<CustomDamageTypeItem>();
                customData.OverrideDamageTypeIndex = selectedDamageTypeIndex;
                newItem.DamageType = CustomDamageTypeItem.DamageClasses[selectedDamageTypeIndex];
                 //以下测试代码***********************************************
                customData.ApplyDamageType(newItem); // 手动应用一次
                //**********************************************************
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
            if (!player.CanAfford(amount)) {
                return false;
            }
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

    internal class ItemSlot : UIElement
    {
        public int StoredType;
        public int StoredStack = 1;
        public int StoredPrefix;
        public int StoredDamageTypeIndex = -1; // 记录放到槽里的物品原有的伤害类型修改

        public ItemSlot() {
            Width.Set(52, 0f);
            Height.Set(52, 0f);
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
            spriteBatch.Draw(backTex, dims.Position(), Color.White);

            if (StoredType > 0) {
                Main.instance.LoadItem(StoredType);
                if (ContentSamples.ItemsByType.TryGetValue(StoredType, out Item item) && !item.IsAir) {
                    Texture2D itemTex = TextureAssets.Item[StoredType].Value;
                    if (itemTex != null) {
                        float scale = Math.Min(40f / itemTex.Width, 40f / itemTex.Height);
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

            Width.Set(410, 0f);
            Height.Set(36, 0f);
            BackgroundColor = new Color(40, 40, 60, 200);
            BorderColor = new Color(80, 70, 140, 200);
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
                BackgroundColor = new Color(70, 70, 100, 230);
                Main.HoverItem = _previewItem.Clone();
                Main.hoverItemName = _previewItem.Name;
            }
            else {
                BackgroundColor = new Color(40, 40, 60, 200);
            }

            string prefixName = Lang.prefix[_prefixId].Value;
            Utils.DrawBorderString(spriteBatch, prefixName, new Vector2(dims.X + 8, dims.Y + 8), Color.White, 0.8f);

            Item baseItem = ContentSamples.ItemsByType[_itemType];
            long cost = (long)(baseItem.value * 3f);
            string costText = Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.CostShort") + " " + EnchantingWorkshopUI.FormatCoins(cost);
            Utils.DrawBorderString(spriteBatch, costText, new Vector2(dims.X + dims.Width - 120, dims.Y + 8), Color.Gold, 0.7f);
        }
    }
}


