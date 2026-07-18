using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace terraria_gldty.Content.Tiles
{
    public class EnchantingWorkshopTile : ModTile
    {
        public override void SetStaticDefaults() {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.IgnoredByNpcStepUp[Type] = true;

            // 作为工匠作坊的替代工作台
            AdjTiles = [TileID.TinkerersWorkbench];

            DustType = DustID.MagicMirror;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(80, 100, 180), name);
        }

        public override void NumDust(int x, int y, bool fail, ref int num) {
            num = fail ? 1 : 3;
        }

        public override void MouseOver(int i, int j) {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<Items.EnchantingWorkshop>();
        }

        public override bool RightClick(int i, int j) {
            Player player = Main.LocalPlayer;
            if (player == null) return false;

            // 打开附魔作坊 UI
            if (Main.netMode != NetmodeID.Server) {
                Common.UI.EnchantingWorkshopUI.EnchantingWorkshopUISystem.Instance?.ShowUI();
            }
            return true;
        }
    }
}
