using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID; // 引入 DustID
using Terraria.ModLoader;
using terraria_gldty.Configs;
namespace terraria_gldty.Common.Players
{
    public class CMoonPlayer : ModPlayer
    {
        public bool hasCMoonEffect;

        public override void ResetEffects() {
            hasCMoonEffect = false;
        }

        public override void PreUpdateMovement() {
            if (!hasCMoonEffect) return;

            // 1. 完全禁用垂直重力与垂直位移
            Player.gravity = 0f;
            // 2. 读取模组配置，判断是否开启“锁定垂直位移”
            if (ModContent.GetInstance<CMoonConfig>().DisableVerticalVelocity) {
                Player.velocity.Y = 0f; // 开启配置时，清空垂直速度
            }

            // 2. 确定水平坠落方向
            float fallDirection = Player.direction; 

            // 3. 模拟水平坠落加速度与最大下落速度
            float horizontalGravity = 0.4f; 
            float maxHorizontalSpeed = 10f; 

            Player.velocity.X += fallDirection * horizontalGravity;

            if (Math.Abs(Player.velocity.X) > maxHorizontalSpeed) {
                Player.velocity.X = fallDirection * maxHorizontalSpeed;
            }

            // // 4. 水平碰撞判定
            // if (Player.collideX) {
            //     Player.velocity.X = 0f;
            //     Player.fallStart = (int)(Player.position.Y / 16f);
            // }
        }

        // 需求 2：去掉跑动动作 (在绘制前强制固定帧)
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (hasCMoonEffect) {
                // --- 视觉效果：旋转贴图 ---
                // 头部朝前
                float rotationAngle = Player.direction == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                Player.fullRotation = rotationAngle;

                // 修改旋转中心，确保角色围绕身体中心旋转，而不是脚底
                // 玩家高度默认是 42，宽度 20。身体中心大约在 (10, 21)
                Player.fullRotationOrigin = new Vector2(Player.width / 2f, Player.height / 2f);

                // --- 视觉效果：禁用跑动动画 ---
                // 泰拉瑞亚默认身体帧：0是站立，1-2是举手，3-4是跳跃/浮空，6-19是跑步循环
                // 我们将身体帧固定在 4（跳跃/浮空帧），看起来更像被重力控制
                Player.bodyFrame.Y = Player.bodyFrame.Height * 4; 
                Player.legFrame.Y = Player.legFrame.Height * 4; // 腿也固定
            }
            else {
                // 复位：没有 Buff 时强制重置
                Player.fullRotation = 0f;
            }
        }

        // 需求 1：加上坠落特效
        public override void PostUpdate() {
            // 只有当 Buff 激活且玩家在移动时才生成粒子
            if (hasCMoonEffect && Math.Abs(Player.velocity.X) > 1f) {
                // 在玩家身后（根据移动方向）生成粒子
                // 粒子类型推荐：
                // DustID.PurpleTorch (紫色，有 C-MOON 的感觉)
                // DustID.Demonite (暗紫色)
                // DustID.GravitationPot (重力药水原版粒子)
                
                // 玩家在左，粒子在右；玩家在右，粒子在左
                Vector2 trailPosition = Player.Center - Player.velocity * 0.5f; 

                // 随机生成 1-2 个粒子
                for (int i = 0; i < 2; i++) {
                    // 创建紫色粒子
                    Dust dust = Dust.NewDustDirect(
                        Player.position, // 位置
                        Player.width, // 宽度范围
                        Player.height, // 高度范围
                        DustID.PurpleTorch, // 粒子类型：紫色火把（可更换）
                        -Player.velocity.X * 0.2f, // X轴速度（反向，形成尾迹）
                        0f, // Y轴速度（锁死）
                        100, // 透明度 (0-255)
                        Color.MediumPurple, // 颜色（通常粒子自带颜色，这里起微调作用）
                        1.2f // 缩放
                    );

                    // 粒子细节调整
                    dust.noGravity = true; // 粒子不受重力影响
                    dust.velocity *= 0.5f; // 降低粒子扩散速度
                    dust.fadeIn = 0.5f; // 渐入
                }
            }
        }
    }
}

//旧版
// using Microsoft.Xna.Framework;
// using System;
// using Terraria;
// using Terraria.DataStructures;
// using Terraria.ModLoader;

// namespace terraria_gldty.Common.Players
// {
//     public class CMoonPlayer : ModPlayer
//     {
//         public bool hasCMoonEffect;

//         public override void ResetEffects() {
//             hasCMoonEffect = false;
//         }

//         public override void PreUpdateMovement() {
//             if (!hasCMoonEffect) return;

//             // 1. 完全禁用垂直重力与垂直位移
//             Player.gravity = 0f;
//             //Player.velocity.Y = 0f; // 清空垂直下落速度，防止受垂直重力惯性影响

//             // 2. 确定水平坠落方向（1 表示右，-1 表示左）
//             float fallDirection = Player.direction; 

//             // 3. 模拟水平坠落加速度与最大下落速度
//             float horizontalGravity = 10f; // 水平重力加速度0.4f
//             float maxHorizontalSpeed = 10f; // 最大水平坠落速度

//             // 施加水平加速度
//             Player.velocity.X += fallDirection * horizontalGravity;

//             // 限制最大水平速度
//             if (Math.Abs(Player.velocity.X) > maxHorizontalSpeed) {
//                 Player.velocity.X = fallDirection * maxHorizontalSpeed;
//             }

//             // 4. 水平碰撞判定（1.4.4 中使用 isCollidingHorizontally）
//             // if (Player.isCollidingHorizontally) {
//             //     // 撞墙后清空水平速度，模拟“落地/撞墙”
//                 // Player.velocity.X = 0f;
//                 // 重置多段跳等下落状态
//                 // Player.fallStart = (int)(Player.position.Y / 16f);
//             // }
//         }

//         // 修改绘制属性
//         public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
//             if (hasCMoonEffect) {
//                 // 视觉效果：有 Buff 时将玩家贴图旋转 90 度
//                 float rotationAngle = Player.direction == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2;

//                 Player.fullRotation = rotationAngle;
//                 // 设置旋转中心为玩家贴图中心
//                 Player.fullRotationOrigin = new Vector2(Player.width / 2f, Player.height / 2f);
//             }
//             else {
//                 // 复位：没有 Buff 时强制重置贴图角度为 0，恢复正常姿态
//                 Player.fullRotation = 0f;
//             }
//         }
//     }
// }