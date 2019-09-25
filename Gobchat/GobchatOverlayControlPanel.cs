using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using RainbowMage.OverlayPlugin;

namespace Gobchat
{
    public partial class GobchatOverlayControlPanel : UserControl
    {
        private readonly GobchatOverlay overlay;
        private readonly GobchatOverlayConfig config;
        private FileSystemWatcher watcher;

        public GobchatOverlayControlPanel(GobchatOverlay overlay)
        {
            InitializeComponent();
            this.overlay = overlay;
            this.config = overlay.Config;

            SetupConfigEventHandler();
            SetupInitialValues();            
        }

        private void SetupConfigEventHandler()
        {
            this.config.VisibleChanged += (o, e) => {
                UISync(() => { this.checkOverlayVisible.Checked = e.IsVisible; });
            };
            this.config.PluginActiveChanged += (o, e) => {
                UISync(() => { this.checkPluginActive.Checked = e.IsPluginActive; });
            };
            this.config.ClickThruChanged += (o, e) => {
                UISync(() => { this.checkOverlayClickThrough.Checked = e.IsClickThru; });
            };
            this.config.LockChanged += (o, e) => {
                UISync(() => { this.checkOverlayLocked.Checked = e.IsLocked; });
            };
            this.config.UrlChanged += (o, e) => {
                UISync(() => { this.textUrl.Text = e.NewUrl; SetupFileWatcher(); });
            };

            this.config.GlobalHotkeyChanged += (o, e) => {
                UISync(() => { this.textGlobalHotkey.Text = GetHotkeyString(this.config.GlobalHotkeyModifiers, e.NewHotkey); });
            };
            this.config.GlobalHotkeyModifiersChanged += (o, e) => {
                UISync(() => {this.textGlobalHotkey.Text = GetHotkeyString(e.NewHotkey, this.config.GlobalHotkey); });
            };
            this.config.GlobalHotkeyEnabledChanged += (o, e) => {
                UISync(() =>{ this.checkEnableGlobalHotkey.Checked = e.NewGlobalHotkeyEnabled; });
            };

            this.config.MentionsChanged += (o, e) => {
                UISync(() => { this.textMentions.Text = string.Join(", ", e.Mentions); });
            };
        }

        private void SetupInitialValues()
        {
            this.checkOverlayVisible.Checked = config.IsVisible;
            this.checkPluginActive.Checked = config.IsPluginActive;
            this.checkOverlayClickThrough.Checked = config.IsClickThru;
            this.checkOverlayLocked.Checked = config.IsLocked;
            this.checkShowDebug.Checked = config.IsDebug;
            this.textUrl.Text = config.Url;

            this.textGlobalHotkey.Text = GetHotkeyString(config.GlobalHotkeyModifiers, config.GlobalHotkey);
            this.checkEnableGlobalHotkey.Checked = config.GlobalHotkeyEnabled;

            this.textMentions.Text = string.Join(", ", config.Mentions);

            SetupFileWatcher();
        }

        private void SetupFileWatcher()
        {
            if (this.config.Url == "")
                return;
            var path = System.IO.Path.GetDirectoryName(config.Url);
            path = System.Text.RegularExpressions.Regex.Replace(path, @"file:[\\\/]+", "");
            if (!System.IO.Directory.Exists(path))
            {
                this.overlay.getLogger().LogError($"URL does not exist: {path}");
                return;
            }

            watcher = new System.IO.FileSystemWatcher()
            {
                Path = path,
                NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.FileName,
                IncludeSubdirectories = true,
            };

            watcher.Created += btnReloadOverlay_Click;
            watcher.Deleted += btnReloadOverlay_Click;
            watcher.Renamed += btnReloadOverlay_Click;
            watcher.Changed += btnReloadOverlay_Click;
            watcher.EnableRaisingEvents = false;
        }

        private void UISync(Action action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }

        #region UI Events

        private void checkOverlayVisible_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsVisible = this.checkOverlayVisible.Checked;
        }

        private void checkPluginActive_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsPluginActive = this.checkPluginActive.Checked;
        }

        private void checkOverlayClickThrough_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsClickThru = this.checkOverlayClickThrough.Checked;
        }

        private void checkOverlayLocked_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsLocked = this.checkOverlayLocked.Checked;
        }

        private void btnUrl_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            try
            {
                ofd.InitialDirectory = System.IO.Path.GetDirectoryName(config.Url);
            }
            catch (Exception) { }

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.config.Url = new Uri(ofd.FileName).ToString();
                this.textUrl.Text = this.config.Url;
                SetupFileWatcher();
            }
        }

        private void textUrl_Leave(object sender, EventArgs e)
        {
            this.config.Url = textUrl.Text;
            SetupFileWatcher();
        }

        private void textMentions_Leave(object sender, EventArgs e)
        {
            string text = this.textMentions.Text;
            if (text == null) config.Mentions = new string[0];
            text = text.Trim();
            if (text.Length == 0) config.Mentions = new string[0];
            string[]split = text.Split(new char[] { ',' });
            split = split.Select(s => s.Trim().ToLower()).Distinct().Where(s => s.Length>0).ToArray();
            config.Mentions = split;
        }

        private void btnReloadOverlay_Click(object sender, EventArgs e)
        {
            this.overlay.Navigate(this.config.Url);
        }

        private void checkShowDebug_CheckedChanged(object sender, EventArgs e)
        {
            this.config.IsDebug = this.checkShowDebug.Checked;
        }

        private void textGlobalHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            var key = RemoveModifiers(e.KeyCode, e.Modifiers);
            this.config.GlobalHotkey = key;
            this.config.GlobalHotkeyModifiers = e.Modifiers;
        }

        private void checkEnableGlobalHotkey_CheckedChanged(object sender, EventArgs e)
        {
            this.config.GlobalHotkeyEnabled = this.checkEnableGlobalHotkey.Checked;
        }

        #endregion



        private static Keys RemoveModifiers(Keys keyCode, Keys modifiers)
        {
            var key = keyCode;
            var modifierList = new List<Keys>() { Keys.ControlKey, Keys.LControlKey, Keys.Alt, Keys.ShiftKey, Keys.Shift, Keys.LShiftKey, Keys.RShiftKey, Keys.Control, Keys.LWin, Keys.RWin };
            foreach (var mod in modifierList)
            {
                if (key.HasFlag(mod))
                {
                    if (key == mod)
                        key &= ~mod;
                }
            }
            return key;
        }

        private static string GetHotkeyString(Keys modifier, Keys key, String defaultText = "")
        {
            StringBuilder sbKeys = new StringBuilder();
            if ((modifier & Keys.Shift) == Keys.Shift)
            {
                sbKeys.Append("Shift + ");
            }
            if ((modifier & Keys.Control) == Keys.Control)
            {
                sbKeys.Append("Ctrl + ");
            }
            if ((modifier & Keys.Alt) == Keys.Alt)
            {
                sbKeys.Append("Alt + ");
            }
            if ((modifier & Keys.LWin) == Keys.LWin || (modifier & Keys.RWin) == Keys.RWin)
            {
                sbKeys.Append("Win + ");
            }
            sbKeys.Append(Enum.ToObject(typeof(Keys), key).ToString());
            return sbKeys.ToString();
        }


    }
}
