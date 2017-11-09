using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    /// <summary>
    /// Класс предсказания перемещения техники
    /// </summary>
    class PredictionWorld
    {
        public Dictionary<long, LocalVehicle> vehicles = new Dictionary<long, LocalVehicle>();
        public double[,] speedMap;
        int count;
        int step;

        public PredictionWorld(ref Game game, TerrainType[][] map,Dictionary<long,LocalVehicle> input)
        {
            foreach(KeyValuePair<long,LocalVehicle> pair in input)
            {
                LocalVehicle v = pair.Value;
                vehicles.Add(pair.Key,new LocalVehicle(ref v));
            }

            speedMap = new double[map.Length, map.Length];
            count = map.Length;
            step = 1024 / count;

            for(int x = 0;x < count;x++)
            {
                for(int y = 0;y < count;y++)
                {
                    switch(map[x][y])
                    {
                        case TerrainType.Plain:
                            speedMap[x, y] = game.PlainTerrainSpeedFactor;
                            break;
                        case TerrainType.Swamp:
                            speedMap[x, y] = game.SwampTerrainSpeedFactor;
                            break;
                        case TerrainType.Forest:
                            speedMap[x, y] = game.ForestTerrainSpeedFactor;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Предсказание перемещения техники на одиин тик. Сейчас без учета колизий
        /// </summary>
        public void Predict()
        {
            foreach (KeyValuePair<long, LocalVehicle> pair in vehicles)
            {
                LocalVehicle veh = pair.Value;
                if (veh.X < 1024 && veh.Y < 1024)
                {
                    double factor = speedMap[(int)(veh.X / step), (int)(veh.Y / step)];
                    veh.X += veh.XSpeed * factor;
                    veh.Y += veh.YSpeed * factor;
                }
            }
        }

        List<LocalVehicle> selected = new List<LocalVehicle>();

        public void UpdateMove(Move m, ref Game game)
        {
            switch(m.Action)
            {
                case ActionType.ClearAndSelect:
                    ClearAndSelect(m.Left, m.Right, m.Top, m.Bottom);
                    break;
                case ActionType.Move:
                    Move(m.X, m.Y, ref game);
                    break;
            }
        }

        void ClearAndSelect(double left, double right, double top, double bottom)
        {
            selected.Clear();
            foreach (KeyValuePair<long, LocalVehicle> pair in vehicles)
            {
                LocalVehicle v = pair.Value;
                if (v.X >= left && v.X <= right && v.Y <= bottom && v.Y >= top)
                    selected.Add(v);
            }
        }

        void Move(double X, double Y,ref Game game)
        {
            double abs = Math.Sqrt(X * X + Y * Y);

            Dictionary<VehicleType, double> xspeed = new Dictionary<VehicleType, double>();
            Dictionary<VehicleType, double> yspeed = new Dictionary<VehicleType, double>();

            xspeed.Add(VehicleType.Tank, X * game.TankSpeed / abs);
            xspeed.Add(VehicleType.Ifv, X * game.IfvSpeed / abs);
            xspeed.Add(VehicleType.Arrv, X * game.ArrvSpeed / abs);
            xspeed.Add(VehicleType.Fighter, X * game.FighterSpeed / abs);
            xspeed.Add(VehicleType.Helicopter, X * game.HelicopterSpeed / abs);

            yspeed.Add(VehicleType.Tank, Y * game.TankSpeed / abs);
            yspeed.Add(VehicleType.Ifv, Y * game.IfvSpeed / abs);
            yspeed.Add(VehicleType.Arrv, Y * game.ArrvSpeed / abs);
            yspeed.Add(VehicleType.Fighter, Y * game.FighterSpeed / abs);
            yspeed.Add(VehicleType.Helicopter, Y * game.HelicopterSpeed / abs);

            foreach (KeyValuePair<long, LocalVehicle> pair in vehicles)
            {
                LocalVehicle v = pair.Value;
                v.XSpeed = xspeed[v.type];
                v.YSpeed = yspeed[v.type];
            }
        }
    }
}
