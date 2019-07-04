using DCMLIB;
using DICOMLib;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DicomParser
{
    public partial class frmImage : Form
    {
        short[] sspixels;    //OW像素缓冲区,ss
        ushort[] uspixels;    //OW像素缓冲区,us
        byte[] obpixels;    //OB像素缓冲区,ob
        DCMDataSet items;
        double level;          //窗位
        double window;          //窗宽
        public frmImage(DCMDataSet items)
        {
            InitializeComponent();
            this.items = items;

        }
        private void frmImage_Load(object sender, EventArgs e)
        {

            this.Width = items[DicomTags.Columns].GetValue<ushort>();
            this.Height = items[DicomTags.Rows].GetValue<ushort>();
            this.window = items[DicomTags.WindowWidth].GetValue<double>();
            this.level = items[DicomTags.WindowCenter].GetValue<double>();
            tsLevel.Text = level.ToString();
            tsWindow.Text = window.ToString();
            ushort ba = items[DicomTags.BitsAllocated].GetValue<ushort>();
            ushort bs = items[DicomTags.BitsStored].GetValue<ushort>();
            ushort hb = items[DicomTags.HighBit].GetValue<ushort>();
            ushort pr = items[DicomTags.PixelRepresentation].GetValue<ushort>();
            double k = items[DicomTags.RescaleSlope].GetValue<double>();
            double b = items[DicomTags.RescaleIntercept].GetValue<double>();

            if (ba == 16)  //OW
            {
                sspixels = items[DicomTags.PixelData].GetValue<short[]>();
                //Parallel.For(0, uspixels.Length, idx =>
                for (int idx = 0; idx < Width * Height; idx++)
                {
                    short val = sspixels[idx];
                    //逐像素单元转换处理得到像素矩阵:
                    //todo:将val先左移15-hb位，然后右移16-bs位
                    val = (short)(val << (15 - hb));
                    val = (short)(val >> (16 - bs));
                    sspixels[idx] = (short)(val * k + b);  //线性变换后放回sspixels
                }
                //);
            }
            else
            {
                obpixels = items[DicomTags.PixelData].GetValue<byte[]>();
                //逐像素单元转换处理得到像素矩阵......
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            Bitmap bmp = new Bitmap(this.Width, this.Height, e.Graphics);
            for (int idx = 0; idx < Width * Height; idx++)
            //Parallel.For(0, Height * Width, idx =>
            {
                int pixel;
                if (sspixels != null) //ow ss
                    pixel = sspixels[idx];
                else if (uspixels != null) //ow us
                    pixel = uspixels[idx];
                else  //ob
                    pixel = obpixels[idx];
                //窗宽窗位变换
                //todo:小于窗口下沿置0,大于窗口上沿置255.窗口内线性变换........

                if (pixel <= level - window / 2)
                    pixel = 0;
                else if (pixel > level + window/ 2)
                    pixel = 255;
                else
                    pixel = (int)(((pixel - level) / window + 0.5) * 255);
                //显示为灰度值
                Color p = Color.FromArgb(pixel, pixel, pixel);  //灰度值
                int row = idx / Width;   //行号，y
                int col = idx % Width;  //列号，x
                bmp.SetPixel(col, row, p);
            }
            //);
            e.Graphics.DrawImage(bmp, 0, 0);
        }
   
      

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tsList_Click(object sender, EventArgs e)
        {
            if (this.tsList.Text == "宽肺窗") //宽肺窗：窗宽1500，窗位-400
            {
                this.level = -400;  
                this.window = 1500;
                this.Refresh();
            }

            if (this.tsList.Text == "骨窗") 
            {
                this.level = 1400;
                this.window = 600;
                this.Refresh();
            }

            if (this.tsList.Text == "脑窗") 
            {
                this.level = 60;
                this.window =35;
                this.Refresh();
            }

        }
    }
}
