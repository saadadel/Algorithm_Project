using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
            int x = Convert.ToInt32(initialsed, 2);
            int len = initialsed.Length;
             ImageMatrix = ImageOperations.incrept(ImageMatrix, ref x, len, pos);
             huffman h = new huffman(ImageMatrix);
             Dictionary<byte, string> red = h.getRedCode();
             Dictionary<byte, string> blue = h.getBlueCode();
             Dictionary<byte, string> green = h.getGreenCode();
             ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

       
       
    }
}
