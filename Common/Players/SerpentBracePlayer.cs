using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace terraria_gldty.Common.Players
{
    public class SerpentBracePlayer : ModPlayer
    {
        public bool hasSerpentBrace;
        public int attackCount;

        // 延迟协同攻击控制变量
        private int extraAttackTimer;
        private Item pendingExtraWhip;
        private EntitySource_ItemUse_WithAmmo pendingSource;
        private Vector2 pendingPosition;
        private Vector2 pendingVelocity;
        private int pendingDamage;
        private float pendingKnockback;

        public override void ResetEffects()
        {
            hasSerpentBrace = false;
        }

        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (hasSerpentBrace && item.CountsAsClass(DamageClass.SummonMeleeSpeed) && ProjectileID.Sets.IsAWhip[type])
            {
                attackCount++;

                if (attackCount >= 3)
                {
                    attackCount = 0; // 重置计数

                    // 检索第二把鞭子（无则用主鞭）
                    Item secondaryWhip = FindSecondaryWhip(item) ?? item;

                    // 暂存攻击参数，并开启延迟定时器（12帧 ≈ 0.2秒错开时间）
                    pendingExtraWhip = secondaryWhip.Clone();
                    pendingSource = source;
                    pendingPosition = position;
                    pendingVelocity = velocity;
                    pendingDamage = secondaryWhip == item ? damage : Player.GetWeaponDamage(secondaryWhip);
                    pendingKnockback = secondaryWhip == item ? knockback : Player.GetWeaponKnockback(secondaryWhip, secondaryWhip.knockBack);

                    extraAttackTimer = 12; // 可根据需要调整延迟帧数（数值越大错开越久）
                }
            }

            return base.Shoot(item, source, position, velocity, type, damage, knockback);
        }

        public override void PostUpdate()
        {
            // 处理延迟追加攻击逻辑
            if (extraAttackTimer > 0)
            {
                extraAttackTimer--;

                if (extraAttackTimer == 0 && pendingExtraWhip != null)
                {
                    ExecuteExtraWhipAttack();
                }
            }
        }

        private void ExecuteExtraWhipAttack()
        {
            if (pendingExtraWhip.shoot > ProjectileID.None)
            {
                // 稍微旋转角度，形成交错挥打的视觉效果
                Vector2 extraVelocity = pendingVelocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f)));

                // 1. 生成协同攻击的鞭子射弹
                Projectile.NewProjectile(
                    pendingSource,
                    pendingPosition,
                    extraVelocity,
                    pendingExtraWhip.shoot,
                    pendingDamage,
                    pendingKnockback,
                    Player.whoAmI
                );

                // 2. 播放第二条鞭子原生的挥舞音效（形成交错音效）
                SoundStyle whipSound = pendingExtraWhip.UseSound ?? SoundID.Item152; // 若无配置则使用默认挥鞭声
                SoundEngine.PlaySound(whipSound, Player.Center);
            }

            // 清理引用
            pendingExtraWhip = null;
        }

        /// <summary>
        /// 检索玩家背包中除去主手外的第二把鞭子
        /// </summary>
        private Item FindSecondaryWhip(Item mainWhip)
        {
            for (int i = 0; i < 50; i++)
            {
                Item invItem = Player.inventory[i];

                if (!invItem.IsAir && invItem != mainWhip)
                {
                    if (invItem.CountsAsClass(DamageClass.SummonMeleeSpeed) && ProjectileID.Sets.IsAWhip[invItem.shoot])
                    {
                        return invItem;
                    }
                }
            }

            return null;
        }
    }
}