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
        Dictionary<byte, int> redFrequencies;
        private Dictionary<byte, string> redCode;

        private minHeap blueHeap;
        private Dictionary<byte, string> blueCode;
        Dictionary<byte, int> blueFrequencies;

        private minHeap greenHeap;
        private Dictionary<byte, string> greenCode;
        Dictionary<byte, int> greenFrequencies;

        private void computeFrequencies(RGBPixel[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (redFrequencies.ContainsKey(arr[i, j].red))
                        redFrequencies[arr[i, j].red]++;
                    else
                        redFrequencies.Add(arr[i, j].red, 1);

                    if (blueFrequencies.ContainsKey(arr[i, j].blue))
                        blueFrequencies[arr[i, j].blue]++;
                    else
                        blueFrequencies.Add(arr[i, j].blue, 1);

                    if (greenFrequencies.ContainsKey(arr[i, j].green))
                        greenFrequencies[arr[i, j].green]++;
                    else
                        greenFrequencies.Add(arr[i, j].green, 1);
                }
            }
        }
        private void buildTree(ref minHeap temp)
        {
            node temp1, temp2, newNode;
            while (temp.count() > 1)
            {
                temp1 = temp.extractMin();
                temp2 = temp.extractMin();
                newNode = new node(0, temp1.frequency + temp2.frequency, temp1, temp2);
                temp.add(newNode);
            }
        }
        private void getBinaryCode(node x, string y, ref Dictionary<byte, string> temp)
        {
            if (x.left == null && x.right == null)
            {
                temp.Add(x.value, y);
                return;
            }
            getBinaryCode(x.left, y + "1", ref temp);
            getBinaryCode(x.right, y + "0", ref temp);
        }
        public huffman(RGBPixel[,] arr)
        {
            redFrequencies = new Dictionary<byte, int>();
            greenFrequencies = new Dictionary<byte, int>();
            blueFrequencies = new Dictionary<byte, int>();
            computeFrequencies(arr);
            redHeap = new minHeap();
            blueHeap = new minHeap();
            greenHeap = new minHeap();
            foreach (var item in redFrequencies)
            {
                node newNode = new node(item.Key, item.Value);
                redHeap.add(newNode);
            }
            foreach (var item in blueFrequencies)
            {
                node newNode = new node(item.Key, item.Value);
                blueHeap.add(newNode);
            }
            foreach (var item in greenFrequencies)
            {
                node newNode = new node(item.Key, item.Value);
                greenHeap.add(newNode);
            }
            buildTree(ref redHeap);
            buildTree(ref blueHeap);
            buildTree(ref greenHeap);

            redCode = new Dictionary<byte, string>();
            blueCode = new Dictionary<byte, string>();
            greenCode = new Dictionary<byte, string>();
            getBinaryCode(redHeap.getRoot(), "", ref redCode);
            getBinaryCode(blueHeap.getRoot(), "", ref blueCode);
            getBinaryCode(greenHeap.getRoot(), "", ref greenCode);
        }

        public Dictionary<byte, string> getRedCode()
        {
            return redCode;
        }
        public Dictionary<byte, string> getBlueCode()
        {
            return blueCode;
        }
        public Dictionary<byte, string> getGreenCode()
        {
            return greenCode;
        }
        public void writeToFile(string path)
        {
            string s = string.Format("red code:{0}", Environment.NewLine);
            foreach (var item in redCode)
            {
                s += string.Format("{0}:{1} {2}",item.Key,item.Value,Environment.NewLine);
            }
            s += string.Format("green code:{0}", Environment.NewLine);
            foreach (var item in greenCode)
            {
                s += string.Format("{0}:{1} {2}", item.Key, item.Value, Environment.NewLine);
            }
            s += string.Format("blue code:{0}", Environment.NewLine);
            foreach (var item in blueCode)
            {
                s += string.Format("{0}:{1} {2}", item.Key, item.Value, Environment.NewLine);
            }
            s += string.Format("red frequencies:{0}", Environment.NewLine);
            foreach (var item in redFrequencies)
            {
                s += string.Format("{0}:{1} {2}", item.Key, item.Value, Environment.NewLine);
            }
            s += string.Format("green frequencies:{0}", Environment.NewLine);
            foreach (var item in greenFrequencies)
            {
                s += string.Format("{0}:{1} {2}", item.Key, item.Value, Environment.NewLine);
            }
            s +=string.Format( "blue frequencies:{0}",Environment.NewLine);
            foreach (var item in blueFrequencies)
            {
                s += string.Format("{0}:{1} {2}", item.Key, item.Value, Environment.NewLine);
            }
            System.IO.File.WriteAllText(path,s);
        }
    }
    class node
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
    class minHeap
    {
        private List<node> arr;

        public minHeap()
        {
            this.arr = new List<node>();
        }

        private void minHeapify(int index)
        {
            int left = (index * 2) + 1;
            int right = (index * 2) + 2;
            int smallest = index;
            int size = arr.Count;
            if (left < size && arr[left].frequency < arr[index].frequency)
            {
                smallest = left;
            }
            if (right < size && arr[right].frequency < arr[smallest].frequency)
            {
                smallest = right;
            }
            if (smallest != index)
            {
                node temp = arr[index];
                arr[index] = arr[smallest];
                arr[smallest] = temp;
                minHeapify(smallest);
            }
        }

        public node extractMin()
        {
            int size = arr.Count;
            if (size == 0)
                return null;
            node min = arr[0];
            if (size == 1)
            {
                arr.RemoveAt(0);
                return min;
            }
            else
            {
                arr[0] = arr[size - 1];
                arr.RemoveAt(size - 1);
                minHeapify(0);
                return min;
            }
        }

        public void add(node k)
        {
            arr.Add(k);
            int index = arr.Count - 1;
            while (index >= 0 && arr[(index - 1) / 2].frequency > arr[index].frequency)
            {
                node temp = arr[(index - 1) / 2];
                arr[(index - 1) / 2] = arr[index];
                arr[index] = temp;
                index = (index - 1) / 2;
            }
        }

        public int count()
        {
            return arr.Count;
        }

        public node getRoot()
        {
            return arr[0];
        }
    }
}
