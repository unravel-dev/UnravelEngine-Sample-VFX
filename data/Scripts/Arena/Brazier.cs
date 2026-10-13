using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// One brazier: the flame emitter on this entity, its embers and smoke on the children, and
    /// the light that flickers with them.
    ///
    /// Stop(true) stops this emitter and every emitter below it. Stopping only ends the spawning:
    /// the flames, embers and smoke already alive finish their lives, so the fire dies down
    /// instead of vanishing. Play(true) lights it again.
    /// </summary>
    [ScriptSourceFile]
    public class Brazier : ScriptComponent
    {
        ParticleEmitterComponent flames;
        LightComponent glow;
        bool isLit = true;

        public override void OnStart()
        {
            flames = owner.GetComponent<ParticleEmitterComponent>();
            glow = owner.GetComponentInChildren<LightComponent>();
        }

        public bool IsLit()
        {
            return isLit;
        }

        public void SetLit(bool lit)
        {
            isLit = lit;
            if (lit)
            {
                flames.Play(true);
            }
            else
            {
                flames.Stop(true);
            }
            glow.enabled = lit;
        }
    }
}
