using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Items.Weapons;

namespace terraria_gldty.Content.Buff
{
    public class AuricMinionBuff : ModBuff
    {
        // 替换为原版铜短剑贴图
        public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;

        public override void SetStaticDefaults() {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<AuricMinionProj>()] > 0) {
                player.buffTime[buffIndex] = 18000;
            }
            else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}