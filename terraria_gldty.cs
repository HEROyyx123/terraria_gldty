using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace terraria_gldty
{
    public class terraria_gldty : Mod
    {
        public override void Load() {
            Common.Systems.PackRecipeConditions.Initialize();
            Common.ModIntegration.ModIntegrationSystem.RegisterIntegration(
                new Common.ModIntegration.CalamityIntegration.CalamityIntegration()
            );
        }



        public override object Call(params object[] args) {
            if (args == null || args.Length == 0)
                return null;

            string method = args[0] as string;

            switch (method) {
                case "GetPackKeys":
                    return new List<string>(Common.Systems.PackLootRegistry.PackKeys.Keys);

                case "GetPackClassName":
                    if (args.Length > 1 && args[1] is string key) {
                        return Common.Systems.PackLootRegistry.GetPackClassName(key);
                    }
                    return null;

                case "OverridePackContents":
                    if (args.Length > 2 && args[1] is string packKey && args[2] is Action<Terraria.ModLoader.ItemLoot> action) {
                        Common.Systems.PackLootRegistry.OverrideLoot[packKey] = action;
                        return true;
                    }
                    return false;

                case "IsModLoaded":
                    if (args.Length > 1 && args[1] is string modName) {
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