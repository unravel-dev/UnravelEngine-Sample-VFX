using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>What the HUD shows about a spell.</summary>
    public class SpellInfo
    {
        /// <summary>The number key that selects the spell, and its place on the spell bar.</summary>
        public int slot;
        public string name;
        public string description;
        /// <summary>The spell's colour on the HUD, as CSS.</summary>
        public string accent;
    }

    /// <summary>
    /// Base of the six spells. A spell's entity holds its effect as child entities, one emitter
    /// each; casting moves the entity to the target and fires the emitters.
    ///
    /// The emitters come in three kinds, and the spells show all of them:
    ///   - burst emitters: emission rate 0, so they spawn nothing by themselves. Emit(count) puts
    ///     exactly that many particles out at the next update, whatever the play state;
    ///   - short non-looping emitters (a lifetime of emission, loop off, auto play off so they
    ///     wait for the cast): Play() runs them once;
    ///   - looping emitters whose rate the script changes while they run.
    /// The burst emitters simulate in world space, so moving the spell to the next target leaves
    /// the particles of the last cast where they were.
    /// </summary>
    [ScriptSourceFile]
    public abstract class Spell : ScriptComponent
    {
        /// <summary>Every spell in the scene; SpellCaster picks from these by slot.</summary>
        public static readonly List<Spell> All = new List<Spell>();

        /// <summary>Asset key of the sound the cast makes; empty for none.</summary>
        public string castSound = "";
        public float castVolume = 0.8f;

        /// <summary>Seconds before the spell can be cast again.</summary>
        public float cooldown = 0.2f;

        float readyAt;

        public override void OnCreate()
        {
            All.Add(this);
        }

        public override void OnDestroy()
        {
            All.Remove(this);
        }

        public abstract SpellInfo GetInfo();

        /// <summary>Casts at <paramref name="target"/> (a point on the floor) unless the spell is cooling down.</summary>
        public bool TryCast(Vector3 target)
        {
            if (Time.time < readyAt)
            {
                return false;
            }
            readyAt = Time.time + cooldown;
            PlaySound(castSound, castVolume);
            Cast(target);
            return true;
        }

        protected abstract void Cast(Vector3 target);

        /// <summary>The emitter on the child entity called <paramref name="childName"/>.</summary>
        protected ParticleEmitterComponent GetEmitter(string childName)
        {
            var child = transform.FindChild(childName, true);
            if (!child)
            {
                Log.Error($"{owner.name}: no child entity named {childName}");
                return null;
            }
            return child.GetComponent<ParticleEmitterComponent>();
        }

        /// <summary>The LightFlash on the child entity called <paramref name="childName"/>.</summary>
        protected LightFlash GetLight(string childName)
        {
            var child = transform.FindChild(childName, true);
            return child ? child.GetComponent<LightFlash>() : null;
        }

        /// <summary>
        /// Plays a spell sound on the listener (2D), so the caster's own spells land with full weight: a
        /// one-shot placed in the world falls off from 1 m, and the arena is over 10 m from the camera.
        /// A <paramref name="pitch"/> above 1 plays the sound faster and higher, below 1 slower and lower.
        /// </summary>
        protected static void PlaySound(string key, float volume, float pitch = 1.0f)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Audio.PlayOneShot(Assets.GetAsset<AudioClip>(key), volume, pitch);
            }
        }

        /// <summary>The emitter on the entity called <paramref name="childName"/> below <paramref name="root"/>.</summary>
        protected static ParticleEmitterComponent GetEmitterIn(Entity root, string childName)
        {
            var child = root.transform.FindChild(childName, true);
            return child ? child.GetComponent<ParticleEmitterComponent>() : null;
        }

        /// <summary>The dummies within <paramref name="radius"/> of <paramref name="point"/>, nearest first.</summary>
        protected static List<TrainingDummy> GetDummiesNear(Vector3 point, float radius)
        {
            var found = new List<TrainingDummy>();
            foreach (var dummy in TrainingDummy.All)
            {
                if (GetFlatDistance(dummy.GetCenter(), point) <= radius)
                {
                    found.Add(dummy);
                }
            }
            found.Sort((a, b) => GetFlatDistance(a.GetCenter(), point).CompareTo(GetFlatDistance(b.GetCenter(), point)));
            return found;
        }

        /// <summary>Distance on the floor, ignoring height.</summary>
        protected static float GetFlatDistance(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, 0.0f, a.z - b.z).magnitude;
        }
    }
}
