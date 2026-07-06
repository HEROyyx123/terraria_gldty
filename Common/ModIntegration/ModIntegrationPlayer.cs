using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.ModIntegration
{
    /// <summary>
    /// 联动模组专用的 ModPlayer，用 Dictionary 存储所有联动模组的礼包状态
    /// 避免每次新增联动模组都要修改 PackPlayer.cs
    /// </summary>
    public class ModIntegrationPlayer : ModPlayer
    {
        /// <summary> Flag Key → 是否已领取 </summary>
        public Dictionary<string, bool> IntegrationFlags { get; private set; } = new();

        public bool GetFlag(string key) => IntegrationFlags.TryGetValue(key, out var val) && val;
        public void SetFlag(string key, bool value = true) => IntegrationFlags[key] = value;

        public override void SaveData(TagCompound tag) {
            foreach (var kvp in IntegrationFlags) {
                if (kvp.Value) {
                    tag[kvp.Key] = true;
                }
            }
        }

        public override void LoadData(TagCompound tag) {
            IntegrationFlags.Clear();
            // 只加载已注册的标记（在 Load 阶段已由 ModIntegrationSystem 注册）
            foreach (var key in ModIntegrationSystem.GetAllFlagKeys()) {
                IntegrationFlags[key] = tag.ContainsKey(key);
            }
        }
    }
}