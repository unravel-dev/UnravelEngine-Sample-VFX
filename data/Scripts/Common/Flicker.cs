using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Makes a light flicker like a flame: its intensity wanders around the value it was built with.
    /// The particles of a fire are not lit and light nothing, so a fire's light is a separate point
    /// light that has to move with the flames by itself.
    /// </summary>
    [ScriptSourceFile]
    public class Flicker : ScriptComponent
    {
        /// <summary>How far the intensity strays, as a fraction of its base value.</summary>
        public float amount = 0.3f;

        public float speed = 1.0f;

        LightComponent flame;
        float baseIntensity;
        float seed;

        public override void OnStart()
        {
            flame = owner.GetComponent<LightComponent>();
            baseIntensity = flame.intensity;
            // Each light starts somewhere else in its pattern so the braziers do not pulse together.
            seed = Random.Range(0.0f, 100.0f);
        }

        public override void OnUpdate()
        {
            float t = Time.time * speed + seed;
            // Three unrelated sines read as irregular without a noise texture.
            float wave = 0.5f * Mathf.Sin(t * 7.3f) + 0.3f * Mathf.Sin(t * 13.1f + 1.7f) + 0.2f * Mathf.Sin(t * 23.7f + 4.1f);
            flame.intensity = baseIntensity * (1.0f + amount * wave);
        }
    }
}
