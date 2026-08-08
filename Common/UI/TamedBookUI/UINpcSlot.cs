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

        public UINpcSlot(int npcType, float scale = 0.75f) {
            NpcType = npcType;
            _scale = scale;

            Width.Set(54f * _scale, 0f);
            Height.Set(54f * _scale, 0f);
            BackgroundColor = new Color(63, 82, 151) * 0.7f;
            BorderColor = Color.Black;
        }

        public override void MouseOver(UIMouseEvent evt) {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
            
            bool isDeleteMode = TamedSystem.Instance?.tamedUI?.IsDeleteMode ?? false;
            BackgroundColor = isDeleteMode ? (Color.Red * 0.6f) : (new Color(98, 115, 178) * 0.8f);
        }

        public override void MouseOut(UIMouseEvent evt) {
            base.MouseOut(evt);
            bool isDeleteMode = TamedSystem.Instance?.tamedUI?.IsDeleteMode ?? false;
            BackgroundColor = isDeleteMode ? (Color.Maroon * 0.5f) : (new Color(63, 82, 151) * 0.7f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            bool isDeleteMode = TamedSystem.Instance?.tamedUI?.IsDeleteMode ?? false;
            var modPlayer = Main.LocalPlayer.GetModPlayer<TamedPlayer>();

            // 判断该格子是否为“上一次召唤/当前选中”的怪物
            bool isSelected = (modPlayer.LastSummonedType == NpcType);

            if (isDeleteMode && !IsMouseHovering) {
                BackgroundColor = Color.Maroon * 0.5f;
                BorderColor = Color.Red;
            } else if (!isDeleteMode && !IsMouseHovering) {
                BackgroundColor = isSelected ? new Color(90, 115, 190) * 0.9f : new Color(63, 82, 151) * 0.7f;
                BorderColor = isSelected ? Color.Gold : Color.Black;
            }

            base.DrawSelf(spriteBatch);

            CalculatedStyle dimensions = GetInnerDimensions();
            
            // 安全获取纹理
            if (NpcType <= 0 || NpcType >= NPCLoader.NPCCount) return;
            
            Main.instance.LoadNPC(NpcType); // 确保贴图已加载
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
            iconScale *= 0.85f;

            Vector2 drawPosition = dimensions.Center();
            drawPosition.Y += (frame.Height * iconScale) / 4f;

            spriteBatch.Draw(texture, drawPosition, frame, Color.White, 0f, frame.Size() / 2f, iconScale, SpriteEffects.None, 0f);

            // 如果是被选中的怪物，在其周围绘制金黄色发光/呼吸边框
            if (isSelected) {
                Rectangle outerDimensions = GetDimensions().ToRectangle();
                
                // 呼吸动画色彩计算
                float pulse = (float)(System.Math.Sin(Main.GameUpdateCount * 0.1f) + 1f) * 0.5f;
                Color glowColor = Color.Lerp(Color.Gold, Color.Yellow, pulse);

                // 绘制 2 像素宽的高亮线边框
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X, outerDimensions.Y, outerDimensions.Width, 2), glowColor);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X, outerDimensions.Y + outerDimensions.Height - 2, outerDimensions.Width, 2), glowColor);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X, outerDimensions.Y, 2, outerDimensions.Height), glowColor);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(outerDimensions.X + outerDimensions.Width - 2, outerDimensions.Y, 2, outerDimensions.Height), glowColor);
            }

            if (isDeleteMode) {
                Vector2 xPos = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 2f);
                Utils.DrawBorderString(spriteBatch, "X", xPos, Color.Red, 0.75f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);

            if (IsMouseHovering) {
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

                sb.AppendLine($"生命值: {sampleNPC.lifeMax}");
                sb.AppendLine($"防御力: {sampleNPC.defense}");
                sb.AppendLine($"基础伤害: [c/FFD700:{sampleNPC.defDamage}] (当前伤害: [c/FF6347:{displayDamage}])");

                if (sampleNPC.noGravity) sb.AppendLine("[c/9ACD32:飞行/无重力]");
                if (sampleNPC.noTileCollide) sb.AppendLine("[c/9ACD32:穿墙]");

                if (isDeleteMode) {
                    sb.AppendLine("\n[c/FF3333:<点击彻底删除此怪物图鉴>]");
                } else {
                    sb.AppendLine("\n[c/55FF55:<点击选择并召唤>]");
                }

                Main.hoverItemName = sb.ToString();
            }
        }
    }
}