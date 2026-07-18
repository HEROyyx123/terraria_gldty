using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    /// <summary>
    /// 提供 Mod 用到的自定义合成条件
    /// </summary>
    public class PackRecipeConditions
    {
        /// <summary> 已击败世界吞噬者 或 克苏鲁之脑 </summary>
        public static Condition DownedEvilBoss { get; private set; }

        /// <summary> 已击败史莱姆王 </summary>
        public static Condition DownedSlimeKing { get; private set; }

        /// <summary> 已击败血肉墙 </summary>
        public static Condition DownedWallOfFlesh { get; private set; }

        public static void Initialize() {
            DownedEvilBoss = new Condition(
                "Mods.terraria_gldty.Conditions.DownedEvilBoss",
                () => NPC.downedBoss2 || NPC.downedBoss3  // EoW = downedBoss2, BoC = downedBoss3
            );

            DownedSlimeKing = new Condition(
                "Mods.terraria_gldty.Conditions.DownedSlimeKing",
                () => NPC.downedSlimeKing
            );

            DownedWallOfFlesh = new Condition(
                "Mods.terraria_gldty.Conditions.DownedWallOfFlesh",
                () => Main.hardMode
            );
        }
    }
}