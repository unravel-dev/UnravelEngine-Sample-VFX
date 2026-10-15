using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Drives the point light of a spell. Most spell particles cast no light (Meteor Storm's particle
    /// lights are the exception), so the flash of an explosion on the floor and the dummies comes
    /// from this light, timed with the burst.
    /// The light stays disabled while it is dark, which costs nothing.
    /// </summary>
    [ScriptSourceFile]
    public class LightFlash : ScriptComponent
    {
        LightComponent glow;
        float peak;
        float duration;
        float elapsed;
        bool isSwell;
        bool isFlickering;
        bool isActive;

        public override void OnStart()
        {
            glow = owner.GetComponent<LightComponent>();
            glow.enabled = false;
        }

        /// <summary>Jumps to <paramref name="intensity"/> and fades out over <paramref name="seconds"/>.</summary>
        public void Flash(float intensity, float seconds, bool flicker = false)
        {
            Begin(intensity, seconds, false, flicker);
        }

        /// <summary>Rises to <paramref name="intensity"/> and falls back over <paramref name="seconds"/>.</summary>
        public void Swell(float intensity, float seconds)
        {
            Begin(intensity, seconds, true, false);
        }

        /// <summary>Holds the light at <paramref name="intensity"/> until the next Flash, Swell or Hold.</summary>
        public void Hold(float intensity)
        {
            isActive = false;
            glow.intensity = intensity;
            glow.enabled = intensity > 0.0f;
        }

        void Begin(float intensity, float seconds, bool swell, bool flicker)
        {
            peak = intensity;
            duration = Mathf.Max(seconds, 0.01f);
            elapsed = 0.0f;
            isSwell = swell;
            isFlickering = flicker;
            isActive = true;
            glow.enabled = true;
        }

        public override void OnUpdate()
        {
            if (!isActive)
            {
                return;
            }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float shape = isSwell ? Mathf.Sin(t * Mathf.PI) : (1.0f - t) * (1.0f - t);
            if (isFlickering)
            {
                // Lightning strobes: a random drop every frame.
                shape *= Random.Range(0.35f, 1.0f);
            }
            glow.intensity = peak * shape;
            if (t >= 1.0f)
            {
                isActive = false;
                glow.enabled = false;
            }
        }
    }
}
