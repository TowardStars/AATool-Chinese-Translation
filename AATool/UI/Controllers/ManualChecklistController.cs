using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AATool.Configuration;
using AATool.Data.Objectives;
using AATool.Data.Progress;
using AATool.Graphics;
using AATool.Net;
using AATool.UI.Controls;
using AATool.UI.Interfaces;
using AATool.UI.Screens;
using AATool.Utilities;
using Microsoft.Xna.Framework;

namespace AATool.UI.Controllers
{
    public class ManualChecklistController
    {
        private static readonly string[] RootSections = { "story", "nether", "end", "husbandry", "adventure" };

        private readonly UIMainScreen screen;
        private readonly static Dictionary<Rectangle, List<ICheckableControl>> ControlsByBounds = new();
        private readonly static  Dictionary<string, ICheckableControl> ControlsByKey = new();
        private readonly HashSet<string> checks = new();

        private ICheckableControl hovered;

        private static string ChecklistPath =>
            Path.Combine(Paths.System.ManualChecklistFolder, $"checklist_{Tracker.CurrentVersion}.txt");
        
        public ManualChecklistController(UIMainScreen screen)
        {
            this.screen = screen;
        }

        public void Invalidate()
        {
            List<UIControl> controls = new();
            this.screen.GetTreeRecursive(controls);
            
            var checks = controls
                .Where(x => x is ICheckableControl)
                .Cast<ICheckableControl>()
                .ToList();
            
            this.RebuildKeyLookups(checks);
            this.RebuildBoundsLookups(checks);
            Tracker.ManualChecklistInvalidated = true;
        }

        private void RebuildKeyLookups(List<ICheckableControl> checks)
        {
            ControlsByKey.Clear();

            foreach (ICheckableControl check in checks)
            {
                if (check.Objective is Criterion crit)
                {
                    ControlsByKey[crit.OwnerId + crit.Id] = check;
                }
                else if (check.Objective is Advancement adv)
                {
                    ControlsByKey[adv.Id] = check;
                }
            }
        }

        private void RebuildBoundsLookups(List<ICheckableControl> controls)
        {
            ControlsByBounds.Clear();
            
            foreach (ICheckableControl control in controls)
            {
                Rectangle bucket = control.Parent.Bounds;
                if (!ControlsByBounds.ContainsKey(bucket))
                {
                    ControlsByBounds[bucket] = new();
                }
                    
                ControlsByBounds[bucket].Add(control);
            }
        }
            

        public void Update(Time time)
        {
            Point cursor = Input.Cursor(this.screen);
            this.hovered = null;
            
            foreach (ICheckableControl check in ControlsByBounds
                .Where(x => x.Key.Contains(cursor))
                .SelectMany(x => x.Value))
            {
                if (!check.ManualCheckBounds.Contains(cursor)) 
                    continue;
                
                this.HandleHover(check);

                break;
            }
        }

        public void Toggle(string key)
        {
            if (!this.checks.Remove(key))
            {
                this.checks.Add(key);
            }

            this.Save();

            Tracker.ManualChecklistInvalidated = true;
        }

        private void HandleHover(ICheckableControl check)
        {
            this.hovered = check;

            if (!Input.LeftClickStarted)
                return;

            if (check.Objective is not (Advancement or Criterion))
                return;

            if (check.Objective is Advancement adv && adv.HasCriteria)
                return;

            this.Toggle(check.Key);
        }

        public void Draw(Canvas canvas)
        {
            if (this.hovered?.Objective is not (Advancement or Criterion))
                return;

            if (this.hovered.Objective is Advancement adv && (adv.HasCriteria || adv.Id.EndsWith("/root")))
                return;

            Rectangle bounds = this.hovered.ManualCheckBounds;
            bounds.Inflate(3, 3);
            
            Rectangle leftEdge   = new(bounds.X, bounds.Y, 2, bounds.Height);
            Rectangle rightEdge  = new(bounds.Right - 2, bounds.Y, 2, bounds.Height);
            Rectangle topEdge    = new(bounds.X, bounds.Y, bounds.Width, 2);
            Rectangle bottomEdge = new(bounds.X, bounds.Bottom - 2, bounds.Width, 2);
            
            canvas.DrawRectangle(leftEdge, Config.Main.TextColor, layer: Layer.Fore);
            canvas.DrawRectangle(rightEdge, Config.Main.TextColor, layer: Layer.Fore);
            canvas.DrawRectangle(topEdge, Config.Main.TextColor, layer: Layer.Fore);
            canvas.DrawRectangle(bottomEdge, Config.Main.TextColor, layer: Layer.Fore);
        }

        public static WorldState GetCurrentState()
        {
            return File.Exists(ChecklistPath) ? ReadChecklist() : new() ;
        }

        private static WorldState ReadChecklist()
        {
            using StreamReader reader = new(ChecklistPath);
            WorldState state = new();

            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (Tracker.Advancements.TryGet(line, out Advancement advancement))
                {
                    if (!state.Advancements.ContainsKey(advancement.Id))
                    {
                        state.Advancements[advancement.Id] = new();
                    }
                }
                else if (ControlsByKey.TryGetValue(line, out ICheckableControl check)
                    && check.Objective is Criterion criterion)
                {
                    string key = Criterion.Key(criterion.OwnerId, criterion.Id);

                    if (!state.Criteria.ContainsKey(key))
                    {
                        state.Criteria[key] = new();
                    }
                }
            }

            Contribution contribution = new(Uuid.Empty);
            contribution.Advancements = state.Advancements;
            contribution.Criteria = state.Criteria;
            state.Players[Uuid.Empty] = contribution;
            
            return state;
        }

        public void Load()
        {
            this.checks.Clear();
            if (!File.Exists(ChecklistPath))
                return;

            using StreamReader reader = new(ChecklistPath);
            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                this.checks.Add(line);
            }
        }

        public void Save()
        {
            this.HandleRootAdvancements();

            Directory.CreateDirectory(Path.GetDirectoryName(ChecklistPath));
            using StreamWriter writer = new(ChecklistPath, append: false);
            
            foreach (string key in this.checks)
            {
                writer.WriteLine(key);
            }
        }

        private void HandleRootAdvancements()
        {
            foreach (string section in RootSections)
            {
                string rootAdvancement = $"minecraft:{section}/root";

                this.checks.Remove(rootAdvancement);

                foreach (string key in this.checks.ToArray())
                {
                    if (key.StartsWith($"minecraft:{section}/"))
                    { 
                        this.checks.Add(rootAdvancement);
                        break;
                    }
                }
            }
        }

        public void Clear()
        {
            this.checks.Clear();
            this.Save();
            Tracker.ManualChecklistInvalidated = true;
        }
    }
}