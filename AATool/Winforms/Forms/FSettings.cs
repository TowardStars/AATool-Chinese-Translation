using System;
using System.Windows.Forms;
using AATool.Configuration;
using AATool.Net;
using AATool.Net.Requests;
using AATool.UI.Screens;
using Microsoft.Xna.Framework;

namespace AATool.Winforms.Forms
{
    public partial class FSettings : Form
    {
        private bool activated;

        public FSettings()
        {
            this.InitializeComponent();
            if (!this.DesignMode)
                this.LoadSettings();
        }

        public void InvalidateSettings() => this.tracker.InvalidateSettings();
        public void UpdateOverlayWidth() => this.overlay.UpdateWidth();
        public void UpdateNotesState() => this.main.UpdateNotesState();
        public void UpdateBadgeList() => this.main.UpdateBadgeList();
        public void UpdateFrameList() => this.main.UpdateFrameList();
        public void UpdateRainbow(Color color) => this.main.UpdateRainbow(color);

        private void OnActivated(object sender, EventArgs e)
        {
            //center window on first load
            if (!this.activated)
            {
                this.CenterToParent();
                this.activated = true;
            }      
        }

        private void LoadSettings()
        {
            this.tracker.LoadSettings();
            this.main.LoadSettings();
            this.network.LoadSettings();
            this.overlay.LoadSettings();
            this.debug.LoadSettings();
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            if (sender == this.done)
            {
                this.Close();
            }
            else if (sender == this.reset)
            {
                string msg = "这将清除所有自定义设置，包括主题、您的自定义存档路径和合作信息等。您确实要恢复为默认设置？";
                DialogResult confirmation = MessageBox.Show(this, msg, "Warning!", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning, 
                    MessageBoxDefaultButton.Button2);

                if (confirmation == DialogResult.Yes)
                {
                    Tracker.TrySetCategory("All Advancements");
                    Tracker.TrySetVersion("1.16");
                    Config.ResetAllToDefaults();
                    this.LoadSettings();
                    UIMainScreen.ForceLayoutRefresh();
                }
            }
            else if (sender == this.update)
            {
                new UpdateRequest(true).SendAsync();
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Peer.UnbindController(this.network);
            Main.PrimaryScreen.CloseSettingsMenu();
        }
    }
}
