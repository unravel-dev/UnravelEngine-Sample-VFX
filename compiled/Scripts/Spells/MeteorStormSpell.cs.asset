using System.Collections;
using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Meteor Storm: a burning sigil opens on the floor and a volley of meteors falls on it, one after
    /// another, each leaving a fiery ribbon and a smoke trail and blowing debris, sparks and smoke out
    /// of the floor where it lands.
    ///
    /// Shows: the script launches each meteor once - one Emit with a position, a velocity and a
    /// lifetime on the rock (mesh particles) and on its glowing core - and the particles take it from
    /// there: they fly the exact arc gravity gives them and die where they meet the floor (collision
    /// kill). The core's collision event fires the whole impact through sub-emitters, with no script:
    /// a flash that lights the arena (particle lights), flipbook fire drawn on cutout polygons, rock
    /// debris that bounces and settles (mesh particles with a collision radius, raising dust at every
    /// bounce through their own sub-emitter), sparks that draw their own path (trail render mode) and
    /// bounce, embers that light the floor, normal-mapped smoke lit by the scene, rings and a
    /// crater. The ribbon, the smoke and the sparks behind a meteor come from a slot entity the script
    /// moves along the same arc: they spawn per metre travelled (rate over distance), so the trail
    /// stays even at any speed. A closing ring marks each landing point for exactly the fall time
    /// (Emit with a lifetime).
    /// </summary>
    [ScriptSourceFile]
    public class MeteorStormSpell : Spell
    {
        public int meteorCount = 7;
        /// <summary>Metres around the target the meteors land in; the last one lands on the target.</summary>
        public float spread = 3.2f;
        /// <summary>Seconds between two meteors.</summary>
        public float interval = 0.22f;
        /// <summary>Seconds a meteor falls.</summary>
        public float fallSeconds = 1.05f;
        /// <summary>Metres above the floor the meteors start.</summary>
        public float skyHeight = 13.0f;
        /// <summary>Metres to the side (across the view) and behind the target the meteors start.</summary>
        public float skySide = 6.5f;
        public float skyBehind = 5.0f;
        public float rockSize = 0.85f;
        public float lastRockSize = 1.4f;
        public float impactRadius = 2.4f;
        /// <summary>One meteor's incoming roar, made to last FallSoundSeconds: it is played faster or slower
        /// so that it ends as the rock lands.</summary>
        public string fallSound = "app:/data/Audio/meteor_fall.wav";
        public float fallVolume = 0.4f;
        public string impactSound = "app:/data/Audio/meteor_impact.wav";
        public float impactVolume = 0.45f;
        /// <summary>The last meteor's heavier blow and longer rumble.</summary>
        public string lastImpactSound = "app:/data/Audio/meteor_impact_big.wav";
        public float skyLightIntensity = 250.0f;

        static readonly SpellInfo Info = new SpellInfo
        {
            slot = 6,
            name = "Meteor Storm",
            description = "Burning rocks fall from the sky and blast debris across the floor.",
            accent = "#ff7442"
        };

        /// <summary>Gravity the particles fall with: standard gravity times the emitters' gravity scale of 1.</summary>
        static readonly Vector3 Gravity = new Vector3(0.0f, -9.81f, 0.0f);
        static readonly Color BurnColor = new Color(1.0f, 0.4f, 0.08f, 1.0f);
        const int SlotCount = 8;
        /// <summary>Seconds the fall sound lasts at pitch 1, up to the moment of impact.</summary>
        const float FallSoundSeconds = 1.05f;
        const float DecalHeight = 0.04f;

        /// <summary>One meteor's trail: an entity flown along the meteor's arc with three emitters on it.</summary>
        class Slot
        {
            public Entity entity;
            public ParticleEmitterComponent ribbon;
            public ParticleEmitterComponent smoke;
            public ParticleEmitterComponent sparks;
        }

        readonly List<Slot> slots = new List<Slot>();
        int nextSlot;
        ParticleEmitterComponent sigil;
        ParticleEmitterComponent groundGlow;
        ParticleEmitterComponent targetRings;
        ParticleEmitterComponent rock;
        ParticleEmitterComponent core;
        ParticleEmitterComponent impactRing;
        ParticleEmitterComponent impactDustRing;
        ParticleEmitterComponent impactSmoke;
        LightFlash skyLight;
        Entity view;

        public override SpellInfo GetInfo()
        {
            return Info;
        }

        public override void OnStart()
        {
            for (int index = 1; index <= SlotCount; ++index)
            {
                var entity = transform.FindChild($"Meteor Slot {index}");
                slots.Add(new Slot
                {
                    entity = entity,
                    ribbon = GetEmitterIn(entity, $"Meteor Ribbon {index}"),
                    smoke = GetEmitterIn(entity, $"Meteor Smoke {index}"),
                    sparks = GetEmitterIn(entity, $"Meteor Sparks {index}")
                });
            }
            sigil = GetEmitter("Meteor Sigil");
            groundGlow = GetEmitter("Meteor Ground Glow");
            targetRings = GetEmitter("Meteor Target Rings");
            rock = GetEmitter("Meteor Rock");
            core = GetEmitter("Meteor Core");
            impactRing = GetEmitter("Meteor Impact Ring");
            impactDustRing = GetEmitter("Meteor Impact Dust Ring");
            impactSmoke = GetEmitter("Meteor Impact Smoke");
            skyLight = GetLight("Meteor Sky Light");
            view = Scene.FindEntityByName("Main Camera");
        }

        protected override void Cast(Vector3 target)
        {
            // The emitters' collision planes are placed from the spell's position: the floor under the target.
            transform.position = target;
            var floor = target + Vector3.up * DecalHeight;
            float storm = meteorCount * interval + fallSeconds;
            sigil.Emit(1, new EmitParams { position = floor });
            groundGlow.Emit(1, new EmitParams { position = floor, lifetime = storm + 0.6f });
            skyLight.Swell(skyLightIntensity, storm + 0.4f);
            StartCoroutine(Storm(target));
        }

        IEnumerator Storm(Vector3 target)
        {
            var skyOffset = GetSkyOffset();
            for (int index = 0; index < meteorCount; ++index)
            {
                bool isLast = index == meteorCount - 1;
                var landing = isLast ? target : target + GetRandomInDisc(spread);
                Launch(landing, skyOffset, isLast);
                yield return new WaitForSeconds(interval * Random.Range(0.7f, 1.3f));
            }
        }

        /// <summary>
        /// Where the meteors start, from the target: up, to the left of the view and behind the target,
        /// so they cross the screen diagonally from any camera angle.
        /// </summary>
        Vector3 GetSkyOffset()
        {
            var forward = view ? view.transform.forward : Vector3.forward;
            forward = new Vector3(forward.x, 0.0f, forward.z);
            forward = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
            var right = Vector3.Cross(Vector3.up, forward);
            return Vector3.up * skyHeight - right * skySide + forward * skyBehind;
        }

        static Vector3 GetRandomInDisc(float radius)
        {
            float angle = Random.Range(0.0f, 2.0f * Mathf.PI);
            float distance = radius * Mathf.Sqrt(Random.Range(0.0f, 1.0f));
            return new Vector3(Mathf.Cos(angle) * distance, 0.0f, Mathf.Sin(angle) * distance);
        }

        void Launch(Vector3 landing, Vector3 skyOffset, bool isLast)
        {
            // Each meteor comes in at its own angle: the volley fans out of one patch of sky.
            var start = landing + skyOffset + GetRandomInDisc(2.6f) + Vector3.up * Random.Range(-1.5f, 1.5f);
            float seconds = fallSeconds * Random.Range(0.9f, 1.1f);
            // The launch velocity that lands on the target after `seconds` under gravity.
            var velocity = (landing - start - Gravity * (0.5f * seconds * seconds)) / seconds;
            // A lifetime past the landing: the floor ends the flight (collision kill), not the clock.
            var launch = new EmitParams { position = start, velocity = velocity, lifetime = seconds + 1.0f };
            core.Emit(1, launch);
            launch.size = isLast ? lastRockSize : rockSize * Random.Range(0.85f, 1.15f);
            rock.Emit(1, launch);
            targetRings.Emit(1, new EmitParams { position = landing + Vector3.up * DecalHeight, lifetime = seconds });
            PlaySound(fallSound, fallVolume, FallSoundSeconds / seconds);
            var slot = slots[nextSlot];
            nextSlot = (nextSlot + 1) % slots.Count;
            StartCoroutine(Fly(slot, start, velocity, seconds, landing, isLast));
        }

        /// <summary>The last meteor's second shockwave, a moment after the first.</summary>
        IEnumerator Aftershock(Vector3 floor)
        {
            yield return new WaitForSeconds(0.12f);
            impactRing.Emit(1, new EmitParams { position = floor });
            impactDustRing.Emit(1, new EmitParams { position = floor });
        }

        IEnumerator Fly(Slot slot, Vector3 start, Vector3 velocity, float seconds, Vector3 landing, bool isLast)
        {
            // Move first and start the trail a frame later, so the jump from the last landing to
            // the sky leaves no streak (the trail spawns along the path its entity moves).
            slot.entity.transform.position = start;
            yield return null;
            slot.ribbon.Play();
            slot.smoke.Play();
            slot.sparks.Play();
            float elapsed = 0.0f;
            while (elapsed < seconds)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, seconds);
                slot.entity.transform.position = start + velocity * elapsed + Gravity * (0.5f * elapsed * elapsed);
                yield return null;
            }
            slot.ribbon.Stop();
            slot.smoke.Stop();
            slot.sparks.Stop();
            Land(landing, isLast);
        }

        /// <summary>
        /// The impact's particles come from the core's collision through sub-emitters; the script only adds
        /// what is not a particle (the shake, the sound, the dummies) and a bigger blast for the last meteor.
        /// </summary>
        void Land(Vector3 at, bool isLast)
        {
            OrbitCamera.Shake(isLast ? 0.6f : 0.22f, isLast ? 0.7f : 0.3f);
            if (isLast)
            {
                PlaySound(lastImpactSound, 1.0f);
            }
            else
            {
                // A slightly different pitch each time, so a volley of impacts does not repeat one sound.
                PlaySound(impactSound, impactVolume, Random.Range(0.88f, 1.12f));
            }
            if (isLast)
            {
                impactSmoke.Emit(6, new EmitParams { position = at + Vector3.up * 0.4f });
                StartCoroutine(Aftershock(at + Vector3.up * DecalHeight));
            }
            foreach (var dummy in GetDummiesNear(at, impactRadius))
            {
                float closeness = 1.0f - GetFlatDistance(dummy.GetCenter(), at) / impactRadius;
                dummy.Hit(BurnColor, 0.4f + closeness, 0.2f);
            }
        }
    }
}
