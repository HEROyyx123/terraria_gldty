using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Players
{
    public class SilverBracerGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // 保存敌人身上当前生效的鞭子标记 <鞭子射弹ID, 剩余帧数>
        public Dictionary<int, int> customWhipTags = new Dictionary<int, int>();

        public void AddTag(int whipProjType, int duration, int maxAllowed)
        {
            if (customWhipTags.ContainsKey(whipProjType))
            {
                // 已有该标记则刷新至最大持续时间（6秒）
                customWhipTags[whipProjType] = duration;
            }
            else
            {
                // 超出允许的最大标记数量时，移除最早加上的标记
                if (customWhipTags.Count >= maxAllowed)
                {
                    int oldestKey = -1;
                    foreach (var key in customWhipTags.Keys)
                    {
                        oldestKey = key;
                        break;
                    }
                    if (oldestKey != -1) customWhipTags.Remove(oldestKey);
                }

                customWhipTags.Add(whipProjType, duration);
            }
        }

        public override void AI(NPC npc)
        {
            // 在后台维护 6 秒倒计时
            if (customWhipTags.Count > 0)
            {
                List<int> keys = new List<int>(customWhipTags.Keys);
                foreach (int whipType in keys)
                {
                    customWhipTags[whipType]--;
                    if (customWhipTags[whipType] <= 0)
                    {
                        customWhipTags.Remove(whipType);
                    }
                }
            }
        }

        // 召唤物打中敌人时，同时叠加所有存活标记的伤害与暴击
        public override void ModifyHitByProjectile(NPC npc, Projectile proj, ref NPC.HitModifiers modifiers)
        {
            if ((proj.minion || proj.sentry || ProjectileID.Sets.MinionShot[proj.type]) && customWhipTags.Count > 0)
            {
                int totalFlatDamage = 0;
                float totalCritChance = 0f;

                foreach (var tag in customWhipTags)
                {
                    GetWhipStats(tag.Key, out int flatDmg, out float crit);
                    totalFlatDamage += flatDmg;
                    totalCritChance += crit;
                }

                // 叠加固定标记额外伤害
                modifiers.FlatBonusDamage += totalFlatDamage;

                // 判定与叠加暴击率加成
                if (totalCritChance > 0 && Main.rand.NextFloat() < totalCritChance)
                {
                    modifiers.SetCrit();
                }
            }
        }

        /// <summary>
        /// 原版鞭子标记参数表
        /// </summary>
        private void GetWhipStats(int whipProjType, out int flatDamage, out float critBonus)
        {
            flatDamage = 0;
            critBonus = 0f;

            switch (whipProjType)
            {
                case ProjectileID.BlandWhip: // 皮鞭
                    flatDamage = 4;
                    break;
                case ProjectileID.ThornWhip: // 荆棘鞭
                    flatDamage = 6;
                    break;
                case ProjectileID.BoneWhip: // 脊骨鞭
                    flatDamage = 7;
                    break;
                case ProjectileID.FireWhip: // 崩裂鞭
                    flatDamage = 12;
                    break;
                case ProjectileID.CoolWhip: // 寒霜鞭
                    flatDamage = 8;
                    break;
                case ProjectileID.SwordWhip: // 杜兰达尔
                    flatDamage = 9;
                    break;
                case ProjectileID.ScytheWhip: // 黑暗收割
                    flatDamage = 13;
                    break;
                case ProjectileID.MaceWhip: // 晨星
                    flatDamage = 16;
                    critBonus = 0.08f;
                    break;
                case ProjectileID.RainbowWhip: // 万花筒
                    flatDamage = 20;
                    critBonus = 0.12f;
                    break;
                default:
                    flatDamage = 5;
                    break;
            }
        }
    }
}