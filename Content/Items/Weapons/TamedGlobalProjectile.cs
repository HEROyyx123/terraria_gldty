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
            if (source is Terraria.DataStructures.EntitySource_Parent parent && parent.Entity is NPC npc) {
                var tamedGlobal = npc.GetGlobalNPC<TamedGlobalNPC>();
                if (tamedGlobal.isTamed) {
                    projectile.friendly = true;
                    projectile.hostile = false;
                    projectile.trap = false;
                    projectile.DamageType = DamageClass.Summon;

                    int ownerID = tamedGlobal.ownerPlayerID;
                    if (ownerID >= 0 && ownerID < Main.maxPlayers) {
                        Player owner = Main.player[ownerID];
                        projectile.owner = ownerID;
                        
                        // 【修改】：使用 Scale(0.5f) 将召唤伤害加成倍率削弱至 50%
                        StatModifier summonModifier = owner.GetTotalDamage(DamageClass.Summon).Scale(0.5f);
                        projectile.damage = (int)summonModifier.ApplyTo(projectile.damage);
                    }

                    // 重新修正狙击弹、枪弹等所有弹幕的初始飞出矢量
                    if (npc.targetRect.Width > 0 && npc.targetRect.Height > 0) {
                        Vector2 targetCenter = npc.targetRect.Center.ToVector2();
                        Vector2 dir = (targetCenter - projectile.Center).SafeNormalize(Vector2.Zero);
                        
                        float speed = projectile.velocity.Length();
                        if (speed < 2f) speed = 12f;

                        projectile.velocity = dir * speed;
                        projectile.rotation = projectile.velocity.ToRotation();
                    }
                }
            }
        }
    }
}
