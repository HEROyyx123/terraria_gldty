using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using terraria_gldty.Common.Systems;

namespace terraria_gldty.Content.Items.Weapons
{
    public class TamedGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source) {
            // 判断弹幕是否由 NPC 发射
            if (source is Terraria.DataStructures.EntitySource_Parent parent && parent.Entity is NPC npc) {
                if (npc.GetGlobalNPC<TamedGlobalNPC>().isTamed) {
                    // 将弹幕转为召唤物/友方弹幕
                    projectile.friendly = true;
                    projectile.hostile = false;
                    projectile.trap = false;
                    projectile.DamageType = DamageClass.Summon;

                    // 获取主人的召唤伤害加成
                    int ownerID = npc.GetGlobalNPC<TamedGlobalNPC>().ownerPlayerID;
                    if (ownerID >= 0 && ownerID < Main.maxPlayers) {
                        Player owner = Main.player[ownerID];
                        projectile.owner = ownerID;
                        projectile.damage = (int)owner.GetTotalDamage(DamageClass.Summon).ApplyTo(projectile.damage);
                    }

                    // 技能索敌修正：如果怪物当前有攻击目标，将弹幕朝向目标敌人
                    if (npc.targetRect.Width > 0) {
                        Vector2 targetCenter = npc.targetRect.Center.ToVector2();
                        Vector2 dir = (targetCenter - projectile.Center).SafeNormalize(Vector2.Zero);
                        if (projectile.velocity != Vector2.Zero) {
                            projectile.velocity = dir * projectile.velocity.Length();
                        }
                    }
                }
            }
        }
    }
}
//旧版
// using Microsoft.Xna.Framework;
// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace TamedMinions.Common
// {
//     public class TamedGlobalProjectile : GlobalProjectile
//     {
//         public override bool InstancePerEntity => true;

//         public override void OnSpawn(Projectile projectile, Terraria.DataStructures.IEntitySource source) {
//             if (source is Terraria.DataStructures.EntitySource_Parent parentSource && parentSource.Entity is NPC npc) {
//                 if (npc.GetGlobalNPC<TamedGlobalNPC>().isTamed) {
//                     // 转变为友方召唤弹幕
//                     projectile.friendly = true;
//                     projectile.hostile = false;
//                     projectile.DamageType = DamageClass.Summon;
//                     projectile.trap = false;

//                     // 开启独立无敌帧 CD
//                     projectile.usesLocalNPCImmunity = true;
//                     projectile.localNPCHitCooldown = 15;
//                 }
//             }
//         }

//         // 【关键修复 3】：强行禁止驯服怪物发出的弹幕伤害玩家
//         public override bool CanHitPlayer(Projectile projectile, Player target) {
//             if (projectile.friendly) {
//                 return false;
//             }
//             return base.CanHitPlayer(projectile, target);
//         }

//         public override bool? CanHitNPC(Projectile projectile, NPC target) {
//             if (projectile.friendly && target.type == NPCID.TargetDummy) {
//                 return true;
//             }
//             return base.CanHitNPC(projectile, target);
//         }
//     }
// }