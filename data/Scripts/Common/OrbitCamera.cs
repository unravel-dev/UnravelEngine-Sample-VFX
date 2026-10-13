using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Orbits the camera around the arena: right mouse drag turns, the wheel zooms. The left button
    /// stays free for casting. Spells call Shake for impacts.
    /// </summary>
    [ScriptSourceFile]
    public class OrbitCamera : ScriptComponent
    {
        public Vector3 pivot = new Vector3(0.0f, 1.0f, 1.5f);
        public float minDistance = 8.0f;
        public float maxDistance = 26.0f;

        /// <summary>Degrees per unit of mouse movement, scaled by the frame time.</summary>
        public float sensitivity = 10.0f;

        public float zoomSpeed = 1.5f;

        /// <summary>The camera the arena is seen through, for spells that shake it.</summary>
        public static OrbitCamera Main;

        float distance;
        float yaw;
        float pitch;
        float shakeStrength;
        float shakeDuration;
        float shakeLeft;

        public override void OnCreate()
        {
            Main = this;
        }

        public override void OnStart()
        {
            // Start from where the camera was placed in the scene.
            var offset = transform.position - pivot;
            distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
            yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            pitch = Mathf.Asin(Mathf.Clamp(offset.y / Mathf.Max(offset.magnitude, 0.001f), -1.0f, 1.0f)) * Mathf.Rad2Deg;
            Apply();
        }

        /// <summary>Shakes the view by up to <paramref name="strength"/> metres, calming over <paramref name="seconds"/>.</summary>
        public static void Shake(float strength, float seconds)
        {
            if (Main == null)
            {
                return;
            }
            Main.shakeStrength = Mathf.Max(strength, Main.shakeStrength * Main.shakeLeft / Mathf.Max(Main.shakeDuration, 0.001f));
            Main.shakeDuration = seconds;
            Main.shakeLeft = seconds;
        }

        public override void OnUpdate()
        {
            if (Input.IsDown("Mouse Right"))
            {
                yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime, 8.0f, 75.0f);
            }
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, minDistance, maxDistance);
            Apply();
        }

        void Apply()
        {
            float yawRadians = yaw * Mathf.Deg2Rad;
            float pitchRadians = pitch * Mathf.Deg2Rad;
            var direction = new Vector3(Mathf.Sin(yawRadians) * Mathf.Cos(pitchRadians),
                                        Mathf.Sin(pitchRadians),
                                        Mathf.Cos(yawRadians) * Mathf.Cos(pitchRadians));
            var shake = Vector3.zero;
            if (shakeLeft > 0.0f)
            {
                // The shake runs on unscaled time so it still settles in slow motion.
                shakeLeft = Mathf.Max(0.0f, shakeLeft - Time.unscaledDeltaTime);
                shake = Random.insideUnitSphere * (shakeStrength * shakeLeft / shakeDuration);
            }
            transform.position = pivot + direction * distance + shake;
            transform.LookAt(pivot + shake * 0.5f);
        }
    }
}
