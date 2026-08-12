using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration
{
    public class MagicStorageIntegration : IModIntegration
    {
        public string TargetModName => "MagicStorage";
        public bool IsLoaded { get; set; }

        // 安全缓存 MagicStorage 物品的 Item ID
        public int StorageHeartId { get; private set; }
        public int StorageUnitId { get; private set; }
        public int CraftingAccessId { get; private set; }
        public int StorageComponentId { get; private set; }
        public int EnvironmentAccessId { get; private set; }

        public void Load() {
            if (!ModLoader.TryGetMod("MagicStorage", out Mod magicStorage))
                return;

            // 安全获取 MagicStorage 核心物品 ID
            if (magicStorage.TryFind<ModItem>("StorageHeart", out var heart))
                StorageHeartId = heart.Type;

            if (magicStorage.TryFind<ModItem>("StorageUnit", out var unit))
                StorageUnitId = unit.Type;

            if (magicStorage.TryFind<ModItem>("CraftingAccess", out var crafting))
                CraftingAccessId = crafting.Type;

            if (magicStorage.TryFind<ModItem>("StorageComponent", out var component))
                StorageComponentId = component.Type;
                
            if (magicStorage.TryFind<ModItem>("EnvironmentAccess", out var configAccess))
                EnvironmentAccessId = configAccess.Type;
        }

        public Dictionary<string, string> GetPackEntries() {
            return new Dictionary<string, string>
            {
                // Key: 礼包唯一标识 Key, Value: 对应礼包物品类名
                { "MagicStoragePack", "MagicStoragePacks" }
            };
        }

        public string[] GetPlayerFlagKeys() {
            return new string[]
            {
                // 玩家是否已领取 MagicStorage 礼包的标志 Key
                "ReceivedMagicStoragePack"
            };
        }

        public void ModifyExistingPackLoot(string packKey, ItemLoot itemLoot) {
            // 如果你还想在原版 Boss 礼包里额外送几个存储单元，可以在这里追加：
            /*
            if (packKey == "ReceivedEvilBossPack" && StorageUnitId > 0) {
                itemLoot.Add(ItemDropRule.Common(StorageUnitId, chanceDenominator: 1, minimumDropped: 4, maximumDropped: 4));
            }
            */
        }

        public void AppendGuideHints(List<(string action, string flagKey, string hintKey)> hints) {
            // 在 GuideBook 计划书中插入提示
            hints.Add(("check", "ReceivedMagicStoragePack", "Mods.terraria_gldty.Items.GuideBook.HintMagicStoragePack"));
        }
    }
}