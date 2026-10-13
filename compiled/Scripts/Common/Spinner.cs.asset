using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Turns the entity around the vertical axis and, optionally, bobs it up and down.
    ///
    /// Particles care about this in two ways: a horizontal billboard (a rune circle on the floor)
    /// turns with its emitter's entity, and a local-space emitter carries its live particles round
    /// with the entity, which turns a straight path into a spiral.
    /// </summary>
    [ScriptSourceFile]
    public class Spinner : ScriptComponent
    {
        public float degreesPerSecond = 30.0f;

        /// <summary>Metres up and down; 0 keeps the entity at its height.</summary>
        public float bobHeight = 0.0f;

        /// <summary>Bobs per second.</summary>
        public float bobSpeed = 0.5f;

        Vector3 restPosition;
        float bobPhase;

        public override void OnStart()
        {
            restPosition = transform.localPosition;
        }

        public override void OnUpdate()
        {
            transform.RotateAxis(degreesPerSecond * Time.deltaTime, Vector3.up);
            if (bobHeight > 0.0f)
            {
                bobPhase += bobSpeed * Time.deltaTime;
                transform.localPosition = restPosition + Vector3.up * (Mathf.Sin(bobPhase * 2.0f * Mathf.PI) * bobHeight);
            }
        }
    }
}
