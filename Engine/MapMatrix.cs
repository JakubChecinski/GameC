using System;
using System.Collections.Generic;
using Game.Engine.Monsters.MonsterFactories;
using Game.Engine.Monsters;
using System.Windows;
using Game.Engine.Interactions;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.IO;
using Game.Engine.Interactions.InteractionFactories;

namespace Game.Engine
{
    // container class for an integer matrix that represents game map grid
    // map codes:
    // 0 - unpassable terrain (the character cannot go there)
    // 1 - normal terrain (walkable, but nothing happens)
    // 1000 - battle with a monster
    // 2000 to 2999 - portal to another map
    // 3000 and above - a custom interaction
    // -1 to -999 - unpassable terrain (just like 0 but with nicer display)

    [Serializable]
    class MapMatrix
    {
        // map parameters
        private int monsters;
        private int walls;


        // other fields and properties
        private Random rng;
        private GameSession parentSession;

        public Dictionary<int, MonsterFactory> MonDict; // key - position number on board, value - monster factory located there
        public Dictionary<int, Monster> MemorizedMonsters { get; set; } // for keeping exactly the same monster between battles 
        public Dictionary<int, Interaction> Interactions { get; private set; } // same as MonDict, but for interactions

        public int[,] Matrix { get; set; } // matrix with all board positions 
        public int Width { get; protected set; } = 25;
        public int Height { get; protected set; } = 20;
        public MapMatrix(GameSession parent, string Path)
        {
            parentSession = parent;
            Matrix = new int[Height, Width];
            List<string> positionList = new List<string>();
            List<int> finalList = new List<int>();

            Regex reg = new Regex("\t");
            foreach (string pos in File.ReadAllLines(Path))
            {
                positionList.Add(pos);
            }
            foreach (string pos in positionList)
            {
                string[] elements = reg.Split(pos);
                List<string> elementList = new List<string>();
                foreach (string element in elements)
                {
                    elementList.Add(element);
                }
                foreach (string el in elementList)
                {
                    finalList.Add(Int32.Parse(el));
                }
            }
            MemorizedMonsters = new Dictionary<int, Monster>();
            Interactions = new Dictionary<int, Interaction>();
            MonDict = new Dictionary<int, MonsterFactory>();
            int index = 0;
            for (int y = 1; y < Height - 1; y++)
            {
                for (int x = 1; x < Width; x++)
                {
                    Matrix[y, x] = finalList[index];
                    if (finalList[index] > 1000 && finalList[index] < 2000)
                    {
                        MonDict.Add(y * Width + x, Index.MonsterFactories[Matrix[y, x] - 1001]);
                    }
                    else if (finalList[index] > 3000)
                    {
                        Interactions.Add(y * Width + x, Index.GetInteractionByNumber(parentSession, finalList[index] - 3000));
                    }
                    index++;
                }
            }

        }
        public MapMatrix(GameSession parent, List<int> portals, List<Interaction> inters, int randomCode, (int, int) mapParams)
        {
            parentSession = parent;
            monsters = mapParams.Item1;
            walls = mapParams.Item2;
            Matrix = new int[Height, Width];
            rng = new Random(randomCode);
            // make map walkable
            for (int y = 1; y < Height - 1; y++)
            {
                for (int x = 1; x < Width - 1; x++)
                {
                    Matrix[y, x] = 1;
                }
            }
            // decorate map with stuff

            DecorateWithObstacles();
            DecorateWithPortals(portals);
            DecorateWithInteractions(inters);
            DecorateWithMonsters();
            // trim walls
            for (int y = 0; y < Height; y++) Matrix[y, 0] = 0;
            for (int x = 0; x < Width; x++) Matrix[0, x] = 0;
            for (int y = 0; y < Height; y++) Matrix[y, Width - 1] = 0;
            for (int x = 0; x < Width; x++) Matrix[Height - 1, x] = 0;
            // initialize 
            InitializeRandomFactoryList();
            MemorizedMonsters = new Dictionary<int, Monster>();
        }

