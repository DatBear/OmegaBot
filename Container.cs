﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
 using BattleNet.Logging;

namespace BattleNet
{
    public class Container
    {
        protected String Name;
        protected Int32 Width, Height;
        public ContainerType Type { get; set; }

        public List<Item> Items;
        public List<BitArray> Fields;
        
        public Container(string name, ContainerType type, Int32 width, Int32 height)
        {
            Items = new List<Item>();
            Name = name;
            Type = type;
            Width = width;
            Height = height;

            BitArray fieldLine = new BitArray(Width, false);

            Fields = new List<BitArray>(Height);

            for (int i = 0; i < Height; i++)
            {
                Fields.Add(new BitArray(fieldLine));
            }


        }

        protected void SetItemFields(Item item, bool value)
        {
            try
            {
                for (int y = 0; y < item.Height; y++)
                    for (int x = 0; x < item.Width; x++)
                        Fields[(int)item.Y + y][(int)item.X + x] = value;
            }
            catch
            {
                Logger.Write($"Coordinate Exception, Container:{Name}, {Type}");
            }
        }

        protected Boolean RectangleIsFree(int rectangleX, int rectangleY, int rectangleWidth, int rectangleHeight)
        {
            if ((rectangleX + rectangleWidth > Width) || (rectangleY + rectangleHeight > Height))
                return false;

            for (int y = rectangleY; y < rectangleY + rectangleHeight; y++)
            {
                for (int x = rectangleX; x < rectangleX + rectangleWidth; x++)
                {
                    if (Fields[y][x])
                        return false;
                }
            }
            return true;
        }

        public void Add(Item item)
        {
            Items.Add(item);
            SetItemFields(item, true);
        }

        public void Remove(Item item)
        {
            SetItemFields(item, false);
            Items.Remove(item);
        }

        public UInt32 NumberFields()
        {
            UInt32 i = 0;
            for (Int32 y = 0; y < Height; y++)
            {
                for (Int32 x = 0; x < Width; x++)
                    if (Fields[y][x])
                        i++;
            }

            return i;
        }

        public Boolean FindFreeSpace(Item item, out Coordinate output)
        {
            UInt16 itemWidth = item.Width;
            UInt16 itemHeight = item.Height;
            for (UInt16 y = 0; y < Height; y++)
            {
                for (UInt16 x = 0; x < Width; x++)
                {
                    if (RectangleIsFree(x, y, item.Width, item.Height))
                    {
                        output = new Coordinate(x, y);
                        return true;
                    }
                }
            }
            output = null;
            return false;
        }

        public Boolean FindFreeSpace(Item item)
        {
            Coordinate output;
            return FindFreeSpace(item, out output);
        }

    }
}