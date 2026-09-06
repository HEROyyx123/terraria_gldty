using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.Systems
{
    public class BridgeBuilderPlayer : ModPlayer
    {
        // 持久化保存槽位里的物品
        public Item platformItem = new Item();
        public Item lightItem = new Item();

        public override void SaveData(TagCompound tag)
        {
            tag["platformItem"] = platformItem;
            tag["lightItem"] = lightItem;
        }

        public override void LoadData(TagCompound tag)
        {
            platformItem = tag.Get<Item>("platformItem") ?? new Item();
            lightItem = tag.Get<Item>("lightItem") ?? new Item();
        }
    }
}