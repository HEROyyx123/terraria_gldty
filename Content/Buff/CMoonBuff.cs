using Terraria;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;
namespace terraria_gldty.Content.Buff
{
    public class CMoonBuff : ModBuff
    {
        public override void SetStaticDefaults() {
            Main.buffNoSave[Type] = false; // 退出游戏是否保存 Buff
            Main.buffNoTimeDisplay[Type] = false; // 是否显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex) {
            // 将标识同步给 ModPlayer 逻辑处理
            if (player != null && player.active) {
                // 安全获取 ModPlayer 实例
                player.GetModPlayer<CMoonPlayer>().hasCMoonEffect = true;
            }
        }
    }
}