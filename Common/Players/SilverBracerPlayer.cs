using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Players
{
    public class SilverBracerPlayer : ModPlayer
    {
        public bool hasSilverBracer;
        public int maxWhipTags = 1;
        public float whipTagDurationMult = 1.0f;

        // 保存玩家身上处于 6 秒管理状态下的鞭子 Buff <BuffID, 剩余帧数>
        private Dictionary<int, int> activeWhipBuffs = new Dictionary<int, int>();

        // 临时记录击中前玩家拥有的 Buff 快照
        private List<int> buffsBeforeHit = new List<int>();

        public override void ResetEffects()
        {
            hasSilverBracer = false;
            maxWhipTags = 1;
            whipTagDurationMult = 1.0f;
        }

        // 1. 攻击判定前记录 Buff 快照
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hasSilverBracer && ProjectileID.Sets.IsAWhip[proj.type])
            {
                buffsBeforeHit.Clear();
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    if (Player.buffType[i] > 0)
                    {
                        buffsBeforeHit.Add(Player.buffType[i]);
                    }
                }
            }
        }

        // 2. 命中结算：更新 NPC 标记与玩家 UI Buff 时间
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ProjectileID.Sets.IsAWhip[proj.type])
            {
                int duration = (int)(180 * whipTagDurationMult); // 360 帧 / 6 秒

                // A. 处理 NPC 端底层多标记与持续时间刷新
                var whipGlobal = target.GetGlobalNPC<SilverBracerGlobalNPC>();
                whipGlobal.AddTag(proj.type, duration, maxWhipTags);

                // B. 刷新/注册玩家端 UI Buff
                if (hasSilverBracer)
                {
                    for (int i = 0; i < Player.MaxBuffs; i++)
                    {
                        int currentBuff = Player.buffType[i];
                        if (currentBuff <= 0) continue;

                        // 情况 1: 本次命中产生的新 Buff (不在击中前的列表中)
                        // 情况 2: 已经是当前正在管理的鞭子 Buff (即使击中前就存在，也要刷新持续时间)
                        if (!buffsBeforeHit.Contains(currentBuff) || activeWhipBuffs.ContainsKey(currentBuff))
                        {
                            RegisterOrUpdateWhipBuff(currentBuff, duration);
                        }
                    }
                }
            }
        }

        private void RegisterOrUpdateWhipBuff(int buffId, int duration)
        {
            // 如果已在管理队列中，直接将时间强行重置/刷新回 6 秒 (360帧)
            if (activeWhipBuffs.ContainsKey(buffId))
            {
                activeWhipBuffs[buffId] = duration;
            }
            else
            {
                // 超出最大允许数量时，按 FIFO 移除最早的 Buff
                if (activeWhipBuffs.Count >= maxWhipTags)
                {
                    int oldestBuff = -1;
                    foreach (var key in activeWhipBuffs.Keys)
                    {
                        oldestBuff = key;
                        break;
                    }
                    if (oldestBuff != -1)
                    {
                        activeWhipBuffs.Remove(oldestBuff);
                        int index = Player.FindBuffIndex(oldestBuff);
                        if (index != -1) Player.DelBuff(index);
                    }
                }

                activeWhipBuffs.Add(buffId, duration);
            }
        }

        // 3. 每帧维持 Buff 并向 Player 强制同步剩余帧数
        public override void PostUpdateBuffs()
        {
            if (activeWhipBuffs.Count > 0)
            {
                List<int> keys = new List<int>(activeWhipBuffs.Keys);
                foreach (int buffId in keys)
                {
                    activeWhipBuffs[buffId]--;

                    if (activeWhipBuffs[buffId] <= 0)
                    {
                        // 6 秒倒计时结束，清除 Buff
                        activeWhipBuffs.Remove(buffId);
                        int index = Player.FindBuffIndex(buffId);
                        if (index != -1) Player.DelBuff(index);
                    }
                    else
                    {
                        // 强制将最新的倒计时数据更新给原版 Player 系统，确保 UI 显示持续刷新
                        Player.AddBuff(buffId, activeWhipBuffs[buffId], quiet: true);
                    }
                }
            }
        }
    }
}