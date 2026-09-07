using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace terraria_gldty.Common.Systems
{
    public class BridgeBuilderPlayer : ModPlayer
    {
        // =========================================================
        // 持久化保存的建造器物品
        // =========================================================

        public Item platformItem = new Item();
        public Item lightItem = new Item();

        // =========================================================
        // 初始化
        // =========================================================

        public override void Initialize()
        {
            platformItem = new Item();
            platformItem.SetDefaults(ItemID.None);

            lightItem = new Item();
            lightItem.SetDefaults(ItemID.None);
        }

        // =========================================================
        // 保存玩家数据
        // =========================================================

        public override void SaveData(TagCompound tag)
        {
            if (platformItem != null && !platformItem.IsAir)
            {
                tag["platformItem"] = platformItem;
            }

            if (lightItem != null && !lightItem.IsAir)
            {
                tag["lightItem"] = lightItem;
            }
        }

        // =========================================================
        // 读取玩家数据
        // =========================================================

        public override void LoadData(TagCompound tag)
        {
            platformItem = new Item();
            platformItem.SetDefaults(ItemID.None);

            lightItem = new Item();
            lightItem.SetDefaults(ItemID.None);

            if (tag.ContainsKey("platformItem"))
            {
                platformItem =
                    tag.Get<Item>("platformItem")
                    ?? CreateEmptyItem();
            }

            if (tag.ContainsKey("lightItem"))
            {
                lightItem =
                    tag.Get<Item>("lightItem")
                    ?? CreateEmptyItem();
            }
        }

        // =========================================================
        // 玩家进入世界
        //
        // 关键修复：
        // 将存档中的物品立即同步给 BridgeBuilderSettings。
        // 这样不打开 UI 也可以直接使用建造器。
        // =========================================================

        public override void OnEnterWorld()
        {
            SyncToBuilderSettings();
        }

        // =========================================================
        // 同步给实际建造系统
        // =========================================================

        private void SyncToBuilderSettings()
        {
            if (platformItem == null)
            {
                platformItem = CreateEmptyItem();
            }

            if (lightItem == null)
            {
                lightItem = CreateEmptyItem();
            }

            BridgeBuilderSettings.PlatformItem =
                platformItem;

            BridgeBuilderSettings.LightItem =
                lightItem;
        }

        // =========================================================
        // 创建空物品
        // =========================================================

        private static Item CreateEmptyItem()
        {
            Item item = new Item();
            item.SetDefaults(ItemID.None);
            return item;
        }
    }
}