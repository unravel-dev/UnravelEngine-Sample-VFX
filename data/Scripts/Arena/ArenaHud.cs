using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Drives app:/data/UI/arena_hud.rhtml through the data model "arena": the spell bar, the
    /// selected spell, and the live particle count with the state of the arena's switches.
    /// It reads SpellCaster a few times a second and marks the whole model dirty. The spell slots
    /// and the switches are buttons (data-event-click calls the model's methods).
    ///
    /// The stylesheet sizes everything in rem. On a touch screen the HUD takes its touch layout and
    /// the root font size grows with the screen's density, so text stays readable and buttons stay
    /// big enough for a finger.
    /// </summary>
    [ScriptSourceFile]
    public class ArenaHud : ScriptComponent
    {
        public float refreshSeconds = 0.2f;

        /// <summary>One slot of the spell bar.</summary>
        public class SpellSlot
        {
            public int key;
            public string name;
            public string accent;
            public bool selected;
        }

        /// <summary>The object behind the data model "arena".</summary>
        public class HudModel
        {
            public List<SpellSlot> spells = new List<SpellSlot>();
            public string spellName = "";
            public string spellDescription = "";
            public string spellAccent = "#ffffff";
            public string lastCast = "";
            public int particles;
            public int emitters;
            public bool frozen;
            public bool windy;
            public bool slowMotion;
            public bool braziers = true;
            public bool raining;
            /// <summary>The touch layout: touch hints instead of keys, taller switches.</summary>
            public bool touch;
            /// <summary>Reads the caster again right after a button changed it.</summary>
            internal System.Action refresh;

            /// <summary>data-event-click="SelectSpell(spell.key)" on a spell slot.</summary>
            public void SelectSpell(int key)
            {
                SpellCaster.Main?.SelectSlot(key);
                refresh?.Invoke();
            }

            public void ToggleFreeze()
            {
                var caster = SpellCaster.Main;
                caster?.SetFrozen(!caster.IsFrozen());
                refresh?.Invoke();
            }

            public void ToggleSlowMotion()
            {
                var caster = SpellCaster.Main;
                caster?.SetSlowMotion(!caster.IsSlowMotion());
                refresh?.Invoke();
            }

            public void ToggleWind()
            {
                var caster = SpellCaster.Main;
                caster?.SetWindy(!caster.IsWindy());
                refresh?.Invoke();
            }

            public void ToggleBraziers()
            {
                var caster = SpellCaster.Main;
                caster?.SetBraziersLit(!caster.AreBraziersLit());
                refresh?.Invoke();
            }

            public void ToggleRain()
            {
                var caster = SpellCaster.Main;
                caster?.SetRaining(!caster.IsRaining());
                refresh?.Invoke();
            }
        }

        /// <summary>The root font size the stylesheet's rem lengths are measured in (dp).</summary>
        const float BaseFontSize = 16.0f;
        /// <summary>Screen.dpi at 100 % display scaling.</summary>
        const float BaseDpi = 96.0f;
        /// <summary>A phone is held closer than a monitor, so its HUD needs less than the full density scale.</summary>
        const float TouchViewingScale = 0.75f;

        readonly HudModel hud = new HudModel();
        UIDataModel model;

        public override void OnCreate()
        {
            // The name matches <body data-model="arena">.
            model = owner.GetComponent<UIDocumentComponent>().GetModel("arena");
            hud.refresh = Refresh;
            hud.touch = Input.touchSupported;
            model.Bind(hud);
        }

        public override void OnStart()
        {
            if (hud.touch)
            {
                ScaleForTouch();
            }
            InvokeRepeating(0.0f, refreshSeconds, Refresh);
        }

        /// <summary>Grows the root font size, and with it every rem length, with the screen's density.</summary>
        void ScaleForTouch()
        {
            var root = owner.GetComponent<UIDocumentComponent>().document?.root;
            if (root == null)
            {
                return;
            }
            float scale = Mathf.Max(1.0f, Screen.dpi / BaseDpi * TouchViewingScale);
            root.style["font-size"] = (BaseFontSize * scale).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "dp";
        }

        void Refresh()
        {
            var caster = SpellCaster.Main;
            if (caster == null)
            {
                return;
            }
            if (hud.spells.Count != caster.GetSpells().Count)
            {
                hud.spells.Clear();
                foreach (var spell in caster.GetSpells())
                {
                    var info = spell.GetInfo();
                    hud.spells.Add(new SpellSlot { key = info.slot, name = info.name, accent = info.accent });
                }
            }
            var selected = caster.GetSelected();
            for (int index = 0; index < hud.spells.Count; ++index)
            {
                hud.spells[index].selected = caster.GetSpells()[index] == selected;
            }
            if (selected != null)
            {
                var info = selected.GetInfo();
                hud.spellName = info.name;
                hud.spellDescription = info.description;
                hud.spellAccent = info.accent;
            }
            hud.lastCast = caster.GetLastCast();
            hud.particles = caster.CountParticles();
            hud.emitters = caster.CountEmitters();
            hud.frozen = caster.IsFrozen();
            hud.windy = caster.IsWindy();
            hud.slowMotion = caster.IsSlowMotion();
            hud.braziers = caster.AreBraziersLit();
            hud.raining = caster.IsRaining();
            // Everything here changes from outside the document, so the model cannot see it change.
            model.DirtyAll();
        }
    }
}
