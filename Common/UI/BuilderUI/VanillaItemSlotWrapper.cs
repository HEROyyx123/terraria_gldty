using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.GameInput;

namespace terraria_gldty.Common.UI.BuilderUI
{
    public class VanillaItemSlotWrapper : UIElement
    {
        public Item Item;
        private readonly int _context;
        private readonly float _scale;

        public VanillaItemSlotWrapper(int context = ItemSlot.Context.BankItem, float scale = 1f)
        {
            _context = context;
            _scale = scale;
            Item = new Item();
            Item.SetDefaults(ItemID.None);

            Width.Set(TextureAssets.InventoryBack.Width() * scale, 0);
            Height.Set(TextureAssets.InventoryBack.Height() * scale, 0);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = _scale;
            Rectangle rectangle = GetDimensions().ToRectangle();

            if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
            {
                Main.LocalPlayer.mouseInterface = true;
                ItemSlot.Handle(ref Item, _context);
            }

            ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft());
            Main.inventoryScale = oldScale;
        }
    }
}