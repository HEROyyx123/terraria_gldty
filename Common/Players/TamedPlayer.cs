using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID; // 补充此命名空间，使 ContentSamples 生效
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using terraria_gldty.Common.Systems;
namespace terraria_gldty.Common.Players
{
    public class TamedPlayer : ModPlayer
    {
        public List<int> UnlockedNPCTypes = new List<int>();
        // 记录上一次召唤的怪物类型 (NetID)
        public int LastSummonedType = 0;

        public override void SaveData(TagCompound tag) {
            tag["UnlockedNPCTypes"] = UnlockedNPCTypes;
            tag["LastSummonedType"] = LastSummonedType;
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
        }

        public int GetNPCCost(int npcType) {
            // 安全读取 NPC 实例模板
            if (ContentSamples.NpcsByNetId.TryGetValue(npcType, out NPC sampleNPC)) {
                if (sampleNPC.defDamage > 120) return 3;
                if (sampleNPC.defDamage > 60) return 2;
            }
            return 1;
        }

        public bool TrySummonNPC(int npcType)
        {
            int cost = GetNPCCost(npcType);

            int npcIndex = NPC.NewNPC(
                Player.GetSource_Misc("TamedSummon"),
                (int)Player.Center.X,
                (int)Player.Center.Y,
                npcType
            );

            if (npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                
                if (npc.GetGlobalNPC<TamedGlobalNPC>() is TamedGlobalNPC tamedNPC) {
                    tamedNPC.isTamed = true;
                    tamedNPC.ownerPlayerID = Player.whoAmI;
                    tamedNPC.minionSlotCost = cost;
                }

                npc.friendly = true;
                npc.netUpdate = true;

                // 召唤成功后记录为上一次召唤的怪物
                LastSummonedType = npcType;

                Main.NewText($"已召唤 {npc.GivenOrTypeName} ", Color.LightGreen);
                return true;
            }

            return false;
        }

        public bool RemoveUnlockedNPC(int npcType)
        {
            if (UnlockedNPCTypes.Contains(npcType))
            {
                UnlockedNPCTypes.Remove(npcType);
                // 如果删除了当前选中的怪物，重置为 0
                if (LastSummonedType == npcType) {
                    LastSummonedType = 0;
                }
                return true;
            }
            return false;
        }
    }
}