using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    public class AnomalyGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        private bool hasLanded = false;

        public override void AI(NPC npc)
        {
            if (!hasLanded && (npc.velocity.Y == 0 || npc.collideY))
            {
                hasLanded = true;
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.DemonTorch, Main.rand.NextFloat(-2f, 2f), -1f);
                }
            }
        }

        // 击杀事件怪物增加事件进度
        public override void OnKill(NPC npc)
        {
            if (AnomalyWeatherSystem.IsAnomalyActive && npc.lifeMax > 5 && !npc.friendly)
            {
                AnomalyWeatherSystem.AddEventProgress(1);
            }
        }
    }
}