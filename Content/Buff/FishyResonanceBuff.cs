using Terraria;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;
namespace terraria_gldty.Content.Buff
{
    public class FishyResonanceBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.persistentBuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 激活玩家类中的崴脚机制
            player.GetModPlayer<BambooCicadaPlayer>().hasFishyResonance = true;

            // 正面属性：提升 10% 移动速度与近战攻速
            player.moveSpeed += 0.10f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
        }
    }
}
