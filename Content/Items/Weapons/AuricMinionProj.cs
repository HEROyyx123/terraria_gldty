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
    public class AuricMinionProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;

        public override void SetStaticDefaults() {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            // 1. 延长拖尾记录至 20 帧，增强极速残影
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults() {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.scale = 1.3f;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4; // 提升高频无敌帧打击感
        }

        public override bool? CanCutTiles() => false;

        public override void AI() {
            Player player = Main.player[Projectile.owner];

            // 0. 同类召唤物排斥
            float overlapRadius = 40f;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && other.type == Projectile.type) {
                    float distance = Vector2.Distance(Projectile.Center, other.Center);
                    if (distance < overlapRadius) {
                        Vector2 pushAway = Projectile.Center - other.Center;
                        if (pushAway == Vector2.Zero) {
                            pushAway = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                        }
                        pushAway.Normalize();
                        Projectile.velocity += pushAway * 0.8f;
                    }
                }
            }

            // 1. 存活判定
            if (player.dead || !player.active) {
                player.ClearBuff(ModContent.BuffType<AuricMinionBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<AuricMinionBuff>())) {
                Projectile.timeLeft = 2;
            }

            // 2. 目标搜寻
            NPC targetNPC = null;
            float maxDistance = 1200f;

            if (player.HasMinionAttackTargetNPC) {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < maxDistance) {
                    targetNPC = npc;
                }
            }

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

            // 3. 行为与特效逻辑
            if (targetNPC != null) {
                Vector2 targetDir = targetNPC.Center - Projectile.Center;
                float distance = targetDir.Length();
                targetDir.Normalize();

                float speed = MathHelper.Clamp(distance * 0.25f, 24f, 42f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * speed, 0.2f);

                // 【特效 1】：极速冲刺时的金光 + 蓝白电光尾迹
                for (int i = 0; i < 2; i++) {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
                    d.noGravity = true;
                    d.velocity = -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(2f, 2f);
                }

                if (Main.rand.NextBool(3)) {
                    Dust elec = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 0.8f);
                    elec.noGravity = true;
                }
            }
            else {
                Projectile.ai[0] += 0.08f;

                float idleAngle = Projectile.ai[0] + Projectile.identity * 1.5f;
                Vector2 idleOffset = new Vector2((float)Math.Cos(idleAngle) * 75f, -90f + (float)Math.Sin(idleAngle) * 25f);
                Vector2 targetIdlePos = player.Center + idleOffset;

                Vector2 toIdle = targetIdlePos - Projectile.Center;
                float distToIdle = toIdle.Length();

                if (distToIdle > 1400f) {
                    Projectile.Center = player.Center;
                }

                float speed = MathHelper.Clamp(distToIdle * 0.12f, 10f, 26f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * speed, 0.12f);

                // 待机时偶尔散发粒子
                if (Main.rand.NextBool(10)) {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 150, default, 1.0f);
                    d.noGravity = true;
                }
            }

            // 旋转与强光源
            if (Projectile.velocity != Vector2.Zero) {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }

            // 强烈金红双色动态发光
            Lighting.AddLight(Projectile.Center, 1.2f, 0.9f, 0.2f);
        }

        // 【特效 2】：命中时的爆发粒子与冲击波
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);

            // 爆炸式金光粒子环
            for (int i = 0; i < 16; i++) {
                Vector2 speed = Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GoldFlame, speed.X, speed.Y, 100, default, 1.8f);
                d.noGravity = true;
            }

            // 电火花飞溅
            for (int i = 0; i < 8; i++) {
                Vector2 speed = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, speed.X, speed.Y, 100, default, 1.0f);
                d.noGravity = true;
            }
        }

        // 【特效 3 & 4】：自定义高阶渲染（渐变残影 + 能量法阵 + 强光外发光）
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 currentDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 调色盘：从圣金黄渐变至灾厄红
            Color goldColor = new Color(255, 215, 0, 0);
            Color reddishColor = new Color(255, 60, 0, 0);

            // 1. 能量法阵/旋转发光底座
            float auraRotation = (float)Main.GlobalTimeWrappedHourly * 6f + Projectile.identity;
            float auraScale = Projectile.scale * (1.1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.1f);
            Color auraColor = goldColor * 0.4f;
            for (int i = 0; i < 4; i++) {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(auraRotation + i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, currentDrawPos + offset, null, auraColor, auraRotation, drawOrigin, auraScale, SpriteEffects.None, 0);
            }

            // 2. 渐变长残影 (渐变色：末端偏红，前端偏金)
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float trailFactor = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                
                // 颜色沿尾迹渐变
                Color trailColor = Color.Lerp(reddishColor, goldColor, trailFactor) * trailFactor * 0.7f;
                float oldRot = Projectile.oldRot[k];
                float scale = Projectile.scale * (0.7f + 0.3f * trailFactor);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, oldRot, drawOrigin, scale, SpriteEffects.None, 0);
            }

            // 3. 高亮边缘 Glow 轮廓
            for (int i = 0; i < 4; i++) {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, currentDrawPos + offset, null, goldColor * 0.8f, Projectile.rotation, drawOrigin, Projectile.scale * 1.08f, SpriteEffects.None, 0);
            }

            // 4. 弹幕实体本体
            Main.EntitySpriteDraw(texture, currentDrawPos, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}