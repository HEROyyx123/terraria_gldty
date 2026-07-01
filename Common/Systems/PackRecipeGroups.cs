using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Systems
{
    /// <summary>
    /// 注册 Mod 用到的合成配方组
    /// </summary>
    public class PackRecipeGroups : ModSystem
    {
        public override void AddRecipeGroups() {
            // 四种职业徽章任意一种
            var anyEmblem = new RecipeGroup(
                () => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.WarriorEmblem)}",
                ItemID.WarriorEmblem,
                ItemID.RangerEmblem,
                ItemID.SorcererEmblem,
                ItemID.SummonerEmblem
            );
            RecipeGroup.RegisterGroup("terraria_gldty:AnyEmblem", anyEmblem);
        }
    }
}