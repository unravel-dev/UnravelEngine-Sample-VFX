using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Orbits the camera around the arena: right mouse drag turns, the wheel zooms. The left button
    /// stays free for casting. On a touch screen one finger dragged turns and two fingers pinch to
    /// zoom; a tap is left for casting. Spells call Shake for impacts.
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

        /// <summary>Degrees the camera turns for a finger dragged across the screen's height.</summary>
        public float touchOrbitSpeed = 180.0f;

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
            if (Input.touchCount == 1)
            {
                Orbit(Input.GetTouch(0));
            }
            else if (Input.touchCount >= 2)
            {
                Pinch(Input.GetTouch(0), Input.GetTouch(1));
            }
            Apply();
        }

        /// <summary>A finger dragged turns the camera as the right mouse button does.</summary>
        void Orbit(Touch touch)
        {
            if (touch.Phase != TouchPhase.Moved)
            {
                return;
            }
            float degreesPerPixel = touchOrbitSpeed / Mathf.Max(1.0f, Screen.height);
            yaw += touch.DeltaPosition.x * degreesPerPixel;
            pitch = Mathf.Clamp(pitch + touch.DeltaPosition.y * degreesPerPixel, 8.0f, 75.0f);
        }

        /// <summary>Two fingers spreading apart move the camera in by the ratio of their distances.</summary>
        void Pinch(Touch first, Touch second)
        {
            float spread = (first.Position - second.Position).magnitude;
            float lastSpread = (first.Position - first.DeltaPosition - (second.Position - second.DeltaPosition)).magnitude;
            if (spread < 1.0f || lastSpread < 1.0f)
            {
                return;
            }
            distance = Mathf.Clamp(distance * lastSpread / spread, minDistance, maxDistance);
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
