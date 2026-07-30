using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.UI.Globals
{
    public class CustomDamageTypeItem : GlobalItem
    {
        // 标记当前物品修改后的伤害类型 ID (-1 表示未修改，使用原版默认)
        public int OverrideDamageTypeIndex = -1;

        // 必须开启 InstancePerEntity，保证每个物品实例拥有独一无二的数据
        public override bool InstancePerEntity => true;

        public override GlobalItem Clone(Item item, Item itemClone) {
            CustomDamageTypeItem myClone = (CustomDamageTypeItem)base.Clone(item, itemClone);
            myClone.OverrideDamageTypeIndex = OverrideDamageTypeIndex;
            return myClone;
        }

        public override void SaveData(Item item, TagCompound tag) {
            if (OverrideDamageTypeIndex != -1) {
                tag["OverrideDamageTypeIndex"] = OverrideDamageTypeIndex;
            }
        }

        public override void LoadData(Item item, TagCompound tag) {
            if (tag.ContainsKey("OverrideDamageTypeIndex")) {
                OverrideDamageTypeIndex = tag.GetInt("OverrideDamageTypeIndex");
                // 关键点 1：重新加载存档时，直接更新 item.DamageType
                ApplyDamageType(item);
            } else {
                OverrideDamageTypeIndex = -1;
            }
        }

        // 统一原版常见伤害类型映射列表
        public static readonly List<DamageClass> DamageClasses = new List<DamageClass>() {
            DamageClass.Melee,          // 0: 近战
            DamageClass.Ranged,         // 1: 远程
            DamageClass.Magic,          // 2: 魔法
            DamageClass.Summon,         // 3: 召唤
            DamageClass.Throwing,       // 4: 投掷
            DamageClass.Generic,        // 5: 通用/无特定类型
            DamageClass.Default,        // 6: 默认
            DamageClass.SummonMeleeSpeed// 7: 鞭子 (鞭子特有: 召唤+近战速度)
        };

        public static readonly List<string> DamageClassNames = new List<string>() {
            "近战 (Melee)",
            "远程 (Ranged)",
            "魔法 (Magic)",
            "召唤 (Summon)",
            "投掷 (Throwing)",
            "通用 (Generic)",
            "无类型 (Default)",
            "鞭子 (SummonMelee)"
        };

        /// <summary>
        /// 统一应用伤害类型的辅助方法
        /// </summary>
        public void ApplyDamageType(Item item) {
            if (OverrideDamageTypeIndex >= 0 && OverrideDamageTypeIndex < DamageClasses.Count) {
                item.DamageType = DamageClasses[OverrideDamageTypeIndex];
            }
        }

        // 删掉原来的 ModifyWeaponDamage 里的赋值逻辑（不再需要靠攻击触发）
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
            // 保留为空或处理其他伤害计算逻辑，不要在这里赋值 DamageType
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            if (OverrideDamageTypeIndex >= 0 && OverrideDamageTypeIndex < DamageClasses.Count) {
                tooltips.Add(new TooltipLine(Mod, "ConvertedDamageType", $"[更懒的体验已修改伤害类型为: {DamageClassNames[OverrideDamageTypeIndex]}]") {
                    OverrideColor = Main.DiscoColor
                });
            }
        }
    }
}