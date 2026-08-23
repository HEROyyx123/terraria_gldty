using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.Systems
{
    public class BoundGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        // 记录原始物品的 Type，如果为 -1 表示未绑定（原版/普通物品）
        public int BoundSourceType { get; set; } = -1;

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            BoundGlobalItem myClone = (BoundGlobalItem)base.Clone(item, itemClone);
            myClone.BoundSourceType = BoundSourceType;
            return myClone;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (BoundSourceType != -1)
            {
                tag["BoundSourceType"] = BoundSourceType;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (tag.ContainsKey("BoundSourceType"))
            {
                BoundSourceType = tag.GetInt("BoundSourceType");
            }
            else
            {
                BoundSourceType = -1;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (BoundSourceType > 0)
            {
                string sourceName = Lang.GetItemNameValue(BoundSourceType);
                tooltips.Add(new TooltipLine(Mod, "BoundSourceInfo", $"原始形态: {sourceName}")
                {
                    //OverrideColor = new Microsoft.Xna.Framework.Color(180, 100, 255)
                    OverrideColor=Main.DiscoColor
                });
                tooltips.Add(new TooltipLine(Mod, "BoundSourceHint", "可在【魂石兑换机】中还原为原始形态")
                {
                    //OverrideColor = new Microsoft.Xna.Framework.Color(150, 150, 150)
                    OverrideColor=Main.DiscoColor
                });
            }
        }
    }
}