using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Items;

namespace terraria_gldty.Common.Systems
{
    public class TamedGlobalNPC : GlobalNPC
    {
        public bool isTamed;
        public int ownerPlayerID = -1;
        public int minionSlotCost = 1;

        private int contactAttackCooldown = 0;
        public int finalDamage = 0;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc) {
            if (isTamed) {
                npc.friendly = true;
            }
        }

        public override bool PreAI(NPC npc)
        {
            // 1. 自动驯服蠕虫类怪物的身体与尾部
            if (!isTamed && (npc.aiStyle == NPCAIStyleID.Worm || npc.realLife > -1))
            {
                int headIndex = (npc.realLife > -1) ? npc.realLife : (int)npc.ai[3];
                if (headIndex >= 0 && headIndex < Main.maxNPCs)
                {
                    NPC headNPC = Main.npc[headIndex];
                    if (headNPC.active && headNPC.GetGlobalNPC<TamedGlobalNPC>().isTamed)
                    {
                        isTamed = true;
                        ownerPlayerID = headNPC.GetGlobalNPC<TamedGlobalNPC>().ownerPlayerID;
                        minionSlotCost = 0;
                        npc.friendly = true;
                        npc.dontTakeDamage = headNPC.dontTakeDamage;
                        npc.life = headNPC.life;
                        npc.netUpdate = true;
                    }
                }
            }

            if (!isTamed) return base.PreAI(npc);

            // 【新增关键修复】：防止沙虫/千足蜈蚣等地形依赖型蠕虫在空中秒死
            if (npc.aiStyle == NPCAIStyleID.Worm || npc.realLife >= 0)
            {
                npc.noTileCollide = true; // 允许穿墙，防止沙虫因不在沙子/泥土里而自毁
                npc.noGravity = true;     // 赋予空中飞行能力
            }

            if (ownerPlayerID < 0 || ownerPlayerID >= Main.maxPlayers)
            {
                npc.active = false;
                return false;
            }

            Player owner = Main.player[ownerPlayerID];
            if (!owner.active || owner.dead)
            {
                npc.active = false;
                return false;
            }

            npc.target = owner.whoAmI;

            if (contactAttackCooldown > 0) contactAttackCooldown--;


            // 右键收回机制[cite: 5]
            // 2. 右键收回机制
            // 【修改】：改为判定鼠标位置距离怪物中心 80 像素以内，大幅提升点击容错率
            if (Main.mouseRight && Main.mouseRightRelease && Vector2.Distance(Main.MouseWorld, npc.Center) < 80f)
            {
                if (owner.HeldItem.type == ModContent.ItemType<SoulChain>())
                {
                    if (Vector2.Distance(owner.Center, npc.Center) < 150f)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.MagicMirror, 0, 0);
                        }
                        Main.NewText($"已收回 {npc.GivenOrTypeName}！", Color.Cyan);

                        if (npc.aiStyle == NPCAIStyleID.Worm || npc.realLife >= 0)
                        {
                            int headID = npc.realLife >= 0 ? npc.realLife : npc.whoAmI;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                NPC other = Main.npc[i];
                                if (other.active && (other.whoAmI == headID || other.realLife == headID || other.ai[3] == headID))
                                {
                                    other.active = false;
                                }
                            }
                        }

