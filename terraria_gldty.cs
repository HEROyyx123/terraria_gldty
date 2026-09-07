using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace terraria_gldty
{
    public class terraria_gldty : Mod
    {
        public override void Load()
        {
            Common.Systems.PackRecipeConditions.Initialize();
            Common.ModIntegration.ModIntegrationSystem.RegisterIntegration(
                new Common.ModIntegration.CalamityIntegration.CalamityIntegration()
            );
            Common.ModIntegration.ModIntegrationSystem.RegisterIntegration(new Common.ModIntegration.MagicStorageIntegration());
        }

    //     public override void PostSetupContent()
    //     {
    //         if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
    //             return;

    //         List<int> eventNPCs =
    //             Common.Systems.AnomalyWeatherSystem.GetBossChecklistNPCs();

    //         List<int> loot = new List<int>
    // {
    //     ModContent.ItemType<Content.Items.AnomalyAccessory1>(),
    //     ModContent.ItemType<Content.Items.AnomalyAccessory2>()
    // };

    //         Dictionary<string, object> extraData =
    //             new Dictionary<string, object>
    //             {
    //                 ["displayName"] =
    //                     Language.GetOrRegister(
    //                         "Mods.terraria_gldty.BossChecklistIntegration.AnomalyWeather.EntryName",
    //                         () => "异界狂雨"
    //                     ),

    //                 ["spawnInfo"] =
    //                     Language.GetOrRegister(
    //                         "Mods.terraria_gldty.BossChecklistIntegration.AnomalyWeather.SpawnInfo",
    //                         () =>
    //                             "在地表使用恐怖气球。\n" +
    //                             "天空将降下异常暴雨与危险生物。\n" +
    //                             "击杀事件生成的敌怪以推进事件进度。"
    //                     ),

    //                 ["despawnMessage"] =
    //                     Language.GetOrRegister(
    //                         "Mods.terraria_gldty.BossChecklistIntegration.AnomalyWeather.DespawnMessage",
    //                         () => "异界狂雨已经平息……"
    //                     ),

    //                 ["spawnItems"] = new List<int>
    //                 {
    //             ModContent.ItemType<Content.Items.TurbulentBarometer>()
    //                 },

    //                 // 注意：这里必须是 loot，不是 collectibles
    //                 ["loot"] = loot,

    //                 ["customPortrait"] =
    //                     (Action<SpriteBatch, Rectangle, Color>)
    //                     ((spriteBatch, rect, color) =>
    //                     {
    //                         Texture2D texture =
    //                             ModContent.Request<Texture2D>(
    //                                 "terraria_gldty/Content/Items/TurbulentBarometer"
    //                             ).Value;

    //                         float scale = Math.Min(
    //                             (float)rect.Width / texture.Width,
    //                             (float)rect.Height / texture.Height
    //                         );

    //                         Vector2 drawPosition =
    //                             new Vector2(
    //                                 rect.Center.X,
    //                                 rect.Center.Y
    //                             );

    //                         spriteBatch.Draw(
    //                             texture,
    //                             drawPosition,
    //                             null,
    //                             color,
    //                             0f,
    //                             texture.Size() / 2f,
    //                             scale,
    //                             SpriteEffects.None,
    //                             0f
    //                         );
    //                     })
    //             };

    //         bossChecklist.Call(
    //             "LogEvent",
    //             this,
    //             "AnomalyWeather",
    //             2.5f,

    //             // 是否已经完成过一次
    //             (Func<bool>)(() =>
    //                 Common.Systems.AnomalyWeatherSystem.HasCompletedAnomalyEvent
    //             ),

    //             // 关键：这里不再为空
    //             eventNPCs,

    //             extraData
    //         );
    //     }

        public override object Call(params object[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            string method = args[0] as string;

            switch (method)
            {
                case "GetPackKeys":
                    return new List<string>(Common.Systems.PackLootRegistry.PackKeys.Keys);

                case "GetPackClassName":
                    if (args.Length > 1 && args[1] is string key)
                    {
                        return Common.Systems.PackLootRegistry.GetPackClassName(key);
                    }
                    return null;

                case "OverridePackContents":
                    if (args.Length > 2 && args[1] is string packKey && args[2] is Action<Terraria.ModLoader.ItemLoot> action)
                    {
                        Common.Systems.PackLootRegistry.OverrideLoot[packKey] = action;
                        return true;
                    }
                    return false;

                case "IsModLoaded":
                    if (args.Length > 1 && args[1] is string modName)
                    {
                        return ModLoader.TryGetMod(modName, out _);
                    }
                    return false;

                case "GetModIntegrationKeys":
                    return Common.ModIntegration.ModIntegrationSystem.GetAllPackKeys();

                default:
                    return null;
            }
        }
    }
}