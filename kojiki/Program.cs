using System;
using System.Windows.Forms;
using System.Drawing;
using NAudio.Wave;
using System.IO;

namespace kojiki
{
    class Program : Form
    {
        const int w = 800, h = 600;
        const int buttonN = 3;  // ボタンの数
        private float vol;
        private const int titleNumber = 0;
        private const int configNumber = 1;
        private const int gameNumber = 2;


        // file pass + name
        private string file = "Asset/";
        private string[] bgPass = new string[]
         { "kojiki_memu_back.bmp", "conf.bmp", "ana.bmp", "textbox.png" };

        // NAudio
        private AudioFileReader reader = new AudioFileReader("Asset/title_kari.wav");
        private WaveOut waveOut = new WaveOut();

        // ボタン+ボタンname
        private Button[] bt = new Button[buttonN];
        private string[] buttonName = new string[buttonN]
         { "START", "CONFIG", "EXIT" };

        private TrackBar tb;
        private Label[] lb = new Label[4];

        // 本文
        private Label text;
        private Timer timer;
        private int clickCount = 1;
        private int nextChar = 0;
        private int textSpeed = 50; //[ms]
        private int textFontSize = 12;  // フォントサイズ[pt]
        private string[] font = new string[]
            {"Arial", "ＭＳ Ｐ明朝", "ＭＳ Ｐゴシック"};
        private string stext;

        private Image im;
        private Panel[] panel = new Panel[3];

        public static void Main()
        {
            Application.Run(new Program());
        }

        // コンストラクタ
        public Program()
        {
            //フォームのアイコンを設定する
            this.Icon = new System.Drawing.Icon(file + "kojiki_icon3.ico");
            this.Text = "古事記";
            // サイズ固定（最大化とかはできる）
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Width = w; this.Height = h;

            // パネル作成
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
            for (int i = 0; i < panel.Length; i++)
                panel[i].BackgroundImage = Image.FromFile(file + bgPass[i]);

            // テキストボックス
            im = Image.FromFile(file + bgPass[3]);

            text = new Label();
            text.Width = 530; text.Height = 120;
            text.Location = new Point(200, 420);
            text.BackColor = Color.Transparent;
            // フォントサイズを指定
            text.Font = new Font(font[1], textFontSize);

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
            panel[gameNumber].MouseClick += new MouseEventHandler(textClick);
            text.MouseClick += new MouseEventHandler(textClick);
            panel[gameNumber].Paint += new PaintEventHandler(pnl_Paint);
            panel[gameNumber].Controls.Add(text);

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

            for (int i = 0; i < panel.Length; i++)
            {
                if (i != titleNumber) panel[i].Visible = false;
                else panel[i].Visible = true;
            }
        }

        public void DrawConfig()
        {
            for (int i = 0; i < panel.Length; i++)
            {
                if (i != configNumber) panel[i].Visible = false;
                else panel[i].Visible = true;
            }
        }


        public void DrawGame()
        {
            waveOut.Stop();

            for (int i = 0; i < panel.Length; i++)
            {
                if (i != gameNumber) panel[i].Visible = false;
                else panel[i].Visible = true;
            }

            stext = ShowText(clickCount);
            timer = new Timer();
            // タイマーセットアップ&起動
            InitializeTimer();
        }

        public string ShowText(int n)
        {
            StreamReader sr = new StreamReader(file + "test.txt", System.Text.Encoding.Default);
            int counter = 0;
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                counter++;
                if (n == counter) break;
            }
            return line;
        }

        //====================タイマー==============================
        private void InitializeTimer()
        {
            timer.Interval = textSpeed;
            timer.Enabled = true;
            timer.Tick += new EventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (nextChar == stext.Length)
            {
                timer.Enabled = false;
            }
            else
            {
                text.Text += stext[nextChar];
                nextChar++;
            }
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
                    timer.Enabled = false;
                }
            }
        }

        public void textClick(Object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (timer.Enabled)
                {
                    text.Text = stext;
                    timer.Enabled = false;
                }
                else
                {
                    clickCount++;
                    text.ResetText();
                    stext = ShowText(clickCount);
                    nextChar = 0;
                    timer.Enabled = true;
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

        //========================ペイント==================================
        public void pnl_Paint(Object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            double p = 0.94;
            double pX, pY;

            pX = im.Width * p;
            pY = im.Height * p;
            g.DrawImage(im, 15, 0, (int)pX, (int)pY);
        }
    }
}
