using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Content.Buff;
namespace terraria_gldty.Common.Players
{
    public class BambooCicadaPlayer : ModPlayer
    {
        public bool hasFishyResonance;
        public bool hasSprainedAnkleDebuff;
        public int sprainCooldown = 0;

        public override void ResetEffects()
        {
            hasFishyResonance = false;
            hasSprainedAnkleDebuff = false;
        }

        public override void PreUpdate()
        {
            if (sprainCooldown > 0)
            {
                sprainCooldown--;
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            // 触发条件：拥有【鱼音缭绕】Buff、处于地面上且正在水平移动
            if (hasFishyResonance && sprainCooldown == 0)
            {
                bool isMovingHorizontally = (Player.controlLeft || Player.controlRight) && Math.Abs(Player.velocity.X) > 0.5f;
                bool isOnGround = Player.velocity.Y == 0f;

                if (isMovingHorizontally && isOnGround)
                {
                    // 移动时每帧判定，约为 5% 概率每秒触发 (1/120 帧)
                    if (Main.rand.NextBool(120))
                    {
                        TriggerSprain();
                    }
                }
            }
        }

        private void TriggerSprain()
        {
            // 3秒内部冷却，防止连续崴脚
            sprainCooldown = 180;

            // 附加 1.2 秒【崴脚】Debuff
            Player.AddBuff(ModContent.BuffType<Content.Buff.SprainedAnkleDebuff>(), 72);

            // 播放骨折音效
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f, Volume = 0.8f }, Player.position);

            // 弹出提示文字
            CombatText.NewText(Player.getRect(), new Color(255, 120, 50), "崴脚了！", true);

            // 生成碎骨/灰尘粒子
            Vector2 feetPos = new Vector2(Player.position.X, Player.position.Y + Player.height - 4);
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(feetPos, Player.width, 8, DustID.Bone, 0f, -2f, 100, default, 1.1f);
                d.velocity *= 1.2f;
            }

            
        }
    }
}
