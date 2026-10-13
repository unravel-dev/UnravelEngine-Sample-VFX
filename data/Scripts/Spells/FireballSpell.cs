using System.Collections;
using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Fireball: a projectile flies from the spell focus to the target and explodes.
    ///
    /// Shows: emitters that move. The projectile's core and its rolling body simulate in local
    /// space and stay on it; its flames, smoke and embers simulate in world space and stay behind
    /// as a trail. An emitter that moves spreads each frame's spawns along the path it covered that
    /// frame (temporal motion), so a fast trail stays continuous. Stop leaves the trail to fade out.
    /// The explosion layers burst emitters: a flash, a flipbook fireball that blends between its
    /// frames, rolling smoke that takes over from it, a smoke column, sparks and burning chunks
    /// bent down by gravity, a dust ring and a shock ring on the floor, a scorch mark with glowing
    /// embers in it, and falling ash.
    /// </summary>
    [ScriptSourceFile]
    public class FireballSpell : Spell
    {
        /// <summary>Name of the entity the fireball is launched from.</summary>
        public string launchFrom = "Spell Focus";
        public float speed = 14.0f;
        /// <summary>Metres the path rises above the straight line, at its middle.</summary>
        public float arcHeight = 1.6f;
        public float blastRadius = 3.5f;
        public string explosionSound = "app:/data/Audio/explosion.wav";
        public float flashIntensity = 900.0f;

        static readonly SpellInfo Info = new SpellInfo
        {
            slot = 2,
            name = "Fireball",
            description = "A burning projectile with a smoke trail. It explodes where it lands.",
            accent = "#ffa25c"
        };

        static readonly Color BurnColor = new Color(1.0f, 0.45f, 0.1f, 1.0f);

        class Projectile
        {
            public Entity entity;
            public ParticleEmitterComponent emitter;
            public LightComponent glow;
        }

        readonly List<Projectile> projectiles = new List<Projectile>();
        int nextProjectile;
        ParticleEmitterComponent launchFlash;
        ParticleEmitterComponent launchSparks;
        Entity explosion;
        Vector3 explosionOffset;
        ParticleEmitterComponent flash;
        ParticleEmitterComponent fire;
        ParticleEmitterComponent billow;
        ParticleEmitterComponent column;
        ParticleEmitterComponent debris;
        ParticleEmitterComponent chunks;
        ParticleEmitterComponent dustRing;
        ParticleEmitterComponent shockRing;
        ParticleEmitterComponent embers;
        ParticleEmitterComponent scorch;
        ParticleEmitterComponent scorchGlow;
        LightFlash blastLight;

        public override SpellInfo GetInfo()
        {
            return Info;
        }

        public override void OnStart()
        {
            // Three projectiles take turns, so up to three can be in the air at once.
            for (int index = 1; index <= 3; ++index)
            {
                var entity = transform.FindChild($"Fireball Projectile {index}");
                var projectile = new Projectile
                {
                    entity = entity,
                    emitter = entity.GetComponent<ParticleEmitterComponent>(),
                    glow = entity.GetComponentInChildren<LightComponent>()
                };
                projectile.glow.enabled = false;
                projectiles.Add(projectile);
            }
            launchFlash = GetEmitter("Fireball Launch Flash");
            launchSparks = GetEmitter("Fireball Launch Sparks");
            explosion = transform.FindChild("Explosion");
            explosionOffset = explosion.transform.localPosition;
            flash = explosion.GetComponent<ParticleEmitterComponent>();
            fire = GetEmitter("Explosion Fire");
            billow = GetEmitter("Explosion Smoke Billow");
            column = GetEmitter("Explosion Column");
            debris = GetEmitter("Explosion Debris");
            chunks = GetEmitter("Explosion Chunks");
            dustRing = GetEmitter("Explosion Dust Ring");
            shockRing = GetEmitter("Explosion Shock Ring");
            embers = GetEmitter("Explosion Embers");
            scorch = GetEmitter("Explosion Scorch");
            scorchGlow = GetEmitter("Explosion Scorch Glow");
            blastLight = GetLight("Explosion Light");
        }

        protected override void Cast(Vector3 target)
        {
            var projectile = projectiles[nextProjectile];
            nextProjectile = (nextProjectile + 1) % projectiles.Count;
            var focus = Scene.FindEntityByName(launchFrom);
            var from = focus ? focus.transform.position : target + Vector3.up * 6.0f;
            launchFlash.Emit(1, new EmitParams { position = from });
            launchSparks.Emit(30, new EmitParams { position = from });
            StartCoroutine(Fly(projectile, from, target));
        }

        IEnumerator Fly(Projectile projectile, Vector3 from, Vector3 to)
        {
            // Move first and start the trail a frame later. A playing world-space trail fills the
            // path its emitter covered in a frame, and the jump from the last impact to the
            // focus would leave a streak across the arena.
            projectile.entity.transform.position = from;
            yield return null;
            projectile.emitter.Play(true);
            projectile.glow.enabled = true;
            float duration = Mathf.Max(Vector3.Distance(from, to) / speed, 0.25f);
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                projectile.entity.transform.position = Vector3.Lerp(from, to, t) + Vector3.up * (arcHeight * 4.0f * t * (1.0f - t));
                yield return null;
            }
            projectile.emitter.Stop(true);
            projectile.glow.enabled = false;
            Explode(to);
        }

        void Explode(Vector3 at)
        {
            explosion.transform.position = at + explosionOffset;
            flash.Emit(1);
            fire.Emit(8);
            billow.Emit(5);
            column.Play();
            debris.Emit(80);
            chunks.Emit(14);
            dustRing.Emit(1);
            shockRing.Emit(1);
            embers.Emit(40);
            scorch.Emit(1);
            scorchGlow.Emit(1);
            blastLight.Flash(flashIntensity, 0.9f);
            OrbitCamera.Shake(0.45f, 0.5f);
            PlaySound(explosionSound, 1.0f);
            foreach (var dummy in GetDummiesNear(at, blastRadius))
            {
                float closeness = 1.0f - GetFlatDistance(dummy.GetCenter(), at) / blastRadius;
                dummy.Hit(BurnColor, 0.6f + closeness, 0.3f);
            }
        }
    }
}
