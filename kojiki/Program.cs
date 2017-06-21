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
        private bool flag;

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

            double x = 0.5 * (double)w - 50, y = 0.5 * (double)h - 25;

            // ボタン作成
            for (int i = 0; i < buttonN; i++)
            {
                bt[i] = new Button();
                bt[i].Text = buttonName[i];
                bt[i].Location = new Point((int)x, (int)(y + 2 * i * bt[0].Height));
            }

            // ボタン動作
            bt[0].Click += new EventHandler(startBt_Click);
            bt[1].Click += new EventHandler(confBt_Click);
            bt[2].Click += new EventHandler(exitBt_Click);

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
            tb.Location = new Point((int)x, (int)y);

            // マウスClick動作
            this.MouseClick += new MouseEventHandler(mouseClick);

            // 値が変更された際のイベントハンドらーを追加
            tb.ValueChanged += new EventHandler(tb_ValueChanged);

            DrawTitle();
        }

        public void DrawTitle()
        {
            // 背景
            this.BackgroundImage = Image.FromFile("Asset/kojiki_memu_back.bmp");
            reader.Position = 0;
            waveOut.Init(reader);
            waveOut.Volume = vol;
            waveOut.Play();

            this.Controls.Remove(tb);

            // ボタン表示
            for (int i = 0; i < buttonN; i++) bt[i].Parent = this;

            flag = false;
        }

        //======================ボタン================================
        public void startBt_Click(Object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("Asset/ana.bmp");

            // ボタン消去
            for (int i = 0; i < buttonN; i++) this.Controls.Remove(bt[i]);

            waveOut.Stop();

            flag = true;
        }

        public void confBt_Click(Object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("Asset/conf.bmp");
            // ボタン消去
            for (int i = 0; i < buttonN; i++) this.Controls.Remove(bt[i]);

            tb.Parent = this;

            flag = true;

        }

        public void exitBt_Click(Object sender, EventArgs e)
        {
            Application.Exit();
        }

        //=======================マウス================================
        public void mouseClick(object sender, MouseEventArgs e)
        {
            string msg = "タイトルへ戻りますか？"; // クライアント座標
            if (e.Button == MouseButtons.Right)
            {
                if (flag)
                {
                    DialogResult result = MessageBox.Show(msg, "メニュー", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        waveOut.Stop();
                        DrawTitle();
                    }
                }
            }
        }

        public void tb_ValueChanged(Object sender, EventArgs e)
        {
            vol = tb.Value / 100f;
        }
    }
}
