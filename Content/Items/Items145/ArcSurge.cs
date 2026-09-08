using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using terraria_gldty.Content.Items.Items145.Proj;

namespace terraria_gldty.Content.Items.Items145
{
    /// <summary>
    /// 电弧涌动
    ///
    /// 基于 Terraria 1.4.5.8 原版 Arc Surge 行为移植。
    /// 
    /// 特性：
    /// - 180 魔法伤害
    /// - 18 魔力
    /// - 4% 暴击
    /// - useTime = 6
    /// - useAnimation = 18
    /// - 自动连射
    /// - 鼠标位置瞬时红色闪电
    /// - 每次射击额外寻找最多 2 个目标
    /// - 支持闪电之击相同的染料系统
    /// </summary>
    public class ArcSurge : ModItem
    {
        /// <summary>
        /// 当前绑定的染料。
        /// 0 = 无染料。
        /// </summary>
        public int AppliedDyeType = 0;

        public override string Texture =>
            "Terraria/Images/Item_6173";

        public override void SetDefaults()
        {
            // 原版 Arc Surge
            Item.damage = 180;
            Item.DamageType = DamageClass.Magic;

            Item.mana = 18;

            Item.width = 40;
            Item.height = 40;

            // 注意：
            // 原版内部 useTime = 6，
            // useAnimation = 18。
            // 因此一次按键动画期间会连续发射 3 次。
            Item.useTime = 6;
            Item.useAnimation = 18;

            Item.useStyle = ItemUseStyleID.Shoot;

            Item.noMelee = true;
            Item.autoReuse = true;

            Item.knockBack = 4f;

            Item.crit = 4;

            Item.shoot = ModContent.ProjectileType<ArcSurgeProj>();
            Item.shootSpeed = 8f;

            //Item.UseSound = SoundID.Item20;

            Item.rare = ItemRarityID.Red;

            Item.value = Item.sellPrice(gold: 1);
        }

        /// <summary>
        /// 原版不能在水、蜂蜜、微光中使用。
        /// Lava 可以使用。
        /// </summary>
        public override bool CanUseItem(Player player)
        {
            // Player.wet = 水
            // honeyWet = 蜂蜜
            // shimmerWet = 微光
            // lavaWet = 岩浆
            //
            // 原版允许岩浆，因此这里只排除非岩浆液体。
            if (player.wet && !player.lavaWet)
                return false;

            if (player.honeyWet)
                return false;

            if (player.shimmerWet)
                return false;

            return base.CanUseItem(player);
        }

        public override bool CanRightClick()
        {
            return true;
        }

        /// <summary>
        /// 右键绑定 / 卸下染料。
        /// 与 StormDecree 保持一致。
        /// </summary>
        public override void RightClick(Player player)
        {
            Item mouseItem = Main.mouseItem;

            // 手上拿着染料 -> 绑定
            if (!mouseItem.IsAir && mouseItem.dye > 0)
            {
                // 如果已有旧染料，先退回
                if (AppliedDyeType > 0)
                {
                    player.QuickSpawnItem(
                        player.GetSource_Misc("ArcSurgeDyeUnequip"),
                        AppliedDyeType,
                        1
                    );
                }

                AppliedDyeType = mouseItem.type;

                mouseItem.stack--;

                if (mouseItem.stack <= 0)
                    mouseItem.TurnToAir();

                Main.NewText(
                    "已成功为电弧涌动绑定染料！",
                    Color.Cyan
                );
            }
            // 空手右键 -> 卸下染料
            else if (AppliedDyeType > 0)
            {
                player.QuickSpawnItem(
                    player.GetSource_Misc("ArcSurgeDyeUnequip"),
                    AppliedDyeType,
                    1
                );

                AppliedDyeType = 0;

                Main.NewText(
                    "已卸下电弧涌动染料。",
                    Color.Yellow
                );
            }
        }

        /// <summary>
        /// 防止右键染料操作时消耗武器。
        /// </summary>
        public override bool ConsumeItem(Player player)
        {
            return false;
        }

        /// <summary>
        /// 保存染料。
        /// </summary>
        public override void SaveData(TagCompound tag)
        {
            tag["AppliedDyeType"] = AppliedDyeType;
        }

        /// <summary>
        /// 读取染料。
        /// </summary>
        public override void LoadData(TagCompound tag)
        {
            AppliedDyeType = tag.GetInt("AppliedDyeType");
        }

