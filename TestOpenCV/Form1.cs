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
using System.IO;
namespace TestOpenCV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //Test();
        }
        Image<Bgr, byte> img;
        Image<Gray, byte> Grayimg;
        PictureBox[] pict;
      
        private void Test(Image<Bgr, byte> img, Image<Gray, byte> Grayimg, bool save=false)
        {

            //如果支持用显卡,则用显卡运算
            CvInvoke.UseOpenCL = CvInvoke.HaveOpenCLCompatibleGpuDevice;

            //构建级联分类器,利用已经训练好的数据,识别人脸
            //var face = new CascadeClassifier("haarcascade_frontalface_alt.xml");
            var face = new CascadeClassifier("lbpcascade_animeface.xml");


            //加载要识别的图片
            //var img = new Image<Bgr, byte>("0.png");
            //var img2 = new Image<Gray, byte>(img.ToBitmap());

            //把图片从彩色转灰度
            CvInvoke.CvtColor(img, Grayimg, ColorConversion.Bgr2Gray);

            //亮度增强
            CvInvoke.EqualizeHist(Grayimg, Grayimg);

            //在这一步就已经识别出来了,返回的是人脸所在的位置和大小
            var facesDetected = face.DetectMultiScale(Grayimg, 1.1, 10, new Size(50, 50));

            //循环把人脸部分切出来并保存
           // CutandSave(facesDetected);
            ShowCut(facesDetected);
            CutandSave(facesDetected);
            MessageBox.Show($"{facesDetected.Count()}个");

            //释放资源退出
            img.Dispose();
            Grayimg.Dispose();
            face.Dispose();

            return;
        }

        private void CutandSave(Rectangle[] facesDetected)
        {
            int count = 0;
            var b = img.ToBitmap();
            pict = new PictureBox[facesDetected.Count()];
            foreach (var item in facesDetected)
            {
                count++;
                var bmpOut = new Bitmap(item.Width, item.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var g = Graphics.FromImage(bmpOut);
                g.DrawImage(b, new Rectangle(0, 0, item.Width, item.Height), new Rectangle(item.X, item.Y, item.Width, item.Height), GraphicsUnit.Pixel);
                g.Dispose();
                generatorPictureBox(count, bmpOut);
                //bmpOut.Save($"{count}.png", System.Drawing.Imaging.ImageFormat.Png);
               // bmpOut.Dispose();
            }
            b.Dispose();
        }

        private void ShowCut(Rectangle[] facesDetected)
        {
            Image rectg = img.ToBitmap();
            foreach (var item in facesDetected)
            {
                var bmpRect = new Bitmap(item.Width, item.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var g = Graphics.FromImage(bmpRect);
                rectg = DrawRectangle(rectg, item.X, item.Y, item.Width, item.Height);
            }
            pictureBox1.Image = rectg;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog oi = new OpenFileDialog
            {
                //oi.InitialDirectory = "c:\\";
                Filter = "图片(*.jpg,*.jpeg,*.gif,*.bmp,*.png) | *.jpg;*.jpeg;*.gif;*.bmp;*.png| 所有文件(*.*) | *.*",
                RestoreDirectory = true,
                FilterIndex = 1
            };
            if (oi.ShowDialog() == DialogResult.OK)
            {
                var filename = oi.FileName;

                Image image;
                try
                {
                    image = Image.FromFile(filename);
                }
                catch
                {
                    MessageBox.Show("未能加载图片,可能时不正确的格式", "错误的预期", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                pictureBox2.Image = image;
                img = new Image<Bgr, byte>(filename);
                Grayimg = new Image<Gray, byte>(img.ToBitmap());
            }
        }

        /// <summary>
        /// 画矩形
        /// </summary>
        /// <param name="x">左上角的坐标</param>
        /// <param name="y">右下角的坐标</param>
        private Image DrawRectangle(Image image, int x, int y, int width, int height)
        {
            var g = Graphics.FromImage(image);
            var pen = new Pen(Color.Red);
            g.DrawRectangle(pen, x,y,width,height);
            return image;
        }

        public void generatorPictureBox(int count,Image bmpOut)
        {

            pict[count - 1] = new System.Windows.Forms.PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(50, 50),//设置图片大小
                BorderStyle = BorderStyle.None,//取消边框
                Image = bmpOut,
                Location = new Point(10 + (count - 1) * 60, 5)//设置图片位置  竖向排列
            };
            splitContainer2.Panel2.Controls.Add(pict[count - 1]); //添加picturebox

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Test(img,Grayimg);
        }

        public class GrayImage
        {
            public GrayImage ()
            {
            }
            /// <summary>
            /// 批量灰度化
            /// </summary>
            public void ImagesToGray(List<string> path)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var img = new Image<Bgr, byte>(path[i]);
                    var Grayimg = new Image<Gray, byte>(img.ToBitmap());
                    CvInvoke.CvtColor(img, Grayimg, ColorConversion.Bgr2Gray);
                    Grayimg.Save(path[i]);
                    Grayimg.Dispose();
                }
            }
        }

        private void btn_ToGray_Click(object sender, EventArgs e)
        {
            GrayImage gi = new GrayImage();
            ///Dictionary<string, Image<Bgr, byte>> BgrImageDic = new Dictionary<string, Image<Bgr, byte>>();
            List<string> imgpaths = new List<string>();
            string path = FolderPath();
            var files = Directory.GetFiles(path, "*.jpg");
            for (int i = 0; i < files.Count(); i++)
            {
                var imgpath = files[i];
                //img = new Image<Bgr, byte>(imgpath);
                imgpaths.Add(imgpath);
            }
            gi.ImagesToGray(imgpaths);
            MessageBox.Show("转化完成");
        }

        /// <summary>
        /// 获取文件夹路径
        /// </summary>
        /// <param name="hint"></param>
        /// <returns></returns>
        private string FolderPath()
        {
            string Path = "";
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
             //  Description = hint
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return "";
                }
                Path = dialog.SelectedPath;
            }
            return Path;
        }
    }
}


