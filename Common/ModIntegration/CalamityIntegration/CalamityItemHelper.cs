using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration.CalamityIntegration
{
    /// <summary>
    /// 灾厄物品引用工具类
    /// 统一管理跨 Mod 物品和物块引用
    /// </summary>
    public static class CalamityItemHelper
    {
        private static Mod _calamityMod;
        private static bool _initialized = false;

        private static readonly Dictionary<string, int> _itemCache = new();
        private static readonly Dictionary<string, int> _tileCache = new();

        public static void Initialize(Mod calamityMod) {
            if (_initialized) return;
            _calamityMod = calamityMod;
            _initialized = true;

            if (_calamityMod == null) {
                ModContent.GetInstance<terraria_gldty>().Logger.Warn("CalamityItemHelper: 未检测到灾厄模组，物品查询将返回 0");
            }
        }

        /// <summary> 根据内部名称获取灾厄物品的 Type </summary>
        public static int GetItemType(string itemName) {
            if (_calamityMod == null) return 0;

            if (_itemCache.TryGetValue(itemName, out int cachedType))
                return cachedType;

            try {
                if (_calamityMod.TryFind<ModItem>(itemName, out ModItem modItem)) {
                    _itemCache[itemName] = modItem.Type;
                    return modItem.Type;
                }
            }
            catch (Exception) { }

            _itemCache[itemName] = 0;
            return 0;
        }

        /// <summary> 根据内部名称获取灾厄物块的 Type </summary>
        public static int GetTileType(string tileName) {
            if (_calamityMod == null) return 0;

            if (_tileCache.TryGetValue(tileName, out int cachedType))
                return cachedType;

            try {
                if (_calamityMod.TryFind<ModTile>(tileName, out ModTile modTile)) {
                    _tileCache[tileName] = modTile.Type;
                    return modTile.Type;
                }
            }
            catch (Exception) { }

            _tileCache[tileName] = 0;
            return 0;
        }
    }
}