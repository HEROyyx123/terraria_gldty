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

       private int storedItemType;
       private int storedItemStack;
        private int storedItemPrefix;
        // private bool _dragging;
        // private Vector2 _dragOffset;
   
        private bool _dragging;
        private Vector2 _dragOffset;

        public override void OnInitialize() {
            panel = new UIPanel();
            panel.SetPadding(12);
            panel.Width.Set(480, 0f);
            panel.Height.Set(500, 0f);
            panel.Left.Set(Main.screenWidth / 2 - 240, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 250, 0f);
            panel.BackgroundColor = new Color(30, 30, 50, 230);
            panel.BorderColor = new Color(100, 80, 180, 255);
            Append(panel);

            var titleText = new UIText(Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.DisplayName"), 1.1f);
            titleText.Left.Set(10, 0f);
            titleText.Top.Set(8, 0f);
            titleText.TextColor = Color.LightSkyBlue;
            panel.Append(titleText);

            closeButton = new UIImageButton(ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonDelete"));
            closeButton.Width.Set(18, 0f);
            closeButton.Height.Set(18, 0f);
            closeButton.Left.Set(450, 0f);
            closeButton.Top.Set(8, 0f);
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
            statusText.Top.Set(75, 0f);
            statusText.TextColor = Color.Gold;
            panel.Append(statusText);

           scrollbar = new UIScrollbar();
           scrollbar.Height.Set(340, 0f);
            scrollbar.Left.Set(455, 0f);
            scrollbar.Top.Set(110, 0f);
            panel.Append(scrollbar);

            prefixList = new UIList();
            prefixList.Height.Set(340, 0f);
            prefixList.Width.Set(430, 0f);
            prefixList.Left.Set(10, 0f);
            prefixList.Top.Set(110, 0f);
            prefixList.SetScrollbar(scrollbar);
            panel.Append(prefixList);
        }

        public void OpenUI() {
           slot.StoredType = storedItemType;
           slot.StoredStack = storedItemStack;
            slot.StoredPrefix = storedItemPrefix;
            UpdatePrefixList();
            panel.Left.Set(Main.screenWidth / 2 - 240, 0f);
            panel.Top.Set(Main.screenHeight / 2 - 250, 0f);
        }

       private void CloseUI() {
            if (Main.LocalPlayer != null && storedItemType > 0 && !Main.LocalPlayer.dead) {
               Item item = new Item();
               item.SetDefaults(storedItemType);
               item.stack = storedItemStack;
                item.Prefix(storedItemPrefix);
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), item);
            }
           storedItemType = 0;
           storedItemStack = 0;
            storedItemPrefix = 0;
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

            //*************************************
            
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
            //*************************************


            if (slot.StoredType != storedItemType) {
               storedItemType = slot.StoredType;
               storedItemStack = slot.StoredStack;
                storedItemPrefix = slot.StoredPrefix;
                UpdatePrefixList();
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

            storedItemType = 0;
           storedItemStack = 0;
           slot.StoredType = 0;
           slot.StoredStack = 0;
           slot.StoredPrefix = 0;

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
        }

        private bool TryRemoveCoins(Player player, long amount) {
            if (amount <= 0) return true;
            long totalCoins = 0;
            for (int i = 0; i < 58; i++) {
                Item item = player.inventory[i];
                if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin) {
                    totalCoins += (long)item.stack * item.value;
                }
            }
            if (totalCoins < amount) return false;

            long remaining = amount;
            int[] coinTypes = { ItemID.PlatinumCoin, ItemID.GoldCoin, ItemID.SilverCoin, ItemID.CopperCoin };
            long[] coinValues = { 1000000, 10000, 100, 1 };

            for (int t = 0; t < 4 && remaining > 0; t++) {
                for (int i = 0; i < 58 && remaining > 0; i++) {
                    Item item = player.inventory[i];
                    if (item.type == coinTypes[t]) {
                        long toRemove = Math.Min(item.stack, remaining / coinValues[t]);
                        if (toRemove > 0) {
                            item.stack -= (int)toRemove;
                            remaining -= toRemove * coinValues[t];
                            if (item.stack <= 0) item.TurnToAir();
                        }
                    }
                }
            }
            return true;
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
                Main.mouseItem = item;
               StoredType = 0;
               StoredStack = 0;
                StoredPrefix = 0;
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (StoredType <= 0 && !cursorItem.IsAir) {
                if (ContentSamples.ItemsByType.TryGetValue(cursorItem.type, out Item testItem) && !testItem.IsAir) {
                    StoredType = cursorItem.type;
                    StoredStack = cursorItem.stack;
                    StoredPrefix = cursorItem.prefix;
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
            }
        }
    }

    internal class PrefixEntry : UIPanel
    {
        private readonly int _itemType;
        private readonly int _prefixId;

        public PrefixEntry(int itemType, int prefixId) {
            _itemType = itemType;
            _prefixId = prefixId;

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

            string prefixName = Lang.prefix[_prefixId].Value;
            Utils.DrawBorderString(spriteBatch, prefixName, new Vector2(dims.X + 8, dims.Y + 8), Color.White, 0.8f);

            Item baseItem = ContentSamples.ItemsByType[_itemType];
            long cost = (long)(baseItem.value * 3f);
            string costText = Language.GetTextValue("Mods.terraria_gldty.EnchantingWorkshop.CostShort") + " " + EnchantingWorkshopUI.FormatCoins(cost);
            Utils.DrawBorderString(spriteBatch, costText, new Vector2(dims.X + dims.Width - 120, dims.Y + 8), Color.Gold, 0.7f);
        }
    }
}
