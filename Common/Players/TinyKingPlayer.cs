using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Buff;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace terraria_gldty.Common.Players
{
    public class TinyKingPlayer : ModPlayer
    {
        public bool hasTinyKingEffect;
        public override void ResetEffects() {
            hasTinyKingEffect = false;
        }

        // 辅助方法：判断 NPC 是否为女性
        private bool IsFemaleNPC(NPC npc)
        {   
            
            // 1. 原版女性 Town NPC 列表
            int[] femaleNPCIDs = new int[]
            {
                NPCID.Nurse,            // 护士
                NPCID.Dryad,            // 树妖
                NPCID.Mechanic,         // 机械师
                NPCID.PartyGirl,        // 派对女孩
                NPCID.Stylist,          // 发型师
                NPCID.WitchDoctor,      // 巫医 (原版标记为 male，但可根据偏好调整)
                NPCID.Steampunker,      // 蒸汽朋克人
                NPCID.Princess,         // 公主
                NPCID.BestiaryGirl      // 动物学家 (ZooKeeper)
            };

            // 检查是否在原版女性列表中
            foreach (int id in femaleNPCIDs)
            {
                if (npc.type == id) return true;
            }

            // 2. 兼容性：如果其他 Mod 的 NPC 在其内部或设置中包含了女声/女性标记（可选扩展）
            // 在大部分情况下，仅匹配原版 + 自定义 Mod NPC ID 即可

            return false;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (!hasTinyKingEffect) return true;// 如果没有 Buff，正常死亡

            int buffType = ModContent.BuffType<TinyKingBuff>();

            // 检查玩家是否拥有“小小的王”Buff
            if (Player.HasBuff(buffType))
            {
                // 1. 寻找当前世界中所有存活且活跃的女性 NPC
                List<int> femaleNpcIndices = new List<int>();

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    
                    // 必须是活跃的、Town NPC，且通过女性判定
                    if (npc.active && npc.townNPC && IsFemaleNPC(npc))
                    {
                        femaleNpcIndices.Add(i);
                    }
                }

                // 2. 如果存在至少一个女性 NPC，触发伤害转移
                if (femaleNpcIndices.Count > 0)
                {
                    int randomIndex = Main.rand.Next(femaleNpcIndices.Count);
                    NPC targetNpc = Main.npc[femaleNpcIndices[randomIndex]];

                    // 转移致死伤害：直接对 NPC 造成其最大生命值倍数的伤害，确保击杀
                    int transferDamage = targetNpc.lifeMax * 2;
                    
                    targetNpc.StrikeNPC(new NPC.HitInfo
                    {
                        Damage = transferDamage,
                        Knockback = 0f,
                        HitDirection = 0,
                        Crit = false
                    });

                    // 3. 抵消玩家死亡
                    Player.statLife = 1; 
                    Player.immune = true;
                    Player.immuneTime = 120; // 2秒无敌

                    // 4. 移除 Buff
                    Player.ClearBuff(buffType);

                    // 5. 提示文本
                    CombatText.NewText(Player.getRect(), Microsoft.Xna.Framework.Color.Purple, "小小的王权能发动！", true);
                    CombatText.NewText(targetNpc.getRect(), Microsoft.Xna.Framework.Color.Purple, "你付出了一个代价！", true);
                    // 6. 播放音效
                    // =================【1. 播放音效强化】=================
                    // 播放玩家侧音效
                    SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.2f, Volume = 0.9f }, Player.Center);
                    
                    // 播放 NPC 侧音效：雷击/重击/心碎声
                    SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 1f }, targetNpc.Center);
                    //**************************************************************
                    return false; // 拦截死亡
                }
            }
            //若无触发移除 Buff
            Player.ClearBuff(buffType);
            SoundEngine.PlaySound(SoundID.NPCDeath59 with { Pitch = -0.2f, Volume = 0.9f }, Player.Center);
            CombatText.NewText(Player.getRect(), Microsoft.Xna.Framework.Color.Purple, "权能拒绝了你！", true);
            return true; // 无转移目标或无 Buff，正常死亡
        }
    }
}