        /// <summary>
        /// 发射弹幕。
        ///
        /// 每次 Shoot 会生成：
        /// 1. 鼠标方向主闪电
        /// 2. 最多两个自动锁定目标闪电
        ///
        /// 由于 useTime = 6 / useAnimation = 18，
        /// Terraria 一次按键会调用三轮 Shoot，
        /// 因此最多产生 9 条闪电。
        /// </summary>
        public override bool Shoot(
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            Vector2 cursor = Main.MouseWorld;

            // -------------------------------------------------
            // 1. 鼠标主闪电
            // -------------------------------------------------

            int mainIndex = Projectile.NewProjectile(
                source,
                cursor,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            if (mainIndex >= 0 && mainIndex < Main.maxProjectiles)
            {
                Projectile projectile = Main.projectile[mainIndex];

                // ai[1] 保存染料
                projectile.ai[1] = AppliedDyeType;

                // localAI[0] = 0 表示鼠标主闪电
                projectile.localAI[0] = 0f;
            }

            // -------------------------------------------------
            // 2. 寻找最多两个额外目标
            // -------------------------------------------------

            List<NPC> targets = FindExtraTargets(
                player,
                cursor
            );

            foreach (NPC npc in targets)
            {
                int index = Projectile.NewProjectile(
                    source,
                    npc.Center,
                    Vector2.Zero,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                if (index >= 0 && index < Main.maxProjectiles)
                {
                    Projectile projectile = Main.projectile[index];

                    projectile.ai[1] = AppliedDyeType;

                    // localAI[0] = 1 表示自动目标闪电
                    projectile.localAI[0] = 1f;

                    projectile.netUpdate = true;
                }
            }

            return false;
        }

        /// <summary>
        /// 寻找电弧涌动的额外目标。
        ///
        /// 官方条件：
        /// 1. 目标可以被攻击
        /// 2. 玩家与目标之间没有墙体遮挡
        /// 3. 目标位于玩家中心的
        ///    62.5 格宽 × 50 格高矩形内
        /// 4. 玩家 -> 鼠标 与 玩家 -> NPC
        ///    的夹角小于 60°
        /// 5. 最多选择两个
        ///
        /// 如果符合条件的敌人超过两个，
        /// 原版会随机选择两个。
        /// </summary>
        private List<NPC> FindExtraTargets(
            Player player,
            Vector2 cursor
        )
        {
            List<NPC> candidates = new List<NPC>();

            Vector2 cursorDirection =
                cursor - player.Center;

            if (cursorDirection.LengthSquared() < 0.01f)
                return candidates;

            cursorDirection.Normalize();

            // 62.5 格宽
            float searchWidth = 625f * 16f;  //62.5f

            // 50 格高
            float searchHeight = 500f * 16f;  //50f

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active)
                    continue;

                if (!npc.CanBeChasedBy())
                    continue;

                if (npc.friendly)
                    continue;

                // ---------------------------------------------
                // 矩形范围
                // ---------------------------------------------

                Vector2 offset =
                    npc.Center - player.Center;

                if (Math.Abs(offset.X) > searchWidth * 0.5f)
                    continue;

                if (Math.Abs(offset.Y) > searchHeight * 0.5f)
                    continue;

                // ---------------------------------------------
                // 视线检查
                // ---------------------------------------------

                if (!Collision.CanHitLine(
                    player.Center,
                    1,
                    1,
                    npc.Center,
                    1,
                    1))
                {
                    continue;
                }

                // ---------------------------------------------
                // 60°夹角检查
                // ---------------------------------------------

                float distance = offset.Length();

                if (distance <= 0.01f)
                    continue;

                Vector2 npcDirection =
                    offset / distance;

                float dot =
                    Vector2.Dot(
                        cursorDirection,
                        npcDirection
                    );

                dot = MathHelper.Clamp(
                    dot,
                    -1f,
                    1f
                );

                float angle =
                    MathF.Acos(dot);

                if (angle >= MathHelper.ToRadians(60f))
                    continue;

                candidates.Add(npc);
            }

            // -------------------------------------------------
            // 原版：
            // 如果超过两个目标，随机选择两个。
            // -------------------------------------------------

            while (candidates.Count > 2)
            {
                int removeIndex =
                    Main.rand.Next(candidates.Count);

                candidates.RemoveAt(removeIndex);
            }

            return candidates;
        }

        /// <summary>
        /// Tooltip。
        /// </summary>
        public override void ModifyTooltips(
            List<TooltipLine> tooltips
        )
        {
            // 不主动删除原版 Tooltip。
            // 这里补充染料信息。

            if (AppliedDyeType > 0)
            {
                Item dyeItem = new Item();
                dyeItem.SetDefaults(AppliedDyeType);

                tooltips.Add(
                    new TooltipLine(
                        Mod,
                        "ArcSurgeDye",
                        $"[i:{AppliedDyeType}] 已绑定染料: {dyeItem.Name}"
                    )
                    {
                        OverrideColor = Color.LightSkyBlue
                    }
                );
            }
            else
            {
                tooltips.Add(
                    new TooltipLine(
                        Mod,
                        "ArcSurgeDye",
                        "提示: 右键手持染料可为电弧涌动染色"
                    )
                    {
                        OverrideColor = Color.Gray
                    }
                );
            }
        }
    }
}