                        npc.active = false;
                        return false;
                    }
                }
            }

            // 距离过远自动传送[cite: 5]
            float distanceToOwner = Vector2.Distance(npc.Center, owner.Center);
            if (distanceToOwner > 2000f) {
                npc.Center = owner.Center;
                npc.velocity = Vector2.Zero;
                npc.netUpdate = true;
            }

            // 索敌逻辑[cite: 5]
            NPC targetNPC = null;
            float maxSearchDistance = 800f;
            float closestDistance = maxSearchDistance;

            if (owner.HasMinionAttackTargetNPC) {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && (!target.friendly || target.type == NPCID.TargetDummy)) {
                    targetNPC = target;
                    closestDistance = Vector2.Distance(npc.Center, targetNPC.Center);
                }
            }

            if (targetNPC == null) {
                for (int i = 0; i < Main.maxNPCs; i++) {
                    NPC target = Main.npc[i];
                    bool isEnemy = target.active && !target.friendly && target.whoAmI != npc.whoAmI && !target.dontTakeDamage;
                    bool isDummy = target.active && target.type == NPCID.TargetDummy;

                    if (isEnemy || isDummy) {
                        float dist = Vector2.Distance(npc.Center, target.Center);
                        if (dist < closestDistance) {
                            closestDistance = dist;
                            targetNPC = target;
                        }
                    }
                }
            }

            if (targetNPC != null)
            {
                npc.targetRect = targetNPC.Hitbox;

                int targetDir = targetNPC.Center.X >= npc.Center.X ? 1 : -1;
                npc.direction = targetDir;
                npc.spriteDirection = targetDir;

                if (npc.noGravity)
                {
                    if (Vector2.Distance(npc.Center, targetNPC.Center) > 30f)
                    {
                        Vector2 toTarget = (targetNPC.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        float maxSpeed = 7.0f;
                        float inertia = 0.08f;
                        npc.velocity = Vector2.Lerp(npc.velocity, toTarget * maxSpeed, inertia);
                    }
                    else
                    {
                        npc.velocity *= 0.95f;
                    }
                }
                else
                {
                    if (Vector2.Distance(npc.Center, targetNPC.Center) > 30f)
                    {
                        npc.velocity.X = MathHelper.Lerp(npc.velocity.X, targetDir * 5f, 0.1f);
                    }
                }

                if (contactAttackCooldown <= 0 && npc.Hitbox.Intersects(targetNPC.Hitbox))
                {
                    int baseDamage = npc.damage > 0 ? npc.damage : (npc.defDamage > 0 ? npc.defDamage : 20);

                    StatModifier summonModifier = owner.GetTotalDamage(DamageClass.Summon);
                    int finalDamage = (int)summonModifier.ApplyTo(baseDamage);

                    bool isCrit = Main.rand.NextBool((int)owner.GetTotalCritChance(DamageClass.Summon));

                    int actualDamageDone = (int)targetNPC.SimpleStrikeNPC(
                        damage: finalDamage,
                        hitDirection: targetDir,
                        crit: isCrit,
                        knockBack: 3f,
                        damageType: DamageClass.Summon,
                        damageVariation: true
                    );

                    if (actualDamageDone > 0)
                    {
                        owner.addDPS(actualDamageDone);
                    }

                    contactAttackCooldown = 20;
                }
            } 
            else {
                npc.targetRect = owner.Hitbox;

                if (distanceToOwner > 120f) {
                    int dir = owner.Center.X > npc.Center.X ? 1 : -1;
                    npc.direction = dir;
                    npc.spriteDirection = dir;

                    if (npc.noGravity) {
                        Vector2 toOwner = (owner.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        npc.velocity = Vector2.Lerp(npc.velocity, toOwner * 5f, 0.03f);
                    }
                }
            }

            return true;
        }

        // 头顶醒目标记绘制[cite: 5]
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (!isTamed || !npc.active) return;

            // 蠕虫类怪物只在头部节（Head）上绘制标记，避免身体每个节都画图标[cite: 5]
            if (npc.realLife >= 0 && npc.realLife != npc.whoAmI) return;

            // 1. 计算头顶浮动坐标
            Vector2 headTop = new Vector2(npc.Center.X, npc.position.Y) - screenPos;
            float bounce = (float)System.Math.Sin(Main.GameUpdateCount * 0.12f) * 4f;
            headTop.Y -= (22f + bounce); // 位于头顶上方 22 像素处浮动

            // 2. 呼吸灯颜色计算（青色到春绿色的动态发光效果）
            float pulse = (float)(System.Math.Sin(Main.GameUpdateCount * 0.15f) + 1f) * 0.5f;
            Color glowColor = Color.Lerp(new Color(50, 255, 150), new Color(0, 230, 255), pulse);

            // 3. 绘制【▼】指示箭头
            string arrowText = "▼";
            Vector2 arrowSize = FontAssets.MouseText.Value.MeasureString(arrowText);
            Vector2 arrowPos = headTop - arrowSize / 2f;

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                arrowText,
                arrowPos.X,
                arrowPos.Y,
                glowColor,
                Color.Black,
                Vector2.Zero,
                1.2f
            );

            // 4. 在箭头上方绘制【★】金色友方标记
            string starText = "★";
            Vector2 starSize = FontAssets.MouseText.Value.MeasureString(starText);
            Vector2 starPos = headTop - new Vector2(starSize.X / 2f, starSize.Y + 8f);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                starText,
                starPos.X,
                starPos.Y,
                Color.Gold,
                Color.Black,
                Vector2.Zero,
                0.9f
            );
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
            if (isTamed) return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override bool CanHitNPC(NPC npc, NPC target) {
            if (isTamed) {
                if (!target.friendly || target.type == NPCID.TargetDummy) {
                    return true;
                }
                return false;
            }
            return base.CanHitNPC(npc, target);
        }
    }
}