using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Items;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;

namespace terraria_gldty.Common.Systems
{
    public class AnomalyWeatherSystem : ModSystem
    {
        // =========================================================
        // 事件狀態
        // =========================================================

        public static bool IsAnomalyActive = false;
        public static int EventProgress = 0;
        public const int MaxEventProgress = 180;
        private int spawnTimer = 0;
        private const int BaseSpawnInterval = 45;
        public static bool HasCompletedAnomalyEvent = false;

        // =========================================================
        // 怪物池
        // =========================================================

        public static List<int> PreHardmodePool = new();
        public static List<int> HardmodePool = new();

        // =========================================================
        // 世界生命週期
        // =========================================================

        public override void OnWorldLoad()
        {
            // 当前世界的事件状态
            ResetEventState();

            // 当前世界自己的 Boss Checklist 完成状态
            HasCompletedAnomalyEvent = false;
        }

        public override void OnWorldUnload()
        {
            // 只清理当前世界的运行状态
            // 不要清空怪物池
            ResetEventState();
        }

        private static void ResetEventState()
        {
            IsAnomalyActive = false;
            EventProgress = 0;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (HasCompletedAnomalyEvent)
            {
                tag["HasCompletedAnomalyEvent"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            HasCompletedAnomalyEvent =
                tag.ContainsKey("HasCompletedAnomalyEvent") &&
                tag.GetBool("HasCompletedAnomalyEvent");
        }

        // =========================================================
        // 事件控制
        // =========================================================

        public static void StartEvent()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            IsAnomalyActive = true;
            EventProgress = 0;

            ModContent.GetInstance<AnomalyWeatherSystem>().spawnTimer = 0;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        public static void StopEvent()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            IsAnomalyActive = false;
            EventProgress = 0;

            ModContent.GetInstance<AnomalyWeatherSystem>().spawnTimer = 0;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        // =========================================================
        // 怪物池
        //
        // 这些是 Mod 级别的数据，不属于单个世界。
        // 不能在 OnWorldUnload() 中清空，
        // 否则切换世界后不会自动重新建立。
        // =========================================================

        public override void PostSetupContent()
        {
            PreHardmodePool.Clear();
            HardmodePool.Clear();

            NPC tempNPC = new NPC();

            for (int type = 1; type < NPCLoader.NPCCount; type++)
            {
                if (!ContentSamples.NpcsByNetId.TryGetValue(type, out NPC sample))
                    continue;

                // 排除 Boss、友善 NPC、城鎮 NPC
                if (sample.boss || sample.friendly || sample.townNPC)
                    continue;

                // 排除小動物與 Boss 附屬部屬
                if (NPCID.Sets.CountsAsCritter[type] || NPCID.Sets.ShouldBeCountedAsBoss[type])
                    continue;

                tempNPC.SetDefaults(type);

                // 基本有效性檢查（排除傷害為 0 或血量極低的 NPC）
                if (tempNPC.lifeMax <= 5 || tempNPC.damage <= 0)
                    continue;

                string internalName = NPCID.Search.GetName(type);

                if (internalName.Contains("Cultist") || internalName.Contains("Probe") || type == NPCID.MartianProbe)
                    continue;

                if (NPCID.Sets.BelongsToInvasionOldOnesArmy[type])
                    continue;

                if (NPCID.Sets.BossHeadTextures[type] >= 0)
                    continue;

                if (tempNPC.dontTakeDamage)
                    continue;

                if (tempNPC.aiStyle == NPCAIStyleID.Worm)
                    continue;

                if (tempNPC.aiStyle == NPCAIStyleID.FaceClosestPlayer && !Main.npcFrameCount[type].Equals(1))
                    continue;

                if (tempNPC.ModNPC != null)
                {
                    string name = tempNPC.ModNPC.Name;

                    if (name.Contains("Hand") || name.Contains("Arm") || name.Contains("Claw") ||
                        name.Contains("Head") || name.Contains("Tail") || name.Contains("Body") ||
                        name.Contains("Minion") || name.Contains("Piece") || name.Contains("Part") ||
                        name.Contains("Cultist") || name.Contains("Probe"))
                    {
                        continue;
                    }
                }

                // =========================================================
                // 正確的肉前 / 肉後判定邏輯
                // =========================================================

                bool isHardmodeEnemy = false;

                if (type < NPCID.Count)
                {
                    // 原版怪物：血量 > 110 或 傷害 > 30，或者 ID 大於等於肉後起點怪物（Herpling/113）
                    isHardmodeEnemy = type >= NPCID.Herpling || tempNPC.lifeMax >= 110 || tempNPC.damage >= 30;
                }
                else
                {
                    // Mod 怪物：血量 >= 120 或 傷害 >= 35
                    isHardmodeEnemy = tempNPC.lifeMax >= 120 || tempNPC.damage >= 35;
                }

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
        // =========================================================
        // 世界更新（核心：補回缺失的驅動方法）
        // =========================================================

        public override void PostUpdateWorld()
        {
            if (!IsAnomalyActive)
                return;

            // -----------------------------------------------------
            // 天氣效果
            // -----------------------------------------------------

            Main.rainTime = 180;
            Main.maxRaining = 0.95f;
            Main.raining = true;
            Main.windSpeedTarget = 1.2f;

            if (Main.rand.NextBool(180))
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Thunder);
                }
            }

            // -----------------------------------------------------
            // 客戶端視覺粒子
            // -----------------------------------------------------

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
                            int redDustType = Main.rand.NextBool() ? DustID.Blood : DustID.LifeDrain;

                            Dust d = Dust.NewDustDirect(dustPos, 10, 10, redDustType, 0f, Main.rand.NextFloat(-5f, -2f));
                            d.noGravity = true;
                            d.scale = Main.rand.NextFloat(1.5f, 2.6f);
                            d.velocity.X = Main.windSpeedCurrent * 7f;
                        }
                        else
                        {
                            Dust d = Dust.NewDustDirect(dustPos, 10, 10, DustID.Shadowflame, 0f, Main.rand.NextFloat(-3f, -1f));
                            d.noGravity = true;
                            d.scale = Main.rand.NextFloat(1.2f, 2.2f);
                            d.velocity.X = Main.windSpeedCurrent * 5f;
                        }
                    }
                }
            }

            // =====================================================
            // 伺服器 / 單機 刷怪邏輯
            // =====================================================

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            spawnTimer++;

            Player targetPlayer = GetRandomSurfacePlayer();

            if (targetPlayer == null)
                return;

            int currentInterval = GetEffectiveInterval(targetPlayer);

            if (spawnTimer >= currentInterval)
            {
                spawnTimer = 0;
                TriggerRainEvent(targetPlayer);
            }
        }

        // =========================================================
        // 光照
        // =========================================================

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (!IsAnomalyActive)
                return;

            if (Main.hardMode)
            {
                tileColor.R = (byte)(tileColor.R * 0.95f);
                tileColor.G = (byte)(tileColor.G * 0.10f);
                tileColor.B = (byte)(tileColor.B * 0.15f);

                backgroundColor.R = (byte)(backgroundColor.R * 0.85f);
                backgroundColor.G = (byte)(backgroundColor.G * 0.05f);
                backgroundColor.B = (byte)(backgroundColor.B * 0.08f);
            }
            else
            {
                tileColor.R = (byte)(tileColor.R * 0.45f);
                tileColor.G = (byte)(tileColor.G * 0.25f);
                tileColor.B = (byte)(tileColor.B * 0.70f);

                backgroundColor.R = (byte)(backgroundColor.R * 0.30f);
                backgroundColor.G = (byte)(backgroundColor.G * 0.15f);
                backgroundColor.B = (byte)(backgroundColor.B * 0.60f);
            }
        }

        public override void ModifyLightingBrightness(ref float scale)
        {
            if (IsAnomalyActive)
            {
                scale *= Main.hardMode ? 0.75f : 0.85f;
            }
        }

        // =========================================================
        // 事件進度
        // =========================================================

        public static void AddEventProgress(int amount = 1)
        {
            if (!IsAnomalyActive)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            EventProgress += amount;

            if (EventProgress >= MaxEventProgress)
            {
                HasCompletedAnomalyEvent = true;
                CompleteEvent();
            }
        }

        private static void CompleteEvent()
        {
            IsAnomalyActive = false;
            EventProgress = 0;

            ModContent.GetInstance<AnomalyWeatherSystem>().spawnTimer = 0;

            string winMsg;
            Color winColor;

            int rewardType = Main.hardMode ? ModContent.ItemType<AnomalyAccessory2>() : ModContent.ItemType<AnomalyAccessory1>();

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];

                if (!p.active || p.dead || !p.ZoneOverworldHeight)
                    continue;

                int itemIndex = Item.NewItem(p.GetSource_TileInteraction(0, 0), p.getRect(), rewardType);

                if (Main.netMode == NetmodeID.Server && itemIndex >= 0 && itemIndex < Main.maxItems)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
                }
            }

            if (Main.hardMode)
            {
                winMsg = "腥風血雨終於散去，被壓制的生機重新復蘇！";
                winColor = new Color(255, 180, 100);
            }
            else
            {
                winMsg = "狂暴的異域降水終於停息，天空重歸寧靜！";
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

        // =========================================================
        // UI
        // =========================================================

        public override void UpdateUI(GameTime gameTime)
        {
            if (IsAnomalyActive && !Main.gameMenu)
            {
                Main.ReportInvasionProgress(EventProgress, MaxEventProgress, 0, 0);
            }
        }

        public static List<int> GetBossChecklistNPCs()
        {
            HashSet<int> result = new HashSet<int>();

            foreach (int npcType in PreHardmodePool)
            {
                result.Add(npcType);
            }

            foreach (int npcType in HardmodePool)
            {
                result.Add(npcType);
            }

            return new List<int>(result);
        }

        // =========================================================
        // 刷怪間隔
        // =========================================================

        private int GetEffectiveInterval(Player player)
        {
            float rateMultiplier = 1.0f;

            if (player.ZoneWaterCandle || player.HasBuff(BuffID.WaterCandle))
            {
                rateMultiplier += 0.5f;
            }

            if (player.HasBuff(BuffID.Battle))
            {
                rateMultiplier += 1.0f;
            }

            return Math.Max(1, (int)(BaseSpawnInterval / rateMultiplier));
        }

        // =========================================================
        // 隨機尋找地表玩家
        // =========================================================

        private Player GetRandomSurfacePlayer()
        {
            List<Player> validPlayers = new();

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (player.active && !player.dead && player.ZoneOverworldHeight)
                {
                    validPlayers.Add(player);
                }
            }

            return validPlayers.Count > 0 ? Main.rand.Next(validPlayers) : null;
        }

        // =========================================================
        // 觸發事件
        // =========================================================

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

        // =========================================================
        // 事件怪（修正概率寫法）
        // =========================================================

        // =========================================================
        // 事件怪（修復肉後陸地怪空降 Despawn 問題）
        // =========================================================

        private void SpawnMonster(Player targetPlayer)
        {
            // =========================================================
            // 1. 选择怪物
            // =========================================================

            // if (Main.netMode != NetmodeID.MultiplayerClient)
            // {
            //     Main.NewText(
            //         $"异变刷怪：肉后={Main.hardMode}，肉后池={HardmodePool.Count}，肉前池={PreHardmodePool.Count}",
            //         Color.Cyan
            //     );
            // }

            int selectedType = 0;

            // 肉后：
            // 70% 从肉后池选择
            // 30% 从肉前池选择
            if (Main.hardMode)
            {
                if (HardmodePool.Count > 0 &&
                    (Main.rand.NextFloat() < 0.7f || PreHardmodePool.Count == 0))
                {
                    selectedType = Main.rand.Next(HardmodePool);
                }
                else if (PreHardmodePool.Count > 0)
                {
                    selectedType = Main.rand.Next(PreHardmodePool);
                }
            }
            // 肉前
            else
            {
                if (PreHardmodePool.Count > 0)
                {
                    selectedType = Main.rand.Next(PreHardmodePool);
                }
                else if (HardmodePool.Count > 0)
                {
                    selectedType = Main.rand.Next(HardmodePool);
                }
            }

            // =========================================================
            // 2. 兜底
            // =========================================================

            if (selectedType <= 0)
                return;

            // 确认这个 NPC 类型确实存在
            if (!ContentSamples.NpcsByNetId.TryGetValue(
                    selectedType,
                    out NPC sample))
            {
                return;
            }

            // 二次安全检查
            if (sample.friendly ||
                sample.townNPC ||
                sample.boss ||
                sample.lifeMax <= 5 ||
                sample.damage <= 0)
            {
                return;
            }

            // =========================================================
            // 3. 生成位置
            //
            // 不再强行寻找地面。
            // 在玩家上方生成，让 NPC 自己处理 AI / 重力。
            // =========================================================

            int xOffset =
                Main.rand.Next(400, 800) *
                (Main.rand.NextBool() ? 1 : -1);

            Vector2 spawnPos =
                targetPlayer.Center +
                new Vector2(xOffset, -600f);

            // 限制在世界范围内
            spawnPos.X = MathHelper.Clamp(
                spawnPos.X,
                100f,
                Main.maxTilesX * 16f - 100f
            );

            spawnPos.Y = MathHelper.Clamp(
                spawnPos.Y,
                100f,
                Main.maxTilesY * 16f - 100f
            );

            // =========================================================
            // 4. 创建 NPC
            // =========================================================

            IEntitySource source =
                targetPlayer.GetSource_FromThis();

            int npcIndex = NPC.NewNPC(
                source,
                (int)spawnPos.X,
                (int)spawnPos.Y,
                selectedType
            );

            if (npcIndex < 0 ||
                npcIndex >= Main.maxNPCs)
            {
                return;
            }

            NPC npc = Main.npc[npcIndex];

            if (!npc.active)
                return;

            // =========================================================
            // 5. 标记为异变事件生成
            // =========================================================

            npc.GetGlobalNPC<AnomalyGlobalNPC>()
                .SpawnedByAnomaly = true;

            // =========================================================
            // 6. 统一设置空降效果
            // =========================================================

            if (npc.noGravity)
            {
                // 飞行 / 漂浮单位
                npc.velocity.Y =
                    Main.rand.NextFloat(5f, 10f);
            }
            else
            {
                // 有重力单位
                npc.velocity.Y =
                    Main.rand.NextFloat(2f, 5f);
            }

            // =========================================================
            // 7. 生成视觉效果
            // =========================================================

            if (Main.netMode != NetmodeID.Server)
            {
                int dustCount =
                    Main.hardMode ? 30 : 15;

                int dustType =
                    Main.hardMode
                        ? DustID.Blood
                        : DustID.Shadowflame;

                for (int i = 0; i < dustCount; i++)
                {
                    Dust d = Dust.NewDustDirect(
                        npc.position,
                        npc.width,
                        npc.height,
                        dustType,
                        Main.rand.NextFloat(-1f, 1f),
                        Main.rand.NextFloat(1f, 3f)
                    );

                    d.noGravity = true;

                    if (Main.hardMode)
                    {
                        d.scale =
                            Main.rand.NextFloat(1.4f, 2.2f);

                        d.velocity *= 1.3f;
                    }
                    else
                    {
                        d.scale =
                            Main.rand.NextFloat(1.1f, 1.8f);
                    }
                }
            }

            // =========================================================
            // 8. 联机同步
            // =========================================================

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(
                    MessageID.SyncNPC,
                    -1,
                    -1,
                    null,
                    npcIndex
                );
            }
        }

        // =========================================================
        // 危險物
        // =========================================================

        private void SpawnHazard(Player targetPlayer)
        {
            Vector2 spawnPos = targetPlayer.Center + new Vector2(Main.rand.Next(-700, 700), -650);
            IEntitySource source = targetPlayer.GetSource_FromThis();

            if (Main.hardMode && Main.rand.NextBool(3))
            {
                int bigProjType = ModContent.ProjectileType<Content.Items.Weapons.GiganticBoulder>();
                Vector2 bigVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(6f, 10f));

                int pIndex = Projectile.NewProjectile(source, spawnPos, bigVelocity, bigProjType, 0, 0f, Main.myPlayer);

                if (pIndex >= 0 && pIndex < Main.maxProjectiles)
                {
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, pIndex);
                    }
                }

                return;
            }

            int randVal = Main.rand.Next(100);
            int projType = randVal < 45 ? ProjectileID.Boulder : randVal < 80 ? ProjectileID.BouncyBoulder : ProjectileID.Grenade;
            int damage = Main.hardMode ? 80 : 45;

            Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(8f, 14f));
            int projIndex = Projectile.NewProjectile(source, spawnPos, velocity, projType, damage, 3f, Main.myPlayer);

            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
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

        // =========================================================
        // 世界數據同步
        // =========================================================

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(IsAnomalyActive);
            writer.Write(EventProgress);
        }

        public override void NetReceive(BinaryReader reader)
        {
            IsAnomalyActive = reader.ReadBoolean();
            EventProgress = reader.ReadInt32();

            if (!IsAnomalyActive)
            {
                ModContent.GetInstance<AnomalyWeatherSystem>().spawnTimer = 0;
            }
        }
    }
}