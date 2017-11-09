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

        public Dictionary<long, long> scores = new Dictionary<long, long>();

        int collision_step;
        int collision_count;
        float radius;
        float radius2;
        Dictionary<long, LocalVehicle>[,] ground_groups;
        Dictionary<long, LocalVehicle>[,] air_groups;

        public float[,] speedMapGround;
        public float[,] speedMapAir;
        int count;
        int step;

        public PredictionWorld(ref Game game, ref World world, TerrainType[][] map, WeatherType[][] weather,Dictionary<long,LocalVehicle> input)
        {
            count = map.Length;
            step = 1024 / count;

            SetupVehicles(ref input);
            SetupTerrain(ref game, map);
            SetupWeather(ref game, weather);
            SetupGroundCollisionGroups(ref game);
            SetupAirCollisionGroups(ref game);
            PutAllVehiclesInGroups();

            for (int i = 0; i < world.Players.Length; i++)
            {
                scores.Add(world.Players[i].Id, 0);
            }
        }

        void SetupGroundCollisionGroups(ref Game game)
        {
            collision_step = (int)(Math.Pow(Math.Log(game.VehicleRadius,2) + 1,2));
            collision_count = 1024 / collision_step;
            radius = (float)game.VehicleRadius;
            radius2 = radius * radius;
            ground_groups = new Dictionary<long, LocalVehicle>[collision_count + 2, collision_count + 2];

            for(int x = 0;x < collision_count + 2;x++)
            {
                for(int y = 0;y < collision_count + 2;y++)
                {
                    ground_groups[x, y] = new Dictionary<long, LocalVehicle>();
                }
            }
        }

        void SetupAirCollisionGroups(ref Game game)
        {
            collision_step = (int)(Math.Pow(Math.Log(game.VehicleRadius, 2) + 1, 2));
            collision_count = 1024 / collision_step;
            radius = (float)game.VehicleRadius;
            radius2 = radius * radius;
            air_groups = new Dictionary<long, LocalVehicle>[collision_count + 2, collision_count + 2];

            for (int x = 0; x < collision_count + 2; x++)
            {
                for (int y = 0; y < collision_count + 2; y++)
                {
                    air_groups[x, y] = new Dictionary<long, LocalVehicle>();
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

            foreach (KeyValuePair<long, LocalVehicle> pair in air_vehicles)
            {
                LocalVehicle veh = pair.Value;
                air_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Add(pair.Key, veh);
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
            speedMapGround = new float[map.Length, map.Length];
            for (int x = 0; x < count; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    switch (map[x][y])
                    {
                        case TerrainType.Plain:
                            speedMapGround[x, y] = (float)game.PlainTerrainSpeedFactor;
                            break;
                        case TerrainType.Swamp:
                            speedMapGround[x, y] = (float)game.SwampTerrainSpeedFactor;
                            break;
                        case TerrainType.Forest:
                            speedMapGround[x, y] = (float)game.ForestTerrainSpeedFactor;
                            break;
                    }
                }
            }
        }

        void SetupWeather(ref Game game, WeatherType[][] weather)
        {
            speedMapAir = new float[weather.Length, weather.Length];
            for (int x = 0; x < count; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    switch (weather[x][y])
                    {
                        case WeatherType.Clear:
                            speedMapAir[x, y] = (float)game.ClearWeatherSpeedFactor;
                            break;
                        case WeatherType.Cloud:
                            speedMapAir[x, y] = (float)game.CloudWeatherSpeedFactor;
                            break;
                        case WeatherType.Rain:
                            speedMapAir[x, y] = (float)game.RainWeatherSpeedFactor;
                            break;
                    }
                }
            }
        }

        void MakeUnmoved()
        {
            foreach (KeyValuePair<long, LocalVehicle> pair in ground_vehicles)
            {
                pair.Value.moved = true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_vehicles)
            {
                pair.Value.moved = true;
            }
        }

        /// <summary>
        /// Предсказание перемещения техники на одиин тик. Сейчас без учета колизий
        /// </summary>
        public void Predict()
        {
            MakeUnmoved();
            int interactions = 0;
            do
            {
                interactions = 0;
                foreach (KeyValuePair<long, LocalVehicle> pair in ground_vehicles)
                {
                    if (pair.Value.moved)
                    {
                        LocalVehicle veh = pair.Value;
                        if (veh.X < 1024 && veh.Y < 1024)
                        {
                            ground_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Remove(pair.Key);

                            float factor = speedMapGround[(int)(veh.X / step), (int)(veh.Y / step)];
                            veh.X += veh.XSpeed * factor;
                            veh.Y += veh.YSpeed * factor;
                            if (TestCollideGround(veh.X, veh.Y))
                            {
                                veh.X -= veh.XSpeed * factor;
                                veh.Y -= veh.YSpeed * factor;
                            }
                            else
                            {
                                interactions++;
                                veh.moved = false;
                            }
                            ground_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Add(pair.Key, veh);
                        }
                    }
                }
            } while (interactions != 0);
            
            do
            {
                interactions = 0;
                foreach (KeyValuePair<long, LocalVehicle> pair in air_vehicles)
                {
                    if (pair.Value.moved)
                    {
                        LocalVehicle veh = pair.Value;
                        if (veh.X < 1024 && veh.Y < 1024)
                        {
                            air_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Remove(pair.Key);

                            float factor = speedMapGround[(int)(veh.X / step), (int)(veh.Y / step)];
                            veh.X += veh.XSpeed * factor;
                            veh.Y += veh.YSpeed * factor;
                            if (TestCollideAir(veh.X, veh.Y,veh.playerID))
                            {
                                veh.X -= veh.XSpeed * factor;
                                veh.Y -= veh.YSpeed * factor;
                            }
                            else
                            {
                                interactions++;
                                veh.moved = false;
                            }
                            air_groups[(int)(veh.X / collision_step), (int)(veh.Y / collision_step)].Add(pair.Key, veh);
                        }
                    }
                }
            } while (interactions != 0);

            
        }
        
        bool TestCollideGround(float x0, float y0)
        {
            int x = (int)(x0 / collision_step);
            int y = (int)(y0 / collision_step);
            float dx;
            float dy;

            foreach(KeyValuePair<long,LocalVehicle> pair in ground_groups[x,y])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x - 1, y])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x + 1, y])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x, y - 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x, y + 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x - 1, y - 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x - 1, y + 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x + 1, y - 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in ground_groups[x + 1, y + 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2)
                    return true;
            }

            return false;
        }

        bool TestCollideAir(float x0, float y0, long playerId)
        {
            int x = (int)(x0 / collision_step);
            int y = (int)(y0 / collision_step);
            float dx;
            float dy;

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x, y])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x - 1, y])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x + 1, y])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x, y - 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x, y + 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x - 1, y - 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x - 1, y + 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x + 1, y - 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_groups[x + 1, y + 1])
            {
                LocalVehicle v = pair.Value;
                dx = x0 - v.X;
                dy = y0 - v.Y;
                if (dx * dx + dy * dy <= radius2 && v.playerID == playerId)
                    return true;
            }

            return false;
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

        public void UpdateMove(Move m, ref Game game, long playerID)
        {
            switch(m.Action)
            {
                case ActionType.ClearAndSelect:
                    ClearAndSelect(m.Left, m.Right, m.Top, m.Bottom, playerID);
                    break;
                case ActionType.Move:
                    Move(m.X, m.Y, ref game);
                    break;
            }
        }

        void ClearAndSelect(double left, double right, double top, double bottom, long playerID)
        {
            selected.Clear();
            foreach (KeyValuePair<long, LocalVehicle> pair in ground_vehicles)
            {
                LocalVehicle v = pair.Value;
                
                if (v.X >= left && v.X <= right && v.Y <= bottom && v.Y >= top && v.playerID == playerID)
                    selected.Add(v);
            }

            foreach (KeyValuePair<long, LocalVehicle> pair in air_vehicles)
            {
                LocalVehicle v = pair.Value;
                if (v.X >= left && v.X <= right && v.Y <= bottom && v.Y >= top && v.playerID == playerID)
                    selected.Add(v);
            }
        }

        void Move(double X, double Y,ref Game game)
        {
            double abs = Math.Sqrt(X * X + Y * Y);

            Dictionary<VehicleType, float> xspeed = new Dictionary<VehicleType, float>();
            Dictionary<VehicleType, float> yspeed = new Dictionary<VehicleType, float>();

            xspeed.Add(VehicleType.Tank, (float)(X * game.TankSpeed / abs));
            xspeed.Add(VehicleType.Ifv, (float)(X * game.IfvSpeed / abs));
            xspeed.Add(VehicleType.Arrv, (float)(X * game.ArrvSpeed / abs));
            xspeed.Add(VehicleType.Fighter, (float)(X * game.FighterSpeed / abs));
            xspeed.Add(VehicleType.Helicopter, (float)(X * game.HelicopterSpeed / abs));

            yspeed.Add(VehicleType.Tank, (float)(Y * game.TankSpeed / abs));
            yspeed.Add(VehicleType.Ifv, (float)(Y * game.IfvSpeed / abs));
            yspeed.Add(VehicleType.Arrv, (float)(Y * game.ArrvSpeed / abs));
            yspeed.Add(VehicleType.Fighter, (float)(Y * game.FighterSpeed / abs));
            yspeed.Add(VehicleType.Helicopter, (float)(Y * game.HelicopterSpeed / abs));

            foreach (LocalVehicle pair in selected)
            {
                LocalVehicle v = pair;
                v.XSpeed = xspeed[v.type];
                v.YSpeed = yspeed[v.type];
            }
        }
    }
}
