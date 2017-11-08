using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    class LocalVehicle
    {
        public double X;
        public double Y;
        public long playerID;

        public LocalVehicle()
        {

        }

        public LocalVehicle(ref Vehicle v)
        {
            X = v.X;
            Y = v.Y;
            playerID = v.PlayerId;
        }

        public void Update(ref VehicleUpdate update)
        {
            X = update.X;
            Y = update.Y;
        }
    }
}
