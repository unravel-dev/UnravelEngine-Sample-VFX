using System.Collections;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Lightning: energy gathers on the floor for a quarter of a second, then bolts strike down the
    /// same channel three times. The impact flashes, a shock ring runs out, sparks spray and fall,
    /// arcs crawl over the ground, smoke and embers hang in the air, and the strike arcs on to the
    /// dummies nearby.
    ///
    /// Shows: timing a layered effect from a coroutine; burst emitters fired with Emit; a sheet of
    /// bolt variants where each particle holds one random tile; short non-looping emitters started
    /// with Play; and EmitParams, which places a single arc particle between two points - halfway,
    /// as long as the gap, turned along it.
    /// Each layer faces the camera the way its shape needs: the bolt, its glow, the flash and the
    /// crackles at the strike are vertical billboards that stay upright from every angle, the rings,
    /// the scorch and the arcs crawling outward lie flat on the floor, and only round or volumetric
    /// things (sparks, embers, smoke) turn fully to the camera.
    /// </summary>
    [ScriptSourceFile]
    public class LightningSpell : Spell
    {
        public float chargeSeconds = 0.25f;
        public float chainRadius = 6.0f;
        public int maxChains = 3;
        public float chargeLightIntensity = 150.0f;
        public float flashIntensity = 650.0f;

        static readonly SpellInfo Info = new SpellInfo
        {
            slot = 1,
            name = "Lightning",
            description = "Bolts from the sky. The strike arcs on to up to three dummies nearby.",
            accent = "#9fc6ff"
        };

        static readonly Color ArcColor = new Color(0.6f, 0.75f, 1.0f, 1.0f);

        ParticleEmitterComponent chargeRing;
        ParticleEmitterComponent chargeSparks;
        ParticleEmitterComponent bolt;
        ParticleEmitterComponent boltGlow;
        ParticleEmitterComponent flash;
        ParticleEmitterComponent groundFlash;
        ParticleEmitterComponent shockRing;
        ParticleEmitterComponent sparks;
        ParticleEmitterComponent groundArcs;
        ParticleEmitterComponent impactArcs;
        ParticleEmitterComponent chainArcs;
        ParticleEmitterComponent smoke;
        ParticleEmitterComponent embers;
        ParticleEmitterComponent scorch;
        ParticleEmitterComponent scorchGlow;
        LightFlash strikeLight;

        public override SpellInfo GetInfo()
        {
            return Info;
        }

        public override void OnStart()
        {
            chargeRing = GetEmitter("Lightning Charge Ring");
            chargeSparks = GetEmitter("Lightning Charge Sparks");
            bolt = GetEmitter("Lightning Bolt");
            boltGlow = GetEmitter("Lightning Bolt Glow");
            flash = GetEmitter("Lightning Flash");
            groundFlash = GetEmitter("Lightning Ground Flash");
            shockRing = GetEmitter("Lightning Shock Ring");
            sparks = GetEmitter("Lightning Sparks");
            groundArcs = GetEmitter("Lightning Ground Arcs");
            impactArcs = GetEmitter("Lightning Impact Arcs");
            chainArcs = GetEmitter("Lightning Chain Arcs");
            smoke = GetEmitter("Lightning Smoke");
            embers = GetEmitter("Lightning Embers");
            scorch = GetEmitter("Lightning Scorch");
            scorchGlow = GetEmitter("Lightning Scorch Glow");
            strikeLight = GetLight("Lightning Light");
        }

        protected override void Cast(Vector3 target)
        {
            transform.position = target;
            StartCoroutine(Strike(target));
        }

        IEnumerator Strike(Vector3 target)
        {
            chargeRing.Emit(1);
            chargeSparks.Emit(48);
            strikeLight.Swell(chargeLightIntensity, chargeSeconds * 2.0f);
            yield return new WaitForSeconds(chargeSeconds);
            bolt.Emit(2);
            boltGlow.Emit(1);
            flash.Emit(1);
            groundFlash.Emit(1);
            shockRing.Emit(1);
            sparks.Emit(90);
            smoke.Emit(10);
            embers.Emit(30);
            scorch.Emit(1);
            scorchGlow.Emit(1);
            groundArcs.Play();
            impactArcs.Play();
            strikeLight.Flash(flashIntensity, 0.5f, true);
            OrbitCamera.Shake(0.25f, 0.35f);
            Chain(target);
            // Real lightning strikes more than once down the same channel.
            yield return new WaitForSeconds(0.08f);
            bolt.Emit(1);
            flash.Emit(1);
            sparks.Emit(25);
            yield return new WaitForSeconds(0.1f);
            bolt.Emit(1);
            boltGlow.Emit(1);
            Chain(target);
        }

        void Chain(Vector3 target)
        {
            var start = target + Vector3.up;
            int chained = 0;
            foreach (var dummy in GetDummiesNear(target, chainRadius))
            {
                if (chained == maxChains)
                {
                    break;
                }
                var end = dummy.GetCenter();
                var gap = end - start;
                float length = gap.magnitude;
                if (length < 0.5f)
                {
                    continue;
                }
                // align_to_direction turns a particle's top along its velocity, and the arc texture runs
                // along its height: a slow velocity along the gap lines the arc up, and size makes
                // it exactly as long as the gap.
                chainArcs.Emit(1, new EmitParams
                {
                    position = start + gap * 0.5f,
                    velocity = gap.normalized * 0.3f,
                    size = length,
                    lifetime = 0.3f
                });
                sparks.Emit(14, new EmitParams { position = end });
                impactArcs.Emit(3, new EmitParams { position = end });
                dummy.Hit(ArcColor, 1.0f);
                ++chained;
            }
        }
    }
}
