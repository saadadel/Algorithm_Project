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
        

        private void getBinaryCode(node x, string y, ref Dictionary<byte, string> temp)
        {
            if (x.left == null && x.right == null)
            {
                temp.Add(x.value, y);
                return;
            }
            getBinaryCode(x.left, y + "0", ref temp);
            getBinaryCode(x.right, y + "1", ref temp);
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

 
          public huffman(RGBPixel[,] arr)
        {
            redHeap = new minHeap();
            blueHeap = new minHeap();
            greenHeap = new minHeap();
            redCode = new Dictionary<byte, string>();
            blueCode = new Dictionary<byte, string>();
            greenCode = new Dictionary<byte, string>();
            redFrequencies = new Dictionary<byte, int>();
            greenFrequencies = new Dictionary<byte, int>();
            blueFrequencies = new Dictionary<byte, int>();
            computeFrequencies(arr); 
                }

          public Dictionary<byte, string> getRedCode()
        {
              foreach (var item in redFrequencies)    
             {  
                node newNode = new node(item.Key, item.Value);
                redHeap.add(newNode);
            }
            buildTree(ref redHeap); 
            getBinaryCode(redHeap.getRoot(), "", ref redCode); 
            return redCode;
        }
      
        public Dictionary<byte, string> getBlueCode()
        {
              foreach (var item in blueFrequencies) 
            {
                node newNode = new node(item.Key, item.Value); 
                blueHeap.add(newNode);
            }
            buildTree(ref blueHeap);
            getBinaryCode(blueHeap.getRoot(), "", ref blueCode); 
            return blueCode;
        }
 
        public Dictionary<byte, string> getGreenCode()
        {
              foreach (var item in greenFrequencies)  
            {
                node newNode = new node(item.Key, item.Value);
                greenHeap.add(newNode);
            }
            buildTree(ref greenHeap);  
            getBinaryCode(greenHeap.getRoot(), "", ref greenCode);   
             return greenCode;
        }
        public void writeToFile(string path, ref RGBPixel[,] arr)
        {
            string s = string.Format("red:{0}", Environment.NewLine);
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
            System.IO.File.WriteAllText(path, s);
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
