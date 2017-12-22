using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Linq;
namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            string initialsed = initialseed.Text;
            int pos = int.Parse(tapText.Text);
            //int x = Convert.ToInt32(initialsed, 2);
            int len = initialsed.Length;
            ImageMatrix = ImageOperations.incrept(ImageMatrix, ref initialsed, len, pos);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int seed = Convert.ToInt32(initialseed.Text);
            int tap = Convert.ToInt32(tapText.Text);
            string path = textBox2.Text;
            huffman h = new huffman(ImageMatrix);
            Dictionary<byte, string> red = h.getRedCode();
            Dictionary<byte, string> blue = h.getBlueCode();
            Dictionary<byte, string> green = h.getGreenCode();
            h.writeHeader(path, ref ImageMatrix, seed, tap);
            ImageOperations.saveinbinaryfile(red, green, blue, ImageMatrix,path);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string path = textBox1.Text;
            Stream readStream = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(readStream);

            int height = br.ReadInt32();
            int width = br.ReadInt32();
            int initial = br.ReadInt32();
            int tap = br.ReadInt32();

            int redCount = br.ReadInt16();
            int redpadding = br.ReadByte();
            byte[] redBytes = br.ReadBytes(redCount);
            int greenCount = br.ReadInt16();
            int greenpadding = br.ReadByte();
            byte[] greenBytes = br.ReadBytes(greenCount);
            int blueCount = br.ReadInt16();
            int bluepadding = br.ReadByte();
            byte[] blueBytes = br.ReadBytes(blueCount);
            br.Close();
            string red = "";
            foreach (var item in redBytes)
            {
                red += Convert.ToString(item, 2).PadLeft(8, '0');
            }
            string green = "";
            foreach (var item in greenBytes)
            {
                green += Convert.ToString(item, 2).PadLeft(8, '0');
            }
            string blue = "";
            foreach (var item in blueBytes)
            {
                blue += Convert.ToString(item, 2).PadLeft(8, '0');
            }
            Dictionary<string, byte> redDic = new Dictionary<string, byte>(256);
            Dictionary<string, byte> greenDic = new Dictionary<string, byte>(256);
            Dictionary<string, byte> blueDic = new Dictionary<string, byte>(256);
            ImageOperations.getdic(ref redDic, red, ref  redpadding, "");
            ImageOperations.getdic(ref greenDic, green, ref  greenpadding, "");
            ImageOperations.getdic(ref blueDic, blue, ref  bluepadding, "");
              string res=height.ToString()+" "+width.ToString()+ " " +initial+" "+tap+System.Environment.NewLine;
              foreach (var item in redDic)
              {
                  res += string.Format("{0}:{1}{2}", item.Key, item.Value, System.Environment.NewLine);
              }
               res += "blue:"+System.Environment.NewLine;
              foreach (var item in blueDic)
              {
                  res += string.Format("{0}:{1}{2}", item.Key, item.Value, System.Environment.NewLine);
              }
               res += "green:"+System.Environment.NewLine;
              foreach (var item in greenDic)
              {
                  res += string.Format("{0}:{1}{2}", item.Key, item.Value, System.Environment.NewLine);
              }
              System.IO.File.WriteAllText("path.txt", res);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        
       
    }
}
