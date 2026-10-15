using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// An emitter the arena's wind pushes. SpellCaster toggles the wind; this adds it to the
    /// emitter's acceleration, on top of the acceleration the emitter was built with.
    ///
    /// The acceleration bends where a particle ends up, and it is applied when the particle is born,
    /// so smoke already in the air keeps its path and the new puffs lean over.
    /// </summary>
    [ScriptSourceFile]
    public class WindAffected : ScriptComponent
    {
        /// <summary>How strongly this emitter follows the wind: light smoke more than heavy embers.</summary>
        public float response = 1.0f;

        ParticleEmitterComponent emitter;
        Vector3 calmAcceleration;

        public override void OnStart()
        {
            emitter = owner.GetComponent<ParticleEmitterComponent>();
            calmAcceleration = emitter.acceleration;
        }

        public void SetWind(Vector3 wind)
        {
            emitter.acceleration = calmAcceleration + wind * response;
        }
    }
}
