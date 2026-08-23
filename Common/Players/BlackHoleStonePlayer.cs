using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.Players
{
    public class BlackHoleStonePlayer : ModPlayer
    {
        public bool HasBlackHoleStone;
        public bool PickUpAll = true;
        public int[] FilterItems = new int[10];
        public float PickupRange = 0f;
        public bool HasOpenedUI;

        public override void ResetEffects() {
            HasBlackHoleStone = false;
            for (int i = 0; i < 58; i++) {
                Item item = Player.inventory[i];
                if (!item.IsAir && item.type == ModContent.ItemType<Content.Items.BlackHoleStone>()) {
                    HasBlackHoleStone = true;
                    break;
                }
            }
        }

        public bool IsItemInFilter(Item item) {
            if (PickUpAll) return true;
            if (item.IsAir) return false;
            for (int i = 0; i < 10; i++) {
                if (FilterItems[i] > 0 && FilterItems[i] == item.type) return true;
            }
            return false;
        }

        public bool IsItemTypeInFilter(int itemType) {
            if (PickUpAll) return true;
            if (itemType <= 0) return false;
            for (int i = 0; i < 10; i++) {
                if (FilterItems[i] > 0 && FilterItems[i] == itemType) return true;
            }
            return false;
        }

        // 修改：使用 ItemIO 或验证 ItemData 安全性
        public override void SaveData(TagCompound tag) {
            tag["PickUpAll"] = PickUpAll;
            tag["PickupRange"] = PickupRange;
            tag["HasOpenedUI"] = HasOpenedUI;
            
            // 存入 FilterItems 数组
            tag["FilterItems"] = FilterItems;
        }

        public override void LoadData(TagCompound tag) {
            PickUpAll = tag.GetBool("PickUpAll");
            PickupRange = tag.GetFloat("PickupRange");
            HasOpenedUI = tag.GetBool("HasOpenedUI");

            if (tag.ContainsKey("FilterItems")) {
                FilterItems = tag.GetIntArray("FilterItems");
                // 防护：载入后检查 ID 合法性，如果越界直接重置为 0，防止 UI 崩掉
                for (int i = 0; i < FilterItems.Length; i++) {
                    if (FilterItems[i] >= ItemLoader.ItemCount || FilterItems[i] < 0) {
                        FilterItems[i] = 0;
                    }
                }
            } else {
                FilterItems = new int[10];
            }
        }
    }

    public class BlackHoleStoneGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public bool SpawnedFromContainer;

        public override void OnSpawn(Item item, IEntitySource source) {
            if (source is EntitySource_ItemOpen || source is EntitySource_Gift) {
                SpawnedFromContainer = true;
            }
        }

        public override bool CanPickup(Item item, Player player) {
            if (SpawnedFromContainer)
                return true;

            var modPlayer = player.GetModPlayer<BlackHoleStonePlayer>();
            if (modPlayer.HasBlackHoleStone && !modPlayer.PickUpAll) {
                return modPlayer.IsItemInFilter(item);
            }
            return true;
        }

        public override void GrabRange(Item item, Player player, ref int grabRange) {
            var modPlayer = player.GetModPlayer<BlackHoleStonePlayer>();
            if (modPlayer.HasBlackHoleStone && modPlayer.PickupRange > 0f) {
                if (modPlayer.PickUpAll || modPlayer.IsItemInFilter(item)) {
                    grabRange += (int)(modPlayer.PickupRange * 16f);
                }
            }
        }
    }
}