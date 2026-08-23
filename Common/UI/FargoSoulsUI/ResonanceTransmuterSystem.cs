using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using terraria_gldty.Common.Systems;
using terraria_gldty.Common.ModIntegration.FargoSoulsHelper; // 确保引用逻辑与 Helper 所在的命名空间

namespace terraria_gldty.Common.UI.FargoSoulsUI // 保证命名空间完全一致
{
    public class ResonanceTransmuterSystem : ModSystem
    {
        public override void PostSetupContent()
        {
            // 确保在所有 Mod 内容加载完毕后再扫描 Fargo 物品
            FargoSoulsHelper.Initialize();
        }
    }

    public static class TransmutationLogic
    {
        public static Item Transmute(Item inputItem, ref Item catalystItem, int targetType)
        {
            if (inputItem.IsAir) return new Item();

            var globalItem = inputItem.GetGlobalItem<BoundGlobalItem>();

            if (globalItem.BoundSourceType > 0)
            {
                Item revertedItem = new Item(globalItem.BoundSourceType, inputItem.stack);
                inputItem.TurnToAir();
                return revertedItem;
            }

            int inputGroup = FargoSoulsHelper.GetGroupType(inputItem.type);
            int targetGroup = FargoSoulsHelper.GetGroupType(targetType);

            if (inputGroup > 0 && inputGroup == targetGroup && !catalystItem.IsAir && catalystItem.stack >= 1)
            {
                catalystItem.stack -= 1;
                if (catalystItem.stack <= 0) catalystItem.TurnToAir();

                Item resultItem = new Item(targetType, inputItem.stack);
                
                var resultGlobal = resultItem.GetGlobalItem<BoundGlobalItem>();
                resultGlobal.BoundSourceType = inputItem.type;

                inputItem.TurnToAir();
                return resultItem;
            }

            return new Item();
        }

        public static List<int> GetAvailableTargets(Item inputItem)
        {
            List<int> targets = new();
            if (inputItem.IsAir) return targets;

            var globalItem = inputItem.GetGlobalItem<BoundGlobalItem>();

            if (globalItem.BoundSourceType > 0)
            {
                targets.Add(globalItem.BoundSourceType);
                return targets;
            }

            int group = FargoSoulsHelper.GetGroupType(inputItem.type);
            switch (group)
            {
                case 1: targets.AddRange(FargoSoulsHelper.PreHMEncList); break;
                case 2: targets.AddRange(FargoSoulsHelper.HMEncList); break;
                case 3: targets.AddRange(FargoSoulsHelper.ForcesList); break;
                case 4: targets.AddRange(FargoSoulsHelper.SoulsList); break;
            }

            targets.Remove(inputItem.type);
            return targets;
        }
    }
}