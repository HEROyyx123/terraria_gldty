using Terraria;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Buff 
{
    public class RedZapped : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 标记为 Debuff
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true; // 退出世界时不保存该 Debuff
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 这里可以添加持续伤害逻辑，如：
            npc.lifeRegen -= 20; 
        }
    }
}