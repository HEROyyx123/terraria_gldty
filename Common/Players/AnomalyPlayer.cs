using Terraria;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Players
{
    public class AnomalyPlayer : ModPlayer
    {
        public bool hasContactDamageReduction = false;

        public override void ResetEffects()
        {
            hasContactDamageReduction = false;
        }

        // 当 NPC 接触碰撞玩家引发伤害时触发
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (hasContactDamageReduction)
            {
                modifiers.SourceDamage *= 0.85f; // 接触伤害降低 15%
            }
        }
    }
}