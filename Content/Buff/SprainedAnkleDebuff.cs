using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.ID; // 引入 DustID

using terraria_gldty.Common.Players;
namespace terraria_gldty.Content.Buff
{
    public class SprainedAnkleDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {   
            // 激活玩家类中的崴脚机制
            player.GetModPlayer<BambooCicadaPlayer>().hasSprainedAnkleDebuff= true;
            // 移动速度降低 85%
            player.moveSpeed *= 0.15f;
            player.maxRunSpeed *= 0.15f;

            // 禁用冲刺
            player.dashDelay = 30;

        }
    }
}
