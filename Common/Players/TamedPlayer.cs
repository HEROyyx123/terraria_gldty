using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using terraria_gldty.Common.Systems;

namespace terraria_gldty.Common.Players
{
    public class TamedPlayer : ModPlayer
    {
        public List<int> UnlockedNPCTypes = new List<int>();
        public int LastSummonedType = 0;

        // 【新增】：永久强化标记
        public bool usedSoulMark = false;      // 肉前物品：同种怪物上限 +1
        public bool usedContractShackles = false; // 肉后物品：同种怪物上限 +1

        /// <summary>
        /// 获取当前玩家对某种怪物的最大允许召唤数量
        /// 默认基础值为 1，使用肉前物品后为 2，使用肉后物品后为 3
        /// </summary>
        public int GetMaxSameNPCCount() {
            int max = 1;
            if (usedSoulMark) max++;
            if (usedContractShackles) max++;
            return max;
        }

        public override void SaveData(TagCompound tag) {
            tag["UnlockedNPCTypes"] = UnlockedNPCTypes;
            tag["LastSummonedType"] = LastSummonedType;
            // 保存永久强化状态
            tag["usedSoulMark"] = usedSoulMark;
            tag["usedContractShackles"] = usedContractShackles;
        }

        public override void LoadData(TagCompound tag) {
            if (tag.ContainsKey("UnlockedNPCTypes")) {
                var rawList = tag.Get<List<int>>("UnlockedNPCTypes");
                UnlockedNPCTypes = new List<int>();
                foreach (int type in rawList) {
                    if (type > 0 && type < NPCLoader.NPCCount) {
                        UnlockedNPCTypes.Add(type);
                    }
                }
            }

            if (tag.ContainsKey("LastSummonedType")) {
                int lastType = tag.GetInt("LastSummonedType");
                if (lastType > 0 && lastType < NPCLoader.NPCCount) {
                    LastSummonedType = lastType;
                }
            }

            // 读取永久强化状态
            usedSoulMark = tag.GetBool("usedSoulMark");
            usedContractShackles = tag.GetBool("usedContractShackles");
        }

        public int GetNPCCost(int npcType) {
            if (ContentSamples.NpcsByNetId.TryGetValue(npcType, out NPC sampleNPC)) {
                if (sampleNPC.defDamage > 120) return 3;
                if (sampleNPC.defDamage > 60) return 2;
            }
            return 1;
        }

        public static int GetWormHeadType(int rawType)
        {
            switch (rawType)
            {
                case NPCID.EaterofWorldsBody: case NPCID.EaterofWorldsTail: return NPCID.EaterofWorldsHead;
                case NPCID.GiantWormBody: case NPCID.GiantWormTail: return NPCID.GiantWormHead;
                case NPCID.DevourerBody: case NPCID.DevourerTail: return NPCID.DevourerHead;
                case NPCID.BoneSerpentBody: case NPCID.BoneSerpentTail: return NPCID.BoneSerpentHead;
                case NPCID.WyvernBody: case NPCID.WyvernBody2: case NPCID.WyvernLegs: case NPCID.WyvernTail: return NPCID.WyvernHead;
                case NPCID.DiggerBody: case NPCID.DiggerTail: return NPCID.DiggerHead;
                case NPCID.TombCrawlerBody: case NPCID.TombCrawlerTail: return NPCID.TombCrawlerHead;
                case NPCID.DuneSplicerBody: case NPCID.DuneSplicerTail: return NPCID.DuneSplicerHead;
                case NPCID.StardustWormBody: case NPCID.StardustWormTail: return NPCID.StardustWormHead;
                case NPCID.SeekerBody: case NPCID.SeekerTail: return NPCID.SeekerHead;
                case NPCID.StardustCellSmall: return NPCID.StardustCellBig;
                case NPCID.CultistDragonBody1: case NPCID.CultistDragonBody2: case NPCID.CultistDragonTail: return NPCID.CultistDragonHead;
                case NPCID.SolarCrawltipedeBody: case NPCID.SolarCrawltipedeTail: return NPCID.SolarCrawltipedeHead;
                default: return rawType;
            }
        }

        public bool TrySummonNPC(int rawNpcType)
        {
            int npcType = GetWormHeadType(rawNpcType);
            int cost = GetNPCCost(npcType);

            // 【修改】：统计场上已存在的同种怪物数量，并比对最大限制
            int currentSameNpcCount = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC existingNpc = Main.npc[i];
                if (existingNpc.active && existingNpc.type == npcType)
                {
                    var globalNPC = existingNpc.GetGlobalNPC<TamedGlobalNPC>();
                    if (globalNPC.isTamed && globalNPC.ownerPlayerID == Player.whoAmI)
                    {
                        currentSameNpcCount++;
                    }
                }
            }

            int maxAllowed = GetMaxSameNPCCount();
            if (currentSameNpcCount >= maxAllowed)
            {
                Main.NewText($"场上已有 {currentSameNpcCount} 只 {ContentSamples.NpcsByNetId[npcType].GivenOrTypeName}，已达同种召唤上限 ({maxAllowed})！", Color.Orange);
                return false;
            }

            int npcIndex = NPC.NewNPC(
                Player.GetSource_Misc("TamedSummon"),
                (int)Player.Center.X,
                (int)Player.Center.Y,
                npcType
            );

            if (npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];

                if (npc.aiStyle == NPCAIStyleID.Worm || npc.aiStyle == NPCAIStyleID.Worm)
                {
                    npc.noTileCollide = true;
                    npc.noGravity = true;
                }

                if (npc.GetGlobalNPC<TamedGlobalNPC>() is TamedGlobalNPC tamedNPC)
                {
                    tamedNPC.isTamed = true;
                    tamedNPC.ownerPlayerID = Player.whoAmI;
                    tamedNPC.minionSlotCost = cost;
                    tamedNPC.invincibleTimer = 12;
                }

                npc.dontTakeDamage = true;
                npc.friendly = true;
                npc.netUpdate = true;

                LastSummonedType = rawNpcType;

                Main.NewText($"已召唤 {npc.GivenOrTypeName}", Color.LightGreen);
                return true;
            }

            return false;
        }

        public bool RemoveUnlockedNPC(int npcType)
        {
            if (UnlockedNPCTypes.Contains(npcType))
            {
                UnlockedNPCTypes.Remove(npcType);
                if (LastSummonedType == npcType) {
                    LastSummonedType = 0;
                }
                return true;
            }
            return false;
        }
    }
}