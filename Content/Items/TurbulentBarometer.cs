using Microsoft.Xna.Framework;
using terraria_gldty.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Content.Items
{
    public class TurbulentBarometer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.UseSound = SoundID.Item66;
            Item.consumable = false;
        }

        public override bool? UseItem(Player player)
        {
            if (player.ZoneOverworldHeight)
            {
                AnomalyWeatherSystem.IsAnomalyActive = !AnomalyWeatherSystem.IsAnomalyActive;
                AnomalyWeatherSystem.EventProgress = 0; // 重置进度

                string message;
                Color msgColor;

                if (AnomalyWeatherSystem.IsAnomalyActive)
                {
                    if (Main.hardMode)
                    {
                        // 肉后开启播报（血红氛围）
                        message = "天空降下了猩红的血雨，狂暴的异界灾厄撕裂了大地！";
                        msgColor = new Color(255, 40, 40);
                    }
                    else
                    {
                        // 肉前开启播报（紫色氛围）
                        message = "大气的秩序断裂了，无数怪物伴随狂风暴雨和巨石从天而降！";
                        msgColor = new Color(175, 75, 255);
                    }
                }
                else
                {
                    // 手动取消播报
                    message = "混乱的气流逐渐平息……";
                    msgColor = new Color(100, 220, 100);
                }

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(message, msgColor);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    Terraria.Chat.ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral(message), msgColor);
                    NetMessage.SendData(MessageID.WorldData);
                }

                return true;
            }

            Main.NewText("该物品只能在地表使用！", Color.Red);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Cloud, 20)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.ShinyRedBalloon, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}