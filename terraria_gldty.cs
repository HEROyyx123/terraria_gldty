using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace terraria_gldty
{
    public class terraria_gldty : Mod
    {
        public override void Load() {
            // 初始化自定义合成条件
            Common.Systems.PackRecipeConditions.Initialize();
        }

        /// <summary>
        /// 供其他 Mod 调用的接口（通过 ModLoader.GetMod("terraria_gldty").Call(...) 调用）
        ///
        /// 支持的方法：
        ///   Call("GetPackKeys")
        ///       → 返回所有礼包 Key 列表
        ///
        ///   Call("GetPackClassName", "hardmode")
        ///       → 返回 "HardmodePack"
        ///
        ///   Call("OverridePackContents", "hardmode", Action{ItemLoot} callback)
        ///       → 注册覆盖掉落规则，此礼包将使用 callback 代替默认内容
        /// </summary>
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

                default:
                    return null;
            }
        }
    }
}