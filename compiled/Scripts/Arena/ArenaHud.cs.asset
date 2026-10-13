using System.Collections.Generic;
using Unravel.Core;

namespace ParticlesSample
{
    /// <summary>
    /// Drives app:/data/UI/arena_hud.rhtml through the data model "arena": the spell bar, the
    /// selected spell, and the live particle count with the state of the arena's switches.
    /// It reads SpellCaster a few times a second and marks the whole model dirty.
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
        }

        readonly HudModel hud = new HudModel();
        UIDataModel model;

        public override void OnCreate()
        {
            // The name matches <body data-model="arena">.
            model = owner.GetComponent<UIDocumentComponent>().GetModel("arena");
            model.Bind(hud);
        }

        public override void OnStart()
        {
            InvokeRepeating(0.0f, refreshSeconds, Refresh);
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
            // Everything here changes from outside the document, so the model cannot see it change.
            model.DirtyAll();
        }
    }
}
