using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using terraria_gldty.Common.UI.FargoSoulsUI;

namespace terraria_gldty.Content.Tiles
{
    public class ResonanceTransmuterTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 100, 200), CreateMapEntryName());
            DustType = DustID.PurpleCrystalShard;
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Main.mouseRightRelease = false;

            // 切换 UI 显示状态
            if (UISystem.Instance.TransmuterUserInterface.CurrentState == null)
            {
                UISystem.Instance.OpenUI();
            }
            else
            {
                UISystem.Instance.CloseUI();
            }

            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<Items.ResonanceTransmuterItem>();
        }
    }
}