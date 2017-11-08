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
            Brush enemy = Brushes.Red;
            Brush my = Brushes.Green;
            float radius = (float)game.VehicleRadius;
            foreach(KeyValuePair<long,LocalVehicle> pair in vehicles)
            {
                LocalVehicle veh = pair.Value;
                
                gr.FillEllipse(veh.playerID == myPlayerID ? my : enemy, (float)(veh.X - radius), (float)(veh.Y - radius), radius, radius);
            }

            pictureBox1.Image = map;
            Update();
        }
        
    }
}
