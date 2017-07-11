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

        // NAudio
        private AudioFileReader opening = new AudioFileReader(
                            @"Asset\BGM\serious\01.丘へ続く道2.bgm");
        private AudioFileReader reader;
        private LoopStream loop;   // class-LoopStream内の型
        private WaveOut waveOut = new WaveOut();

        const int numFile = 4;  // BGM内のファイル数
        private string name;
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
        private TrackBar tb;
        private Label[] lb = new Label[4];
        private string[] tbStr = new string[] { "min", "Max", "音量" };

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

            tb = new TrackBar();
            // 値が変更された際のイベントハンドラーを追加
            tb.ValueChanged += new EventHandler(tb_ValueChanged);

            double x = 0.5 * (double)w - 50, y = 0.5 * (double)h - 25;

            // ボタン作成
            for (int i = 0; i < buttonN; i++)
            {
                bt[i] = new Button();
                bt[i].Text = buttonName[i];
                bt[i].Width = 100;
                bt[i].Location = new Point((int)x, (int)(y + 2 * i * bt[0].Height));

                bt[i].Click += new EventHandler(bt_Click);
            }
            // ボタン配置
            for (int i = 0; i < buttonN; i++) panel[0].Controls.Add(bt[i]);

            // 背景配置
            for (int i = 0; i < panel.Length; i++)
                panel[i].BackgroundImage = Image.FromFile(file + bgPass[i]);

            // テキストボックス
            im = Image.FromFile(file + txbPass);

            text = new Label();
            text.Width = 530; text.Height = 120;
            text.Location = new Point(200, 420);
            text.BackColor = Color.Transparent;
            // フォントサイズを指定
            text.Font = new Font(font[1], textFontSize);

            // マウスClick動作
            for (int i = 1; i < panel.Length; i++)
            {
                panel[i].MouseClick += new MouseEventHandler(mouseClick);
            }
            panel[gameNumber].MouseClick += new MouseEventHandler(textClick);
            text.MouseClick += new MouseEventHandler(textClick);
            panel[gameNumber].Paint += new PaintEventHandler(pnl_Paint);
            panel[gameNumber].Controls.Add(text);

            SetSoundsList();

            timer = new Timer();
            timer.Enabled = false;
            DrawTitle();
        }

        public void SetSoundsList()
        {
            int k = 0, l = 0;
            for (int i = 0; i < numFile; i++)
            {
                for (int j = 0; j < fileName[i].Length; j++)
                {
                    k++; l++;
                    listBt[k] = new Button();
                    listBt[k].Text = ">" + k.ToString();
                    listBt[k].Width = listBtWidth;
                    listBt[k].Location = new Point(i * 200, j * 30 + 5);
                    panel[listNumber].Controls.Add(listBt[k]);
                    listBt[k].Click += new EventHandler(ListBt_Click);

                    listLb[i] = new Label();
                    name = Path.GetFileName(fileName[i][j]).Replace
                                                ("0" + l.ToString() + ".", "");
                    listLb[i].Text = name.Replace(".bgm", "");
                    listLb[i].Width = 200 - listBtWidth;
                    listLb[i].Location = new Point(i * 200 + listBtWidth, j * 30 + 5);
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

            DrawPage(gameNumber);

            stext = ShowText(clickCount);
            // タイマーセットアップ&起動
            InitializeTimer();
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
            timer.Enabled = true;
            timer.Tick += new EventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (nextChar == stext.Length) timer.Enabled = false;
            else
            {
                text.Text += stext[nextChar];
                nextChar++;
            }
        }

        //======================ボタン================================
        public void bt_Click(Object sender, EventArgs e)
        {
            switch (((Button)sender).Text)
            {
                case "START": DrawGame(); break;
                case "CONFIG": DrawPage(configNumber); break;
                case "SOUNDS-LIST-": DrawPage(listNumber); break;
                case "EXIT":
                    string msg = "ゲームを終了しますか？";
                    DialogResult result = MessageBox.Show(msg, "終了", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    break;
                default: break;
            }

            if (panel[configNumber].Visible)
                // newTrackBar(int 置くパネルNo, double x(横幅の比), double y(高さの比))
                newTrackBar(configNumber, 0.1, 0.15);
            else if (panel[listNumber].Visible)
                newTrackBar(listNumber, 0.1, 0.8);
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
                        reader = new AudioFileReader(@fileName[i][j]);
                        loop = new LoopStream(reader);
                        play(loop);
                        break;
                    }
                }
            }
        }

        public void stopBt_Cilck(Object sender, EventArgs e)
        {
            waveOut.Stop();
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
                    if (timer.Enabled) timer.Enabled = false;
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
        public void newTrackBar(int OnPanel, double x, double y)
        {
            tb.TickStyle = TickStyle.Both;
            // 最小値、最大値を設定
            tb.Minimum = 0;
            tb.Maximum = 100;
            tb.Width = w / 3;
            // 描画される目盛りの刻みを設定
            tb.TickFrequency = 5;

            // トラックバー配置
            int tbX, tbY;
            tbX = (int)(this.Width * x);
            tbY = (int)(this.Height * y);
            tb.Location = new Point(tbX, tbY);
            panel[OnPanel].Controls.Add(tb);

            // ラベル作成
            const int textSize = 30;
            for (int i = 0; i < lb.Length; i++)
            {
                lb[i] = new Label();
                lb[i].Width = textSize;
                if (i == 0) lb[i].Text = Convert.ToString(vol * 100);
                else lb[i].Text = tbStr[i - 1];
            }
            lb[0].Location = new Point(tbX + tb.Width, (int)(tbY + 0.25 * tb.Height));
            lb[1].Location = new Point(tbX, (int)(tbY - 0.5 * tb.Height));
            lb[2].Location = new Point(tbX + tb.Width - textSize, (int)(tbY - 0.5 * tb.Height));
            lb[3].Location = new Point(tbX - textSize, tbY);
            for (int j = 0; j < lb.Length; j++) panel[OnPanel].Controls.Add(lb[j]);
        }

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

        public void play(LoopStream loop)
        {
            loop.Position = 0;
            waveOut.Init(loop);
            waveOut.Volume = vol;
            waveOut.Play();
        }
    }
}

public class LoopStream : WaveStream
{
    WaveStream sourceStream;

    public LoopStream(WaveStream sourceStream)
    {
        this.sourceStream = sourceStream;
        this.EnableLooping = true;
    }

    public bool EnableLooping { get; set; }

    public override WaveFormat WaveFormat
    {
        get { return sourceStream.WaveFormat; }
    }

    public override long Length
    {
        get { return sourceStream.Length; }
    }

    public override long Position
    {
        get { return sourceStream.Position; }
        set { sourceStream.Position = value; }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                if (sourceStream.Position == 0 || !EnableLooping)
                {
                    // something wrong with the source stream
                    break;
                }
                // loop
                sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}
