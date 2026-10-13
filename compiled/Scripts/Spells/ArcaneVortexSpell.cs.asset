using System.Collections;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Arcane Vortex: two rune circles open and turn against each other, glyphs rise from the outer
    /// one, wisps and sparks spiral into the centre faster and faster around a growing column of
    /// light, then the vortex bursts outwards. Every cast opens a vortex of its own.
    ///
    /// Shows: a pooled effect. The vortex is a prefab; each cast takes an instance from an
    /// EntityPool (Acquire places it at the target), drives it, and gives it back once its last
    /// particles have died (Release with a delay), so casts can overlap without spawning and
    /// destroying entities. Also: local simulation space (the wisps, the swirl and the core turn
    /// with the spinning instance, so a straight run to the centre becomes a spiral), rune circles
    /// that turn by their own rotation speed, and emission rates raised every frame while channelling.
    /// </summary>
    [ScriptSourceFile]
    public class ArcaneVortexSpell : Spell
    {
        public string effectPrefab = "app:/data/Prefabs/ArcaneVortex.pfb";
        public float channelSeconds = 3.0f;
        public float burstRadius = 4.5f;
        /// <summary>Seconds after the burst before the instance goes back to the pool: its last particles have died.</summary>
        public float releaseDelay = 3.0f;
        /// <summary>Spin of the vortex at the start and the end of the channel, degrees per second.</summary>
        public float startSpin = 90.0f;
        public float endSpin = 600.0f;
        /// <summary>The vortex light grows from the first to the second value while channelling.</summary>
        public float channelLightStart = 100.0f;
        public float channelLightEnd = 400.0f;
        public float flashIntensity = 1200.0f;

        static readonly SpellInfo Info = new SpellInfo
        {
            slot = 4,
            name = "Arcane Vortex",
            description = "Channels for three seconds, spinning ever faster, then bursts.",
            accent = "#c99bff"
        };

        static readonly Color ArcaneColor = new Color(0.75f, 0.45f, 1.0f, 1.0f);

        EntityPool pool;

        public override SpellInfo GetInfo()
        {
            return Info;
        }

        public override void OnStart()
        {
            pool = EntityPool.For(Assets.GetAsset<Prefab>(effectPrefab));
            pool.Prewarm(2);
        }

        protected override void Cast(Vector3 target)
        {
            StartCoroutine(Channel(pool.Acquire(target, Quaternion.identity), target));
        }

        IEnumerator Channel(Entity vortex, Vector3 target)
        {
            // A new instance starts its scripts on its first frame; the light is driven from then on.
            yield return null;
            var runesOuter = GetEmitterIn(vortex, "Vortex Runes Outer");
            var runesInner = GetEmitterIn(vortex, "Vortex Runes Inner");
            var glyphs = GetEmitterIn(vortex, "Vortex Glyphs");
            var wisps = GetEmitterIn(vortex, "Vortex Wisps");
            var swirl = GetEmitterIn(vortex, "Vortex Swirl");
            var column = GetEmitterIn(vortex, "Vortex Column");
            var core = GetEmitterIn(vortex, "Vortex Core");
            var light = vortex.transform.FindChild("Vortex Light", true).GetComponent<LightFlash>();
            runesOuter.Emit(1);
            runesInner.Emit(1);
            float elapsed = 0.0f;
            while (elapsed < channelSeconds)
            {
                float t = elapsed / channelSeconds;
                glyphs.emissionRate = Mathf.Lerp(6.0f, 30.0f, t);
                wisps.emissionRate = Mathf.Lerp(20.0f, 90.0f, t);
                swirl.emissionRate = Mathf.Lerp(60.0f, 300.0f, t);
                column.emissionRate = Mathf.Lerp(4.0f, 18.0f, t * t);
                core.emissionRate = 14.0f;
                vortex.transform.RotateAxis(Mathf.Lerp(startSpin, endSpin, t * t) * Time.deltaTime, Vector3.up);
                light.Hold(Mathf.Lerp(channelLightStart, channelLightEnd, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            foreach (var emitter in new[] { glyphs, wisps, swirl, column, core })
            {
                emitter.emissionRate = 0.0f;
            }
            GetEmitterIn(vortex, "Vortex Flash").Emit(1);
            GetEmitterIn(vortex, "Vortex Burst").Emit(120);
            GetEmitterIn(vortex, "Vortex Burst Wisps").Emit(24);
            GetEmitterIn(vortex, "Vortex Burst Glyphs").Emit(16);
            GetEmitterIn(vortex, "Vortex Ring").Emit(1);
            GetEmitterIn(vortex, "Vortex Smoke").Emit(10);
            light.Flash(flashIntensity, 0.6f);
            OrbitCamera.Shake(0.35f, 0.4f);
            foreach (var dummy in GetDummiesNear(target, burstRadius))
            {
                dummy.Hit(ArcaneColor, 1.2f, 0.4f);
            }
            pool.Release(vortex, releaseDelay);
        }
    }
}
