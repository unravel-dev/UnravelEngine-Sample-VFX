using System.Collections;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Holy Light: a column of light comes down with thin rays flickering around it, sparkles fall
    /// through it and rise again, a golden circle turns on the floor, rings spread out, and the
    /// dummies inside it are healed.
    ///
    /// Shows: vertical billboards (the beams stand upright and only turn around their own axis to
    /// face the camera; the pivot puts their bottom edge on the floor), several layers of one beam
    /// texture at different widths and brightness, short non-looping streams started with Play,
    /// a staggered burst from a coroutine, and Emit with a position for the heal marks that rise
    /// from each healed dummy.
    /// </summary>
    [ScriptSourceFile]
    public class HolyLightSpell : Spell
    {
        public float healRadius = 3.5f;
        public float lightIntensity = 900.0f;
        public int healMarksPerDummy = 6;

        static readonly SpellInfo Info = new SpellInfo
        {
            slot = 5,
            name = "Holy Light",
            description = "A column of light and a golden circle. Heals the dummies inside it.",
            accent = "#ffd77a"
        };

        static readonly Color HealColor = new Color(1.0f, 0.85f, 0.4f, 1.0f);

        ParticleEmitterComponent beam;
        ParticleEmitterComponent coreBeam;
        ParticleEmitterComponent rays;
        ParticleEmitterComponent runes;
        ParticleEmitterComponent rings;
        ParticleEmitterComponent glow;
        ParticleEmitterComponent descent;
        ParticleEmitterComponent rise;
        ParticleEmitterComponent heal;
        LightFlash holyLight;

        public override SpellInfo GetInfo()
        {
            return Info;
        }

        public override void OnStart()
        {
            beam = GetEmitter("Holy Beam");
            coreBeam = GetEmitter("Holy Core Beam");
            rays = GetEmitter("Holy Rays");
            runes = GetEmitter("Holy Runes");
            rings = GetEmitter("Holy Rings");
            glow = GetEmitter("Holy Glow");
            descent = GetEmitter("Holy Descent");
            rise = GetEmitter("Holy Rise");
            heal = GetEmitter("Holy Heal");
            holyLight = GetLight("Holy Light Glow");
        }

        protected override void Cast(Vector3 target)
        {
            transform.position = target;
            beam.Emit(3);
            coreBeam.Emit(1);
            rays.Play();
            runes.Emit(1);
            glow.Emit(1);
            descent.Play();
            rise.Play();
            holyLight.Swell(lightIntensity, 2.0f);
            StartCoroutine(SpreadRings());
            foreach (var dummy in GetDummiesNear(target, healRadius))
            {
                dummy.Hit(HealColor, 0.8f, 0.8f);
                var center = dummy.GetCenter();
                for (int index = 0; index < healMarksPerDummy; ++index)
                {
                    var offset = new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.6f, 0.4f), Random.Range(-0.35f, 0.35f));
                    heal.Emit(1, new EmitParams { position = center + offset });
                }
            }
        }

        IEnumerator SpreadRings()
        {
            for (int index = 0; index < 3; ++index)
            {
                rings.Emit(1);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}
