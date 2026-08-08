using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using terraria_gldty.Common.Players;
using terraria_gldty.Common.Systems;

namespace terraria_gldty.Content.Items
{
    public class SoulChain : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(0, 5, 0, 0);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player) {
            return true;
        }

        // 手持物品时在【玩家周围】绘制生效范围圈[cite: 2]
        public override void HoldItem(Player player) {
            if (player.whoAmI == Main.myPlayer) {
                Vector2 center = player.Center;
                float radius = 150f;
                int dustCount = 36;

                for (int i = 0; i < dustCount; i++) {
                    float angle = MathHelper.TwoPi * (i / (float)dustCount);
                    Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * radius;
                    
                    Dust d = Dust.NewDustPerfect(center + offset, DustID.Enchanted_Pink, Vector2.Zero, 150, Color.White, 0.8f);
                    d.noGravity = true;
                    d.velocity = Vector2.Zero;
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            var keys = TamedSystem.OpenBookKeybind.GetAssignedKeys();
            string keyName = (keys != null && keys.Count > 0) ? keys[0] : "未绑定";

            tooltips.Add(new TooltipLine(Mod, "UsageTip", $"[c/00FFFF:提示：手持左键快捷召唤上次选择的怪物，圈内右键可捕捉/收回敌怪，按 {keyName} 键打开手册，怪物伤害受召唤伤害加成]"));

            // 获取玩家当前选中的怪物信息[cite: 4]
            var modPlayer = Main.LocalPlayer.GetModPlayer<TamedPlayer>();
            int npcType = modPlayer.LastSummonedType;

            if (npcType > 0 && npcType < NPCLoader.NPCCount && modPlayer.UnlockedNPCTypes.Contains(npcType)) {
                if (ContentSamples.NpcsByNetId.TryGetValue(npcType, out NPC sampleNPC)) {
                    Player player = Main.LocalPlayer;
                    // 计算加成后的召唤伤害[cite: 3, 5]
                    int displayDamage = (int)player.GetTotalDamage(DamageClass.Summon).ApplyTo(sampleNPC.defDamage);

                    tooltips.Add(new TooltipLine(Mod, "TargetHeader", $"[c/FFD700:【快捷召唤目标】: {sampleNPC.GivenOrTypeName}]"));
                    tooltips.Add(new TooltipLine(Mod, "TargetStats", $"  生命: [c/55FF55:{sampleNPC.lifeMax}] | 防御: [c/55FFFF:{sampleNPC.defense}] | 伤害: [c/FF6347:{displayDamage}]"));
                    tooltips.Add(new TooltipLine(Mod, "TargetIconSpace", "      ")); // 占位空行，用于下面绘制怪物贴图
                }
            } else {
                tooltips.Add(new TooltipLine(Mod, "TargetHeader", "[c/AAAAAA:【快捷召唤目标】: 暂未选择 (左键或UI中点击选择)]"));
            }
        }

        // 绘制 Tooltip 中怪物的预览贴图[cite: 3]
        public override void PostDrawTooltipLine(DrawableTooltipLine line) {
            if (line.Mod == "terraria_gldty" && line.Name == "TargetIconSpace") {
                var modPlayer = Main.LocalPlayer.GetModPlayer<TamedPlayer>();
                int npcType = modPlayer.LastSummonedType;

                if (npcType > 0 && npcType < NPCLoader.NPCCount) {
                    Main.instance.LoadNPC(npcType);
                    Texture2D texture = TextureAssets.Npc[npcType].Value;

                    int frameCount = Main.npcFrameCount[npcType];
                    Rectangle frame = new Rectangle(0, 0, texture.Width, texture.Height / frameCount);

                    // 缩放适应 Tooltip 行高[cite: 3]
                    float scale = 1f;
                    if (frame.Height > 36) scale = 36f / frame.Height;
                    if (frame.Width * scale > 60) scale = 60f / frame.Width;

                    Vector2 drawPos = new Vector2(line.X + 16, line.Y - 2);
                    Main.spriteBatch.Draw(texture, drawPos, frame, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }

        public override bool? UseItem(Player player) {
            Vector2 mousePos = Main.MouseWorld;
            var modPlayer = player.GetModPlayer<TamedPlayer>();

            // 右键：捕捉[cite: 2, 5]
            if (player.altFunctionUse == 2) {
                for (int i = 0; i < Main.maxNPCs; i++) {
                    NPC target = Main.npc[i];
                    if (target.active && !target.friendly && target.damage > 0 && !target.boss && !target.dontTakeDamage) {
                        if (Vector2.Distance(player.Center, target.Center) <= 150f && Vector2.Distance(mousePos, target.Center) < 50f) {
                            if (!modPlayer.UnlockedNPCTypes.Contains(target.type)) {
                                modPlayer.UnlockedNPCTypes.Add(target.type);
                                modPlayer.LastSummonedType = target.type;
                                Main.NewText($"成功捕获并解锁图鉴: {target.GivenOrTypeName}！", Color.Green);
                            } else {
                                Main.NewText($"已收录 {target.GivenOrTypeName} 的图鉴信息。", Color.Yellow);
                            }
                            
                            for (int d = 0; d < 20; d++) {
                                Dust.NewDust(target.position, target.width, target.height, DustID.Enchanted_Pink, 0, 0);
                            }

                            target.active = false;
                            return true;
                        }
                    }
                }
            }
            // 左键：快捷召唤[cite: 1]
            else {
                if (modPlayer.UnlockedNPCTypes.Count > 0) {
                    int targetType = modPlayer.LastSummonedType;

                    if (!modPlayer.UnlockedNPCTypes.Contains(targetType)) {
                        targetType = modPlayer.UnlockedNPCTypes[modPlayer.UnlockedNPCTypes.Count - 1];
                        modPlayer.LastSummonedType = targetType;
                    }

                    if (modPlayer.TrySummonNPC(targetType)) {
                        SoundEngine.PlaySound(SoundID.Item44, player.Center);
                    } else {
                        SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
                    }
                    return true;
                } else {
                    if (player.whoAmI == Main.myPlayer) {
                        Main.NewText("灵魂手册中尚未收录任何怪物！", Color.Orange);
                    }
                }
            }
            return base.UseItem(player);
        }

        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.Chain, 10)
                .AddIngredient(ItemID.Book, 1)//ManaCrystal
                .AddIngredient(ItemID.ManaCrystal, 1)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}