using Terraria;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Content.Buff
{
    public class TinyKingBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // 设定 Buff 名称和描述
            // DisplayName.SetDefault("小小的王");
            // Description.SetDefault("致命伤害将转移至随机一位女性NPC");
            
            Main.buffNoSave[Type] = false; // 退出世界是否保存
            Main.debuff[Type] = false;     // 是否为 Debuff
            Main.pvpBuff[Type] = true;     // PvP 中生效
            Main.buffNoTimeDisplay[Type] = false; // 显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Buff 存在时的标记逻辑（主要由 GlobalItem / Player 逻辑判断处理）
             if (player != null && player.active) {
                // 安全获取 ModPlayer 实例
                player.GetModPlayer<TinyKingPlayer>().hasTinyKingEffect = true;
            }

        }
    }
}