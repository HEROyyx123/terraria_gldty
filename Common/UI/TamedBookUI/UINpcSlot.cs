using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using System.Text;
using terraria_gldty.Common.Systems;
using terraria_gldty.Common.Players;

namespace terraria_gldty.Common.UI
{
    public class UINpcSlot : UIPanel
    {
        public int NpcType { get; private set; }
        private float _scale;

        public UINpcSlot(int npcType, float scale = 0.85f) {
            NpcType = npcType;
            _scale = scale;

            Width.Set(50f * _scale, 0f);
            Height.Set(50f * _scale, 0f);
            SetPadding(0);
            
            BackgroundColor = new Color(30, 40, 70) * 0.85f;
            BorderColor = new Color(60, 80, 130);
        }

        public override void MouseOver(UIMouseEvent evt) {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            bool isDeleteMode = TamedSystem.Instance?.tamedUI?.IsDeleteMode ?? false;
            var modPlayer = Main.LocalPlayer.GetModPlayer<TamedPlayer>();
            bool isSelected = (modPlayer.LastSummonedType == NpcType);

            // 鼠标悬浮与模式颜色控制
            if (isDeleteMode) {
                BackgroundColor = IsMouseHovering ? new Color(180, 40, 40) * 0.9f : new Color(100, 30, 30) * 0.7f;
                BorderColor = IsMouseHovering ? Color.Red : new Color(160, 50, 50);
            } else {
                if (IsMouseHovering) {
                    BackgroundColor = new Color(60, 90, 160) * 0.95f;
                    BorderColor = Color.LightCyan;
                } else {
                    BackgroundColor = isSelected ? new Color(50, 80, 150) * 0.9f : new Color(30, 40, 70) * 0.85f;
                    BorderColor = isSelected ? Color.Gold : new Color(60, 80, 130);
                }
            }

            base.DrawSelf(spriteBatch);

            CalculatedStyle dimensions = GetInnerDimensions();
            
            if (NpcType <= 0 || NpcType >= NPCLoader.NPCCount) return;
            
            Main.instance.LoadNPC(NpcType);
            Texture2D texture = TextureAssets.Npc[NpcType].Value;

            int frameCount = Main.npcFrameCount[NpcType];
            Rectangle frame = new Rectangle(0, 0, texture.Width, texture.Height / frameCount);

            float iconScale = 1f;
            if (frame.Width > dimensions.Width || frame.Height > dimensions.Height) {
                iconScale = dimensions.Width / frame.Width;
                if (frame.Height * iconScale > dimensions.Height) {
                    iconScale = dimensions.Height / frame.Height;
                }
            }
            iconScale *= 0.82f;

            // 悬浮时微放大的交互感
            if (IsMouseHovering) iconScale *= 1.1f;

            Vector2 drawPosition = dimensions.Center();

            spriteBatch.Draw(texture, drawPosition, frame, Color.White, 0f, frame.Size() / 2f, iconScale, SpriteEffects.None, 0f);

            // 当前选中的金黄发光边框 + 呼吸效果
            if (isSelected && !isDeleteMode) {
                Rectangle outerDimensions = GetDimensions().ToRectangle();
                
                float pulse = (float)(System.Math.Sin(Main.GameUpdateCount * 0.12f) + 1f) * 0.5f;
                Color glowColor = Color.Lerp(Color.Gold, Color.Yellow, pulse);

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X, outerDimensions.Y, outerDimensions.Width, 2), glowColor);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X, outerDimensions.Y + outerDimensions.Height - 2, outerDimensions.Width, 2), glowColor);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X, outerDimensions.Y, 2, outerDimensions.Height), glowColor);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X + outerDimensions.Width - 2, outerDimensions.Y, 2, outerDimensions.Height), glowColor);
            }

            // 删除模式小红叉角标
            if (isDeleteMode) {
                Vector2 xPos = new Vector2(dimensions.X + dimensions.Width - 12f, dimensions.Y + 2f);
                Utils.DrawBorderString(spriteBatch, "X", xPos, Color.Red, 0.8f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);

            if (IsMouseHovering) {
                // 强制全局关闭玩家悬浮交互，防止在 UI 上的提示框和手持物品冲突
                Main.LocalPlayer.mouseInterface = true;

                NPC sampleNPC = ContentSamples.NpcsByNetId.TryGetValue(NpcType, out NPC npc) ? npc : null;
                if (sampleNPC == null) return;

                bool isDeleteMode = TamedSystem.Instance?.tamedUI?.IsDeleteMode ?? false;
                var modPlayer = Main.LocalPlayer.GetModPlayer<TamedPlayer>();
                bool isSelected = (modPlayer.LastSummonedType == NpcType);

                StringBuilder sb = new StringBuilder();

                Player player = Main.LocalPlayer;
                StatModifier summonModifier = player.GetTotalDamage(DamageClass.Summon);
                int displayDamage = (int)summonModifier.ApplyTo(sampleNPC.defDamage);

                sb.AppendLine($"[c/FFD700:=== {sampleNPC.GivenOrTypeName} ===]");
                if (isSelected) {
                    sb.AppendLine("[c/00FFFF:★ 当前已选择（快捷召唤目标）]");
                }

                sb.AppendLine($"生命值: [c/00FF7F:{sampleNPC.lifeMax}]");
                sb.AppendLine($"防御力: [c/ADD8E6:{sampleNPC.defense}]");
                sb.AppendLine($"基础伤害: [c/FFD700:{sampleNPC.defDamage}] (当前: [c/FF6347:{displayDamage}])");

                if (sampleNPC.noGravity) sb.AppendLine("[c/9ACD32:✦ 飞行/无重力]");
                if (sampleNPC.noTileCollide) sb.AppendLine("[c/9ACD32:✦ 穿墙]");

                if (isDeleteMode) {
                    sb.AppendLine("\n[c/FF3333:✖ 点击彻底删除此怪物图鉴]");
                } else {
                    sb.AppendLine("\n[c/55FF55:✔ 点击选择并召唤]");
                }

                Main.hoverItemName = sb.ToString();
            }
        }
    }
}