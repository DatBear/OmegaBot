﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace BattleNet
{
    class Container
    {
        protected String _name;
        protected Int32 _width, _height;

        public List<Item> _items;
        public List<BitArray> _fields;


        public Container(String name, Int32 width, Int32 height)
        {
            _items = new List<Item>();
            _name = name;
            _width = width;
            _height = height;

            BitArray fieldLine = new BitArray(_width, false);

            _fields = new List<BitArray>(_height);

            for (int i = 0; i < _height; i++)
            {
                _fields.Add(new BitArray(fieldLine));
            }


        }

        protected void SetItemFields(Item item, bool value)
        {
            try
            {
                for (int y = 0; y < item.height; y++)
                    for (int x = 0; x < item.width; x++)
                        _fields[(int)item.y + y][(int)item.x + x] = value;
            }
            catch
            {
                Logging.Logger.Write("Coordinate Exception....");
            }
        }

        protected Boolean RectangleIsFree(int rectangleX, int rectangleY, int rectangleWidth, int rectangleHeight)
        {
            if ((rectangleX + rectangleWidth > _width) || (rectangleY + rectangleHeight > _height))
                return false;

            for (int y = rectangleY; y < rectangleY + rectangleHeight; y++)
            {
                for (int x = rectangleX; x < rectangleX + rectangleWidth; x++)
                {
                    if (_fields[y][x])
                        return false;
                }
            }
            return true;
        }

        public void Add(Item item)
        {
            _items.Add(item);
            SetItemFields(item, true);
        }

        public void Remove(Item item)
        {
            SetItemFields(item, false);
            _items.Remove(item);
        }

        public UInt32 NumberFields()
        {
            UInt32 i = 0;
            for (Int32 y = 0; y < _height; y++)
            {
                for (Int32 x = 0; x < _width; x++)
                    if (_fields[y][x])
                        i++;
            }

            return i;
        }

        public Boolean FindFreeSpace(Item item, out Coordinate output)
        {
            UInt16 ite_width = item.width;
            UInt16 ite_height = item.height;
            for (UInt16 y = 0; y < _height; y++)
            {
                for (UInt16 x = 0; x < _width; x++)
                {
                    if (RectangleIsFree(x, y, item.width, item.height))
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