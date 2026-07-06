using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration
{
    /// <summary>
    /// 模组联动系统的中央调度器。
    /// 这是"把所有订阅mod打包成一个文件"的核心——所有联动模组通过此类注册和管理。
    /// 新增联动模组只需：实现 IModIntegration → 在 RegisterAll 中加一行。
    /// </summary>
    public static class ModIntegrationSystem
    {
        private static readonly List<IModIntegration> Integrations = new();

        /// <summary> 所有已注册的 flag key（用于 ModIntegrationPlayer 的 LoadData） </summary>
        private static readonly List<string> AllFlagKeys = new();

        /// <summary> 获取所有已加载的联动模组列表 </summary>
        public static IReadOnlyList<IModIntegration> ActiveIntegrations =>
            Integrations.Where(i => i.IsLoaded).ToList();

        /// <summary> 注册并初始化一个联动模组 </summary>
        public static void RegisterIntegration(IModIntegration integration) {
            Integrations.Add(integration);

            // 检测目标 Mod 是否存在
            integration.IsLoaded = ModLoader.TryGetMod(integration.TargetModName, out _);

            if (integration.IsLoaded) {
                // 加载引用和条件
                integration.Load();

                // 注册 PackKeys
                var entries = integration.GetPackEntries();
                if (entries != null) {
                    Systems.PackLootRegistry.AddPackKeys(entries);
                }

                // 收集 flag keys
                var flags = integration.GetPlayerFlagKeys();
                if (flags != null) {
                    AllFlagKeys.AddRange(flags);
                }
            }
        }

        /// <summary> 获取所有已注册的 flag keys（用于 ModIntegrationPlayer 存档） </summary>
        public static IEnumerable<string> GetAllFlagKeys() => AllFlagKeys;

        /// <summary> 获取所有联动模组的 PackKeys（合并到一个字典） </summary>
        public static Dictionary<string, string> GetAllPackKeys() {
            var result = new Dictionary<string, string>();
            foreach (var integration in ActiveIntegrations) {
                var entries = integration.GetPackEntries();
                if (entries != null) {
                    foreach (var kvp in entries) {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }
            return result;
        }

        /// <summary> 调用所有联动模组的 ModifyExistingPackLoot（增强原版礼包） </summary>
        public static void ModifyAllExistingPacks(string packKey, ItemLoot itemLoot) {
            foreach (var integration in ActiveIntegrations) {
                integration.ModifyExistingPackLoot(packKey, itemLoot);
            }
        }

        /// <summary> 构建完整的 GuideBook 提示列表（原版 + 所有联动模组） </summary>
        public static List<(string action, string flagKey, string hintKey)> BuildGuideHints() {
            var hints = new List<(string, string, string)>
            {
                // 原版礼包提示顺序
                ("check", nameof(Players.PackPlayer.ReceivedKingSlimePack), "Mods.terraria_gldty.Items.GuideBook.HintKingSlimePack"),
                ("check", nameof(Players.PackPlayer.ReceivedEyeOfCthulhuPack), "Mods.terraria_gldty.Items.GuideBook.HintEyeOfCthulhuPack"),
                ("check", nameof(Players.PackPlayer.ReceivedEvilBossPack), "Mods.terraria_gldty.Items.GuideBook.HintEvilBossPack"),
                ("check", nameof(Players.PackPlayer.ReceivedHardmodePack), "Mods.terraria_gldty.Items.GuideBook.HintHardmodePack"),
                ("check", nameof(Players.PackPlayer.ReceivedMechBossPack), "Mods.terraria_gldty.Items.GuideBook.HintMechBossPack"),
                ("check", nameof(Players.PackPlayer.ReceivedJunglePack), "Mods.terraria_gldty.Items.GuideBook.HintJunglePack"),
                ("check", nameof(Players.PackPlayer.ReceivedSpookyWoodPack), "Mods.terraria_gldty.Items.GuideBook.HintSpookyWoodPack"),
                ("check", nameof(Players.PackPlayer.ReceivedBeetlePack), "Mods.terraria_gldty.Items.GuideBook.HintBeetlePack"),
                ("check", nameof(Players.PackPlayer.ReceivedCultistPack), "Mods.terraria_gldty.Items.GuideBook.HintCultistPack"),
                ("check", nameof(Players.PackPlayer.ReceivedMoonLordPack), "Mods.terraria_gldty.Items.GuideBook.HintMoonLordPack"),
            };

            // 各联动模组插入自己的提示
            foreach (var integration in ActiveIntegrations) {
                integration.AppendGuideHints(hints);
            }

            return hints;
        }
    }
}