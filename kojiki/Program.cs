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

        const int buttonN = 4;  // ボタンの数
        private Panel[] panel = new Panel[buttonN];
        private const int titleNumber = 0;
        private const int configNumber = 1;
        private const int listNumber = 2;
        private const int gameNumber = 3;

        // ボタン+ボタンname
        private Button[] bt = new Button[buttonN];
        private string[] buttonName = new string[buttonN]
         { "START", "CONFIG", "SOUNDS-LIST-", "EXIT" };

        // file pass + name
        private string file = "Asset/";
        private string[] bgPass = new string[]
         { "kojiki_memu_back.bmp", "conf.bmp", "souns_list.bmp", "ana.bmp" };
        private string txbPass = "textbox.png";
        private string framePass = "frame.png";

        // NAudio
        private AudioFileReader opening = new AudioFileReader(
                            @"Asset\BGM\serious\01.丘へ続く道2.bgm");
        private AudioFileReader reader;
        private LoopStream loop;   // class-LoopStream内の型
        private WaveOut waveOut = new WaveOut();

        const int numFile = 4;  // BGM内のファイル数
        private string[][] fileName =
        {
            Directory.GetFiles(
            @"Asset\BGM\battle", "*.bgm"),
            Directory.GetFiles(
            @"Asset\BGM\easy_going", "*.bgm"),
            Directory.GetFiles(
            @"Asset\BGM\quiet", "*.bgm"),
            Directory.GetFiles(
            @"Asset\BGM\serious", "*.bgm")
        };

        const int listBtWidth = 36;
        private Button[] listBt = new Button[30];
        private Button listStopBt;
        private Label[] listLb = new Label[5];

        private float vol = 0.1f;  // 初期値
        private TrackBar[] tb = new TrackBar[3];
        private Label[] trackLabel1 = new Label[4];
        private Label[] trackLabel2 = new Label[4];
        private Label[] trackLabel3 = new Label[4];
        private string[][] tbStr = new string[][]
        {
            new[] { "min", "Max", "音量" },
            new[] { "min", "Max", "早さ" }
        };

        // 本文
        private Label text;
        private Timer timer = new Timer();
        private int clickCount = 1;
        private int nextCharNum = 0;
        private int textSpeed = 50; //[ms]
        private int textFontSize = 12;  // フォントサイズ[pt]
        private string[] font = new string[]
            {"Arial", "ＭＳ Ｐ明朝", "ＭＳ Ｐゴシック"};
        private string stext;

        // テキストスピードのサンプル用
        private Label sampleText;

        private Image im;
        private Image frame;

        public static void Main()
        {
            Application.Run(new Program());
        }

        // コンストラクタ
        public Program()
        {
            //フォームのアイコンを設定する
            this.Icon = new System.Drawing.Icon(file + "kojiki_icon4.ico");
            this.Text = "古事記";
            // サイズ固定（最大化とかはできる）
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Width = w; this.Height = h;

            // パネル作成
            for (int i = 0; i < panel.Length; i++)
            {
                panel[i] = new Panel();
                panel[i].Dock = DockStyle.Fill;
                panel[i].Visible = false;
                this.Controls.Add(panel[i]);
            }

            for (int i = 0; i < tb.Length; i++)
            {
                tb[i] = new TrackBar();
                // 値が変更された際のイベントハンドラーを追加
                tb[i].ValueChanged += new EventHandler(tb_ValueChanged);
            }

            for (int i = 0; i < trackLabel1.Length; i++)
            {
                trackLabel1[i] = new Label();
                trackLabel2[i] = new Label();
                trackLabel3[i] = new Label();
            }

            double x = 0.5 * (double)w - 50, y = 0.5 * (double)h - 25;

            // ボタン作成
            for (int i = 0; i < buttonN; i++)
            {
                bt[i] = new Button();
                bt[i].Text = buttonName[i];
                bt[i].Width = 100;
                bt[i].Location = new Point((int)x, (int)(y + 2 * i * bt[0].Height));

                bt[i].Click += new EventHandler(bt_Click);

                // ボタン配置
                panel[0].Controls.Add(bt[i]);
            }

            // 背景配置
            for (int i = 0; i < panel.Length; i++)
                panel[i].BackgroundImage = Image.FromFile(file + bgPass[i]);

            // 背景枠
            frame = Image.FromFile(file + framePass);

            // テキストボックス
            im = Image.FromFile(file + txbPass);

            // Game画面のテキスト作成
            text = new Label();
            text.Width = 530; text.Height = 120;
            text.Location = new Point(200, 420);
            text.BackColor = Color.Transparent;
            // フォントサイズを指定
            text.Font = new Font(font[1], textFontSize);
            panel[gameNumber].Controls.Add(text);

            // マウスClick動作
            for (int i = 1; i < panel.Length; i++)
            {
                panel[i].MouseClick += new MouseEventHandler(mouseClick);
            }
            panel[gameNumber].MouseClick += new MouseEventHandler(textClick);
            text.MouseClick += new MouseEventHandler(textClick);
            panel[gameNumber].Paint += new PaintEventHandler(pnl_Paint);
            panel[configNumber].Paint += new PaintEventHandler(frame_Paint);
            panel[listNumber].Paint += new PaintEventHandler(frame_Paint);

            SetConfig();
            SetSoundsList();
            InitializeTimer();

            DrawTitle();
        }

        /// <summary>
        /// セットページ
        /// </summary>
        public void SetSoundsList()
        {
            string name;
            int k = 0, l = 0;
            for (int i = 0; i < numFile; i++)
            {
                for (int j = 0; j < fileName[i].Length; j++)
                {
                    k++; l++;
                    listBt[k] = new Button();
                    listBt[k].Text = ">" + k.ToString();
                    listBt[k].Width = listBtWidth;
                    listBt[k].Location = new Point(i * 200 + 20, j * 30 + 20);
                    panel[listNumber].Controls.Add(listBt[k]);
                    listBt[k].Click += new EventHandler(ListBt_Click);

                    listLb[i] = new Label();
                    name = Path.GetFileName(fileName[i][j]).Replace
                                                ("0" + l.ToString() + ".", "");
                    listLb[i].Text = name.Replace(".bgm", "");
                    listLb[i].Width = 200 - listBtWidth;
                    listLb[i].Location = new Point(i * 200 + 20 + listBtWidth, j * 30 + 20);
                    listLb[i].BackColor = Color.Transparent;
                    listLb[i].ForeColor = Color.White;
                    panel[listNumber].Controls.Add(listLb[i]);
                }
                l = 0;
            }

            listStopBt = new Button();
            listStopBt.Text = "■";
            listStopBt.Width = listBtWidth;
            listStopBt.Location = new Point(w - 100, h - 100);
            panel[listNumber].Controls.Add(listStopBt);
            listStopBt.Click += new EventHandler(stopBt_Cilck);

            SetTrackBar(tb[1], listNumber, 0.1, 0.8);
        }

        public void SetConfig()
        {
            // newTrackBar(int 置くパネルNo, double x(横幅の比), double y(高さの比))
            SetTrackBar(tb[0], configNumber, 0.1, 0.15);
            SetTrackBar(tb[2], configNumber, 0.1, 0.3);

            // サンプルテキストの設定
            sampleText = new Label();
            sampleText.Location = new Point(400, 180);
            sampleText.Width = 350;
            sampleText.Height = 50;
            sampleText.Font = new Font(font[1], 14, FontStyle.Italic);
            sampleText.Padding = new Padding(50, 15, 0, 0);
            sampleText.BackColor = Color.FromArgb(80, 204, 204, 204);
            panel[configNumber].Controls.Add(sampleText);
        }
        //=====================描画関数==================================
        public void DrawPage(int pageNumber)
        {
            for (int i = 0; i < panel.Length; i++)
            {
                if (i != pageNumber) panel[i].Visible = false;
                else panel[i].Visible = true;
            }
        }

        public void DrawTitle()
        {
            loop = new LoopStream(opening);
            play(loop);

            DrawPage(titleNumber);
        }

        public void DrawGame()
        {
            waveOut.Stop();
            waveOut.Dispose();
            DrawPage(gameNumber);

            stext = ShowText(clickCount);

            timer.Enabled = true;
        }

        public string ShowText(int n)
        {
            StreamReader sr = new StreamReader("test.txt", System.Text.Encoding.Default);
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
            timer.Enabled = false;
            timer.Tick += new EventHandler(timer_Tick);
        }

        int flug = 0;
        private void timer_Tick(object sender, EventArgs e)
        {
            if (panel[gameNumber].Visible)
            {
                if (nextCharNum == stext.Length)
                    timer.Enabled = false;
                else
                {
                    text.Text += stext[nextCharNum];
                    nextCharNum++;
                }
            }
            else if (panel[configNumber].Visible)
            {
                if (nextCharNum == stext.Length)
                {
                    if (flug < 2000 / (timer.Interval + 10) - 600 / (timer.Interval + 10))
                    {
                        flug++;
                        goto JUMP;
                    }
                    sampleText.ResetText();
                    nextCharNum = 0;
                    flug = 0;
                }
                else
                {
                    sampleText.Text += stext[nextCharNum];
                    nextCharNum++;
                }
            }
            JUMP:;
        }

        //======================ボタン================================
        public void bt_Click(Object sender, EventArgs e)
        {
            switch (((Button)sender).Text)
            {
                case "START":
                    DrawGame();
                    break;
                case "CONFIG":
                    DrawPage(configNumber);
                    stext = "これはサンプルテキストです。";
                    sampleText.ResetText();
                    timer.Enabled = true;
                    break;
                case "SOUNDS-LIST-":
                    DrawPage(listNumber);
                    break;
                case "EXIT":
                    string msg = "ゲームを終了しますか？";
                    DialogResult result = MessageBox.Show(msg, "終了", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        timer.Enabled = false;
                        waveOut.Stop();
                        waveOut.Dispose();
                        Application.Exit();
                    }
                    break;
                default: break;
            }
        }

        public void ListBt_Click(Object sender, EventArgs e)
        {
            string btString = ((System.Windows.Forms.Button)sender).Text;
            string btNum = btString.Replace(">", "");

            int k = 0;
            for (int i = 0; i < numFile; i++)
            {
                for (int j = 0; j < fileName[i].Length; j++)
                {
                    k++;
                    if (k == int.Parse(btNum))
                    {
                        waveOut.Stop();
                        waveOut.Dispose();
                        reader = new AudioFileReader(@fileName[i][j]);
                        loop = new LoopStream(reader);
                        play(loop);

                        loop.Dispose();
                        break;
                    }
                }
            }
        }

        public void stopBt_Cilck(Object sender, EventArgs e)
        {
            waveOut.Stop();
            waveOut.Dispose();
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
                    nextCharNum = 0;
                    waveOut.Stop();
                    if (timer != null) timer.Dispose();
                    DrawTitle();
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
                    nextCharNum = 0;
                    timer.Enabled = true;
                }
            }
        }

        //======================トラックバー=============================
        public void SetTrackBar(TrackBar trackBar, int OnPanel, double x, double y)
        {
            trackBar.TickStyle = TickStyle.Both;
            // 最小値、最大値を設定
            trackBar.Minimum = 0;
            trackBar.Maximum = 100;
            trackBar.Width = w / 3;
            // 描画される目盛りの刻みを設定
            trackBar.TickFrequency = 5;

            // トラックバー配置
            int tbX, tbY;
            tbX = (int)(this.Width * x);
            tbY = (int)(this.Height * y);
            trackBar.Location = new Point(tbX, tbY);

            if (trackBar == tb[0] || trackBar == tb[1])
            {
                trackBar.Value = (int)(vol * 100);
            }
            else if (trackBar == tb[2])
            {
                trackBar.Value = 101 - textSpeed;
            }

            panel[OnPanel].Controls.Add(trackBar);

            TrackBarLabels(trackBar, tbX, tbY, trackBar.Width, trackBar.Height);

            if (OnPanel == configNumber)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (trackBar == tb[0]) panel[OnPanel].Controls.Add(trackLabel1[i]);
                    else if (trackBar == tb[2]) panel[OnPanel].Controls.Add(trackLabel3[i]);
                }
            }
            else if (OnPanel == listNumber)
            {
                for (int i = 0; i < 4; i++) panel[OnPanel].Controls.Add(trackLabel2[i]);
            }

        }

        public void TrackBarLabels(TrackBar trackBar, int tbX, int tbY, int tbw, int tbh)
        {
            // ラベル作成
            const int textSize = 30;
            if (trackBar == tb[0])
            {
                for (int i = 0; i < trackLabel1.Length; i++)
                {
                    trackLabel1[i].Width = textSize;
                    if (i == 0) trackLabel1[i].Text = Convert.ToString(vol * 100);
                    else trackLabel1[i].Text = tbStr[0][i - 1];
                    trackLabel1[i].ForeColor = Color.White;
                    trackLabel1[i].BackColor = Color.Transparent;
                }
                trackLabel1[0].Location = new Point(tbX + tbw, (int)(tbY + 0.25 * tbh));
                trackLabel1[1].Location = new Point(tbX, (int)(tbY - 0.5 * tbh));
                trackLabel1[2].Location = new Point(tbX + tbw - textSize, (int)(tbY - 0.5 * tbh));
                trackLabel1[3].Location = new Point(tbX - textSize - 10, (int)(tbY + 0.25 * tbh));
            }
            else if (trackBar == tb[1])
            {
                for (int i = 0; i < trackLabel2.Length; i++)
                {
                    trackLabel2[i].Width = textSize;
                    if (i == 0) trackLabel2[i].Text = Convert.ToString(vol * 100);
                    else trackLabel2[i].Text = tbStr[0][i - 1];
                    trackLabel2[i].ForeColor = Color.White;
                    trackLabel2[i].BackColor = Color.Transparent;
                }
                trackLabel2[0].Location = new Point(tbX + tbw, (int)(tbY + 0.25 * tbh));
                trackLabel2[1].Location = new Point(tbX, (int)(tbY - 0.5 * tbh));
                trackLabel2[2].Location = new Point(tbX + tbw - textSize, (int)(tbY - 0.5 * tbh));
                trackLabel2[3].Location = new Point(tbX - textSize - 10, (int)(tbY + 0.25 * tbh));
            }
            else if (trackBar == tb[2])
            {
                for (int i = 0; i < trackLabel3.Length; i++)
                {
                    trackLabel3[i].Width = textSize;
                    if (i == 0) trackLabel3[i].Text = Convert.ToString(textSpeed);
                    else trackLabel3[i].Text = tbStr[1][i - 1];
                    trackLabel3[i].ForeColor = Color.White;
                    trackLabel3[i].BackColor = Color.Transparent;
                }
                trackLabel3[0].Location = new Point(tbX + tbw, (int)(tbY + 0.25 * tbh));
                trackLabel3[1].Location = new Point(tbX, (int)(tbY - 0.5 * tbh));
                trackLabel3[2].Location = new Point(tbX + tbw - textSize, (int)(tbY - 0.5 * tbh));
                trackLabel3[3].Location = new Point(tbX - textSize - 10, (int)(tbY + 0.25 * tbh));
            }
        }

        public void tb_ValueChanged(Object sender, EventArgs e)
        {
            if ((TrackBar)sender == tb[0])
            {
                vol = tb[0].Value / 100f;
                waveOut.Volume = vol;
                trackLabel1[0].Text = Convert.ToString(tb[0].Value);
            }
            else if ((TrackBar)sender == tb[1])
            {
                vol = tb[1].Value / 100f;
                waveOut.Volume = vol;
                trackLabel2[0].Text = Convert.ToString(tb[1].Value);
            }
            else if ((TrackBar)sender == tb[2])
            {
                timer.Interval = 101 - tb[2].Value;
                trackLabel3[0].Text = Convert.ToString(tb[2].Value);
            }
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

            g.Dispose();
        }

        public void frame_Paint(Object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawImage(frame, 0, 0, 780, 560);

            g.Dispose();
        }

        public void play(LoopStream loop)
        {
            loop.Position = 0;
            waveOut.Init(loop);
            waveOut.Volume = vol;
            waveOut.Play();
        }
    }
}
