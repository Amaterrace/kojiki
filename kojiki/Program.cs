using System;
using System.Windows.Forms;
using System.Drawing;
using NAudio.Wave;

namespace kojiki
{
    class Program : Form
    {
        const int w = 800, h = 600;
        const int buttonN = 3;  // ボタンの数
        private float vol;
        AudioFileReader reader = new AudioFileReader("Asset/title_kari.wav");
        WaveOut waveOut = new WaveOut();
        Button[] bt = new Button[buttonN];
        string[] buttonName = new string[buttonN] { "START", "CONFIG", "EXIT" };
        private TrackBar tb;
        private Label[] lb = new Label[4];
        private Panel[] panel = new Panel[3];

        public static void Main()
        {
            Application.Run(new Program());
        }

        // コンストラクタ
        public Program()
        {
            this.Text = "古事記";
            // サイズ固定（最大化とかはできる）
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Width = w; this.Height = h;

            // パネル作成
            // panel[0] == タイトル画面
            // panel[1] == コンフィグ画面
            // panel[2] == ゲーム画面
            for (int i = 0; i < panel.Length; i++)
            {
                panel[i] = new Panel();
                panel[i].Dock = DockStyle.Fill;
                this.Controls.Add(panel[i]);
            }

            double x = 0.5 * (double)w - 50, y = 0.5 * (double)h - 25;

            // ボタン作成
            for (int i = 0; i < buttonN; i++)
            {
                bt[i] = new Button();
                bt[i].Text = buttonName[i];
                bt[i].Location = new Point((int)x, (int)(y + 2 * i * bt[0].Height));

                bt[i].Click += new EventHandler(bt_Click);
            }
            // ボタン配置
            for (int i = 0; i < buttonN; i++) panel[0].Controls.Add(bt[i]);

            // 背景配置
            panel[0].BackgroundImage = Image.FromFile("Asset/kojiki_memu_back.bmp");
            panel[1].BackgroundImage = Image.FromFile("Asset/conf.bmp");
            panel[2].BackgroundImage = Image.FromFile("Asset/ana.bmp");

            // トラックバー作成
            tb = new TrackBar();
            tb.TickStyle = TickStyle.Both;
            // 初期値を設定
            tb.Value = 0;
            vol = 0.1f;
            // 最小値、最大値を設定
            tb.Minimum = 0;
            tb.Maximum = 100;
            tb.Width = w / 3;
            // 描画される目盛りの刻みを設定
            tb.TickFrequency = 5;

            // トラックバー配置
            int tbX, tbY;
            tbX = (int)(this.Width / 10);
            tbY = (int)(this.Height / 4);
            tb.Location = new Point(tbX, tbY);
            panel[1].Controls.Add(tb);

            // ラベル作成
            const int textSize = 30;
            for (int i = 0; i < lb.Length; i++)
            {
                lb[i] = new Label();
                lb[i].Width = textSize;
            }
            lb[0].Text = Convert.ToString(vol * 100);
            lb[1].Text = "min";
            lb[2].Text = "Max";
            lb[3].Text = "音量";
            lb[0].Location = new Point(tbX + tb.Width, (int)(tbY + 0.25 * tb.Height));
            lb[1].Location = new Point(tbX, (int)(tbY - 0.5 * tb.Height));
            lb[2].Location = new Point(tbX + tb.Width - textSize, (int)(tbY - 0.5 * tb.Height));
            lb[3].Location = new Point(tbX - textSize, tbY);
            for (int j = 0; j < lb.Length; j++) panel[1].Controls.Add(lb[j]);


            // マウスClick動作
            for (int i = 1; i < panel.Length; i++)
            {
                panel[i].MouseClick += new MouseEventHandler(mouseClick);
            }

            // 値が変更された際のイベントハンドラーを追加
            tb.ValueChanged += new EventHandler(tb_ValueChanged);

            DrawTitle();
        }

        public void DrawTitle()
        {
            reader.Position = 0;
            waveOut.Init(reader);
            waveOut.Volume = vol;
            waveOut.Play();

            panel[0].Visible = true;
            panel[1].Visible = false;
            panel[2].Visible = false;
        }

        public void DrawConfig()
        {
            panel[0].Visible = false;
            panel[1].Visible = true;
            panel[2].Visible = false;
        }

        public void DrawGame()
        {
            waveOut.Stop();

            panel[0].Visible = false;
            panel[1].Visible = false;
            panel[2].Visible = true;
        }

        //======================ボタン================================
        public void bt_Click(Object sender, EventArgs e)
        {
            if (sender == bt[0])  // startボタン
            {
                DrawGame();
            }
            else if (sender == bt[1])  // configボタン
            {
                DrawConfig();
            }
            else if (sender == bt[2])  // exitボタン
            {
                string msg = "ゲームを終了しますか？";
                DialogResult result = MessageBox.Show(msg, "終了", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
        }

        //=======================マウス================================
        public void mouseClick(object sender, MouseEventArgs e)
        {
            string msg = "タイトルへ戻りますか？";
            if (e.Button == MouseButtons.Right)
            {
                    DialogResult result = MessageBox.Show(msg, "メニュー", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        waveOut.Stop();
                        DrawTitle();
                    }
            }
        }

        //======================トラックバー=============================
        public void tb_ValueChanged(Object sender, EventArgs e)
        {
            vol = tb.Value / 100f;
            waveOut.Volume = vol;
            lb[0].Text = Convert.ToString(vol * 100);
        }
    }
}
