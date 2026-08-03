using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace terraria_gldty.Content.Items.Weapons
{
    public class BambooCicadaProjectile : ModProjectile
    {
        private int soundTimer = 0;
        private int fishBubbleTimer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 8f; // 滞空时间 8秒
            ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 288f;     // 线长 18格 (18 * 16px)
            ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 13f;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.Yoyo;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // 每隔一段时间（比如 30 帧/0.5秒）播放一次旋转微音，或者播放循环音效
            if (Main.rand.NextBool(30)) {
                SoundEngine.PlaySound(new SoundStyle("terraria_gldty/Assets/Sounds/BambooCicada"){
                             PitchVariance = 0.2f, // 每次播放时音调会有 ±20% 的随机微调，避免连续听起来机械单调
                             Volume = 0.8f         // 调整音量 (0.0 ~ 1.0)
                             }, Projectile.position);
            }

            // 1. 蝉鸣声波粒子效果
            soundTimer++;
            if (soundTimer % 12 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustOffset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(8f, 20f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Enchanted_Gold, -dustOffset * 0.1f, 100, default, 0.8f);
                    d.noGravity = true;
                }
            }

            // 2. 魔性鱼泡水花与群体困惑 (每 1.5 秒 / 90 帧)
            fishBubbleTimer++;
            if (fishBubbleTimer >= 90)
            {
                fishBubbleTimer = 0;

                // 播放咕噜/水花音效
                SoundEngine.PlaySound(SoundID.Splash, Projectile.Center);

                // 喷溅水花
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust bubble = Dust.NewDustPerfect(Projectile.Center, DustID.Water, vel, 100, Color.LightBlue, 1.2f);
                    bubble.noGravity = false;
                }

                // 环形鱼音洗脑：使周围敌人获得【困惑】Debuff
                float auraRadius = 120f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) <= auraRadius)
                    {
                        npc.AddBuff(BuffID.Confused, 120); // 困惑 2 秒
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中敌人时额外概率造成困惑
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Confused, 180);
            }
        }
    }
}