        // fill map with monster factories
        private void InitializeRandomFactoryList()
        {
            MonDict = new Dictionary<int, MonsterFactory>();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Matrix[y, x] == 1000)
                    {
                        MonDict.Add(y * Width + x, Index.RandomMonsterFactory());
                    }
                }
            }
        }

        // produce or hint monsters
        public Monster CreateMonster(int x, int y, int playerLevel)
        {
            if (MemorizedMonsters.ContainsKey(y * Width + x) && MemorizedMonsters[y * Width + x] != null) return MemorizedMonsters[y * Width + x];
            if (MonDict.ContainsKey(y * Width + x) && MonDict[y * Width + x] != null)
            {
                return MonDict[y * Width + x].Create();
            }
            return null;
        }
        public Image HintMonsterImage(int x, int y)
        {
            if (MemorizedMonsters.ContainsKey(y * Width + x) && MemorizedMonsters[y * Width + x] != null) return MemorizedMonsters[y * Width + x].GetImage();
            if (MonDict.ContainsKey(y * Width + x) && MonDict[y * Width + x] != null)
            {
                return MonDict[y * Width + x].Hint();
            }
            return null;
        }

        // decorate map with stuff
        private void DecorateWithObstacles()
        {
            List<(int, int)> BlockCoordinates = new List<(int, int)>();
            List<BlockStructure> BlockList = new List<BlockStructure>();

            //adding randomly-sized blocks and specific coordinates, placing structures in the given places
            //for more info, go to BlockStructure.cs
            for (int i = 0; i < 15; i++)
            {
                BlockList.Add(new BlockStructure(rng.Next(3, 6), rng.Next(3, 6)));
            }
            //1st row
            BlockCoordinates.Add((2, 2));
            BlockCoordinates.Add((2, 7));
            BlockCoordinates.Add((2, 11));
            BlockCoordinates.Add((2, 15));
            BlockCoordinates.Add((2, 19));
            //2nd row
            BlockCoordinates.Add((7, 2));
            BlockCoordinates.Add((7, 7));
            BlockCoordinates.Add((7, 11));
            BlockCoordinates.Add((7, 15));
            BlockCoordinates.Add((7, 19));
            //3rd row
            BlockCoordinates.Add((14, 2));
            BlockCoordinates.Add((14, 7));
            BlockCoordinates.Add((14, 11));
            BlockCoordinates.Add((14, 15));
            BlockCoordinates.Add((14, 19));

            for (int i = 0; i < BlockList.Count; i++)
            {
                BlockList[i].LoadStructure(Matrix, BlockCoordinates[i].Item1, BlockCoordinates[i].Item2);
            }

            // making wertical and/or horizontal edges 
            int W = 0;
            //int W = rng.Next(0, 3);       // can be used to produce semi-open maps
            int y0 = 1;
            int ym = Height - 2;
            int x0 = 1;
            int xm = Width - 2;

            for (int y = y0; y <= ym; y++)
            {
                if (W != 2)
                {
                    Matrix[y, x0] = -1;
                    Matrix[y, xm] = -1;
                }
            }
            for (int x = x0; x <= xm; x++)
            {
                if (W != 1)
                {
                    Matrix[y0, x] = -1;
                    Matrix[ym, x] = -1;
                }
            }
            //making passages through map 
            for (int y = y0 + 1; y < ym; y++)
            {
                Matrix[y, 2] = 1;
                Matrix[y, 8] = 1;
                Matrix[y, 12] = 1;
                Matrix[y, 16] = 1;
                Matrix[y, 20] = 1;
                Matrix[y, 24] = 1;
            }
            for (int x = x0 + 1; x < xm; x++)
            {
                Matrix[2, x] = 1;
                Matrix[6, x] = 1;
                Matrix[13, x] = 1;
                Matrix[17, x] = 1;
            }

        }
        private void DecorateWithPortals(List<int> portals)
        {
            Random rng = new Random();
            foreach (int portal in portals)
            {
                while (true)
                {
                    int x = rng.Next(2, Width - 2);
                    int y = rng.Next(2, Height - 2);
                    if (ValidPlace(x, y))
                    {
                        Matrix[y, x] = 2000 + portal;
                        break;
                    }
                }
            }
        }
        private void DecorateWithInteractions(List<Interaction> inters)
        {
            Interactions = new Dictionary<int, Interaction>();
            Random rng = new Random();
            foreach (Interaction inter in inters)
            {
                while (true)
                {
                    int x = rng.Next(2, Width - 2);
                    int y = rng.Next(2, Height - 2);
                    if (ValidPlace(x, y) && Matrix[y, x] == 1)
                    {
                        if (Interactions.ContainsKey(Width * y + x)) continue;
                        Interactions.Add(Width * y + x, inter);
                        Matrix[y, x] = 3000 + Int32.Parse(inter.Name.Replace("interaction", ""));
                        break;
                    }
                }
            }
        }
        private void DecorateWithMonsters()
        {
            Random rng = new Random();
            for (int i = 0; i < monsters; i++)
            {
                int x = rng.Next(2, Width - 2);
                int y = rng.Next(2, Height - 2);
                if (Matrix[y, x] != 1)
                {
                    i--;
                    continue;
                }
                Matrix[y, x] = 1000;
            }
        }
        public bool ValidPlace(int x, int y)
        {
            // utility
            if (x < 1 || y < 1 || x > Width - 2 || y > Height - 2) return false;
            if (Matrix[y, x] > 2000) return false;
            if ((Matrix[y, x - 1] != 1 && Matrix[y, x + 1] != 1) && (Matrix[y - 1, x] == 1 && Matrix[y + 1, x] == 1)) return false;
            if ((Matrix[y - 1, x] != 1 && Matrix[y + 1, x] != 1) && (Matrix[y, x - 1] == 1 && Matrix[y, x + 1] == 1)) return false;
            return true;
        }

    }
}
