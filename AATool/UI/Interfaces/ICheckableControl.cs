using AATool.Data.Objectives;
using AATool.UI.Controls;
using Microsoft.Xna.Framework;

namespace AATool.UI.Interfaces
{
    public interface ICheckableControl
    {
        public UIControl Parent { get; }
        public bool IsChecked { get; set; }
        public Rectangle ManualCheckBounds { get; }
        public Objective Objective { get; } 
        public string Key { get; }
    }
}