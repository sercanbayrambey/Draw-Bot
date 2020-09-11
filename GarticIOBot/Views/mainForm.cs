using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GarticIOBot
{
    public partial class frmMain : Form
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        const int MYACTION_HOTKEY_ID = 1;

        private const int OFFSET_X = 500;
        private const int OFFSET_Y = 100;
        private const int DRAWING_SPEED = 1;
        private bool Status = false;
        public frmMain()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, 0, (int)Keys.Q);
            this.KeyPreview = true;
        }

       

        private async void StartDrawing()
        {
            Status = true;
            this.WindowState = FormWindowState.Minimized;
            Thread.Sleep(4000);
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            bmp = ResizeImage(bmp, 300, 300);
            Point cursorCurentPosition = Cursor.Position;
            Random rnd = new Random();
            for (int i = 0; i < bmp.Height; i++)
            {

                for (int j = 0; j < bmp.Width; j++)
                {
                    if (!Status)
                        return;
                    if (bmp.GetPixel(j, i).R < 50 && bmp.GetPixel(j, i).G < 50 && bmp.GetPixel(j, i).B < 50)
                    {
                        if (rnd.Next(0,5) != 3)
                            continue;
                        Thread.Sleep(1);
                        await Task.Run(() => MoveCursor(j + cursorCurentPosition.X, i + cursorCurentPosition.Y));
                        await Task.Run(() => DoMouseClick(j + cursorCurentPosition.X, i + cursorCurentPosition.Y));
                    }
                }
            }
        }

        private async Task MoveCursor(int x, int y )
        {
            this.Invoke((MethodInvoker)delegate()
            {
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(x, y);
                Cursor.Clip = new Rectangle(this.Location, this.Size);
            });
           
            
        }

        public async Task DoMouseClick(int x,int y)
        {
            uint X = (uint)x;
            uint Y = (uint)y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                Status = false;
            }
            base.WndProc(ref m);
        }


        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        public void OpenSelectImageDialog()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(open.FileName);
            }
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            OpenSelectImageDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartDrawing();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
