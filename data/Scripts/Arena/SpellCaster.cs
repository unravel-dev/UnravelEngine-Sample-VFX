using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// The player's side of the arena: picks a spell, casts it where the mouse points, and toggles
    /// the arena-wide particle switches.
    ///
    ///   1-6          pick a spell                 P   freeze every particle (Pause / Resume)
    ///   Left click   cast at the cursor           T   put the braziers out and light them (Stop / Play)
    ///   Space        cast at a random dummy       G   wind on the smoke and the rain (force over lifetime)
    ///                                             B   slow motion (Time.timeScale)
    ///                                             R   rain over the arena (Play / Stop)
    ///
    /// The actions (Spell 1 ... Slow Motion) are defined in the project's input settings.
    /// </summary>
    [ScriptSourceFile]
    public class SpellCaster : ScriptComponent
    {
        public string cameraName = "Main Camera";
        /// <summary>Casts land on the floor within this radius of the arena's centre.</summary>
        public float arenaRadius = 10.5f;
        public Vector3 windForce = new Vector3(0.9f, 0.05f, 0.35f);
        public float slowMotionScale = 0.25f;
        public string selectSound = "app:/data/Audio/select.wav";
        public string rainName = "Rain";

        /// <summary>The caster in the scene, for the HUD.</summary>
        public static SpellCaster Main;

        readonly List<Spell> spells = new List<Spell>();
        ParticleEmitterComponent[] emitters;
        CameraComponent view;
        Spell selected;
        string lastCast = "";
        bool isFrozen;
        bool isWindy;
        bool isSlowMotion;
        bool areBraziersLit = true;
        bool isRaining;

        public override void OnCreate()
        {
            Main = this;
        }

        public override void OnStart()
        {
            var cameraEntity = Scene.FindEntityByName(cameraName);
            view = cameraEntity ? cameraEntity.GetComponent<CameraComponent>() : null;
            spells.AddRange(Spell.All);
            spells.Sort((a, b) => a.GetInfo().slot.CompareTo(b.GetInfo().slot));
            selected = spells.Count > 0 ? spells[0] : null;
            // Every emitter in the scene, for the freeze switch and the particle count.
            var withEmitters = Scene.FindEntitiesWithComponent<ParticleEmitterComponent>();
            emitters = new ParticleEmitterComponent[withEmitters.Length];
            for (int index = 0; index < withEmitters.Length; ++index)
            {
                emitters[index] = withEmitters[index].GetComponent<ParticleEmitterComponent>();
            }
        }

        public override void OnUpdate()
        {
            foreach (var spell in spells)
            {
                if (Input.IsPressed("Spell " + spell.GetInfo().slot))
                {
                    Select(spell);
                }
            }
            if (Input.IsPressed("Mouse Left") && TryGetCursorTarget(out Vector3 target))
            {
                Cast(target);
            }
            if (Input.IsPressed("Cast At Dummy") && TrainingDummy.All.Count > 0)
            {
                var dummy = TrainingDummy.All[Random.Range(0, TrainingDummy.All.Count)];
                Cast(dummy.transform.position);
            }
            if (Input.IsPressed("Freeze Particles"))
            {
                SetFrozen(!isFrozen);
            }
            if (Input.IsPressed("Toggle Braziers"))
            {
                SetBraziersLit(!areBraziersLit);
            }
            if (Input.IsPressed("Toggle Wind"))
            {
                SetWindy(!isWindy);
            }
            if (Input.IsPressed("Slow Motion"))
            {
                SetSlowMotion(!isSlowMotion);
            }
            if (Input.IsPressed("Toggle Rain"))
            {
                SetRaining(!isRaining);
            }
        }

        public void Select(Spell spell)
        {
            if (spell == selected)
            {
                return;
            }
            selected = spell;
            if (!string.IsNullOrEmpty(selectSound))
            {
                Audio.PlayOneShot(Assets.GetAsset<AudioClip>(selectSound), 0.5f);
            }
        }

        public void Cast(Vector3 target)
        {
            if (selected != null && selected.TryCast(target))
            {
                lastCast = selected.GetInfo().name;
            }
        }

        /// <summary>
        /// Where the cursor points on the arena floor: the camera's ray through the mouse, cut with
        /// the floor's plane (y = 0). No collider is needed for a flat floor.
        /// </summary>
        bool TryGetCursorTarget(out Vector3 target)
        {
            target = Vector3.zero;
            if (view == null || !view.ScreenPointToRay(Input.mousePosition, out Ray ray) || ray.direction.y > -0.01f)
            {
                return false;
            }
            target = ray.origin + ray.direction * (-ray.origin.y / ray.direction.y);
            return new Vector3(target.x, 0.0f, target.z).magnitude <= arenaRadius;
        }

        /// <summary>
        /// Pause freezes an emitter's particles where they are; Resume lets them go on. Bursts still
        /// spawn while paused, so a spell cast into a frozen arena hangs in the air.
        /// </summary>
        public void SetFrozen(bool frozen)
        {
            isFrozen = frozen;
            foreach (var emitter in emitters)
            {
                if (frozen)
                {
                    emitter.Pause();
                }
                else
                {
                    emitter.Resume();
                }
            }
        }

        public void SetBraziersLit(bool lit)
        {
            areBraziersLit = lit;
            foreach (var entity in Scene.FindEntitiesWithComponent<Brazier>())
            {
                entity.GetComponent<Brazier>().SetLit(lit);
            }
        }

        public void SetWindy(bool windy)
        {
            isWindy = windy;
            foreach (var entity in Scene.FindEntitiesWithComponent<WindAffected>())
            {
                entity.GetComponent<WindAffected>().SetWind(windy ? windForce : Vector3.zero);
            }
        }

        /// <summary>
        /// The rain prewarms the first time it plays, so it is already falling everywhere; after a Stop,
        /// Play starts it again from the sky.
        /// </summary>
        public void SetRaining(bool raining)
        {
            var rain = Scene.FindEntityByName(rainName);
            var emitter = rain ? rain.GetComponent<ParticleEmitterComponent>() : null;
            if (emitter == null)
            {
                return;
            }
            isRaining = raining;
            if (raining)
            {
                emitter.Play();
            }
            else
            {
                emitter.Stop();
            }
        }

        /// <summary>
        /// Particles advance with the game clock, so a lower time scale slows every effect, and the
        /// spells' coroutines with them.
        /// </summary>
        public void SetSlowMotion(bool slow)
        {
            isSlowMotion = slow;
            Time.timeScale = slow ? slowMotionScale : 1.0f;
        }

        public Spell GetSelected()
        {
            return selected;
        }

        public List<Spell> GetSpells()
        {
            return spells;
        }

        public string GetLastCast()
        {
            return lastCast;
        }

        public bool IsFrozen()
        {
            return isFrozen;
        }

        public bool IsWindy()
        {
            return isWindy;
        }

        public bool IsSlowMotion()
        {
            return isSlowMotion;
        }

        public bool AreBraziersLit()
        {
            return areBraziersLit;
        }

        public bool IsRaining()
        {
            return isRaining;
        }

        /// <summary>Live particles across every emitter in the scene.</summary>
        public int CountParticles()
        {
            int count = 0;
            foreach (var emitter in emitters)
            {
                count += (int)emitter.numParticles;
            }
            return count;
        }

        public int CountEmitters()
        {
            return emitters.Length;
        }
    }
}
