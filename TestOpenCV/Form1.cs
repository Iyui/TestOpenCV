using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
namespace TestOpenCV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Test();
        }

        private void Test()
        {
            
            //如果支持用显卡,则用显卡运算
            CvInvoke.UseOpenCL = CvInvoke.HaveOpenCLCompatibleGpuDevice;

            //构建级联分类器,利用已经训练好的数据,识别人脸
            var face = new CascadeClassifier("haarcascade_frontalface_alt.xml");

            //加载要识别的图片
            var img = new Image<Bgr, byte>("0.png");
            var img2 = new Image<Gray, byte>(img.ToBitmap());

            //把图片从彩色转灰度
            CvInvoke.CvtColor(img, img2, ColorConversion.Bgr2Gray);

            //亮度增强
            CvInvoke.EqualizeHist(img2, img2);

            //在这一步就已经识别出来了,返回的是人脸所在的位置和大小
            var facesDetected = face.DetectMultiScale(img2, 1.1, 10, new Size(50, 50));

            //循环把人脸部分切出来并保存
            int count = 0;
            var b = img.ToBitmap();
            foreach (var item in facesDetected)
            {
                count++;
                var bmpOut = new Bitmap(item.Width, item.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var g = Graphics.FromImage(bmpOut);
                g.DrawImage(b, new Rectangle(0, 0, item.Width, item.Height), new Rectangle(item.X, item.Y, item.Width, item.Height), GraphicsUnit.Pixel);
                g.Dispose();
                bmpOut.Save($"{count}.png", System.Drawing.Imaging.ImageFormat.Png);
                bmpOut.Dispose();
            }

            //释放资源退出
            b.Dispose();
            img.Dispose();
            img2.Dispose();
            face.Dispose();

            return;
        }
    }
}
