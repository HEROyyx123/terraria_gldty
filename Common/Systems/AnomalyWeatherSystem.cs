using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using terraria_gldty.Content.Items;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    public class AnomalyWeatherSystem : ModSystem
    {
        public static bool IsAnomalyActive = false;

        public static int EventProgress = 0;            // 当前击杀数
        public const int MaxEventProgress = 180;         // 通关所需击杀数 80

        private int spawnTimer = 0;
        private const int BaseSpawnInterval = 45;

        public static List<int> PreHardmodePool = new(); 
        public static List<int> HardmodePool = new(); 

        public override void PostSetupContent()
        {
            // 怪物池动态扫描
            PreHardmodePool.Clear(); 
            HardmodePool.Clear(); 

            for (int type = 1; type < NPCLoader.NPCCount; type++) 
            {
                if (!ContentSamples.NpcsByNetId.TryGetValue(type, out NPC sample)) 
                    continue;

                if (sample.boss || sample.friendly || sample.townNPC) continue; 
                if (NPCID.Sets.CountsAsCritter[type] || NPCID.Sets.ShouldBeCountedAsBoss[type]) continue; 
                if (sample.lifeMax <= 5 || sample.damage <= 0) continue; 

                string internalName = NPCID.Search.GetName(type); 
                if (internalName.Contains("Cultist") || internalName.Contains("Probe") || type == NPCID.MartianProbe) 
                    continue;

                if (NPCID.Sets.BelongsToInvasionOldOnesArmy[type]) 
                    continue;

                if (NPCID.Sets.BossHeadTextures[type] >= 0) continue; 
                if (sample.dontTakeDamage) continue; 
                if (sample.aiStyle == NPCAIStyleID.Worm) continue; 
                if (sample.aiStyle == NPCAIStyleID.FaceClosestPlayer && !Main.npcFrameCount[type].Equals(1)) continue; 

                if (sample.ModNPC != null) 
                {
                    string name = sample.ModNPC.Name; 
                    if (name.Contains("Hand") || name.Contains("Arm") || name.Contains("Claw") || 
                        name.Contains("Head") || name.Contains("Tail") || name.Contains("Body") || 
                        name.Contains("Minion") || name.Contains("Piece") || name.Contains("Part") ||
                        name.Contains("Cultist") || name.Contains("Probe")) 
                    {
                        continue;
                    }
                }

                bool isHardmodeEnemy = sample.lifeMax > 220 || sample.damage > 40 || sample.defense > 20; 

                if (isHardmodeEnemy) 
                {
                    HardmodePool.Add(type); 
                }
                else
                {
                    PreHardmodePool.Add(type); 
                }
            }
        }

        public override void PostUpdateWorld()
        {
            if (!IsAnomalyActive) return; 

            // 1. 强化狂风暴雨环境天气
            Main.rainTime = 180; 
            Main.maxRaining = 0.95f; // 超大暴雨
            Main.raining = true; 
            Main.windSpeedTarget = 1.2f; // 极速强风

            if (Main.rand.NextBool(180))
            {
                SoundEngine.PlaySound(SoundID.Thunder); 
            }

            // 2. 粒子特效（肉前异界紫，肉后强烈血红）
            if (Main.netMode != NetmodeID.Server)
            {
                Player localPlayer = Main.LocalPlayer;
                if (localPlayer.active && !localPlayer.dead && localPlayer.ZoneOverworldHeight)
                {
                    int dustCount = Main.hardMode ? 6 : 3;
                    
                    for (int i = 0; i < dustCount; i++)
                    {
                        Vector2 dustPos = localPlayer.Center + new Vector2(Main.rand.Next(-1000, 1000), Main.rand.Next(-600, 600));
                        
                        if (Main.hardMode)
                        {
                            // 肉后：极度狂暴的血红粒子效果
                            int redDustType = Main.rand.NextBool() ? DustID.Blood : DustID.LifeDrain;
                            Dust d = Dust.NewDustDirect(dustPos, 10, 10, redDustType, 0f, Main.rand.NextFloat(-5f, -2f));
                            d.noGravity = true;
                            d.scale = Main.rand.NextFloat(1.5f, 2.6f);
                            d.velocity.X = Main.windSpeedCurrent * 7f;
                        }
                        else
                        {
                            // 肉前：保持原版紫闪光粒子
                            Dust d = Dust.NewDustDirect(dustPos, 10, 10, DustID.Shadowflame, 0f, Main.rand.NextFloat(-3f, -1f));
                            d.noGravity = true;
                            d.scale = Main.rand.NextFloat(1.2f, 2.2f);
                            d.velocity.X = Main.windSpeedCurrent * 5f;
                        }
                    }
                }
            }

            spawnTimer++; 

            Player targetPlayer = GetRandomSurfacePlayer(); 
            if (targetPlayer == null) return; 

            int currentInterval = GetEffectiveInterval(targetPlayer); 

            if (spawnTimer >= currentInterval) 
            {
                spawnTimer = 0; 
                if (Main.netMode != NetmodeID.MultiplayerClient) 
                {
                    TriggerRainEvent(targetPlayer); 
                }
            }
        }

        // 修改日光与环境光照
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (!IsAnomalyActive) return;

            if (Main.hardMode)
            {
                // 肉后：压暗绿蓝通道，大幅拉高红色通道（强烈血红氛围）
                tileColor.R = (byte)(tileColor.R * 0.95f);
                tileColor.G = (byte)(tileColor.G * 0.10f);
                tileColor.B = (byte)(tileColor.B * 0.15f);

                backgroundColor.R = (byte)(backgroundColor.R * 0.85f);
                backgroundColor.G = (byte)(backgroundColor.G * 0.05f);
                backgroundColor.B = (byte)(backgroundColor.B * 0.08f);
            }
            else
            {
                // 肉前：保持原版暗紫氛围
                tileColor.R = (byte)(tileColor.R * 0.45f);
                tileColor.G = (byte)(tileColor.G * 0.25f);
                tileColor.B = (byte)(tileColor.B * 0.70f);

                backgroundColor.R = (byte)(backgroundColor.R * 0.3f);
                backgroundColor.G = (byte)(backgroundColor.G * 0.15f);
                backgroundColor.B = (byte)(backgroundColor.B * 0.6f);
            }
        }

        // 调整总体环境亮度
        public override void ModifyLightingBrightness(ref float scale)
        {
            if (IsAnomalyActive)
            {
                scale *= Main.hardMode ? 0.75f : 0.85f;
            }
        }

        public static void AddEventProgress(int amount = 1)
        {
            if (!IsAnomalyActive) return; 

            EventProgress += amount; 
            int percentage = (int)((float)EventProgress / MaxEventProgress * 100); 
            if (EventProgress >= MaxEventProgress)
            {
                IsAnomalyActive = false;
                EventProgress = 0;

                string winMsg;
                Color winColor;

                // ----- 新增：发放奖励逻辑 -----
                int rewardType = Main.hardMode ? ModContent.ItemType<AnomalyAccessory2>() : ModContent.ItemType<AnomalyAccessory1>();

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (p.active && !p.dead && p.ZoneOverworldHeight)
                    {
                        // 在玩家脚下生成通关奖励物品
                        int itemIndex = Item.NewItem(p.GetSource_TileInteraction(0, 0), p.getRect(), rewardType);
                        if (Main.netMode == NetmodeID.Server && itemIndex < Main.maxItems)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
                        }
                    }
                }
                // ----------------------------

                if (Main.hardMode)
                {
                    winMsg = "腥风血雨终于散去，被压制的生机重新复苏！";
                    winColor = new Color(255, 180, 100);
                }
                else
                {
                    winMsg = "狂暴的异域降水终于停息，天空重归宁静！";
                    winColor = new Color(100, 220, 100);
                }

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(winMsg, winColor);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    Terraria.Chat.ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral(winMsg), winColor);
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (IsAnomalyActive && !Main.gameMenu)
            {
                Main.ReportInvasionProgress(EventProgress, MaxEventProgress, 0, 0);
            }
        }

        private int GetEffectiveInterval(Player player)
        {
            float rateMultiplier = 1.0f; 
            if (player.ZoneWaterCandle || player.HasBuff(BuffID.WaterCandle)) rateMultiplier += 0.5f; 
            if (player.HasBuff(BuffID.Battle)) rateMultiplier += 1.0f; 
            return (int)(BaseSpawnInterval / rateMultiplier); 
        }

        private Player GetRandomSurfacePlayer()
        {
            List<Player> validPlayers = new(); 
            for (int i = 0; i < Main.maxPlayers; i++) 
            {
                if (Main.player[i].active && !Main.player[i].dead && Main.player[i].ZoneOverworldHeight) 
                {
                    validPlayers.Add(Main.player[i]); 
                }
            }
            return validPlayers.Count > 0 ? Main.rand.Next(validPlayers) : null; 
        }

        private void TriggerRainEvent(Player targetPlayer)
        {
            if (Main.rand.NextBool(1, 5)) 
            {
                SpawnHazard(targetPlayer); 
            }
            else
            {
                SpawnMonster(targetPlayer); 
            }
        }

        private void SpawnMonster(Player targetPlayer)
        {
            List<int> currentPool = new(PreHardmodePool); 
            if (Main.hardMode && HardmodePool.Count > 0) 
            {
                currentPool.AddRange(HardmodePool); 
            }

            if (currentPool.Count == 0) return; 

            int selectedType = Main.rand.Next(currentPool); 
            Vector2 spawnPos = targetPlayer.Center + new Vector2(Main.rand.Next(-700, 700), -650); 

            IEntitySource source = targetPlayer.GetSource_FromThis(); 
            int npcIndex = NPC.NewNPC(source, (int)spawnPos.X, (int)spawnPos.Y, selectedType); 
            
            if (npcIndex < Main.maxNPCs) 
            {
                NPC npc = Main.npc[npcIndex]; 
                npc.velocity.Y = Main.rand.NextFloat(8f, 15f); 

                // 怪物降落时的特效（肉前紫火，肉后血雾爆发）
                int dustCount = Main.hardMode ? 30 : 15;
                int dustType = Main.hardMode ? DustID.Blood : DustID.Shadowflame;

                for (int i = 0; i < dustCount; i++)
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, dustType, 0, 5f);
                    if (Main.hardMode)
                    {
                        d.scale = Main.rand.NextFloat(1.4f, 2.2f);
                        d.velocity *= 1.3f;
                    }
                }

                if (Main.netMode == NetmodeID.Server) 
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex); 
                }
            }
        }

        private void SpawnHazard(Player targetPlayer)
        {
            Vector2 spawnPos = targetPlayer.Center + new Vector2(Main.rand.Next(-700, 700), -650);
            IEntitySource source = targetPlayer.GetSource_FromThis();

            // 肉后 25% 概率生成超大视觉巨石
            if (Main.hardMode && Main.rand.NextBool(3))
            {
                // 显式传入带命名空间的类型参数
                int bigProjType = ModContent.ProjectileType<Content.Items.Weapons.GiganticBoulder>();
                Vector2 bigVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(6f, 10f));

                int pIndex = Projectile.NewProjectile(source, spawnPos, bigVelocity, bigProjType, 0, 0f, Main.myPlayer);
                if (pIndex < Main.maxProjectiles && Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, pIndex);
                }
                return;
            }

            // 原版危险物生成逻辑
            int randVal = Main.rand.Next(100);
            int projType = randVal < 45 ? ProjectileID.Boulder : (randVal < 80 ? ProjectileID.BouncyBoulder : ProjectileID.Grenade);
            int damage = Main.hardMode ? 80 : 45;

            Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(8f, 14f));
            int projIndex = Projectile.NewProjectile(source, spawnPos, velocity, projType, damage, 3f, Main.myPlayer);

            if (projIndex < Main.maxProjectiles)
            {
                Projectile proj = Main.projectile[projIndex];
                proj.hostile = true;
                proj.friendly = false;

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projIndex);
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(IsAnomalyActive); 
            writer.Write(EventProgress); 
        }

        public override void NetReceive(BinaryReader reader)
        {
            IsAnomalyActive = reader.ReadBoolean(); 
            EventProgress = reader.ReadInt32(); 
        }
    }
}