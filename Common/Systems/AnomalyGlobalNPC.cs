using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    public class AnomalyGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // 是否由异变事件生成
        public bool SpawnedByAnomaly;

        private bool hasLanded = false;

        public override void AI(NPC npc)
        {
            // 只有异变事件生成的怪才播放这个降落特效
            if (!SpawnedByAnomaly)
                return;

            if (!hasLanded &&
                (npc.velocity.Y == 0 || npc.collideY))
            {
                hasLanded = true;

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDust(
                            npc.position,
                            npc.width,
                            npc.height,
                            DustID.DemonTorch,
                            Main.rand.NextFloat(-2f, 2f),
                            -1f
                        );
                    }
                }
            }
        }

        public override void OnKill(NPC npc)
        {
            // 客户端不能推进世界事件
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // 必须是异变事件生成的怪
            if (!SpawnedByAnomaly)
                return;

            // 事件已经结束，就不要再增加进度
            if (!AnomalyWeatherSystem.IsAnomalyActive)
                return;

            if (npc.lifeMax <= 5)
                return;

            if (npc.friendly)
                return;

            AnomalyWeatherSystem.AddEventProgress(1);
        }
    }
}