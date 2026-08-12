using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Buff;

namespace terraria_gldty.Content.Items.Weapons
{
    public class LuminiteMinionProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarBar;

        public override void SetStaticDefaults() {
            // 1.4.4 正确标记召唤物可被替代/挤掉的 Set 属性
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            // 支持玩家用权杖右键标记敌人优先攻击
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            // 开启拖尾记录
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults() {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.scale = 1.2f;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;           // 属于仆从
            Projectile.minionSlots = 1f;        // 占用 1 个召唤栏位
            Projectile.penetrate = -1;          // 无限穿透
            Projectile.tileCollide = false;     // 穿墙追击敌人
            Projectile.ignoreWater = true;


            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8; // 每个召唤物独立计算 8 帧 CD
        }

        public override bool? CanCutTiles() => false;

        public override void AI() {

            Player player = Main.player[Projectile.owner];

            // --- 0. 同类召唤物自动排斥（防止视觉和轨迹完全重叠） ---
            float overlapRadius = 40f; // 排斥触发半径
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                // 筛选出属于同一个玩家、同一个类型的其他召唤物
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && other.type == Projectile.type)
                {
                    float distance = Vector2.Distance(Projectile.Center, other.Center);
                    if (distance < overlapRadius)
                    {
                        // 计算互相排斥的方向
                        Vector2 pushAway = Projectile.Center - other.Center;
                        if (pushAway == Vector2.Zero)
                        {
                            // 完全重叠时随机给一个推力方向
                            pushAway = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                        }
                        pushAway.Normalize();
                        // 施加微小推力，让它们自然散开
                        Projectile.velocity += pushAway * 0.6f;
                    }
                }
            }

            // --- 1. 检查召唤物存活状态与 Buff 绑定 ---
            if (player.dead || !player.active) {
                player.ClearBuff(ModContent.BuffType<LuminiteMinionBuff>());
            }

            if (player.HasBuff(ModContent.BuffType<LuminiteMinionBuff>())) {
                Projectile.timeLeft = 2; // 只要有 Buff 就保持存活
            }

            // --- 2. 寻找目标 ---
            NPC targetNPC = null;
            float maxDistance = 900f;

            // 检查右键手动标记目标
            if (player.HasMinionAttackTargetNPC) {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < maxDistance) {
                    targetNPC = npc;
                }
            }

            // 自动寻找屏幕范围内最近的有效敌人
            if (targetNPC == null) {
                float closestDist = maxDistance;
                for (int i = 0; i < Main.maxNPCs; i++) {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy()) {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist) {
                            closestDist = dist;
                            targetNPC = npc;
                        }
                    }
                }
            }

            // --- 3. 行为逻辑 ---
            if (targetNPC != null) {
                // 【攻击模式】：朝敌人发动穿透俯冲攻击
                Vector2 targetDir = targetNPC.Center - Projectile.Center;
                float distance = targetDir.Length();
                targetDir.Normalize();

                float speed = MathHelper.Clamp(distance * 0.15f, 16f, 28f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * speed, 0.12f);

                // 发射冲击粒子
                if (Main.rand.NextBool(3)) {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 100, default, 1.2f);
                    d.noGravity = true;
                    d.velocity = -Projectile.velocity * 0.2f;
                }
            }
            else {
                // 【待机模式】：像守护灵一样悬浮在玩家身边
                Projectile.ai[0] += 0.05f;

                float idleAngle = Projectile.ai[0] + Projectile.identity * 1.5f;
                Vector2 idleOffset = new Vector2((float)Math.Cos(idleAngle) * 60f, -80f + (float)Math.Sin(idleAngle) * 20f);
                Vector2 targetIdlePos = player.Center + idleOffset;

                Vector2 toIdle = targetIdlePos - Projectile.Center;
                float distToIdle = toIdle.Length();

                if (distToIdle > 1200f) {
                    Projectile.Center = player.Center;
                }

                float speed = MathHelper.Clamp(distToIdle * 0.1f, 8f, 22f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * speed, 0.1f);
            }

            // --- 4. 旋转与发光 ---
            if (Projectile.velocity != Vector2.Zero) {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 0.9f, 0.75f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);

            for (int i = 0; i < 6; i++) {
                Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Vortex, speed.X, speed.Y, 100, default, 1.3f);
                d.noGravity = true;
            }
        }

        // --- 5. 自定义绘制 ---
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            
            Color luminiteTeal = new Color(0, 255, 200, 0);

            // 残影
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float trailFactor = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                Color trailColor = luminiteTeal * trailFactor * 0.5f;
                float oldRot = Projectile.oldRot[k];
                
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, oldRot, drawOrigin, Projectile.scale * (0.85f + 0.15f * trailFactor), SpriteEffects.None, 0);
            }

            // 外轮廓
            Vector2 currentDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color glowColor = luminiteTeal * 0.8f;
            
            for (int i = 0; i < 4; i++) {
                Vector2 offset = new Vector2(2.5f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, currentDrawPos + offset, null, glowColor, Projectile.rotation, drawOrigin, Projectile.scale * 1.06f, SpriteEffects.None, 0);
            }

            // 主体
            Main.EntitySpriteDraw(texture, currentDrawPos, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}