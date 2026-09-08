using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace terraria_gldty.Content.Items.Items145.Proj
{
    /// <summary>
    /// Arc Surge / 电弧涌动。
    ///
    /// 闪电路径生成部分按 Terraria 1.4.5.8 的
    /// Terraria.GameContent.LightningGenerator 反编译结果移植。
    ///
    /// 重点：
    /// - 5 层旋转累积
    /// - StepSize = 6
    /// - CalcRotations + SmoothRotations
    /// - 原版 Fork 条件
    /// - 原版 Arc Surge 参数
    ///
    /// 视觉绘制仍使用模组自己的 LightningSDF.fx，
    /// 这样不需要依赖 Terraria 内部 StormLightningDrawer。
    /// </summary>
    public class ArcSurgeProj : ModProjectile
    {
        // =========================================================
        // 【Arc Surge 调试参数】
        // =========================================================

        // Projectile 存活时间。
        // 2 = 保持原版短寿命；改成 4~6 可以更容易观察动画。
        private const int LifeTime = 6;

        // 主闪电宽度。越大越粗。
        private const float MainThickness = 17f;  //13f

        // 每隔多少 tick 换一次随机形状。
        // 1 = 每 tick 变化；2 = 每 2 tick 变化。
        private const uint AnimationSeedStep = 2;  //1

        // ===== LightningGenerator 核心参数 =====

        // 路径采样间距。小 = 更平滑/折点更多；大 = 更粗犷。
        private const int GeneratorStepSize = 4;  //6

        // 随机旋转层数。越高，弯曲层次越丰富。
        private const int GeneratorLayers = 7;  //5

        // 基础弯曲强度。越大，闪电越“扭”。
        private const float GeneratorRotationStrength = 1f;  //0.7f

        // 高层随机旋转的衰减倍率。越接近 1，细节越明显。
        private const float GeneratorLayerStrengthFactor = 1.2f;

        // 从路径的多少比例之后开始减少随机性。
        // 0.7 = 后 30% 更稳定，便于收束到目标。
        private const float GeneratorReduceRandomnessAfter = 0.7f;

        // 分支自身的弯曲强度倍率。
        private const float GeneratorForkRotationStrengthMultiplier = 0.9f;

        // 分支路径采样间距倍率。<1 会让分支更密。
        private const float GeneratorForkStepSizeMultiplier = 0.9f;

        // 分支长度倍率。越大，分支越长。
        private const float GeneratorForkLengthMultiplier = 0.7f;

        // 每条闪电最多生成的分支数量。
        private const int GeneratorMaxForksPerBolt = 3;

        // 最大分支递归深度。0=无分支，3=原版配置。
        private const int GeneratorMaxForkDepth = 3;

        // ===== Shader 参数 =====

        // 整体发光强度。过高会让红色变白。
        private const float ShaderGlowStrength = 3.6f;

        // Shader 亮度呼吸幅度。0 = 完全静态，推荐 0~0.01。
        private const float ShaderPulseStrength = 0.006f;

        // ===== 粒子参数 =====

        // 每隔几个路径点检查一次电火花。
        private const int TrailDustPointStep = 2;

        // 电火花概率分母。3 = 约 1/3 概率。
        private const int TrailDustChance = 3;

        // 命中点电火花数量。
        private const int ImpactDustCount = 7;

        private static readonly Color ArcColor =
            new Color(255, 45, 45);

        // tModLoader 不保证暴露原版 Terraria 内部的 FloatRange / LCG32Random。
        // 因此这里提供本地等价实现，避免直接依赖原版内部类型。
        private readonly struct FloatRange
        {
            public readonly float Minimum;
            public readonly float Maximum;

            public FloatRange(float minimum, float maximum)
            {
                Minimum = minimum;
                Maximum = maximum;
            }

            public bool Contains(float value)
            {
                return value >= Minimum && value <= Maximum;
            }

            public float Lerp(float amount)
            {
                return MathHelper.Lerp(Minimum, Maximum, amount);
            }

            public float Lerp(float value, bool unused = false)
            {
                return MathHelper.Lerp(Minimum, Maximum, value);
            }
        }

        private struct LcgRandom
        {
            public uint State;

            public LcgRandom(uint seed)
            {
                State = seed == 0 ? 0x6D2B79F5u : seed;
            }

            private uint NextUInt()
            {
                // 32-bit LCG；用于本地复现 LightningGenerator 的确定性随机序列。
                State = unchecked(State * 1664525u + 1013904223u);
                return State;
            }

            public double NextDouble()
            {
                return NextUInt() / 4294967296.0;
            }

            public float NextFloat()
            {
                return (float)NextDouble();
            }

            public int Next(int maxValue)
            {
                if (maxValue <= 1)
                    return 0;

                return (int)(NextDouble() * maxValue);
            }
        }

        private sealed class LightningBolt
        {
            public Vector2[] Positions;
            public float[] Rotations;
            public FloatRange ProgressRange;
            public int ForkDepth;
            public bool CollidedWithTile;

            public bool IsMainBolt =>
                ForkDepth == 0;
        }

        /// <summary>
        /// Terraria 1.4.5.8 LightningGenerator 的 Arc Surge 配置与
        /// Generate/GenerateBolt/CalcRotations/SmoothRotations 逻辑移植。
        /// </summary>
        private sealed class ArcLightningGenerator
        {
            private const float SourceRotationLimit = 0f;
            private const float Length = 1f;
            private const float RotationStrength = GeneratorRotationStrength;
            private const int StepSize = GeneratorStepSize;
            private const int Layers = GeneratorLayers;
            private const float LayerStrengthFactor = GeneratorLayerStrengthFactor;
            private const float ReduceRandomnessAfter = GeneratorReduceRandomnessAfter;

            private const float ForkRotationStrengthMultiplier = GeneratorForkRotationStrengthMultiplier;
            private const float ForkStepSizeMultiplier = GeneratorForkStepSizeMultiplier;
            private const float ForkLengthMultiplier = GeneratorForkLengthMultiplier;
            private const int MaxForksPerBolt = GeneratorMaxForksPerBolt;
            private const int MaxForkDepth = GeneratorMaxForkDepth;

            public LightningBolt Generate(
                List<LightningBolt> bolts,
                uint seed,
                Vector2 targetPosition,
                Vector2 direction,
                bool calcPositions = true,
                bool calcRotations = true)
            {
                direction = direction.RotatedBy(
                    (new LcgRandom(seed).NextDouble() * 2.0 - 1.0) *
                    SourceRotationLimit
                ) * Length;

                LightningBolt result = GenerateBolt(
                    bolts,
                    seed,
                    0,
                    calcPositions,
                    targetPosition - direction,
                    targetPosition,
                    RotationStrength,
                    StepSize,
                    new FloatRange(0f, 1f)
                );

                if (calcRotations)
                {
                    foreach (LightningBolt bolt in bolts)
                    {
                        if (bolt.Positions != null &&
                            bolt.Positions.Length >= 2)
                        {
                            bolt.Rotations =
                                CalcRotations(bolt.Positions);
                        }
                    }
                }

                return result;
            }

            private LightningBolt GenerateBolt(
                List<LightningBolt> bolts,
                uint seed,
                int depth,
                bool calcPositions,
                Vector2 startPos,
                Vector2 targetPos,
                float rotationStrength,
                float stepSize,
                FloatRange progressRange)
            {
                LcgRandom random =
                    new LcgRandom(seed);

                float rotation = 0f;
                float[] layerRotations =
                    new float[Layers];

                Point targetTile =
                    targetPos.ToTileCoordinates();

                Vector2 position =
                    startPos;

                Vector2 targetVector =
                    targetPos - startPos;

                float distance =
                    targetVector.Length();

                if (distance <= 0.001f)
                {
                    LightningBolt empty =
                        new LightningBolt
                        {
                            Positions = calcPositions
                                ? new[] { startPos }
                                : null,
                            ForkDepth = depth,
                            ProgressRange = progressRange
                        };

                    if (bolts != null)
                        bolts.Add(empty);

                    return empty;
                }

                targetVector /= distance;

                Vector2 perpendicular =
                    new Vector2(
                        targetVector.Y,
                        -targetVector.X
                    );

                int maxPositions =
                    (int)Math.Max(
                        distance * 2f / stepSize,
                        1f
                    );

                int forkCount = 0;

                Vector2[] positions =
                    calcPositions
                        ? new Vector2[maxPositions]
                        : null;

                LightningBolt bolt =
                    new LightningBolt
                    {
                        Positions = positions,
                        ForkDepth = depth,
                        ProgressRange = progressRange
                    };

                int i;

                for (i = 0; i < maxPositions; i++)
                {
                    if (calcPositions)
                        positions[i] = position;

                    Vector2 toTarget =
                        targetPos - position;

                    float forwardDistance =
                        Vector2.Dot(
                            toTarget,
                            targetVector
                        );

                    if (forwardDistance < stepSize)
                        break;

                    float progress =
                        MathHelper.Clamp(
                            1f -
                            forwardDistance / distance,
                            0f,
                            1f
                        );

                    if (position.ToTileCoordinates() != targetTile &&
                        TileCollision(position))
                    {
                        bolt.ProgressRange =
                            new FloatRange(
                                progressRange.Minimum,
                                progressRange.Lerp(progress)
                            );

                        bolt.CollidedWithTile = true;
                        break;
                    }

                    toTarget /= toTarget.Length();

                    float perpendicularError =
                        0f -
                        Vector2.Dot(
                            toTarget,
                            perpendicular
                        );

                    float deviationRange =
                        Math.Max(
                            0.01f,
                            Math.Min(
                                progress,
                                1f - progress
                            ) *
                            GetPerpendicularDeviationFactor(
                                distance
                            ) *
                            2f
                        );

                    float correction =
                        MathHelper.Clamp(
                            perpendicularError /
                            deviationRange,
                            -1f,
                            1f
                        );

                    int layer;

                    if (PickLayerToReroll(
                        random.NextDouble(),
                        0.5f,
                        out layer))
                    {
                        float layerStrength =
                            rotationStrength;

                        for (
                            int layerIndex = Layers - 1;
                            layerIndex > layer;
                            layerIndex--)
                        {
                            layerStrength /=
                                LayerStrengthFactor;
                        }

                        float randomRotation =
                            (float)random.NextDouble() *
                            2f - 1f;

                        randomRotation +=
                            (
                                correction -
                                randomRotation *
                                Math.Abs(correction)
                            ) / 2f;

                        float newLayerRotation =
                            randomRotation *
                            layerStrength;

                        float oldLayerRotation =
                            layerRotations[layer];

                        float deltaRotation =
                            newLayerRotation -
                            oldLayerRotation;

                        rotation +=
                            deltaRotation;

                        layerRotations[layer] =
                            newLayerRotation;

                        if (layer == Layers - 1)
                        {
                            float forkRandom =
                                random.NextFloat();

                            float forkChance =
                                Utils.Remap(
                                    forkCount,
                                    0f,
                                    MaxForksPerBolt,
                                    1f,
                                    0f
                                );

                            float forkRotation =
                                rotation -
                                deltaRotation *
                                (1f +
                                 GetForkReflectAngleMultiplier(
                                     distance
                                 ));

                            if (
                                bolts != null &&
                                Math.Abs(deltaRotation) >=
                                    rotationStrength *
                                    GetForkGenerationThreshold(
                                        distance
                                    ) &&
                                GetForkProgressRange(
                                    distance
                                ).Contains(progress) &&
                                depth < MaxForkDepth &&
                                forkRandom < forkChance &&
                                Math.Abs(forkRotation) <
                                    MathF.PI * 4f / 9f
                            )
                            {
                                forkCount++;

                                float forkLength =
                                    (1f - progress) *
                                    ForkLengthMultiplier;

                                Vector2 forkTarget =
                                    position +
                                    toTarget.RotatedBy(
                                        forkRotation
                                    ) *
                                    distance *
                                    forkLength;

                                GenerateBolt(
                                    bolts,
                                    unchecked(random.State + 1u),
                                    depth + 1,
                                    calcPositions,
                                    position,
                                    forkTarget,
                                    rotationStrength *
                                        ForkRotationStrengthMultiplier,
                                    stepSize *
                                        ForkStepSizeMultiplier,
                                    new FloatRange(
                                        progressRange.Lerp(
                                            progress
                                        ),
                                        progressRange.Lerp(
                                            progress +
                                            forkLength
                                        )
                                    )
                                );
                            }
                        }
                    }

                    float rerollChance =
                        Utils.Remap(
                            progress,
                            ReduceRandomnessAfter,
                            1f,
                            0f,
                            1f
                        );

                    rerollChance +=
                        Utils.Remap(
                            Math.Abs(correction),
                            0.5f,
                            1f,
                            0f,
                            1f
                        );

                    if (PickHighLayerToReroll(
                        random.NextDouble(),
                        rerollChance,
                        out layer))
                    {
                        rotation -=
                            layerRotations[layer];

                        layerRotations[layer] =
                            0f;
                    }

                    position +=
                        toTarget.RotatedBy(rotation) *
                        stepSize;
                }

                if (calcPositions &&
                    i < maxPositions)
                {
                    Array.Resize(
                        ref positions,
                        i + 1
                    );

                    bolt.Positions =
                        positions;
                }

                if (bolts != null &&
                    (bolt.IsMainBolt || i > 2))
                {
                    bolts.Add(bolt);
                }

                return bolt;
            }

            private static float GetPerpendicularDeviationFactor(
                float distance)
            {
                return Utils.Remap(
                    distance,
                    0f,
                    1000f,
                    5f,
                    1f
                );
            }

            private static float GetForkGenerationThreshold(
                float distance)
            {
                return Utils.Remap(
                    distance,
                    0f,
                    1000f,
                    0.3f,
                    0.5f
                );
            }

            private static float GetForkReflectAngleMultiplier(
                float distance)
            {
                return Utils.Remap(
                    distance,
                    0f,
                    1000f,
                    0.6f,
                    0.2f
                );
            }

            private static FloatRange GetForkProgressRange(
                float distance)
            {
                return new FloatRange(
                    Utils.Remap(
                        distance,
                        0f,
                        1000f,
                        0.1f,
                        0.5f
                    ),
                    0.8f
                );
            }

            private static bool TileCollision(
                Vector2 position)
            {
                Point tilePosition =
                    position.ToTileCoordinates();

                if (!WorldGen.InWorld(
                    tilePosition.X,
                    tilePosition.Y,
                    5))
                {
                    return false;
                }

                Tile tile =
                    Framing.GetTileSafely(
                        tilePosition.X,
                        tilePosition.Y
                    );

                return tile.HasTile &&
                    Main.tileSolid[tile.TileType] &&
                    !Main.tileSolidTop[tile.TileType];
            }

            // Terraria 1.4.5.8 CalcRotations。
            private static float[] CalcRotations(
                Vector2[] positions)
            {
                float[] rotations =
                    new float[positions.Length];

                CalcRotations(
                    positions,
                    rotations
                );

                return rotations;
            }

            // Terraria 1.4.5.8 CalcRotations。
            private static void CalcRotations(
                Vector2[] positions,
                float[] rotations)
            {
                if (rotations.Length < 2)
                    return;

                int index = 0;

                float previousRotation =
                    (positions[0] - positions[1])
                        .ToRotation();

                rotations[index++] =
                    previousRotation;

                while (
                    index <
                    rotations.Length - 1)
                {
                    float currentRotation =
                        (
                            positions[index] -
                            positions[index + 1]
                        ).ToRotation();

                    rotations[index++] =
                        previousRotation +
                        MathHelper.WrapAngle(
                            currentRotation -
                            previousRotation
                        ) / 2f;

                    previousRotation =
                        currentRotation;
                }

                rotations[index] =
                    previousRotation;

                SmoothRotations(rotations);
            }

            // Terraria 1.4.5.8 SmoothRotations。
            private static void SmoothRotations(
                float[] rotations)
            {
                float previous =
                    rotations[0];

                for (
                    int i = 1;
                    i < rotations.Length - 1;
                    i++)
                {
                    float current =
                        rotations[i];

                    float next =
                        rotations[i + 1];

                    rotations[i] =
                        current +
                        (
                            MathHelper.WrapAngle(
                                previous -
                                current
                            ) +
                            MathHelper.WrapAngle(
                                next -
                                current
                            )
                        ) / 2f;

                    previous =
                        current;
                }
            }

            private static bool PickHighLayerToReroll(
                double randomValue,
                float chance,
                out int layer)
            {
                if (!PickLayerToReroll(
                    randomValue,
                    chance,
                    out layer))
                {
                    return false;
                }

                layer =
                    Layers - 1 - layer;

                return true;
            }

            private static bool PickLayerToReroll(
                double randomValue,
                float chance,
                out int layer)
            {
                for (
                    layer = 0;
                    layer < Layers;
                    layer++)
                {
                    if (
                        randomValue >=
                        1f - chance)
                    {
                        return true;
                    }

                    randomValue /=
                        chance;
                }

                return false;
            }
        }

        private List<LightningBolt> _bolts;
        private LightningBolt _mainBolt;
        private Vector2 _arcStart;
        private Vector2 _arcEnd;
        private bool _generated;

        private static Asset<Effect> _lightningEffectAsset;

        public override string Texture =>
            "Terraria/Images/Projectile_1122";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LifeTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;

            Main.projFrames[Projectile.type] = 1;
        }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _lightningEffectAsset =
                ModContent.Request<Effect>(
                    "terraria_gldty/Assets/Effects/LightningSDF",
                    AssetRequestMode.ImmediateLoad
                );
        }

        public override void Unload()
        {
            _lightningEffectAsset = null;
        }

        private Color GetDyeColor()
        {
            int dyeItemId =
                (int)Projectile.ai[1];

            if (dyeItemId <= 0)
                return ArcColor;

            int shaderId =
                GameShaders.Armor.GetShaderIdFromItemId(
                    dyeItemId
                );

            if (shaderId > 0)
            {
                ArmorShaderData shader =
                    GameShaders.Armor.GetSecondaryShader(
                        shaderId,
                        Main.LocalPlayer
                    );

                if (shader != null)
                {
                    try
                    {
                        var field =
                            typeof(ArmorShaderData).GetField(
                                "_uColor",
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.Instance
                            );

                        if (field != null)
                        {
                            Vector3 value =
                                (Vector3)field.GetValue(shader);

                            if (value != Vector3.Zero)
                            {
                                return new Color(
                                    value.X,
                                    value.Y,
                                    value.Z
                                );
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return ArcColor;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active)
            {
                Projectile.Kill();
                return;
            }

            // 实时更新起点为玩家中心，防止移动时闪电脱节
            _arcStart = player.Center;

            if (!_generated)
            {
                _generated = true;
                Vector2 requestedEnd = Projectile.Center;
                _arcEnd = FindValidEndpoint(_arcStart, requestedEnd);
                Projectile.Center = _arcEnd;

                GenerateArcLightning(_arcStart, _arcEnd);
                SpawnImpactDust(_arcEnd);

                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.55f, PitchVariance = 0.15f }, _arcEnd);
                }

                if (Main.netMode != NetmodeID.Server && Projectile.owner == Main.myPlayer)
                {
                    Main.instance.CameraModifiers.Add(
                        new PunchCameraModifier(_arcEnd, new Vector2(0f, 1f), 3.5f, 5f, 8, 700f, "ArcSurgeShake")
                    );
                }
            }

            if (Main.netMode != NetmodeID.Server && _arcEnd != Vector2.Zero)
            {
                Color color = GetDyeColor();
                Lighting.AddLight(_arcEnd, color.ToVector3() * 0.9f);
            }

            if (Projectile.timeLeft <= 1)
                Projectile.Kill();
        }

        private Vector2 FindValidEndpoint(
            Vector2 start,
            Vector2 requestedEnd)
        {
            Vector2 direction =
                requestedEnd - start;

            float distance =
                direction.Length();

            if (distance <= 2f)
                return requestedEnd;

            direction.Normalize();

            const float step = 4f;
            Vector2 lastValid = start;

            for (
                float travelled = 0f;
                travelled <= distance;
                travelled += step)
            {
                Vector2 position =
                    start +
                    direction * travelled;

                int tileX =
                    (int)(position.X / 16f);

                int tileY =
                    (int)(position.Y / 16f);

                if (!WorldGen.InWorld(
                    tileX,
                    tileY,
                    5))
                {
                    break;
                }

                Tile tile =
                    Framing.GetTileSafely(
                        tileX,
                        tileY
                    );

                if (
                    tile.HasTile &&
                    Main.tileSolid[tile.TileType] &&
                    !Main.tileSolidTop[tile.TileType]
                )
                {
                    return lastValid;
                }

                if (tile.LiquidAmount > 0)
                    return lastValid;

                lastValid = position;
            }

            return lastValid;
        }

        private void GenerateArcLightning(Vector2 start, Vector2 end)
        {
            _bolts = new List<LightningBolt>(8);
            ArcLightningGenerator generator = new ArcLightningGenerator();

            // 2. 引入 Main.GameUpdateCount 动态种子（解决问题 1：即使鼠标不动，每次发射形状也会随机改变）
            uint seed = unchecked((uint)(
                Projectile.identity * 397
                ^ Projectile.owner * 7919
                ^ (int)Main.GameUpdateCount * 1337
            ));

            Vector2 direction = end - start;

            _mainBolt = generator.Generate(
                _bolts,
                seed,
                end,
                direction,
                true,
                true
            );

            if (Main.netMode != NetmodeID.Server && _mainBolt != null)
            {
                SpawnTrailDust(_mainBolt);
            }
        }

        private void SpawnTrailDust(
            LightningBolt bolt)
        {
            if (bolt?.Positions == null)
                return;

            Color color =
                GetDyeColor();

            int maxValue =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        bolt.Positions.Length / 10f
                    )
                );

            LcgRandom random =
                new LcgRandom(
                    unchecked(
                        (uint)(
                            Projectile.identity +
                            Projectile.owner * 1009
                        )
                    )
                );

            for (
                int i = 5;
                i < bolt.Positions.Length - 5;
                i++)
            {
                if (random.Next(maxValue) != 0)
                    continue;

                Vector2 velocity =
                    Vector2.UnitY;

                if (
                    bolt.Rotations != null &&
                    i < bolt.Rotations.Length)
                {
                    velocity =
                        -bolt.Rotations[i]
                            .ToRotationVector2();
                }

                velocity *=
                    3f +
                    random.NextFloat() * 6.5f;

                Dust dust =
                    Dust.NewDustPerfect(
                        bolt.Positions[i],
                        DustID.Electric
                    );

                // tModLoader 当前 Dust API 没有 HackFrame；保持电弧粒子颜色与运动即可。
                dust.color = color;
                dust.velocity = velocity;
                dust.fadeIn = 0f;
                dust.scale =
                    0.4f +
                    random.NextFloat() * 0.5f;
                dust.noGravity = true;
            }
        }

        private void SpawnImpactDust(
            Vector2 position)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Color color =
                GetDyeColor();

            for (int i = 0; i < ImpactDustCount; i++)
            {
                Dust dust =
                    Dust.NewDustPerfect(
                        position,
                        DustID.Electric,
                        Main.rand.NextVector2Circular(
                            4f,
                            4f
                        )
                    );

                dust.noGravity = true;
                dust.color = color;
                dust.scale =
                    Main.rand.NextFloat(
                        0.8f,
                        1.35f
                    );
            }
        }

        public override bool PreDraw(
            ref Color lightColor)
        {
            if (
                _bolts == null ||
                _bolts.Count == 0 ||
                _lightningEffectAsset == null)
            {
                return false;
            }

            Effect effect =
                _lightningEffectAsset.Value;

            Color color =
                GetDyeColor();

            float opacity =
                Projectile.timeLeft <= 1
                    ? 0.78f
                    : 1f;

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.PointWrap,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            Matrix projection =
                Matrix.CreateOrthographicOffCenter(
                    0,
                    Main.screenWidth,
                    Main.screenHeight,
                    0,
                    0,
                    -1
                );

            effect.Parameters[
                "WorldViewProjection"
            ]?.SetValue(projection);

            effect.Parameters[
                "Time"
            ]?.SetValue(
                (float)Main.timeForVisualEffects
            );

            effect.Parameters[
                "GlowStrength"
            ]?.SetValue(
                3.2f * opacity
            );

            foreach (LightningBolt bolt in _bolts)
            {
                if (
                    bolt?.Positions == null ||
                    bolt.Positions.Length < 2)
                {
                    continue;
                }

                float width =
                    bolt.IsMainBolt
                        ? MainThickness
                        : MainThickness *
                          MathHelper.Lerp(
                              0.42f,
                              0.18f,
                              MathHelper.Clamp(
                                  bolt.ForkDepth / 3f,
                                  0f,
                                  1f
                              )
                          );

                DrawBolt(
                    bolt,
                    effect,
                    color,
                    opacity,
                    width
                );
            }

            Main.spriteBatch.End();

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

        private void DrawBolt(
            LightningBolt bolt,
            Effect effect,
            Color color,
            float opacity,
            float width)
        {
            Vector2[] positions =
                bolt.Positions;

            int count =
                positions.Length;

            VertexPositionColorTexture[] vertices =
                new VertexPositionColorTexture[
                    (count - 1) * 6
                ];

            int vertexIndex = 0;

            for (int i = 0; i < count - 1; i++)
            {
                Vector2 start =
                    positions[i] -
                    Main.screenPosition;

                Vector2 end =
                    positions[i + 1] -
                    Main.screenPosition;

                Vector2 direction =
                    end - start;

                if (direction.LengthSquared() < 0.01f)
                    continue;

                direction.Normalize();

                Vector2 normal =
                    new Vector2(
                        -direction.Y,
                        direction.X
                    );

                float progress =
                    i / (float)(count - 1);

                float localWidth =
                    width *
                    MathHelper.Lerp(
                        1f,
                        0.72f,
                        progress
                    );

                Vector2 offset =
                    normal * localWidth;

                float branchFade =
                    bolt.IsMainBolt
                        ? 1f
                        : MathHelper.Lerp(
                            1f,
                            0.15f,
                            MathHelper.Clamp(
                                progress,
                                0f,
                                1f
                            )
                          );

                Color c =
                    color *
                    opacity *
                    branchFade;

                float v0 = progress;
                float v1 =
                    (i + 1f) /
                    (count - 1);

                vertices[vertexIndex++] =
                    new VertexPositionColorTexture(
                        new Vector3(start - offset, 0f),
                        c,
                        new Vector2(-1f, v0)
                    );

                vertices[vertexIndex++] =
                    new VertexPositionColorTexture(
                        new Vector3(end - offset, 0f),
                        c,
                        new Vector2(-1f, v1)
                    );

                vertices[vertexIndex++] =
                    new VertexPositionColorTexture(
                        new Vector3(start + offset, 0f),
                        c,
                        new Vector2(1f, v0)
                    );

                vertices[vertexIndex++] =
                    new VertexPositionColorTexture(
                        new Vector3(start + offset, 0f),
                        c,
                        new Vector2(1f, v0)
                    );

                vertices[vertexIndex++] =
                    new VertexPositionColorTexture(
                        new Vector3(end - offset, 0f),
                        c,
                        new Vector2(-1f, v1)
                    );

                vertices[vertexIndex++] =
                    new VertexPositionColorTexture(
                        new Vector3(end + offset, 0f),
                        c,
                        new Vector2(1f, v1)
                    );
            }

            if (vertexIndex < 3)
                return;

            effect.CurrentTechnique
                .Passes[0]
                .Apply();

            Main.graphics.GraphicsDevice
                .DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertexIndex / 3
                );
        }

        public override bool? Colliding(
            Rectangle projHitbox,
            Rectangle targetHitbox)
        {
            if (
                !_generated ||
                _arcStart == Vector2.Zero ||
                _arcEnd == Vector2.Zero)
            {
                return false;
            }

            float pointDistance = 0f;
            const float collisionWidth = 18f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                _arcStart,
                _arcEnd,
                collisionWidth,
                ref pointDistance
            );
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            //const int RedZappedBuffID = 400;
            int RedZappedBuffID = ModContent.BuffType<Content.Buff.RedZapped>();

            int duration =
                Main.rand.Next(
                    4 * 60,
                    7 * 60 + 1
                );

            target.AddBuff(
                RedZappedBuffID,
                duration
            );

            if (Main.netMode != NetmodeID.Server)
            {
                Color color =
                    GetDyeColor();

                for (int i = 0; i < 3; i++)
                {
                    Dust dust =
                        Dust.NewDustPerfect(
                            target.Center,
                            DustID.Electric,
                            Main.rand.NextVector2Circular(
                                2.5f,
                                2.5f
                            )
                        );

                    dust.noGravity = true;
                    dust.color = color;
                    dust.scale = 0.7f;
                }
            }
        }
    }
}
