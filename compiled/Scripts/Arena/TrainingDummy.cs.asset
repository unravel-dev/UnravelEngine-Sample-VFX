using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// A target that reacts to spells: its straw glows in the spell's colour and it rocks on its post.
    ///
    /// The glow writes this dummy's own materials (SetColor / SetFloat on its models), so the other
    /// dummies keep the shared Straw material; ResetMaterial gives the shared one back afterwards.
    /// </summary>
    [ScriptSourceFile]
    public class TrainingDummy : ScriptComponent
    {
        /// <summary>Every dummy in the scene, for spells that look for targets around them.</summary>
        public static readonly List<TrainingDummy> All = new List<TrainingDummy>();

        /// <summary>Strength lost per second once the hold is over.</summary>
        public float glowFade = 2.5f;

        public float wobbleStiffness = 60.0f;
        public float wobbleDamping = 5.0f;

        readonly List<ModelComponent> straw = new List<ModelComponent>();
        Color glowColor = Color.white;
        float glow;
        float glowHold;
        bool isGlowing;
        float wobble;
        float wobbleVelocity;
        Vector3 restEuler;

        public override void OnCreate()
        {
            All.Add(this);
        }

        public override void OnDestroy()
        {
            All.Remove(this);
        }

        public override void OnStart()
        {
            restEuler = transform.localEulerAngles;
            // Only the straw glows, not the wooden post and base.
            foreach (var model in owner.GetComponentsInChildren<ModelComponent>())
            {
                string part = model.owner.name;
                if (part.EndsWith("Body") || part.EndsWith("Head"))
                {
                    straw.Add(model);
                }
            }
        }

        /// <summary>Where spells aim at this dummy: the middle of its body.</summary>
        public Vector3 GetCenter()
        {
            return transform.position + Vector3.up * 1.4f;
        }

        /// <summary>
        /// Glows in <paramref name="color"/> at <paramref name="strength"/> (about 0-1.5), holds it for
        /// <paramref name="holdSeconds"/>, and gives the dummy a push.
        /// </summary>
        public void Hit(Color color, float strength, float holdSeconds = 0.0f)
        {
            glowColor = color;
            glow = Mathf.Max(glow, strength);
            glowHold = Mathf.Max(glowHold, holdSeconds);
            wobbleVelocity += strength * 90.0f * (Random.Float() < 0.5f ? -1.0f : 1.0f);
        }

        public override void OnUpdate()
        {
            UpdateGlow();
            UpdateWobble();
        }

        void UpdateGlow()
        {
            if (glow <= 0.0f)
            {
                if (isGlowing)
                {
                    isGlowing = false;
                    foreach (var model in straw)
                    {
                        model.GetMaterialInstance(0).RevertAll();
                    }
                }
                return;
            }
            isGlowing = true;
            if (glowHold > 0.0f)
            {
                glowHold -= Time.deltaTime;
            }
            else
            {
                glow = Mathf.MoveTowards(glow, 0.0f, glowFade * Time.deltaTime);
            }
            foreach (var model in straw)
            {
                model.GetMaterialInstance(0).emissiveColor = glowColor;
                model.GetMaterialInstance(0).emissiveIntensity = glow * 6.0f;
            }
        }

        void UpdateWobble()
        {
            // A damped spring: the push swings the dummy, the post pulls it back upright.
            float acceleration = -wobbleStiffness * wobble - wobbleDamping * wobbleVelocity;
            wobbleVelocity += acceleration * Time.deltaTime;
            wobble += wobbleVelocity * Time.deltaTime;
            transform.localEulerAngles = restEuler + new Vector3(wobble, 0.0f, wobble * 0.5f);
        }
    }
}
