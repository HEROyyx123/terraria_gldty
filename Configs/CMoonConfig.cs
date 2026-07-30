using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace terraria_gldty.Configs // 确保命名空间适合你的项目结构
{
    // Mode 设置为 ServerSide（服务端/单人游戏通用配置）
    public class CMoonConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // 添加一个 Bool 类型的配置开关，并设置默认值为 false
        [DefaultValue(false)]
        public bool DisableVerticalVelocity;
    }
}