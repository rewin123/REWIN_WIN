using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    static class RoCalculation
    {
        static int step = 20;
        public static int[,] Calc(ref Dictionary<long,LocalVehicle> dic)
        {
            int count = 1024 / step;

            int[,] map = new int[count, count];
            foreach(KeyValuePair<long,LocalVehicle> pair in dic)
            {
                LocalVehicle veh = pair.Value;
                map[(int)veh.X / step, (int)veh.Y / step]++;
            }
            return map;
        }

        public static int[,] Calc(ref Dictionary<long, LocalVehicle> dic, VehicleType type)
        {
            int count = 1024 / step;

            int[,] map = new int[count, count];
            foreach (KeyValuePair<long, LocalVehicle> pair in dic)
            {
                LocalVehicle veh = pair.Value;
                if (veh.type == type)
                {
                    map[(int)veh.X / step, (int)veh.Y / step]++;
                }
            }
            return map;
        }

        public static void DrawOneRo(ref Graphics gr, int[,] cals, Color color)
        {
            int max = cals.GetMax();
            
            int count = 1024 / 20;
            for(int x = 0;x < count;x++)
            {
                for(int y = 0;y < count;y++)
                {
                    gr.FillRectangle(new SolidBrush(Color.FromArgb(255 * cals[x, y] / max, color)), x * step, y *step, step, step);
                }
            }
        }

        public static int GetMax(this int[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);

            int max = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    max = Math.Max(max, array[x, y]);
                }
            }

            return max;
        }
    }
}
