using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine
{

    // an extra class to help MapMatrix with random map generation
    // contains a number of predefined map shapes

    [Serializable]
    class BlockStructure
    {
        static Random rnd = new Random();
        int Width;
        int Height;
        int[,] StructureMatrix;
        int[,] ParentMatrix;
        int StructureNumber = rnd.Next(0, 6);   // choose shape pattern
        int ObstacleNumber = rnd.Next(-1, 0);    
        public BlockStructure(int width, int height)
        {
            Width = width;
            Height = height;
            StructureMatrix = new int[Width,Height];
            // creating matrix with all passable places
            for(int x = 0; x<Width; x++)
            {
                for(int y = 0; y<Height; y++)
                {
                    StructureMatrix[x, y] = 1;
                }
            }
            // random structure, random obstacle (stones turned off)
            int o = ObstacleNumber;
            //shapes
            //L
            int d = StructureNumber;
            if (d == 1)
            {
                for (int x = 0; x < Width; x++)
                {
                    StructureMatrix[x, 0] = o;
                }
                for (int y = 1; y < Height; y++)
                {
                    StructureMatrix[0, y] = o;
                }
            }
            //U
            else if (d == 2)
            {
                for (int x = 0; x < Width; x++)
                {
                    StructureMatrix[x, 0] = -1;
                    StructureMatrix[x, Height - 1] = o;
                }
                for (int y = 1; y < Height; y++)
                {
                    StructureMatrix[0, y] = o;
                }
            }
            //T
            else if (d == 3)
            {
                for (int x = 0; x < Width; x++)
                {
                    StructureMatrix[x, 0] = o;
                }
                double e = Width / 2;
                if (Width % 2 != 0)
                {
                    for (int x = 0; x < Height; x++)
                    {
                        StructureMatrix[Convert.ToInt32(Math.Ceiling(e)), x] = o;
                    }
                }
            }
            //X
            else if (d == 4)
            {
                int y = 0;
                for (int x = 0; x < Width; x++)
                {
                    if (y < Height)
                    {
                        StructureMatrix[x, y] = o;
                        StructureMatrix[x, Height - 1 - y] = o;
                    }
                    y++;
                }
            }
            //W
            else if(d==5)
            {
                int y = 0;
                for (int x = 0; x < Width; x++)
                {
                    if (y < Height)
                    {
                        StructureMatrix[x, y] = o;
                        if (x < Width - 1) StructureMatrix[x + 1, y] = o;
                    }
                    y++;
                }
            }
            // random shape
            else
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    StructureMatrix[x, y] = rnd.Next(-1, 2);
                    if (StructureMatrix[x, y] == 0) StructureMatrix[x, y] = 1;
                }
            }
        }

        //List:
        // L shape
        // T shape
        // U shape
        // X shape
        // W shape
        // random shape

        //converting block to the map
        public void LoadStructure(int[,] Matrix, int x, int y)
        {
            int saved_y = y;
            ParentMatrix = Matrix;

            for (int m = 0; m < Width; m++)
            {
                for (int n = 0; n < Height; n++)
                {
                    ParentMatrix[x, y] = StructureMatrix[m, n];
                    y += 1;
                }
                y = saved_y;
                x += 1;
            }
        }
    }
}
