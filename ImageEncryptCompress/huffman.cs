using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace ImageQuantization
{
    public class huffman
    {
        private minHeap redHeap;
        private Dictionary<byte, string> redCode;
        private int[] redFrequencies;
        private string redTree;

        private minHeap blueHeap;
        private Dictionary<byte, string> blueCode;
        private int[] blueFrequencies;
        private string blueTree;

        private minHeap greenHeap;
        private Dictionary<byte, string> greenCode;
        private int[] greenFrequencies;
        private string greenTree;

        private void getBinaryCode(node x, string y, ref Dictionary<byte, string> byteCode,ref string stringTree)
        {
            if (x.left == null && x.right == null)
            {
                byteCode.Add(x.value, y);
                stringTree += "1" + Convert.ToString(x.value, 2).PadLeft(8, '0');
                return;
            }
            stringTree += "0";
            getBinaryCode(x.left, y + "0", ref byteCode,ref stringTree);
            getBinaryCode(x.right, y + "1", ref byteCode,ref stringTree);
        }


        private void buildTree(ref minHeap temp)
        {
            node temp1, temp2, newNode;
            while (temp.count() > 1)  
            {
                temp1 = temp.extractMin();   
                temp2 = temp.extractMin();  
                newNode = new node(0, temp1.frequency + temp2.frequency, temp2, temp1); // 2(N-1)
                temp.add(newNode);            
            }
        }
                

        private void computeFrequencies(RGBPixel[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)  
            {
                for (int j = 0; j < arr.GetLength(1); j++)   
                {
                    redFrequencies[arr[i, j].red]++;
                    blueFrequencies[arr[i, j].blue]++;
                    greenFrequencies[arr[i, j].green]++;
                }
            }
        }

 
          public huffman(RGBPixel[,] arr)
        {
            redHeap = new minHeap();
            blueHeap = new minHeap();
            greenHeap = new minHeap();
            redCode = new Dictionary<byte, string>(256);
            blueCode = new Dictionary<byte, string>(256);
            greenCode = new Dictionary<byte, string>(256);
            redFrequencies = new int[256];
            greenFrequencies = new int[256];
            blueFrequencies = new int[256];
            redTree = "";
            blueTree = "";
            greenTree = "";
            computeFrequencies(arr); 
        }

          public Dictionary<byte, string> getRedCode()
        {
            for (int i = 0; i < 256;i++)
            {
                if (redFrequencies[i] != 0)
                {
                    node newNode = new node((byte)i, redFrequencies[i]);
                    redHeap.add(newNode);
                }
            }
            buildTree(ref redHeap); 
            getBinaryCode(redHeap.getRoot(), "", ref redCode,ref redTree); 
            return redCode;
        }
      
        public Dictionary<byte, string> getBlueCode()
        {
            for (int i = 0; i < 256;i++)
            {
                if (blueFrequencies[i] != 0)
                {
                    node newNode = new node((byte)i, blueFrequencies[i]);
                    blueHeap.add(newNode);
                }
            }
            buildTree(ref blueHeap);
            getBinaryCode(blueHeap.getRoot(), "", ref blueCode,ref blueTree); 
            return blueCode;
        }
 
        public Dictionary<byte, string> getGreenCode()
        {
            for (int i = 0; i < 256;i++)
            {
                if (greenFrequencies[i] != 0)
                {
                    node newNode = new node((byte)i, greenFrequencies[i]);
                    greenHeap.add(newNode);
                }
            }
            buildTree(ref greenHeap);  
            getBinaryCode(greenHeap.getRoot(), "", ref greenCode,ref greenTree);   
             return greenCode;
        }
        public void writeHeader(string path, ref RGBPixel[,] arr,int seed,int tap)
        {
            
             int width = arr.GetLength(1);
             int height = arr.GetLength(0);
            
            int redPad = 8 - redTree.Length % 8;
            if (redPad != 8)
                redTree = redTree.PadLeft(redPad + redTree.Length, '0');
            int redBytesCount = redTree.Length / 8;
            byte[] redBytes = new byte[redBytesCount];
            for (int i = 0; i < redBytesCount; i++)
            {
                redBytes[i] = Convert.ToByte(redTree.Substring(i * 8, 8), 2);
            }

            int bluePad = 8 - blueTree.Length % 8;
            if (bluePad != 8)
                blueTree = blueTree.PadLeft(bluePad + blueTree.Length, '0');
            int blueBytesCount = blueTree.Length / 8;
            byte[] blueBytes = new byte[blueBytesCount];
            for (int i = 0; i < blueBytesCount; i++)
            {
                blueBytes[i] = Convert.ToByte(blueTree.Substring(i * 8, 8), 2);
            }

            int greenPad = 8 - greenTree.Length % 8;
            if (greenPad != 8)
                greenTree = greenTree.PadLeft(greenPad + greenTree.Length, '0');
            int greenBytesCount = greenTree.Length / 8;
            byte[] greenBytes = new byte[greenBytesCount];
            for (int i = 0; i < greenBytesCount; i++)
            {
                greenBytes[i] = Convert.ToByte(greenTree.Substring(i * 8, 8), 2);
            }

            Stream stream = new FileStream(path, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(height);
            bw.Write(width);
            bw.Write(seed);
            bw.Write(tap);
          
            bw.Write(Convert.ToInt16(redBytesCount));
            bw.Write(Convert.ToByte(redPad));
            foreach (var item in redBytes)
            {
                bw.Write(item);
            }

            bw.Write(Convert.ToInt16(greenBytesCount));
            bw.Write(Convert.ToByte(greenPad));
            foreach (var item in greenBytes)
            {
                bw.Write(item);
            }

            bw.Write(Convert.ToInt16(blueBytesCount));
            bw.Write(Convert.ToByte(bluePad));
            foreach (var item in blueBytes)
            {
                bw.Write(item);
            }

            bw.Flush();
            bw.Close();
            /* string s = string.Format("red:{0}", Environment.NewLine);
            double original = ((arr.GetLength(0) * arr.GetLength(1)) * 3);
            int compressed = 0;
            foreach (var item in redCode)
            {
                compressed += (redFrequencies[item.Key] * item.Value.Length);
                s += string.Format("{0,3}:    {1},    {2},    {3}{4}", item.Key, item.Value, redFrequencies[item.Key], (redFrequencies[item.Key] * item.Value.Length), Environment.NewLine);
            }
            s += string.Format("green:{0}", Environment.NewLine);
            foreach (var item in greenCode)
            {
                compressed += (greenFrequencies[item.Key] * item.Value.Length);
                s += string.Format("{0,3}:    {1},    {2},    {3}{4}", item.Key, item.Value, greenFrequencies[item.Key], (greenFrequencies[item.Key] * item.Value.Length), Environment.NewLine);
            }
            s += string.Format("blue:{0}", Environment.NewLine);
            foreach (var item in blueCode)
            {
                compressed += (blueFrequencies[item.Key] * item.Value.Length);
                s += string.Format("{0,3}:    {1},    {2},    {3}{4}", item.Key, item.Value, blueFrequencies[item.Key], (blueFrequencies[item.Key] * item.Value.Length), Environment.NewLine);
            }
            compressed /= 8;
            original = (compressed / original) * 100;
            s += string.Format("total bytes: {0}{1}", compressed, Environment.NewLine);
            s += string.Format("compression ratio: {0}% {1}", original, Environment.NewLine);
            System.IO.File.WriteAllText(path, s);*/
        }
    }
    public class node
    {
        public byte value;
        public int frequency;
        public node left;
        public node right;

        public node(byte value, int frequency, node left = null, node right = null)
        {
            this.value = value;
            this.frequency = frequency;
            this.left = left;
            this.right = right;
        }
    }
    public class minHeap
    {
        private List<node> elements = new List<node>();

        public int count()
        {
            return elements.Count;
        }

        public node getRoot() 
        {
            return elements[0];
        }


        public void add(node item)
        {
            elements.Add(item);
            this.HeapifyUp(elements.Count - 1);
        }
        private void HeapifyUp(int index)
        {
            var parent = this.GetParent(index);
            if (parent >= 0 && elements[index].frequency < elements[parent].frequency)
            {
                var temp = elements[index];
                elements[index] = elements[parent];
                elements[parent] = temp;
                this.HeapifyUp(parent);     
            }
        }

        public node extractMin()
        {
            if (elements.Count > 0)
            {
                node item = elements[0];
                elements[0] = elements[elements.Count - 1];
                elements.RemoveAt(elements.Count - 1);
                this.HeapifyDown(0);   
                return item;
            }
            throw new InvalidOperationException("no element in heap");
        }


        private void HeapifyDown(int index)
        {
            var smallest = index;
            var left = this.GetLeft(index);
            var right = this.GetRight(index);

            if (left < this.count() && elements[left].frequency < elements[index].frequency)
            {
                smallest = left;
            }

            if (right < this.count() && elements[right].frequency < elements[smallest].frequency)
            {
                smallest = right;
            }

            if (smallest != index)
            {
                var temp = elements[index];
                elements[index] = elements[smallest];
                elements[smallest] = temp;
                this.HeapifyDown(smallest);   
            }

        }

        private int GetParent(int index)
        {
            if (index <= 0)
            {
                return -1;
            }

            return (index - 1) / 2;
        }

        private int GetLeft(int index)
        {
            return 2 * index + 1;
        }

        private int GetRight(int index)
        {
            return 2 * index + 2;
        }
    }
}
