using Terraria;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;
namespace terraria_gldty.Content.Buff
{
    public class SpaceMoveBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // 在游戏暂停时仍更新，确保地图传送逻辑能正常响应
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false; // 不是 Debuff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Buff 存在时的标记逻辑（主要由 GlobalItem / Player 逻辑判断处理）
             if (player != null && player.active) {
                // 安全获取 ModPlayer 实例
                player.GetModPlayer<SpaceMovePlayer>().hasSpaceMoveBuff = true;
            }
        }
    }
}