using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace terraria_gldty.Common.Players
{
    /// <summary>
    /// 玩家数据类，用于记录每个礼包是否已领取/打开过
    /// </summary>
    public class PackPlayer : ModPlayer
    {
        /// <summary> 是否已领取过新手大礼包（跟随角色存档） </summary>
        public bool ReceivedStarterPack { get; set; } = false;

        /// <summary> 是否已打开过克苏鲁之眼礼包 </summary>
        public bool ReceivedEyeOfCthulhuPack { get; set; } = false;
        /// <summary> 是否已打开过邪恶 Boss 礼包 </summary>
        public bool ReceivedEvilBossPack { get; set; } = false;
        /// <summary> 是否已打开过史莱姆王礼包 </summary>
        public bool ReceivedKingSlimePack { get; set; } = false;

        /// <summary> 是否已打开过困难模式礼包 </summary>
        public bool ReceivedHardmodePack { get; set; } = false;
        /// <summary> 是否已打开过机械礼包 </summary>
        public bool ReceivedMechBossPack { get; set; } = false;
        /// <summary> 是否已打开过丛林礼包 </summary>
        public bool ReceivedJunglePack { get; set; } = false;
        /// <summary> 是否已打开过阴森木礼包 </summary>
        public bool ReceivedSpookyWoodPack { get; set; } = false;
        /// <summary> 是否已打开过甲虫礼包 </summary>
        public bool ReceivedBeetlePack { get; set; } = false;
        /// <summary> 是否已打开过教徒礼包 </summary>
        public bool ReceivedCultistPack { get; set; } = false;
        /// <summary> 是否已打开过月总礼包 </summary>
        public bool ReceivedMoonLordPack { get; set; } = false;

        public override void SaveData(TagCompound tag) {
            tag["ReceivedStarterPack"] = ReceivedStarterPack;
            tag["ReceivedEyeOfCthulhuPack"] = ReceivedEyeOfCthulhuPack;
            tag["ReceivedEvilBossPack"] = ReceivedEvilBossPack;
            tag["ReceivedKingSlimePack"] = ReceivedKingSlimePack;
            tag["ReceivedHardmodePack"] = ReceivedHardmodePack;
            tag["ReceivedMechBossPack"] = ReceivedMechBossPack;
            tag["ReceivedJunglePack"] = ReceivedJunglePack;
            tag["ReceivedSpookyWoodPack"] = ReceivedSpookyWoodPack;
            tag["ReceivedBeetlePack"] = ReceivedBeetlePack;
            tag["ReceivedCultistPack"] = ReceivedCultistPack;
            tag["ReceivedMoonLordPack"] = ReceivedMoonLordPack;
        }

        public override void LoadData(TagCompound tag) {
            ReceivedStarterPack = tag.GetBool("ReceivedStarterPack");
            ReceivedEyeOfCthulhuPack = tag.GetBool("ReceivedEyeOfCthulhuPack");
            ReceivedEvilBossPack = tag.GetBool("ReceivedEvilBossPack");
            ReceivedKingSlimePack = tag.GetBool("ReceivedKingSlimePack");
            ReceivedHardmodePack = tag.GetBool("ReceivedHardmodePack");
            ReceivedMechBossPack = tag.GetBool("ReceivedMechBossPack");
            ReceivedJunglePack = tag.GetBool("ReceivedJunglePack");
            ReceivedSpookyWoodPack = tag.GetBool("ReceivedSpookyWoodPack");
            ReceivedBeetlePack = tag.GetBool("ReceivedBeetlePack");
            ReceivedCultistPack = tag.GetBool("ReceivedCultistPack");
            ReceivedMoonLordPack = tag.GetBool("ReceivedMoonLordPack");
        }

        /// <summary>
        /// 每次进入世界时检测，如果是新角色则给予新手大礼包
        /// </summary>
        public override void OnEnterWorld() {
            if (!ReceivedStarterPack) {
                // 给予新手大礼包
                Player.QuickSpawnItem(Player.GetSource_OpenItem(ModContent.ItemType<Content.Items.Packs.StarterPack>()),
                    ModContent.ItemType<Content.Items.Packs.StarterPack>());
                ReceivedStarterPack = true;
            }
        }
    }
}