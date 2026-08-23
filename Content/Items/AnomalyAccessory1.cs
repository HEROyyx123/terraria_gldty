using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players; // 引入刚才创建的 ModPlayer 命名空间

namespace terraria_gldty.Content.Items
{
    public class AnomalyAccessory1 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 1, 50, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) -= 0.20f; // 造成的伤害减少 20%
            player.lifeRegen += 4;                          // 增加生命再生
            
            // 开启受到的接触伤害降低 15%
            player.GetModPlayer<AnomalyPlayer>().hasContactDamageReduction = true;
        }
    }
}