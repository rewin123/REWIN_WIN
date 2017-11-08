using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    public partial class Visual : Form
    {
        Dictionary<long, LocalVehicle> vehicles = new Dictionary<long, LocalVehicle>();
        public Visual()
        {
            InitializeComponent();
        }

        public void UpdateWorld(ref World world,ref Game game, long myPlayerID)
        {
            Vehicle[] news = world.NewVehicles;

            for(int i = 0;i < news.Length;i++)
            {
                vehicles.Add(news[i].Id, new LocalVehicle(ref news[i]));
            }

            VehicleUpdate[] updates = world.VehicleUpdates;
            for(int i = 0;i < updates.Length;i++)
            {
                vehicles[updates[i].Id].Update(ref updates[i]);
                if (updates[i].Durability == 0)
                    vehicles.Remove(updates[i].Id);
            }

            Draw(ref world, ref game, myPlayerID);
        }

        public void Draw(ref World world, ref Game game, long myPlayerID)
        {
            Bitmap map = new Bitmap((int)world.Width, (int)world.Height);
            Graphics gr = Graphics.FromImage(map);

            DrawTerrains(ref world, ref game, myPlayerID, ref gr);
            DrawVehicles(ref world, ref game, myPlayerID, ref gr);

            pictureBox1.Image = map;
            Update();
        }

        void DrawVehicles(ref World world, ref Game game, long myPlayerID, ref Graphics gr)
        {
            Brush enemy = Brushes.Red;
            Brush my = Brushes.Black;
            float radius = (float)game.VehicleRadius;
            foreach (KeyValuePair<long, LocalVehicle> pair in vehicles)
            {
                LocalVehicle veh = pair.Value;

                gr.FillEllipse(veh.playerID == myPlayerID ? my : enemy, (float)(veh.X - radius), (float)(veh.Y - radius), radius, radius);
            }
        }

        void DrawTerrains(ref World world, ref Game game, long myPlayerID, ref Graphics gr)
        {
            int width = world.TerrainByCellXY.Length;
            int height = world.TerrainByCellXY[0].Length;

            int step = 32;

            TerrainType[][] terrains = world.TerrainByCellXY;

            Brush swamp = Brushes.Gray;
            Brush forest = Brushes.DarkOliveGreen;
            

            for(int x = 0;x < width;x++)
            {
                for(int y = 0;y < height;y++)
                {
                    Brush draw = null;
                    switch(terrains[x][y])
                    {
                        case TerrainType.Forest:
                            draw = forest;
                            break;
                        case TerrainType.Swamp:
                            draw = swamp;
                            break;
                    }

                    if(terrains[x][y] != TerrainType.Plain)
                    {
                        gr.FillRectangle(draw, x * step, y * step, step, step);
                    }
                }
            }
        }
    }
}
