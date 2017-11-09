using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    enum DrawType { Vehicles, Concentrations, Predict, PredictRo};
    public partial class Visual : Form
    {
        DrawType drawType = DrawType.Predict;
        Dictionary<long, LocalVehicle> vehicles = new Dictionary<long, LocalVehicle>();
        PredictionWorld prediction;
        public Visual()
        {
            InitializeComponent();
        }

        public void UpdateWorld(ref World world, ref Game game, long myPlayerID, ref Move m)
        {
            Vehicle[] news = world.NewVehicles;

            for (int i = 0; i < news.Length; i++)
            {
                vehicles.Add(news[i].Id, new LocalVehicle(ref news[i]));
            }

            if (prediction == null)
            {
                prediction = new PredictionWorld(ref game, world.TerrainByCellXY, world.WeatherByCellXY, vehicles);
            }
            prediction.UpdateMove(m, ref game, myPlayerID);

            VehicleUpdate[] updates = world.VehicleUpdates;
            for (int i = 0; i < updates.Length; i++)
            {
                vehicles[updates[i].Id].Update(ref updates[i]);
                if (updates[i].Durability == 0)
                    vehicles.Remove(updates[i].Id);
            }

            switch (drawType)
            {
                case DrawType.Vehicles:
                    DrawV(ref world, ref game, myPlayerID);
                    break;
                case DrawType.Concentrations:
                    DrawRo(ref world, ref game, myPlayerID);
                    break;
                case DrawType.Predict:
                    DrawPredic(ref world, ref game, myPlayerID);
                    break;
                case DrawType.PredictRo:
                    DrawPredicRo(ref world, ref game, myPlayerID);
                    break;
            }

        }

        /// <summary>
        /// Рисует машины и типы земли
        /// </summary>
        /// <param name="world"></param>
        /// <param name="game"></param>
        /// <param name="myPlayerID"></param>
        public void DrawV(ref World world, ref Game game, long myPlayerID)
        {
            Bitmap map = new Bitmap((int)world.Width, (int)world.Height);
            Graphics gr = Graphics.FromImage(map);

            DrawTerrains(ref world, ref game, myPlayerID, ref gr);
            DrawVehicles(ref world, ref game, myPlayerID, ref gr);

            pictureBox1.Image = map;
            
            Update();
        }

        delegate void OneAction();


        /// <summary>
        /// Рисует машины
        /// </summary>
        /// <param name="world"></param>
        /// <param name="game"></param>
        /// <param name="myPlayerID"></param>
        /// <param name="gr"></param>
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

        /// <summary>
        /// Рисует типы земли
        /// </summary>
        /// <param name="world"></param>
        /// <param name="game"></param>
        /// <param name="myPlayerID"></param>
        /// <param name="gr"></param>
        void DrawTerrains(ref World world, ref Game game, long myPlayerID, ref Graphics gr)
        {
            int width = world.TerrainByCellXY.Length;
            int height = world.TerrainByCellXY[0].Length;

            int step = 32;

            TerrainType[][] terrains = world.TerrainByCellXY;

            Brush swamp = Brushes.Gray;
            Brush forest = Brushes.DarkOliveGreen;


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Brush draw = null;
                    switch (terrains[x][y])
                    {
                        case TerrainType.Forest:
                            draw = forest;
                            break;
                        case TerrainType.Swamp:
                            draw = swamp;
                            break;
                    }

                    if (terrains[x][y] != TerrainType.Plain)
                    {
                        gr.FillRectangle(draw, x * step, y * step, step, step);
                    }
                }
            }
        }

        void DrawRo(ref World world, ref Game game, long myPlayerID)
        {
            Bitmap map = new Bitmap((int)world.Width, (int)world.Height);
            Graphics gr = Graphics.FromImage(map);

            int[,] data = RoCalculation.Calc(ref vehicles);
            RoCalculation.DrawOneRo(ref gr, data, Color.Green, 20);

            pictureBox1.Image = map;
            Update();
        }

        void DrawPredic(ref World world, ref Game game, long myPlayerID)
        {
            Bitmap map = new Bitmap((int)world.Width, (int)world.Height);
            Graphics gr = Graphics.FromImage(map);

            prediction.Predict();
            DrawTerrains(ref world, ref game, myPlayerID, ref gr);
            DrawVehiclesColor(ref world, ref game, myPlayerID, ref gr, prediction.ground_vehicles, Brushes.Red);
            DrawVehiclesColor(ref world, ref game, myPlayerID, ref gr, prediction.air_vehicles, Brushes.Red);
            DrawVehiclesColor(ref world, ref game, myPlayerID, ref gr, vehicles, Brushes.Black);

            pictureBox1.Image = map;
            Update();
        }

        void DrawPredicRo(ref World world, ref Game game, long myPlayerID)
        {
            Bitmap map = new Bitmap((int)world.Width, (int)world.Height);
            Graphics gr = Graphics.FromImage(map);

            prediction.Predict();

            int[,] data = RoCalculation.Calc(ref vehicles);
            RoCalculation.DrawOneRo(ref gr, data, Color.Green, 20);
            RoCalculation.DrawOneRo(ref gr, prediction.VehRo(), Color.Red, 4);

            //DrawTerrains(ref world, ref game, myPlayerID, ref gr);
            //DrawVehiclesColor(ref world, ref game, myPlayerID, ref gr, prediction.ground_vehicles, Brushes.Red);
            //DrawVehiclesColor(ref world, ref game, myPlayerID, ref gr, prediction.air_vehicles, Brushes.Red);
            //DrawVehiclesColor(ref world, ref game, myPlayerID, ref gr, vehicles, Brushes.Black);

            pictureBox1.Image = map;
            Update();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            drawType = DrawType.Concentrations;
        }

        void DrawVehiclesColor(ref World world, ref Game game, long myPlayerID, ref Graphics gr, Dictionary<long,LocalVehicle> vehicles, Brush color)
        {
            float radius = (float)game.VehicleRadius;
            foreach (KeyValuePair<long, LocalVehicle> pair in vehicles)
            {
                LocalVehicle veh = pair.Value;

                gr.FillEllipse(color, (float)(veh.X - radius), (float)(veh.Y - radius), radius, radius);
            }
        }
    }
}
