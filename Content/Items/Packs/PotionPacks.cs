using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace terraria_gldty.Content.Items.Packs
{
    // =========================================================================
    // 药剂包基类 - 各种药剂包的通用逻辑
    // =========================================================================
    public abstract class PotionPackBase : ModItem
    {
        /// <summary> 打开后给予的药水类型 </summary>
        protected abstract int PotionType { get; }

        /// <summary> 打开的稀有度 </summary>
        protected virtual int PackRarity => ItemRarityID.LightRed;

        /// <summary> 打开后的数量 </summary>
        protected virtual int PotionCount => 30;

        /// <summary> 直接引用原版药水贴图，删除占位符 PNG </summary>
        public override string Texture => "Terraria/Images/Item_" + PotionType;

        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = PackRarity;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item14;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player) {
            player.QuickSpawnItem(player.GetSource_OpenItem(Type), PotionType, PotionCount);
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(PotionType)                          // 1 瓶对应药水
                .AddTile(TileID.Bottles)                            // 放置瓶子的工作台
                .AddCondition(Condition.DownedSkeletron)             // 击败骷髅王后解锁
                .Register();
        }
    }

    // =========================================================================
    // 各种药剂包的具体实现
    // =========================================================================

    public class IronskinPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.IronskinPotion;
        public override void SetDefaults() { base.SetDefaults(); Item.rare = ItemRarityID.Green; }
    }

    public class RegenerationPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.RegenerationPotion;
        public override void SetDefaults() { base.SetDefaults(); Item.rare = ItemRarityID.Green; }
    }

    public class SwiftnessPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.SwiftnessPotion;
        public override void SetDefaults() { base.SetDefaults(); Item.rare = ItemRarityID.Green; }
    }

    public class NightOwlPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.NightOwlPotion;
    }

    public class HunterPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.HunterPotion;
    }

    public class ArcheryPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.ArcheryPotion;
    }

    public class ThornsPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.ThornsPotion;
    }

    public class ObsidianSkinPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.ObsidianSkinPotion;
    }

    public class WaterWalkingPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.WaterWalkingPotion;
    }

    public class MagicPowerPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.MagicPowerPotion;
    }

    public class ManaRegenerationPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.ManaRegenerationPotion;
    }

    public class EndurancePotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.EndurancePotion;
    }

    public class LifeforcePotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.LifeforcePotion;
    }

    public class RagePotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.RagePotion;
    }

    public class WrathPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.WrathPotion;
    }

    public class SummoningPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.SummoningPotion;
    }

    public class BattlePotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.BattlePotion;
    }

    public class BuilderPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.BuilderPotion;
    }

    public class InfernoPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.InfernoPotion;
    }

    public class FeatherfallPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.FeatherfallPotion;
    }

    public class MiningPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.MiningPotion;
    }

    public class SpelunkerPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.SpelunkerPotion;
    }

    public class GillsPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.GillsPotion;
    }

    public class FishingPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.FishingPotion;
    }

    public class CratePotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.CratePotion;
    }

    public class ShinePotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.ShinePotion;
    }

    public class WarmthPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.WarmthPotion;
    }

    public class CalmingPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.CalmingPotion;
    }

    public class TitanPotionPack : PotionPackBase
    {
        protected override int PotionType => ItemID.TitanPotion;
    }
}