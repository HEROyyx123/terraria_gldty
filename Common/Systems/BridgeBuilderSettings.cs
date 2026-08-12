using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    public enum TileShape
    {
        Flat,       
        HalfBlock,  
        SlopeLeft,  
        SlopeRight  
    }

    public enum BuildDirection
    {
        Right = 1,
        Left = -1,
        Both = 0
    }

    public struct TileSnapshot
    {
        public int X;
        public int Y;
        public bool HasTile;
        public ushort TileType;
        public byte Slope;
        public bool IsHalfBlock;
        public short TileFrameX;
        public short TileFrameY;

        public static TileSnapshot Capture(int x, int y)
        {
            Tile t = Main.tile[x, y];
            return new TileSnapshot
            {
                X = x,
                Y = y,
                HasTile = t.HasTile,
                TileType = t.TileType,
                Slope = (byte)t.Slope,
                IsHalfBlock = t.IsHalfBlock,
                TileFrameX = t.TileFrameX,
                TileFrameY = t.TileFrameY
            };
        }

        public void Restore()
        {
            Tile t = Main.tile[X, Y];
            t.HasTile = HasTile;
            t.TileType = TileType;
            t.Slope = (SlopeType)Slope;
            t.IsHalfBlock = IsHalfBlock;
            t.TileFrameX = TileFrameX;
            t.TileFrameY = TileFrameY;
        }
    }

    public static class BridgeBuilderSettings
    {
        public static Item PlatformItem = new Item();
        public static Item LightItem = new Item();

        public static int Length = 100;
        public static int LightSpacing = 10;
        public static TileShape Shape = TileShape.Flat;
        public static BuildDirection Direction = BuildDirection.Right;
        public static bool ShowPreview = true;

        public static int ClearUp = 0;   
        public static int ClearDown = 0; 

        public static List<TileSnapshot> LastBuildHistory = new List<TileSnapshot>();
        private static HashSet<Point> _recordedTiles = new HashSet<Point>();

        public static int DynamicStep
        {
            get
            {
                if (Main.maxTilesX >= 8000) return 300;
                if (Main.maxTilesX >= 6000) return 150;
                return 50;
            }
        }

        public static bool IsTorch(int lightTile)
        {
            if (lightTile < 0) return true;
            return TileID.Sets.Torch[lightTile] || lightTile == TileID.Torches;
        }

        public static void ResetToDefaults()
        {
            Length = 100;
            LightSpacing = 10;
            Shape = TileShape.Flat;
            Direction = BuildDirection.Right;
            ShowPreview = true;
            ClearUp = 0;    
            ClearDown = 0;
        }

        private static void RecordTileBeforeChange(int x, int y)
        {
            Point p = new Point(x, y);
            if (!_recordedTiles.Contains(p))
            {
                _recordedTiles.Add(p);
                LastBuildHistory.Add(TileSnapshot.Capture(x, y));
            }
        }

        public static void ExecuteBuild(int startX, int startY)
        {
            if (PlatformItem.IsAir || PlatformItem.createTile < TileID.Dirt)
            {
                Main.NewText("请先在 UI 的【平台/方块槽位】中放入目标物品！", Color.Red);
                return;
            }

            LastBuildHistory.Clear();
            _recordedTiles.Clear();

            int tileType = PlatformItem.createTile;
            int tileStyle = PlatformItem.placeStyle;
            int lightTile = LightItem.IsAir ? -1 : LightItem.createTile;
            int lightStyle = LightItem.IsAir ? 0 : LightItem.placeStyle;

            if (Direction == BuildDirection.Both)
            {
                BuildOneDirection(startX, startY, 1, Length / 2, tileType, tileStyle, lightTile, lightStyle);
                BuildOneDirection(startX, startY, -1, Length / 2, tileType, tileStyle, lightTile, lightStyle);
            }
            else
            {
                BuildOneDirection(startX, startY, (int)Direction, Length, tileType, tileStyle, lightTile, lightStyle);
            }

            _recordedTiles.Clear();
        }

        private static void BuildOneDirection(int startX, int startY, int dir, int length, int tileType, int tileStyle, int lightTile, int lightStyle)
        {
            int minX = startX;
            int maxX = startX;
            int minY = startY - ClearUp;
            int maxY = startY + ClearDown;

            // 第一阶段：高效清除物块 + 录入快照 + 放置平台
            for (int i = 0; i < length; i++)
            {
                int currentX = startX + (i * dir);
                int currentY = startY;

                if (currentX < 10 || currentX >= Main.maxTilesX - 10) break;

                minX = System.Math.Min(minX, currentX);
                maxX = System.Math.Max(maxX, currentX);

                // 1. 高速清理指定的上下范围物块（直写内存，无粒子、无掉落物、无延迟）
                for (int dy = -ClearUp; dy <= ClearDown; dy++)
                {
                    if (dy == 0) continue; 

                    int targetY = currentY + dy;
                    if (targetY < 10 || targetY >= Main.maxTilesY - 10) continue;

                    Tile destroyTile = Main.tile[currentX, targetY];
                    if (destroyTile.HasTile)
                    {
                        RecordTileBeforeChange(currentX, targetY);
                        
                        // 【核心优化点】：直接清空内存中的数据，避免触发 WorldGen.KillTile 的极高开销
                        destroyTile.ClearTile(); 
                    }
                }

                // 2. 记录平台原始数据
                RecordTileBeforeChange(currentX, currentY);

                // 3. 铺设平整平台
                WorldGen.PlaceTile(currentX, currentY, tileType, mute: true, forced: true, style: tileStyle);
                Tile pTile = Main.tile[currentX, currentY];
                if (pTile.HasTile)
                {
                    pTile.Slope = SlopeType.Solid;
                    pTile.IsHalfBlock = false;
                }

                // 4. 光源判断与记录
                if (lightTile >= 0 && LightSpacing > 0 && i % LightSpacing == 0 && i > 0)
                {
                    bool isTorch = IsTorch(lightTile);
                    if (isTorch)
                    {
                        RecordTileBeforeChange(currentX, currentY - 1);
                    }
                    else
                    {
                        RecordTileBeforeChange(currentX, currentY + 1);
                        RecordTileBeforeChange(currentX, currentY + 2);
                    }

                    TryPlaceLightSource(currentX, currentY, lightTile, lightStyle);
                }
            }

            // 第二阶段：形态锤化
            if (Shape != TileShape.Flat)
            {
                for (int i = 0; i < length; i++)
                {
                    int currentX = startX + (i * dir);
                    int currentY = startY;

                    if (currentX < 10 || currentX >= Main.maxTilesX - 10) break;

                    Tile tile = Main.tile[currentX, currentY];
                    if (tile.HasTile)
                    {
                        switch (Shape)
                        {
                            case TileShape.HalfBlock:
                                tile.IsHalfBlock = true;
                                break;
                            case TileShape.SlopeLeft:
                                tile.Slope = SlopeType.SlopeDownLeft;
                                break;
                            case TileShape.SlopeRight:
                                tile.Slope = SlopeType.SlopeDownRight;
                                break;
                        }
                    }
                }
            }

            // 第三阶段：精细化贴图刷新（【优化点】：只刷边界和未空的实体方块，不重复刷内部空气）
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                for (int y = minY - 1; y <= maxY + 1; y++)
                {
                    if (x < 10 || x >= Main.maxTilesX - 10 || y < 10 || y >= Main.maxTilesY - 10) continue;

                    Tile t = Main.tile[x, y];
                    // 只对边界线上的方块或非空物块进行帧刷新
                    if (t.HasTile || x == minX - 1 || x == maxX + 1 || y == minY - 1 || y == maxY + 1)
                    {
                        WorldGen.SquareTileFrame(x, y, resetFrame: false);
                    }
                }
            }

            // 第四阶段：联机模式安全分包同步（防止大面积修改导致网络数据包溢出闪退）
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                int chunkSize = 30; // 30x30 为安全的同步分包上限
                for (int x = minX - 1; x <= maxX + 1; x += chunkSize)
                {
                    for (int y = minY - 1; y <= maxY + 1; y += chunkSize)
                    {
                        int w = System.Math.Min(chunkSize, maxX + 2 - x);
                        int h = System.Math.Min(chunkSize, maxY + 2 - y);
                        NetMessage.SendTileSquare(-1, x, y, w, h);
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Dig);
        }

        private static void TryPlaceLightSource(int x, int y, int lightTile, int style)
        {
            bool isTorch = IsTorch(lightTile);
            if (isTorch)
            {
                WorldGen.PlaceTile(x, y - 1, lightTile, mute: true, forced: true, style: style);
            }
            else
            {
                WorldGen.PlaceTile(x, y + 1, lightTile, mute: true, forced: true, style: style);
            }
        }

        public static void UndoLastBuild()
        {
            if (LastBuildHistory.Count == 0)
            {
                Main.NewText("当前没有可以撤销的搭建记录！", Color.Red);
                return;
            }

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var snap in LastBuildHistory)
            {
                snap.Restore();

                minX = System.Math.Min(minX, snap.X);
                maxX = System.Math.Max(maxX, snap.X);
                minY = System.Math.Min(minY, snap.Y);
                maxY = System.Math.Max(maxY, snap.Y);
            }

            // 撤销时的边界刷新优化
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                for (int y = minY - 1; y <= maxY + 1; y++)
                {
                    if (x >= 10 && x < Main.maxTilesX - 10 && y >= 10 && y < Main.maxTilesY - 10)
                    {
                        WorldGen.SquareTileFrame(x, y, true);
                    }
                }
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                int chunkSize = 30;
                for (int x = minX - 1; x <= maxX + 1; x += chunkSize)
                {
                    for (int y = minY - 1; y <= maxY + 1; y += chunkSize)
                    {
                        int w = System.Math.Min(chunkSize, maxX + 2 - x);
                        int h = System.Math.Min(chunkSize, maxY + 2 - y);
                        NetMessage.SendTileSquare(-1, x, y, w, h);
                    }
                }
            }

            LastBuildHistory.Clear();
            SoundEngine.PlaySound(SoundID.Item14);
            Main.NewText("已成功撤销上一次搭建！", Color.Green);
        }
    }
}