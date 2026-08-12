using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Content.Buff
{
    public class LuminiteMinionBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarBar;

        public override void SetStaticDefaults() {
            Main.buffNoSave[Type] = true;  // 退出游戏不保存该 Buff
            Main.buffNoTimeDisplay[Type] = true; // 不显示倒计时（无限持续）
        }

        public override void Update(Player player, ref int buffIndex) {
            // 如果玩家拥有对应的召唤物弹幕，则刷新 Buff 持续时间
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Items.Weapons.LuminiteMinionProj>()] > 0) {
                player.buffTime[buffIndex] = 18000;
            }
            else {
                // 如果没有召唤物了，清除 Buff
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}