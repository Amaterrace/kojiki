using System;
using System.Windows.Forms;
using System.Drawing;
using NAudio.Wave;

class Sample : Form
{
    AudioFileReader reader = new AudioFileReader("title_kari.wav");
    WaveOut waveOut = new WaveOut();
    private TrackBar tb;
    private Button startBt;
    private Button exitBt;
    private Button confBt;
    private bool flag;
    const int w = 800, h = 600;

    public static void Main()
    {
        Application.Run(new Sample());
    }

    // コンストラクタ
    public Sample()
    {
        this.Text = "古事記";
        // サイズ固定（最大化とかはできる）
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Width = w; this.Height = h;

        // ボタン作成
        startBt = new Button();
        exitBt = new Button();
        confBt = new Button();
        tb = new TrackBar();
        startBt.Text = "START";
        confBt.Text = "CONFIG";
        exitBt.Text = "EXIT";

        // ボタン表示
        double x = 0.5 * w - 0.5 * startBt.Width, y = 0.5 * h - 0.5 * startBt.Height;
        startBt.Location = new Point((int)x, (int)y);
        confBt.Location = new Point((int)x, (int)(y + 2 * startBt.Height));
        exitBt.Location = new Point((int)x, (int)(y + 4 * startBt.Height));

        // ボタン動作
        startBt.Click += new EventHandler(startBt_Click);
        confBt.Click += new EventHandler(confBt_Click);
        exitBt.Click += new EventHandler(exitBt_Click);


        // マウスClick動作
        this.MouseClick += new MouseEventHandler(mouseClick);

        TitleDraw();
    }

    public void TitleDraw()
    {
        // 背景
        this.BackgroundImage = Image.FromFile("kojiki_memu_back.bmp");
        reader.Position = 0;
        waveOut.Init(reader);
        waveOut.Volume = 0.1f;
        waveOut.Play();

        this.Controls.Remove(tb);

        startBt.Parent = this;
        confBt.Parent = this;
        exitBt.Parent = this;

        flag = false;
    }

    //======================ボタン================================
    public void startBt_Click(Object sender, EventArgs e)
    {
        this.BackgroundImage = Image.FromFile("ana.bmp");

        // ボタン消去
        this.Controls.Remove(startBt);
        this.Controls.Remove(confBt);
        this.Controls.Remove(exitBt);

        waveOut.Stop();

        flag = true;
    }

    public void confBt_Click(Object sender, EventArgs e)
    {
        this.BackgroundImage = Image.FromFile("conf.bmp");
        // ボタン消去
        this.Controls.Remove(startBt);
        this.Controls.Remove(confBt);
        this.Controls.Remove(exitBt);

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
        string msg = "タイトルへ戻りますか？";	// クライアント座標
        if (e.Button == MouseButtons.Right)
        {
            if (flag)
            {
                DialogResult result = MessageBox.Show(msg, "メニュー", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    waveOut.Stop();
                    TitleDraw();
                }
            }
        }
    }
}
