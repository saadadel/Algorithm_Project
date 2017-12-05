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
        private minHeap redFrequency;
        private Dictionary<byte, string> redCode;

        private minHeap blueFrequency;
        private Dictionary<byte, string> blueCode;

        private minHeap greenFrequency;
        private Dictionary<byte, string> greenCode;

        private void computeFrequencies(RGBPixel[,] arr, ref Dictionary<byte, int> temp1, ref Dictionary<byte, int> temp2, ref Dictionary<byte, int> temp3)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (temp1.ContainsKey(arr[i, j].red))
                        temp1[arr[i, j].red]++;
                    else
                        temp1.Add(arr[i, j].red, 1);

                    if (temp2.ContainsKey(arr[i, j].blue))
                        temp2[arr[i, j].blue]++;
                    else
                        temp2.Add(arr[i, j].blue, 1);

                    if (temp3.ContainsKey(arr[i, j].green))
                        temp3[arr[i, j].green]++;
                    else
                        temp3.Add(arr[i, j].green, 1);
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
            Dictionary<byte, int> temp1 = new Dictionary<byte, int>();
            Dictionary<byte, int> temp2 = new Dictionary<byte, int>();
            Dictionary<byte, int> temp3 = new Dictionary<byte, int>();
            computeFrequencies(arr, ref temp1, ref temp2, ref temp3);


            redFrequency = new minHeap();
            blueFrequency = new minHeap();
            greenFrequency = new minHeap();
            foreach (var item in temp1)
            {
                node newNode = new node(item.Key, item.Value);
                redFrequency.add(newNode);
            }
            foreach (var item in temp2)
            {
                node newNode = new node(item.Key, item.Value);
                blueFrequency.add(newNode);
            }
            foreach (var item in temp3)
            {
                node newNode = new node(item.Key, item.Value);
                greenFrequency.add(newNode);
            }
            buildTree(ref redFrequency);
            buildTree(ref blueFrequency);
            buildTree(ref greenFrequency);

            redCode = new Dictionary<byte, string>();
            blueCode = new Dictionary<byte, string>();
            greenCode = new Dictionary<byte, string>();
            getBinaryCode(redFrequency.getRoot(), "", ref redCode);
            getBinaryCode(blueFrequency.getRoot(), "", ref blueCode);
            getBinaryCode(greenFrequency.getRoot(), "", ref greenCode);
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
