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

                bt[i].Click += new EventHandler(bt_Click);
            }

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

            int tbX, tbY;
            tbX = (int)(this.Width / 10);
            tbY = (int)(this.Height / 4);
            tb.Location = new Point(tbX, tbY);

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

            // マウスClick動作
            this.MouseClick += new MouseEventHandler(mouseClick);

            // 値が変更された際のイベントハンドラーを追加
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

        public void DrawConfig()
        {
            this.BackgroundImage = Image.FromFile("Asset/conf.bmp");
            for (int j = 0; j < lb.Length; j++) lb[j].Parent = this;
            tb.Parent = this;
        }

        //======================ボタン================================
        public void bt_Click(Object sender, EventArgs e)
        {
            for (int i = 0; i < buttonN; i++) this.Controls.Remove(bt[i]);

            if (sender == bt[0])  // startボタン
            {
                this.BackgroundImage = Image.FromFile("Asset/ana.bmp");
                waveOut.Stop();
                flag = true;
            }
            else if (sender == bt[1])  // configボタン
            {
                DrawConfig();
                flag = true;
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
                if (flag)
                {
                    DialogResult result = MessageBox.Show(msg, "メニュー", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        for (int i = 0; i < lb.Length; i++) this.Controls.Remove(lb[i]);
                        waveOut.Stop();
                        DrawTitle();
                    }
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
