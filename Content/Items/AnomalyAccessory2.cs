using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class AnomalyAccessory2 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 4, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.endurance += 0.20f; // 受到的所有伤害降低 20%
            player.GetDamage(DamageClass.Generic) -= 0.10f; // 全伤害减少 10%
            player.lifeRegen += 12; // 大幅增加生命再生
        }
    }
}