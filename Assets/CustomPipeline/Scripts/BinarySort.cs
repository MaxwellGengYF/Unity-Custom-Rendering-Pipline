using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
public struct BinarySort<T>
{
    public struct Element
    {
        public float sign;
        public T mesh;
        public int leftValue;
        public int rightValue;
    }
    public Element[] elements;
    public T[] meshes;
    public int count;
    public BinarySort(int capacity)
    {
        count = 0;
        elements = new Element[capacity];
        meshes = new T[capacity];
    }

    public void Add(float sign, T mesh)
    {
        int currentCount = Interlocked.Increment(ref count) - 1;
        if (currentCount >= elements.Length)
        {
            lock (elements)
            {
                Element[] newElements = new Element[elements.Length * 2];
                for (int i = 0; i < elements.Length; ++i)
                {
                    newElements[i] = elements[i];
                }
                elements = newElements;
            }
        }
        elements[currentCount].sign = sign;
        elements[currentCount].mesh = mesh;
    }
    public void Clear()
    {
        count = 0;
    }
    public void Sort()
    {
        for (int i = 0; i < count; ++i)
        {
            elements[i].leftValue = -1;
            elements[i].rightValue = -1;
        }
        for (int i = 1; i < count; ++i)
        {
            int currentIndex = 0;
            STARTFIND:
            if (elements[i].sign < elements[currentIndex].sign)
            {
                if (elements[currentIndex].leftValue < 0)
                {
                    elements[currentIndex].leftValue = i;
                }
                else
                {
                    currentIndex = elements[currentIndex].leftValue;
                    goto STARTFIND;
                }
            }
            else
            {
                if (elements[currentIndex].rightValue < 0)
                {
                    elements[currentIndex].rightValue = i;
                }
                else
                {
                    currentIndex = elements[currentIndex].rightValue;
                    goto STARTFIND;
                }
            }
        }
    }
    public void GetSorted()
    {
        if (count <= 0) return;
        int start = 0;
        Iterate(0, ref start);
    }
    private void Iterate(int i, ref int targetLength)
    {
        int leftValue = elements[i].leftValue;
        if (leftValue >= 0)
        {
            Iterate(leftValue, ref targetLength);
        }
        meshes[targetLength] = elements[i].mesh;
        targetLength++;
        int rightValue = elements[i].rightValue;
        if (rightValue >= 0)
        {
            Iterate(rightValue, ref targetLength);
        }
    }
}
