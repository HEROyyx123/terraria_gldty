using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace terraria_gldty.Content.Items.Items145.Proj
{
    public class StormLightningProj : ModProjectile
    {
        private const int MaxChargeTime = 20; // 蓄力帧数
        private const int StayTime = 70;      // 闪电滞留消散帧数

        private class LightningBranch
        {
            public List<Vector2> Points = new List<Vector2>();
            public float Thickness;
            public List<LightningBranch> Children = new List<LightningBranch>();
        }

        private LightningBranch _mainLightningTree;
        private static Asset<Effect> _lightningEffectAsset;

        private int _targetNPCIndex = -1;
        private Vector2 _lockedWorldPos;
        private Vector2 _impactWorldPos;

        public override void SetDefaults()
        {
            Projectile.width = 240;
            Projectile.height = 240;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxChargeTime + StayTime;
            Main.projFrames[Projectile.type] = 1;
        }

        public override void Load()
        {
            if (Main.dedServ) return;
            _lightningEffectAsset = ModContent.Request<Effect>("terraria_gldty/Assets/Effects/LightningSDF", AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            _lightningEffectAsset = null;
        }

        private Color GetDyeColor()
        {
            int dyeItemId = (int)Projectile.ai[1];
            int shaderId = GameShaders.Armor.GetShaderIdFromItemId(dyeItemId);

            if (shaderId > 0)
            {
                ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(shaderId, Main.LocalPlayer);
                if (shader != null)
                {
                    var field = typeof(ArmorShaderData).GetField("_uColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        Vector3 uColor = (Vector3)field.GetValue(shader);
                        if (uColor != Vector3.Zero)
                            return new Color(uColor.X, uColor.Y, uColor.Z);
                    }
                }
            }
            return Color.White;
        }

        public override void AI()
        {
            // 1. 初始化落点与锁定目标
            if (Projectile.ai[0] == 0)
            {
                _lockedWorldPos = Projectile.Center;
                _targetNPCIndex = FindTargetNPC(Projectile.Center, 150f);
            }

            Color dyeColor = GetDyeColor();

            // 2. 蓄力阶段 (Projectile.ai[0] < MaxChargeTime)
            if (Projectile.ai[0] < MaxChargeTime)
            {
                Vector2 currentTargetPos = GetCurrentTargetPosition();
                Projectile.Center = currentTargetPos;

                Projectile.ai[0]++;
                float chargeProgress = Projectile.ai[0] / MaxChargeTime;
                Lighting.AddLight(currentTargetPos, dyeColor.ToVector3() * chargeProgress);

                // 蓄力粒子效果
                if (Main.rand.NextFloat() <= chargeProgress * 0.4f)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(50f, 50f) * (1f - chargeProgress * 0.5f);
                    Dust d = Dust.NewDustPerfect(currentTargetPos + offset, DustID.Electric, -offset * 0.1f);
                    d.noGravity = true;
                    d.color = dyeColor;
                    d.scale = 0.6f + chargeProgress * 0.5f;
                }

                // 蓄力完成，闪电落下瞬时逻辑
                if (Projectile.ai[0] == MaxChargeTime)
                {
                    // 锁定落点绝对世界坐标
                    _impactWorldPos = currentTargetPos;

                    // 音效
                    string[] soundPaths = new string[]
                    {
                        "terraria_gldty/Assets/Sounds/Instant_thunder_0",
                        "terraria_gldty/Assets/Sounds/Instant_thunder_1",
                        "terraria_gldty/Assets/Sounds/Instant_thunder_2"
                    };
                    SoundEngine.PlaySound(new SoundStyle(soundPaths[Main.rand.Next(soundPaths.Length)])
                    {
                        PitchVariance = 0.2f,
                        Volume = 1.0f
                    }, _impactWorldPos);

                    // 生成闪电树结构与粒子
                    GenerateTreeLightning(_impactWorldPos);

                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        _impactWorldPos, new Vector2(0, 1f), 22f, 8f, 25, 1200f, "StormLightningShake"));

                    for (int i = 0; i < 12; i++)
                    {
                        Dust d = Dust.NewDustPerfect(_impactWorldPos, DustID.Electric, Main.rand.NextVector2Circular(8f, 8f));
                        d.noGravity = true;
                        d.color = dyeColor;
                        d.scale = Main.rand.NextFloat(1.0f, 1.8f);
                    }

                    if (_mainLightningTree != null)
                        SpawnLightningTrailDust(_mainLightningTree, dyeColor);
                }
            }
            // 3. 闪电落地消散阶段 (Projectile.ai[0] >= MaxChargeTime)
            else
            {
                Projectile.ai[0]++;
                Projectile.Center = _impactWorldPos; // 锚定中心
            }
        }

        private int FindTargetNPC(Vector2 searchPos, float maxDistance)
        {
            int closestIndex = -1;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(searchPos, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }
            }
            return closestIndex;
        }

        private Vector2 GetCurrentTargetPosition()
        {
            if (_targetNPCIndex >= 0 && _targetNPCIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[_targetNPCIndex];
                if (npc.active && !npc.friendly) return npc.Center;
            }
            return _lockedWorldPos;
        }

        private void GenerateTreeLightning(Vector2 targetPos)
        {
            Vector2 startPos = targetPos - new Vector2(Main.rand.NextFloat(-40f, 40f), 850f);

            _mainLightningTree = new LightningBranch { Thickness = 65f };
            GenerateBranchRecursive(startPos, targetPos, _mainLightningTree, depth: 0, maxDepth: 4);
        }

        private void GenerateBranchRecursive(Vector2 start, Vector2 end, LightningBranch currentBranch, int depth, int maxDepth)
        {
            currentBranch.Points.Clear();
            currentBranch.Points.Add(start);

            Vector2 current = start;
            Vector2 mainDir = Vector2.Normalize(end - start);
            float totalDist = Vector2.Distance(start, end);

            float stepLength = MathHelper.Clamp(totalDist / 35f, 15f, 25f);
            int maxSteps = (int)(totalDist / stepLength);

            for (int i = 0; i < maxSteps; i++)
            {
                float progress = (float)i / maxSteps;
                Vector2 targetPoint = Vector2.Lerp(start, end, progress);
                Vector2 perp = new Vector2(-mainDir.Y, mainDir.X);

                float offsetRange = MathHelper.Lerp(22f, 10f, progress) * (1f - (depth * 0.15f));
                float randomOffset = Main.rand.NextFloat(-offsetRange, offsetRange);

                Vector2 nextPoint = targetPoint + perp * randomOffset;
                if (nextPoint.Y < current.Y) nextPoint.Y = current.Y + 3f;

                currentBranch.Points.Add(nextPoint);
                current = nextPoint;

                float branchChance = 0.28f / (depth + 1);
                if (Main.rand.NextFloat() < branchChance && depth < maxDepth && i < maxSteps - 3)
                {
                    float sideAngle = Main.rand.NextBool() ? Main.rand.NextFloat(0.35f, 0.7f) : Main.rand.NextFloat(-0.7f, -0.35f);
                    Vector2 sideDir = mainDir.RotatedBy(sideAngle);
                    float branchLength = (totalDist * (1f - progress * 0.5f)) * Main.rand.NextFloat(0.2f, 0.45f) / (depth + 1);
                    Vector2 sideEnd = current + sideDir * branchLength;

                    var childBranch = new LightningBranch { Thickness = currentBranch.Thickness * 0.45f };
                    currentBranch.Children.Add(childBranch);

                    GenerateBranchRecursive(current, sideEnd, childBranch, depth + 1, maxDepth);
                }
            }
            currentBranch.Points.Add(end);
        }

        private void SpawnLightningTrailDust(LightningBranch branch, Color dyeColor)
        {
            for (int i = 0; i < branch.Points.Count; i += 2)
            {
                Vector2 pos = branch.Points[i];
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(pos, DustID.Electric, Main.rand.NextVector2Circular(1.5f, 1.5f));
                    d.noGravity = true;
                    d.color = dyeColor;
                    d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                }
            }
            foreach (var child in branch.Children)
                SpawnLightningTrailDust(child, dyeColor);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Effect effect = _lightningEffectAsset?.Value;
            Vector2 targetPos = GetCurrentTargetPosition();
            Vector2 drawPos = targetPos - Main.screenPosition;

            int dyeItemId = (int)Projectile.ai[1];
            int shaderId = GameShaders.Armor.GetShaderIdFromItemId(dyeItemId);
            Color dyeColor = GetDyeColor();

            float timeAfterImpact = Projectile.ai[0] - MaxChargeTime;

            // 1. 蓄力贴图
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            float texOpacity = Projectile.ai[0] < MaxChargeTime
                ? (Projectile.ai[0] / MaxChargeTime)
                : MathF.Pow(MathHelper.Clamp(1f - (timeAfterImpact / (float)StayTime), 0f, 1f), 2f);

            if (texOpacity > 0f)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                DrawData drawData = new DrawData(texture, drawPos, null, dyeColor * texOpacity, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
                if (shaderId > 0)
                    GameShaders.Armor.GetSecondaryShader(shaderId, Main.LocalPlayer).Apply(Projectile, drawData);

                drawData.Draw(Main.spriteBatch);
                Main.spriteBatch.End();
            }

            // 2. 闪电 SDF 直接屏幕空间绘制
            if (Projectile.ai[0] >= MaxChargeTime && _mainLightningTree != null && effect != null)
            {
                float opacity = MathF.Pow(MathHelper.Clamp(1f - (timeAfterImpact / (float)StayTime), 0f, 1f), 1.8f);

                Lighting.AddLight(targetPos, dyeColor.ToVector3() * opacity * 1.5f);

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, -1);
                effect.Parameters["WorldViewProjection"]?.SetValue(projection);
                effect.Parameters["Time"]?.SetValue((float)Main.timeForVisualEffects);
                effect.Parameters["GlowStrength"]?.SetValue(opacity * 3.0f);

                if (shaderId > 0)
                {
                    DrawData dummyData = new DrawData(TextureAssets.MagicPixel.Value, Vector2.Zero, Color.White);
                    GameShaders.Armor.GetSecondaryShader(shaderId, Main.LocalPlayer).Apply(Projectile, dummyData);
                }

                DrawBranchRecursive(_mainLightningTree, effect, dyeColor, opacity, isRoot: true);

                Main.spriteBatch.End();
            }

            // 3. 恢复引擎默认状态
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            return false;
        }

        private void DrawBranchRecursive(LightningBranch branch, Effect effect, Color baseColor, float opacity, bool isRoot = false)
        {
            int count = branch.Points.Count;
            if (count < 2) return;

            Vector2[] screenPoints = new Vector2[count];
            for (int i = 0; i < count; i++)
                screenPoints[i] = branch.Points[i] - Main.screenPosition;

            Vector2[] miterNormals = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    Vector2 dir = Vector2.Normalize(screenPoints[1] - screenPoints[0]);
                    miterNormals[0] = new Vector2(-dir.Y, dir.X);
                }
                else if (i == count - 1)
                {
                    Vector2 dir = Vector2.Normalize(screenPoints[count - 1] - screenPoints[count - 2]);
                    miterNormals[count - 1] = new Vector2(-dir.Y, dir.X);
                }
                else
                {
                    Vector2 dir1 = Vector2.Normalize(screenPoints[i] - screenPoints[i - 1]);
                    Vector2 dir2 = Vector2.Normalize(screenPoints[i + 1] - screenPoints[i]);

                    Vector2 n1 = new Vector2(-dir1.Y, dir1.X);
                    Vector2 n2 = new Vector2(-dir2.Y, dir2.X);
                    Vector2 avgNormal = Vector2.Normalize(n1 + n2);

                    float dot = Vector2.Dot(avgNormal, n1);
                    float miterLength = dot > 0.2f ? (1.0f / dot) : 1.0f;
                    miterNormals[i] = avgNormal * Math.Min(miterLength, 1.1f);
                }
            }

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(count - 1) * 6];
            int index = 0;

            for (int i = 0; i < count - 1; i++)
            {
                Vector2 start = screenPoints[i];
                Vector2 end = screenPoints[i + 1];

                float progressStart = (float)i / (count - 1);
                float progressEnd = (float)(i + 1) / (count - 1);

                float fadeStart = isRoot ? 1.0f : (1.0f - MathF.Pow(progressStart, 2.0f));
                float fadeEnd = isRoot ? 1.0f : (1.0f - MathF.Pow(progressEnd, 2.0f));

                Color vertColorStart = baseColor * opacity * fadeStart;
                Color vertColorEnd = baseColor * opacity * fadeEnd;

                float widthStart = MathHelper.Lerp(branch.Thickness, branch.Thickness * 0.2f, progressStart);
                float widthEnd = MathHelper.Lerp(branch.Thickness, branch.Thickness * 0.2f, progressEnd);

                Vector2 nStart = miterNormals[i] * widthStart;
                Vector2 nEnd = miterNormals[i + 1] * widthEnd;

                vertices[index++] = new VertexPositionColorTexture(new Vector3(start - nStart, 0), vertColorStart, new Vector2(-1, progressStart));
                vertices[index++] = new VertexPositionColorTexture(new Vector3(end - nEnd, 0), vertColorEnd, new Vector2(-1, progressEnd));
                vertices[index++] = new VertexPositionColorTexture(new Vector3(start + nStart, 0), vertColorStart, new Vector2(1, progressStart));

                vertices[index++] = new VertexPositionColorTexture(new Vector3(start + nStart, 0), vertColorStart, new Vector2(1, progressStart));
                vertices[index++] = new VertexPositionColorTexture(new Vector3(end - nEnd, 0), vertColorEnd, new Vector2(-1, progressEnd));
                vertices[index++] = new VertexPositionColorTexture(new Vector3(end + nEnd, 0), vertColorEnd, new Vector2(1, progressEnd));
            }

            effect.CurrentTechnique.Passes[0].Apply();
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);

            foreach (var child in branch.Children)
            {
                DrawBranchRecursive(child, effect, baseColor, opacity, isRoot: false);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.ai[0] >= MaxChargeTime)
                return projHitbox.Intersects(targetHitbox);
            return false;
        }
    }
}