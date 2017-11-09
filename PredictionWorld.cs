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
        public Dictionary<long, LocalVehicle> ground_vehicles = new Dictionary<long, LocalVehicle>();
        public Dictionary<long, LocalVehicle> air_vehicles = new Dictionary<long, LocalVehicle>();

        int collision_step;
        int collision_count;
        Dictionary<long, LocalVehicle>[,] ground_groups;

        public double[,] speedMapGround;
        public double[,] speedMapAir;
        int count;
        int step;

        public PredictionWorld(ref Game game, TerrainType[][] map, WeatherType[][] weather,Dictionary<long,LocalVehicle> input)
        {
            count = map.Length;
            step = 1024 / count;

            SetupVehicles(ref input);
            SetupTerrain(ref game, map);
            SetupWeather(ref game, weather);
            SetupGroundCollisionGroups(ref game);
        }

        void SetupGroundCollisionGroups(ref Game game)
        {
            collision_step = (int)(Math.Pow(Math.Log(game.VehicleRadius,2) + 1,2));
            collision_count = 1024 / collision_step;
            ground_groups = new Dictionary<long, LocalVehicle>[collision_count + 1, collision_count + 1];

            for(int x = 0;x < collision_count + 1;x++)
            {
                for(int y = 0;y < collision_count + 1;y++)
                {
                    ground_groups[x, y] = new Dictionary<long, LocalVehicle>();
                }
            }
        }

        void PutAllVehiclesInGroups()
        {
            foreach (KeyValuePair<long, LocalVehicle> pair in ground_vehicles)
            {
                LocalVehicle veh = pair.Value;
                ground_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Add(pair.Key, veh);
            }
        }



        void SetupVehicles(ref Dictionary<long, LocalVehicle> input)
        {
            
            foreach (KeyValuePair<long, LocalVehicle> pair in input)
            {
                LocalVehicle v = pair.Value;
                switch (v.type)
                {
                    case VehicleType.Arrv:
                    case VehicleType.Ifv:
                    case VehicleType.Tank:
                        ground_vehicles.Add(pair.Key, new LocalVehicle(ref v));
                        break;
                    case VehicleType.Helicopter:
                    case VehicleType.Fighter:
                        air_vehicles.Add(pair.Key, new LocalVehicle(ref v));
                        break;
                }
            }
        }

        void SetupTerrain(ref Game game,TerrainType[][] map)
        {
            speedMapGround = new double[map.Length, map.Length];
            for (int x = 0; x < count; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    switch (map[x][y])
                    {
                        case TerrainType.Plain:
                            speedMapGround[x, y] = game.PlainTerrainSpeedFactor;
                            break;
                        case TerrainType.Swamp:
                            speedMapGround[x, y] = game.SwampTerrainSpeedFactor;
                            break;
                        case TerrainType.Forest:
                            speedMapGround[x, y] = game.ForestTerrainSpeedFactor;
                            break;
                    }
                }
            }
        }

        void SetupWeather(ref Game game, WeatherType[][] weather)
        {
            speedMapAir = new double[weather.Length, weather.Length];
            for (int x = 0; x < count; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    switch (weather[x][y])
                    {
                        case WeatherType.Clear:
                            speedMapAir[x, y] = game.ClearWeatherSpeedFactor;
                            break;
                        case WeatherType.Cloud:
                            speedMapAir[x, y] = game.CloudWeatherSpeedFactor;
                            break;
                        case WeatherType.Rain:
                            speedMapAir[x, y] = game.RainWeatherSpeedFactor;
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
            foreach (KeyValuePair<long, LocalVehicle> pair in ground_vehicles)
            {
                LocalVehicle veh = pair.Value;
                if (veh.X < 1024 && veh.Y < 1024)
                {
                    ground_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Remove(pair.Key);

                    double factor = speedMapGround[(int)(veh.X / step), (int)(veh.Y / step)];
                    veh.X += veh.XSpeed * factor;
                    veh.Y += veh.YSpeed * factor;
                    ground_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Add(pair.Key, veh);
                }
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_vehicles)
            {
                LocalVehicle veh = pair.Value;
                if (veh.X < 1024 && veh.Y < 1024)
                {
                    double factor = speedMapAir[(int)(veh.X / step), (int)(veh.Y / step)];
                    veh.X += veh.XSpeed * factor;
                    veh.Y += veh.YSpeed * factor;
                }
            }
        }
        

        public int[,] VehRo()
        {
            int[,] map = new int[collision_count, collision_count];
            for (int x = 0; x < collision_count; x++)
            {
                for (int y = 0; y < collision_count; y++)
                {
                    map[x, y] = ground_groups[x, y].Count;
                }
            }
            return map;
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
            foreach (KeyValuePair<long, LocalVehicle> pair in ground_vehicles)
            {
                LocalVehicle v = pair.Value;
                if (v.X >= left && v.X <= right && v.Y <= bottom && v.Y >= top)
                    selected.Add(v);
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_vehicles)
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

            foreach (LocalVehicle pair in selected)
            {
                LocalVehicle v = pair;
                v.XSpeed = xspeed[v.type];
                v.YSpeed = yspeed[v.type];
            }
        }
    }
}
