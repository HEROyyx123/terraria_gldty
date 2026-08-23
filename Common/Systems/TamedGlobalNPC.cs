using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;
using terraria_gldty.Content.Items;

namespace terraria_gldty.Common.Systems
{
    public class TamedGlobalNPC : GlobalNPC
    {
        public bool isTamed;
        public int ownerPlayerID = -1;
        public int minionSlotCost = 1;

        private int contactAttackCooldown = 0;
        public int invincibleTimer = 0;
        
        private Vector2 _savedPlayerCenter;
        private bool _shiftedPlayerPosition;
        private Vector2 _savedPlayerVelocity;

        public bool isGiantAndGlow = false; 
        private bool _hasScaled = false;    
        public Vector2[] oldPos = new Vector2[6]; 

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc) {
            if (isTamed) {
                npc.friendly = true;
            }
        }

        public override bool PreAI(NPC npc)
        {
            // 自动同步蠕虫/长直类怪物的身体与尾部
            bool isWormPart = npc.realLife >= 0 || npc.aiStyle == NPCAIStyleID.Worm;
            if (isWormPart)
            {
                int headIndex = (npc.realLife >= 0) ? npc.realLife : (int)npc.ai[3];
                if (headIndex >= 0 && headIndex < Main.maxNPCs && headIndex != npc.whoAmI)
                {
                    NPC headNPC = Main.npc[headIndex];
                    if (headNPC.active)
                    {
                        TamedGlobalNPC headGlobal = headNPC.GetGlobalNPC<TamedGlobalNPC>();
                        if (headGlobal.isTamed)
                        {
                            isTamed = true;
                            ownerPlayerID = headGlobal.ownerPlayerID;
                            minionSlotCost = 0; 
                            npc.friendly = true;

                            invincibleTimer = headGlobal.invincibleTimer;
                            isGiantAndGlow = headGlobal.isGiantAndGlow;
                            npc.dontTakeDamage = headNPC.dontTakeDamage;

                            npc.life = headNPC.life;
                            npc.lifeMax = headNPC.lifeMax;
                            npc.netUpdate = true;
                        }
                    }
                }
            }

            if (!isTamed) return base.PreAI(npc);

            // 判断主人是否拥有【契约枷锁】强化
            if (ownerPlayerID >= 0 && ownerPlayerID < Main.maxPlayers) {
                Player owner = Main.player[ownerPlayerID];
                if (owner.active && owner.GetModPlayer<TamedPlayer>().usedContractShackles) {
                    isGiantAndGlow = true;
                }
            }

            // 体型与碰撞箱放大处理 (基础放大 1.35 倍)
            if (isGiantAndGlow && !_hasScaled) {
                _hasScaled = true;
                float scaleFactor = 1.35f;

                npc.scale *= scaleFactor;
                
                Vector2 originalCenter = npc.Center;
                npc.width = (int)(npc.width * scaleFactor);
                npc.height = (int)(npc.height * scaleFactor);
                npc.Center = originalCenter;
            }

            // 【新增】：怪物发光效果（使用紫粉色调 R:0.6 G:0.3 B:0.8，可根据需要调整数值）
            if (isGiantAndGlow) {
                Lighting.AddLight(npc.Center, 0.6f, 0.3f, 0.8f);
            }

            // 虚影历史轨迹位置更新
            for (int k = oldPos.Length - 1; k > 0; k--) {
                oldPos[k] = oldPos[k - 1];
            }
            oldPos[0] = npc.position;

            // 移动速度加成
            if (isGiantAndGlow && npc.velocity != Vector2.Zero) {
                npc.velocity *= 1.01f; 
            }

            // 处理 0.2 秒无敌帧
            if (invincibleTimer > 0)
            {
                invincibleTimer--;
                npc.dontTakeDamage = true;
            }
            else
            {
                bool isWormBodyOrTail = npc.realLife >= 0;
                if (isWormBodyOrTail)
                {
                    NPC head = Main.npc[npc.realLife];
                    if (head.active)
                    {
                        npc.dontTakeDamage = head.dontTakeDamage;
                    }
                }
                else
                {
                    if (!npc.townNPC) 
                    {
                        npc.dontTakeDamage = false;
                    }
                }
            }

            // 长直类/蠕虫穿墙与无重力保障
            if (npc.aiStyle == NPCAIStyleID.Worm || npc.realLife >= 0)
            {
                npc.noTileCollide = true;
                npc.noGravity = true;
            }

            if (ownerPlayerID < 0 || ownerPlayerID >= Main.maxPlayers)
            {
                npc.active = false;
                return false;
            }

            Player ownerPlayer = Main.player[ownerPlayerID];
            if (!ownerPlayer.active || ownerPlayer.dead)
            {
                npc.active = false;
                return false;
            }

            if (contactAttackCooldown > 0) contactAttackCooldown--;

            // 右键收回机制
            if (Main.mouseRight && Main.mouseRightRelease && Vector2.Distance(Main.MouseWorld, npc.Center) < 80f)
            {
                if (ownerPlayer.HeldItem.type == ModContent.ItemType<SoulChain>())
                {
                    if (Vector2.Distance(ownerPlayer.Center, npc.Center) < 150f)
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

            // 距离过远自动传送
            float distanceToOwner = Vector2.Distance(npc.Center, ownerPlayer.Center);
            if (distanceToOwner > 2000f) {
                npc.Center = ownerPlayer.Center;
                npc.velocity = Vector2.Zero;
                npc.netUpdate = true;
            }

            // 索敌逻辑
            NPC targetNPC = null;
            float maxSearchDistance = 800f;
            float closestDistance = maxSearchDistance;

            if (ownerPlayer.HasMinionAttackTargetNPC) {
                NPC target = Main.npc[ownerPlayer.MinionAttackTargetNPC];
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

            // 玩家位置置换与接触伤害计算
            npc.target = ownerPlayerID;
            if (targetNPC != null)
            {
                _savedPlayerCenter = ownerPlayer.Center;
                _savedPlayerVelocity = ownerPlayer.velocity;

                ownerPlayer.Center = targetNPC.Center;
                ownerPlayer.velocity = targetNPC.velocity;
                _shiftedPlayerPosition = true;

                npc.targetRect = targetNPC.Hitbox;
                int targetDir = targetNPC.Center.X >= npc.Center.X ? 1 : -1;
                npc.direction = targetDir;
                npc.spriteDirection = targetDir;

                if (contactAttackCooldown <= 0 && npc.Hitbox.Intersects(targetNPC.Hitbox))
                {
                    int baseDamage = npc.damage > 0 ? npc.damage : (npc.defDamage > 0 ? npc.defDamage : 20);
                    
                    StatModifier summonModifier = ownerPlayer.GetTotalDamage(DamageClass.Summon).Scale(0.5f);
                    int finalDamage = (int)summonModifier.ApplyTo(baseDamage);
                    bool isCrit = Main.rand.NextBool((int)ownerPlayer.GetTotalCritChance(DamageClass.Summon));

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
                        ownerPlayer.addDPS(actualDamageDone);
                    }

                    contactAttackCooldown = 20;
                }
            }
            else
            {
                _shiftedPlayerPosition = false;
                npc.targetRect = ownerPlayer.Hitbox;
            }

            return true;
        }

        public override void PostAI(NPC npc)
        {
            if (_shiftedPlayerPosition && ownerPlayerID >= 0 && ownerPlayerID < Main.maxPlayers)
            {
                Player owner = Main.player[ownerPlayerID];
                if (owner.active)
                {
                    owner.Center = _savedPlayerCenter;
                    owner.velocity = _savedPlayerVelocity;
                }
                _shiftedPlayerPosition = false;
            }

            base.PostAI(npc);
        }

        // 【修改】：绘制放大后的残影
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (!isTamed || !npc.active || !isGiantAndGlow) return true;

            Main.instance.LoadNPC(npc.type);
            Texture2D texture = TextureAssets.Npc[npc.type].Value;
            
            SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = npc.frame.Size() * 0.5f;

            // 循环绘制历史帧残影
            for (int i = oldPos.Length - 1; i > 0; i--) {
                if (oldPos[i] == Vector2.Zero) continue;

                float alpha = (float)(oldPos.Length - i) / oldPos.Length * 0.45f;
                Color afterimageColor = Color.Lerp(Color.MediumPurple, Color.Cyan, (float)i / oldPos.Length) * alpha;

                Vector2 drawPos = oldPos[i] + new Vector2(npc.width * 0.5f, npc.height * 0.5f) - screenPos;

                // 【关键改动】：残影按倍率放大，i 越大（越早的残影帧）尺寸越大，呈现扩散巨化视觉效果
                float afterimageScale = npc.scale * (1.2f + i * 0.05f);

                spriteBatch.Draw(
                    texture,
                    drawPos,
                    npc.frame,
                    afterimageColor,
                    npc.rotation,
                    origin,
                    afterimageScale, // 传入放大后的 Scale
                    effects,
                    0f
                );
            }

            return true;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            if (!isTamed || !npc.active) return;
            if (npc.realLife >= 0 && npc.realLife != npc.whoAmI) return;

            Vector2 headTop = new Vector2(npc.Center.X, npc.position.Y) - screenPos;
            float bounce = (float)System.Math.Sin(Main.GameUpdateCount * 0.12f) * 4f;
            headTop.Y -= (22f + bounce);

            Color glowColor;
            if (isGiantAndGlow) {
                float pulseGlow = (float)(System.Math.Sin(Main.GameUpdateCount * 0.2f) + 1f) * 0.5f;
                glowColor = Color.Lerp(new Color(180, 100, 255), new Color(0, 255, 255), pulseGlow);
            } else {
                float pulse = (float)(System.Math.Sin(Main.GameUpdateCount * 0.15f) + 1f) * 0.5f;
                glowColor = Color.Lerp(new Color(50, 255, 150), new Color(0, 230, 255), pulse);
            }

            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, "▼", headTop.X - 6f, headTop.Y - 10f, glowColor, Color.Black, Vector2.Zero, 1.2f);

            string displayName = npc.GivenOrTypeName;
            float textScale = 0.9f;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(displayName) * textScale;

            Utils.DrawBorderStringFourWay(
                spriteBatch, 
                FontAssets.MouseText.Value, 
                displayName, 
                headTop.X - (textSize.X * 0.5f), 
                headTop.Y - 24f, 
                isGiantAndGlow ? Color.Violet : Color.Gold, 
                Color.Black, 
                Vector2.Zero, 
                textScale
            );
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
            if (isTamed) return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override bool CanHitNPC(NPC npc, NPC target) {
            if (isTamed) {
                return !target.friendly || target.type == NPCID.TargetDummy;
            }
            return base.CanHitNPC(npc, target);
        }
    }
}