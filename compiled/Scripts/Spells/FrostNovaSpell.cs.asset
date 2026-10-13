using System.Collections;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Frost Nova: ice shards burst out and stop, two rings of faceted ice crystals erupt from the
    /// floor, cold mist rolls out, frost spreads under it and snow drifts down. The crystals hold
    /// for a moment and shatter into shards and ice dust, and the dummies caught in the ring stay
    /// frozen until they do.
    ///
    /// Shows: particles working with meshes. The crystals are a prefab of lit, faceted meshes that
    /// an EntityPool hands out, one instance per cast; the script lays the ring out, grows every
    /// crystal with an overshoot (inner ring first) and, at the shatter, emits shards and dust at
    /// each crystal's own position with EmitParams. Also: position easing (OutExpo shards burst out
    /// and stop dead) and horizontal billboards that lie on the floor.
    /// </summary>
    [ScriptSourceFile]
    public class FrostNovaSpell : Spell
    {
        public string crystalPrefab = "app:/data/Prefabs/FrostCrystals.pfb";
        public float freezeRadius = 4.5f;
        /// <summary>Seconds the crystals stand before they shatter.</summary>
        public float shatterAfter = 1.85f;
        /// <summary>Seconds a crystal takes to burst out of the floor.</summary>
        public float growSeconds = 0.16f;
        /// <summary>Delay of the outer ring after the inner one.</summary>
        public float outerRingDelay = 0.06f;
        public int innerCount = 8;
        public float innerRadius = 1.3f;
        public float outerRadius = 2.6f;
        public float flashIntensity = 500.0f;

        static readonly SpellInfo Info = new SpellInfo
        {
            slot = 3,
            name = "Frost Nova",
            description = "Ice crystals erupt in a ring and shatter. Dummies caught in it freeze.",
            accent = "#8fe6ff"
        };

        static readonly Color FrostColor = new Color(0.55f, 0.85f, 1.0f, 1.0f);
        const float HiddenScale = 0.001f;

        ParticleEmitterComponent flash;
        ParticleEmitterComponent spikesGlow;
        ParticleEmitterComponent shards;
        ParticleEmitterComponent shatter;
        ParticleEmitterComponent shatterDust;
        ParticleEmitterComponent ring;
        ParticleEmitterComponent mist;
        ParticleEmitterComponent patch;
        ParticleEmitterComponent snow;
        ParticleEmitterComponent sparkles;
        LightFlash coldLight;
        EntityPool crystals;

        public override SpellInfo GetInfo()
        {
            return Info;
        }

        public override void OnStart()
        {
            flash = GetEmitter("Frost Flash");
            spikesGlow = GetEmitter("Frost Spikes Glow");
            shards = GetEmitter("Frost Shards");
            shatter = GetEmitter("Frost Shatter");
            shatterDust = GetEmitter("Frost Shatter Dust");
            ring = GetEmitter("Frost Ring");
            mist = GetEmitter("Frost Mist");
            patch = GetEmitter("Frost Patch");
            snow = GetEmitter("Frost Snow");
            sparkles = GetEmitter("Frost Sparkles");
            coldLight = GetLight("Frost Light");
            crystals = EntityPool.For(Assets.GetAsset<Prefab>(crystalPrefab));
            crystals.Prewarm(2);
        }

        protected override void Cast(Vector3 target)
        {
            transform.position = target;
            flash.Emit(1);
            spikesGlow.Emit(1);
            shards.Emit(40);
            ring.Emit(1);
            mist.Emit(20);
            patch.Emit(1);
            snow.Emit(40);
            sparkles.Emit(50);
            coldLight.Flash(flashIntensity, 1.2f);
            OrbitCamera.Shake(0.15f, 0.25f);
            foreach (var dummy in GetDummiesNear(target, freezeRadius))
            {
                dummy.Hit(FrostColor, 1.0f, shatterAfter);
                sparkles.Emit(12, new EmitParams { position = dummy.GetCenter() });
            }
            StartCoroutine(Erupt(crystals.Acquire(target, Quaternion.identity)));
        }

        IEnumerator Erupt(Entity crystalRing)
        {
            var pieces = new TransformComponent[CountCrystals(crystalRing)];
            var sizes = new float[pieces.Length];
            var delays = new float[pieces.Length];
            for (int i = 0; i < pieces.Length; ++i)
            {
                pieces[i] = crystalRing.transform.FindChild($"Frost Crystal {i + 1}").transform;
                bool inner = i < innerCount;
                int count = inner ? innerCount : pieces.Length - innerCount;
                int slot = inner ? i : i - innerCount;
                float angle = (slot + Random.Range(-0.3f, 0.3f)) * 360.0f / count + (inner ? 20.0f : 0.0f);
                float radius = (inner ? innerRadius : outerRadius) + Random.Range(-0.2f, 0.2f);
                float radians = angle * Mathf.Deg2Rad;
                pieces[i].localPosition = new Vector3(Mathf.Sin(radians) * radius, -0.05f, Mathf.Cos(radians) * radius);
                // Lean out of the nova's centre, turned at random about the crystal's own axis.
                pieces[i].localRotation = Quaternion.Euler(0.0f, angle, 0.0f) *
                                          Quaternion.Euler(Random.Range(14.0f, 30.0f), Random.Range(0.0f, 360.0f), 0.0f);
                pieces[i].localScale = Vector3.one * HiddenScale;
                sizes[i] = inner ? Random.Range(1.0f, 1.4f) : Random.Range(1.6f, 2.3f);
                delays[i] = (inner ? 0.0f : outerRingDelay) + Random.Range(0.0f, 0.04f);
            }
            float elapsed = 0.0f;
            while (elapsed < shatterAfter)
            {
                elapsed += Time.deltaTime;
                for (int i = 0; i < pieces.Length; ++i)
                {
                    float grow = Mathf.Clamp01((elapsed - delays[i]) / growSeconds);
                    pieces[i].localScale = Vector3.one * Mathf.Max(HiddenScale, sizes[i] * EaseOutBack(grow));
                }
                yield return null;
            }
            for (int i = 0; i < pieces.Length; ++i)
            {
                var center = pieces[i].position + Vector3.up * (sizes[i] * 0.45f);
                shatter.Emit(8, new EmitParams { position = center });
                shatterDust.Emit(2, new EmitParams { position = center });
                pieces[i].localScale = Vector3.one * HiddenScale;
            }
            sparkles.Emit(30);
            coldLight.Flash(flashIntensity * 0.4f, 0.3f);
            crystals.Release(crystalRing);
        }

        static int CountCrystals(Entity crystalRing)
        {
            int count = 0;
            while (crystalRing.transform.FindChild($"Frost Crystal {count + 1}"))
            {
                ++count;
            }
            return count;
        }

        /// <summary>Overshoots a little past 1 and settles back: the crystal snaps out of the floor.</summary>
        static float EaseOutBack(float t)
        {
            const float overshoot = 1.70158f;
            float x = t - 1.0f;
            return 1.0f + (overshoot + 1.0f) * x * x * x + overshoot * x * x;
        }
    }
}
