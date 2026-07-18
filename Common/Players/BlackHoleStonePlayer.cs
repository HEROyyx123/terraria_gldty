using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.Players
{
    /// <summary>
    /// 黑洞石玩家数据类 - 管理拾取范围、过滤和UI状态
    /// </summary>
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

        public override void SaveData(TagCompound tag) {
            tag["PickUpAll"] = PickUpAll;
            tag["PickupRange"] = PickupRange;
            tag["HasOpenedUI"] = HasOpenedUI;
            for (int i = 0; i < 10; i++) {
                if (FilterItems[i] > 0) tag[$"FilterItem{i}"] = FilterItems[i];
            }
        }

        public override void LoadData(TagCompound tag) {
            PickUpAll = tag.GetBool("PickUpAll");
            PickupRange = tag.GetFloat("PickupRange");
            HasOpenedUI = tag.GetBool("HasOpenedUI");
            for (int i = 0; i < 10; i++) {
                FilterItems[i] = tag.ContainsKey($"FilterItem{i}") ? tag.GetInt($"FilterItem{i}") : 0;
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