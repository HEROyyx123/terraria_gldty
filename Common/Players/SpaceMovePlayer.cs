using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Players
{
    public class SpaceMovePlayer : ModPlayer
    {
        // 标记玩家当前帧是否拥有该 Buff
        public bool hasSpaceMoveBuff = false;

       // 记录上一帧右键状态
        private bool wasRightMouseDown = false;

        // 每帧开始时重置所有 Buff 标记，确保没有 Buff 时绝对不会触发
        public override void ResetEffects()
        {
            hasSpaceMoveBuff = false;
        }

        // 当玩家退出世界或 Mod 被卸载时重置状态，防止卸载后残留
        public override void Unload()
        {
            wasRightMouseDown = false;
        }

        public override void PostUpdate()
        {
            // // 只在拥有 Buff 且打开了全屏大地图 (Main.mapFullscreen) 时生效
            // if (!hasSpaceMoveBuff)
            //     return;
            // if (!Main.mapFullscreen)
            //     return;

            // // 检测右键点击按下瞬间（避免按住不放连续传送）
            // bool isRightMouseDown = Main.mouseRight;
            // if (isRightMouseDown && !wasRightMouseDown)
            // {
            //     //OnMapRightClickTeleport();
            // }
            // wasRightMouseDown = isRightMouseDown;
            //*******************************************************
            // 核心防护 1：只有当前玩家拥有 Buff 且打开了全屏大地图时，才继续执行
            if (!hasSpaceMoveBuff || !Main.mapFullscreen)
            {
                // 没 Buff 时顺便重置按键状态，防止下次获得 Buff 时误触发
                wasRightMouseDown = Main.mouseRight;
                return;
            }

            // 获取当前帧右键状态
            bool isRightMouseDown = Main.mouseRight;

            // 核心防护 2：检测右键“刚按下”的瞬间（防止长按连续传送）
            if (isRightMouseDown && !wasRightMouseDown)
            {
                OnMapRightClickTeleport();
            }

            // 更新上一帧状态
            wasRightMouseDown = isRightMouseDown;

        }

        private void OnMapRightClickTeleport()
        { 
            // 双重保险：再次校验 Buff
            if (!hasSpaceMoveBuff) return;

            // 1. 获取屏幕中心点（像素坐标）
            Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

            // 2. 计算鼠标相对于屏幕中心的偏移量（像素）
            Vector2 mouseOffset = Main.MouseScreen - screenCenter;

            // 3. 将屏幕像素偏移量转换为大地图上的 Tile 偏移量
            // 关键修正：Main.mapFullscreenScale 是像素/Tile的比率，必须正确映射
            Vector2 tileOffset = mouseOffset / Main.mapFullscreenScale;

            // 4. 计算地图点击位置的目标 Tile 坐标 (Main.mapFullscreenPos 是大地图中心对应的 Tile 坐标)
            Vector2 targetTilePos = Main.mapFullscreenPos + tileOffset;

            // 5. 将 Tile 坐标转换为像素坐标（1 Tile = 16 像素）
            // 减去玩家宽高的一半，使传送后玩家的中心（而不是左上角）落在鼠标点击处
            Vector2 targetWorldPos = targetTilePos * 16f - new Vector2(Player.width * 0.5f, Player.height * 0.5f);

            // 6. 限制在世界边界内（防止传送到世界地图之外导致报错或卡死）
            float minX = 16f * 10;
            float maxX = 16f * (Main.maxTilesX - 10);
            float minY = 16f * 10;
            float maxY = 16f * (Main.maxTilesY - 10);

            targetWorldPos.X = MathHelper.Clamp(targetWorldPos.X, minX, maxX);
            targetWorldPos.Y = MathHelper.Clamp(targetWorldPos.Y, minY, maxY);

            // 起始位置特效
            TeleportEffects(Player.Center);

            // 【修改处】：使用Style = 1 (混沌之杖/黑子式的干脆传送) 或 Style = 0 (普通传送)
            Player.Teleport(targetWorldPos, 1);
            CombatText.NewText(Player.getRect(), Color.Purple, "传送成功", true);

            // 目标位置特效
            TeleportEffects(Player.Center);

            // 防止在地图上右键同时触发了其他物品的使用
            Player.releaseUseItem = false;
        }

        private void TeleportEffects(Vector2 pos)
        {
            // 生成粉紫色/金色粒子，模拟空间移动的微光的维度撕裂感
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(pos - new Vector2(16, 16), 32, 32, Terraria.ID.DustID.PinkFairy, 0f, 0f, 100, default, 1.5f);
                dust.velocity *= 2f;
                dust.noGravity = true;
            }
        }
    }
}