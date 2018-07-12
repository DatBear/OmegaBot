using System;
using System.Linq;
using BattleNet.Connections;
using System.Threading;
using BattleNet.Logging;

namespace BattleNet
{
    class MapPoint
    {
        public Char Character;
        public ConsoleColor Color;
        public MapPoint(Char chara, ConsoleColor color)
        {
            Character = chara;
            Color = color;
        }

        public static implicit operator MapPoint(Char chara)
        {
            return new MapPoint(chara, ConsoleColor.White);
        }
    }
    class AsciiMap
    {
        
        private MapPoint[,] _map;
        private static UInt16 width = 80;
        private static UInt16 height = 80;
        private GameData _gameData;
        D2gsConnection _connection;
        private UInt16 _x;
        private UInt16 _y;
        public AsciiMap(GameData gameData, D2gsConnection connection)
        {
            _gameData = gameData;
            _connection = connection;

            _map = new MapPoint[width,height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _map[x, y] = ' ';
                }
            }
        }

        public void ThreadFunction()
        {
            Logger.LogToConsole = false;
            while (_connection.Socket.Connected)
            {
                PopulateMap();
                DrawScreen();
                Thread.Sleep(50);
            }
            Logger.LogToConsole = true;
        }

        public void PopulateMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _map[x, y] = ' ';
                }
            }

            _x = (_gameData.Me.Location.X);
            _y = (_gameData.Me.Location.Y);


            foreach (NpcEntity npc in _gameData.Npcs.Values)
            {
                if (Math.Abs(npc.Location.X - _x) < 40)
                {
                    if (Math.Abs(npc.Location.Y - _y) < 40)
                    {
                        if(npc.SuperUnique && npc.Life > 0)
                            _map[40 + npc.Location.X - _x, 40 + npc.Location.Y - _y] 
                                = new MapPoint('S', ConsoleColor.DarkRed);
                        else if (npc.IsMinion && npc.Life > 0)
                            _map[40 + npc.Location.X - _x, 40 + npc.Location.Y - _y] 
                                = new MapPoint('M',ConsoleColor.DarkRed);
                        else if (npc.Life > 0)
                            _map[40 + npc.Location.X - _x, 40 + npc.Location.Y - _y] 
                                = new MapPoint('m', ConsoleColor.DarkYellow);
                        else
                            _map[40 + npc.Location.X - _x, 40 + npc.Location.Y - _y] 
                                = '.';
                    }
                }
            }
            foreach (WorldObject obj in _gameData.WorldObjects.Values)
            {
                int x = (obj.Location.X - _x);
                int y = (obj.Location.Y - _y);
                if (Math.Abs(obj.Location.X - _x) < 40)
                {
                    if (Math.Abs(obj.Location.Y - _y) < 40)
                    {
                        if (obj.Type == 0x01ad)
                        {
                            _map[40 + x, 40 + y] = new MapPoint('W', ConsoleColor.Blue);
                        }
                        else
                        {
                            _map[40 + x, 40 + y] = new MapPoint('*', ConsoleColor.DarkGray);
                        }
                    }
                }
            }
            if (_gameData.RedPortal != null)
            {
                if (Math.Abs(_gameData.RedPortal.Location.X - _x) < 40
                    && Math.Abs(_gameData.RedPortal.Location.Y - _y) < 40)
                {
                    _map[40 + _gameData.RedPortal.Location.X - _x,
                          40 + _gameData.RedPortal.Location.Y - _y] = new MapPoint('R', ConsoleColor.Red);
                }
            }

            AddNpc("Malah", '♥');
            AddNpc("Qual-Kehk", 'Q');
            AddNpc("Anya", 'A');
            _map[40, 50] = new MapPoint('@',ConsoleColor.White);
        }

        public void AddNpc(String name, Char symbol)
        {
            NpcEntity npc = GetNpc(name);

            if (npc != null && npc.Initialized && Math.Abs(npc.Location.X - _x) < 40
                    && Math.Abs(npc.Location.Y - _y) < 40)
            {
                _map[40 + npc.Location.X - _x,
                          40 + npc.Location.Y - _y] = new MapPoint(symbol, ConsoleColor.Green);
            }
        }

        public NpcEntity GetNpc(String name)
        {
            NpcEntity npc = (from n in _gameData.Npcs
                             where n.Value.Name == name
                             select n).FirstOrDefault().Value;
            return npc;
        }

        public void DrawScreen()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    lock (Console.Out)
                    {
                        Console.ForegroundColor = _map[x, y].Color;
                        Console.SetCursorPosition(x, y);
                        Console.Write("{0}", _map[x, y].Character);
                        Console.SetCursorPosition(0, 82);
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
