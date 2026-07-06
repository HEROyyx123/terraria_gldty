using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace terraria_gldty.Common.ModIntegration
{
    /// <summary>
    /// 模组联动接口 — 每个被联动的 Mod（如 CalamityMod）实现此接口
    /// 实现后通过 ModIntegrationSystem.RegisterIntegration() 注册即可
    /// </summary>
    public interface IModIntegration
    {
        /// <summary> 目标 Mod 的内部名称，如 "CalamityMod" </summary>
        string TargetModName { get; }

        /// <summary> 目标 Mod 是否已加载（由系统检测，实现类不需要额外逻辑）</summary>
        bool IsLoaded { get; set; }

        /// <summary> 加载时调用：初始化物品/物块引用、合成条件 </summary>
        void Load();

        /// <summary> 返回此联动模组的所有礼包 Key → ClassName 映射 </summary>
        Dictionary<string, string> GetPackEntries();

        /// <summary> 返回此联动模组的所有存档标记名列表 </summary>
        string[] GetPlayerFlagKeys();

        /// <summary> 在原版礼包打开时调用，可向其中添加额外物品 </summary>
        void ModifyExistingPackLoot(string packKey, ItemLoot itemLoot);

        /// <summary> 向 GuideBook 的计划书流程中追加此联动模组的提示（key 和 flag 检查） </summary>
        void AppendGuideHints(List<(string action, string flagKey, string hintKey)> hints);
    }
}