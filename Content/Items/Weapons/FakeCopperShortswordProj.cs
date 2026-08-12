using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items.Weapons
{
    public class FakeCopperShortswordProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.HallowedBar;

        public override void SetDefaults() {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.scale = 1.2f; 

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 120; // 存活时间

            // 开启旧位置记录，用于绘制残影拖尾（精简掉了原来的重复代码）
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void AI() {
            Player player = Main.player[Projectile.owner];

            // 如果玩家死亡，销毁弹幕
            if (!player.active || player.dead) {
                Projectile.Kill();
                return;
            }

            // --- 1. 初始化：为当前飞剑分配专属的弯曲程度与方向 ---
            if (Projectile.localAI[0] == 0f) {
                Projectile.localAI[0] = 1f; // 标记已初始化

                // 随机生成弧度大小（例如 30 到 60 像素之间）
                float randomAmplitude = Main.rand.NextFloat(30f, 60f);
                
                // 随机决定偏转方向（+1 为右/ -1 为左）
                float randomDirection = Main.rand.NextBool() ? 1f : -1f;

                // 存储到 localAI[1] 中，作为该弹幕的专属弯曲系数
                Projectile.localAI[1] = randomAmplitude * randomDirection;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }

            // --- 2. 动态轨迹计算 ---
            Projectile.ai[0] += 1f;
            float progress = Projectile.ai[0] / 30f; // 30帧为一个完整周期

            Vector2 targetPos = Main.MouseWorld;
            Vector2 origin = player.Center;
            
            // 读取之前生成的专属弯曲系数
            float curveAmplitude = Projectile.localAI[1];
            
            // 计算正弦波弧度偏移
            float swingOffset = MathF.Sin(progress * MathHelper.Pi) * curveAmplitude;
            
            Vector2 dirToTarget = (targetPos - origin).SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = dirToTarget.RotatedBy(MathHelper.PiOver2);

            // 最终坐标 = 线性插值位置 + 垂直偏移（形成左右弯曲的弧线）
            Vector2 currentPos = Vector2.Lerp(origin, targetPos, progress) + perpendicular * swingOffset;
            Projectile.Center = currentPos;

            // 剑尖指向运动方向
            Vector2 velocityDir = currentPos - Projectile.oldPos[0];
            if (velocityDir != Vector2.Zero) {
                Projectile.rotation = velocityDir.ToRotation() + MathHelper.PiOver4;
            }

            // 超出时间自动销毁
            if (Projectile.ai[0] >= 30f) {
                Projectile.Kill();
            }

            // --- 3. 特效粒子（Dust） ---
            if (Main.rand.NextBool(2)) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CopperCoin, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(4)) {
                Dust spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Gold, 0f, 0f, 150, Color.Cyan, 0.8f);
                spark.noGravity = true;
            }
        }

        // --- 重写绘制：绘制发光外轮廓与残影 ---
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            
            // 1. 绘制残影 (Afterimage)
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.Orange * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
                float oldRot = Projectile.oldRot[k];
                
                Main.EntitySpriteDraw(texture, drawPos, null, color, oldRot, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            // 2. 绘制发光外轮廓 (Glow Outline)
            Color glowColor = new Color(255, 200, 80, 0) * 0.6f;
            Vector2 currentDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            
            for (int i = 0; i < 4; i++) {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, currentDrawPos + offset, null, glowColor, Projectile.rotation, drawOrigin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
            }

            // 3. 绘制主体
            Main.EntitySpriteDraw(texture, currentDrawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}