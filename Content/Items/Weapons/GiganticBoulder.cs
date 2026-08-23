using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;


namespace terraria_gldty.Content.Items.Weapons
{
    public class GiganticBoulder : ModProjectile
    {
        // 告诉 tModLoader 直接使用原版的巨石贴图作为默认资源，不再自动寻找本地 PNG
        public override string Texture => "Terraria/Images/Projectile_" + Terraria.ID.ProjectileID.Boulder;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            // 判定体设为 64x64（碰撞体不宜过大，防止提前被吊顶挡住）
            Projectile.width = 64;
            Projectile.height = 64;

            // 核心设置：无伤害、纯贴图视觉效果
            Projectile.damage = 0;
            Projectile.friendly = false;
            Projectile.hostile = false; // 不伤害玩家
            
            // 开启地形碰撞，触地即灭
            Projectile.tileCollide = true; 
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600; // 最长存在时间（10秒）
        }

        public override void AI()
        {
            // 1. 随速度旋转
            Projectile.rotation += Projectile.velocity.X * 0.03f + Projectile.velocity.Y * 0.01f;

            // 2. 模拟下落重力
            Projectile.velocity.Y += 0.3f;
            if (Projectile.velocity.Y > 28f)
            {
                Projectile.velocity.Y = 28f;
            }
        }

        // 砸地消亡时的效果（震屏、音效、粒子）
        public override void OnKill(int timeLeft)
        {
            // 1. 播放高强度重击音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);

            // 2. 震屏效果（通过给本地玩家施加视角抖动）
            if (Main.netMode != NetmodeID.Server)
            {
                Player localPlayer = Main.LocalPlayer;
                float distance = Vector2.Distance(localPlayer.Center, Projectile.Center);
                
                // 距离越近震感越强，最大视野范围内触发
                if (distance < 2000f)
                {
                    float intensity = (1f - (distance / 2000f)) * 25f; // 最大抖动幅度 25
                    CameraShakePlayer shakePlayer = localPlayer.GetModPlayer<CameraShakePlayer>();
                    shakePlayer.Shake = intensity;
                }
            }

            // 3. 产生大量巨石碎屑与烟雾粒子
            for (int i = 0; i < 60; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, dustVel.X, dustVel.Y, 0, default, Main.rand.NextFloat(2f, 3.5f));
                d.noGravity = Main.rand.NextBool();
            }

            // 4. 冲击波烟雾粒子
            for (int i = 0; i < 30; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(8f, 4f) - new Vector2(0, 3f);
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, smokeVel.X, smokeVel.Y, 100, default, Main.rand.NextFloat(2.5f, 4f));
                smoke.noGravity = true;
            }
        }

        // 绘制逻辑：借用原版巨石贴图并放大 10 倍
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[ProjectileID.Boulder].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float scale = 10f; // 放大 10 倍

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                lightColor,
                Projectile.rotation,
                drawOrigin,
                scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }

    // 辅助类：处理震屏逻辑
    public class CameraShakePlayer : ModPlayer
    {
        public float Shake = 0f;

        public override void ModifyScreenPosition()
        {
            if (Shake > 0f)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(Shake, Shake);
                Shake -= 0.8f; // 震动衰减速度
                if (Shake < 0f) Shake = 0f;
            }
        }
    }
}