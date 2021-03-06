﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    class LocalVehicle
    {
        public bool moved;
        public float X;
        public float Y;
        public long playerID;
        public VehicleType type;

        public float XSpeed = 0;
        public float YSpeed = 0;

        public LocalVehicle()
        {

        }

        public LocalVehicle(ref Vehicle v)
        {
            X = (float)v.X;
            Y = (float)v.Y;
            playerID = v.PlayerId;
            type = v.Type;
            
        }

        public LocalVehicle(ref LocalVehicle v)
        {
            X = v.X;
            Y = v.Y;
            playerID = v.playerID;
            type = v.type;
        }

        public void Update(ref VehicleUpdate update)
        {
            X = (float)update.X;
            Y = (float)update.Y;
        }
    }
}
