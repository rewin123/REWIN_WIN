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
        Dictionary<TerrainType, SolidBrush> terrain_colors = new Dictionary<TerrainType, SolidBrush>();
        public Visual()
        {
            InitializeComponent();
        }

        public void Draw(World world, Game game)
        {
            
        }
    }
}
