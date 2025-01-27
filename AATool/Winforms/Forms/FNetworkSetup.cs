using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using AATool.Configuration;
using AATool.Net;

namespace AATool.Winforms.Forms
{
    public partial class FNetworkSetup : Form
    {
        private CancellationTokenSource cancelSource;
        private Control[] pages;
        private int index;

        public FNetworkSetup()
        {
            this.InitializeComponent();
            this.Width  = 500;
            this.Height = 320;
            this.back.Hide();

            this.InitializePages();
            this.UpdateAutoIP();
        }

        private void InitializePages()
        {
            this.pages = new Control[] {
                this.page0,
                this.page1,
                this.page2,
                this.page3
            };

            foreach (Control page in this.pages)
            {
                page.Location = this.page0.Location;
                page.Hide();
            }
            this.Navigate(0);
        }

        private void Navigate(int offset)
        {
            this.pages[this.index].Hide();
            this.index += offset;
            this.pages[this.index].Show();
            this.title.Text = this.pages[this.index].Tag.ToString();
            this.next.Enabled = false;

            this.autoServerIP.Checked = true;
            if (this.server.Checked)
                this.autoServerIP.Show();
            else
                this.autoServerIP.Hide();

            this.ip.UseSystemPasswordChar = true;
            this.toggleIP.Text = "显示IP地址";
            this.UpdateControls();
        }

        private void UpdateControls()
        {
            if (this.index < this.pages.Length - 1) 
            {
                this.next.Text = "下一步";
                if (this.index is 0)
                    this.back.Hide();
            }
            else
                this.next.Text = "完成";

            switch (this.index)
            {
                case 0:
                    this.next.Enabled = !string.IsNullOrWhiteSpace(this.mojangName.Text);
                    break;
                case 1:
                    this.next.Enabled = this.client.Checked || this.server.Checked;
                    break;
                case 2:
                    if (this.client.Checked)
                    {
                        this.autoServerIP.Checked = false;
                        this.ipLabel.Text = "输入要连接的服务器的 IP 地址。";
                        this.portLabel.Text = "这必须与托管服务器的人选择的端口匹配。（默认是 25562）";
                    }
                    else
                    {
                        this.ipLabel.Text = "如果你的网络配置比较特殊，可能需要更改这个设置，但如果你在 Minecraft 中选择了“对局域网开放”，你可以保持默认。你的朋友需要这个来连接。";
                        this.portLabel.Text = "这个通常可以保持默认（25562）。如果你不是通过Hamachi玩游戏，你可能需要在路由器上进行端口转发。这个过程因路由器不同而有所不同，但Google是你的好朋友（意思是让你自行网上搜索学习如何端口转发）。";
                    }
                    this.next.Enabled = IPAddress.TryParse(this.ip.Text, out _) && int.TryParse(this.port.Text, out _);
                    break;
                case 3:
                    this.next.Enabled = true;
                    break;
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            //cancel old requests and start a new one
            this.cancelSource?.Cancel();
            this.cancelSource = new CancellationTokenSource();

            if (sender == this.mojangName)
            {
                //cooldown timer to prevent spamming requests for every letter of someone's name
                this.keyboardTimer.Start();
            }
            else if (sender == this.pronouns && this.pronouns.Text.ToLower() is "write your own")
            {
                //clear pronouns box for user to type their own
                this.pronouns.SelectedIndex = -1;
                this.pronouns.Text = "";
            }
            this.UpdateControls();
        }

        private async void TryUpdateSkinAsync(CancellationToken? cancelToken = null)
        {
            Uuid id = Uuid.Empty;
            if (Player.ValidateName(this.mojangName.Text))
                id = await Player.FetchUuidAsync(this.mojangName.Text);

            if (id == Uuid.Empty)
            {
                this.face.Image = null;
                return;
            }

            if (this.face.Image is null || (this.face.Image.Tag is Uuid current && id != current))
            {
                try
                {
                    //asynchronously pull minecraft head from the internet
                    //get player face image async
                    string url = Paths.Web.GetAvatarUrl(id.ToString(), 48);

                    using HttpClient http = new ();
                    using HttpResponseMessage responce = await http.GetAsync(new Uri(url), cancelToken ?? CancellationToken.None);
                    using Stream stream = await responce.Content.ReadAsStreamAsync();
                    this.face.Image = new Bitmap(stream) {
                        Tag = id
                    };
                }
                catch { }
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (sender == this.next)
            {
                if (this.index < this.pages.Length - 1)
                {
                    this.Navigate(1);
                    this.back.Show();
                }
                else
                {
                    this.Apply();
                    this.Close();
                }
            }
            else if (sender == this.back)
            {
                this.Navigate(-1);
            }
            else if (sender == this.toggleIP)
            {
                if (this.ip.UseSystemPasswordChar)
                {
                    string message = "在直播中显示IP地址时要小心！ ♥\n您确定要显示IP地址吗？";
                    DialogResult confirmation = MessageBox.Show(this, message, "IP地址显示确认", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning, 
                        MessageBoxDefaultButton.Button2);

                    if (confirmation == DialogResult.Yes)
                    {
                        this.ip.UseSystemPasswordChar = false;
                        this.toggleIP.Text = "隐藏IP地址";
                    }
                }
                else
                {
                    this.ip.UseSystemPasswordChar = true;
                    this.toggleIP.Text = "显示IP地址";
                }
            }
            else if (sender == this.togglePassword)
            {
                if (this.password.UseSystemPasswordChar)
                {
                    string message = "在直播中显示密码时要小心！ ♥\n您确定要显示密码吗？";
                    DialogResult confirmation = MessageBox.Show(this, message, "密码显示确认",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);

                    if (confirmation == DialogResult.Yes)
                    {
                        this.password.UseSystemPasswordChar = false;
                        this.togglePassword.Text = "隐藏密码";
                    }
                }
                else
                {
                    this.password.UseSystemPasswordChar = true;
                    this.togglePassword.Text = "显示密码";
                }
            }
        }

        private void Apply()
        {
            Config.Net.IsServer.Set(this.server.Checked);
            Config.Net.IP.Set(this.ip.Text);
            Config.Net.Port.Set(this.port.Text);
            Config.Net.Password.Set(this.password.Text);
            Config.Net.AutoServerIP.Set(this.autoServerIP.Checked);
            Config.Net.MinecraftName.Set(this.mojangName.Text);
            Config.Net.PreferredName.Set(this.displayName.Text);
            Config.Net.Pronouns.Set(this.pronouns.Text);
            Config.Net.TrySave();
        }

        private void UpdateAutoIP()
        {
            if (this.autoServerIP.Checked)
            {
                this.ip.Enabled = false;
                if (this.server.Checked && NetworkHelper.TryGetLocalIPAddress(out IPAddress address))
                    this.ip.Text = address.ToString();
            }
            else
            {
                this.ip.Enabled = true;
            }
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (sender == this.autoServerIP || sender == this.server || sender == this.client)
                this.UpdateAutoIP();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            //enough time has passed since last character was typed
            this.keyboardTimer.Stop();
            this.TryUpdateSkinAsync(this.cancelSource.Token);
        }
    }
}
