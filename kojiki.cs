using System;
using System.Windows.Forms;
using System.Drawing;


class Sample : Form
{
	private System.Media.SoundPlayer player = null;
	private Button startBt;
	private Button exitBt;
	private Button confBt;
	private bool flag;

	public static void Main()
	{
		Application.Run(new Sample());
	}

	// コンストラクタ
	public Sample()
	{
		const int w=800, h=600;
		this.Text = "古事記";
		// サイズ固定（最大化とかはできる）
		this.FormBorderStyle = FormBorderStyle.FixedSingle;
		this.Width = w;  this.Height = h;

		// ボタン作成
		startBt = new Button();
		exitBt = new Button();
		confBt = new Button();
		startBt.Text = "START";
		exitBt.Text = "EXIT";

		// ボタン表示
		double x=0.5*w-0.5*startBt.Width, y=0.5*h-0.5*startBt.Height;
		startBt.Location = new Point((int)x, (int)y);
		exitBt.Location = new Point((int)x, (int)(y+2*startBt.Height));

		// ボタン動作
		startBt.Click += new EventHandler(startBt_Click);
		exitBt.Click += new EventHandler(exitBt_Click);

		// マウスClick動作
		this.MouseClick += new MouseEventHandler(mouseClick);

		TitleDraw();
	}

	// タイトル表示
	public void TitleDraw()
	{
		// 背景
		this.BackgroundImage = Image.FromFile("kojiki_memu_back.bmp");
		player = new System.Media.SoundPlayer("title_kari.wav");

		player.PlayLooping();
		startBt.Parent = this;
		exitBt.Parent = this;

		flag = false;
	}

//======================ボタン================================
	public void startBt_Click(Object sender, EventArgs e)
	{
		this.BackgroundImage = Image.FromFile("ana.bmp");

		// ボタン消去
		this.Controls.Remove(startBt);
		this.Controls.Remove(exitBt);

		player.Stop();
        player.Dispose();
        player = null;

		flag = true;
	}

	public void exitBt_Click(Object sender, EventArgs e)
	{
		Application.Exit();
	}

//=======================マウス================================
	public void mouseClick(object sender, MouseEventArgs e) {
        string msg = "タイトルへ戻りますか？";	// クライアント座標
        if(e.Button == MouseButtons.Right)
        {
        	if(flag)
			{
				DialogResult result = MessageBox.Show(msg, "メニュー", MessageBoxButtons.YesNo);
				if(result == DialogResult.Yes)
				{
					TitleDraw();
				}
			}
		}
    }
}