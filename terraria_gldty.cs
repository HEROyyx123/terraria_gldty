using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace terraria_gldty
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class terraria_gldty : Mod
    {
        public override void Load() {
            // 初始化自定义合成条件
            Common.Systems.PackRecipeConditions.Initialize();
        }
    }
}