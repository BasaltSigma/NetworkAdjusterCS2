using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdjusterCS2.Code
{
    internal class AdjusterUpgrades
    {
        public static IEnumerable<AdjusterUpgradeModel> Modes = new[]
        {
            new AdjusterUpgradeModel
            {
                Id = "OverpassAdjustment",
            },
            new AdjusterUpgradeModel
            {
                Id = "UnderpassAdjustment",
            },
            new AdjusterUpgradeModel
            {
                Id = "SlopeAdjustment",
            }
        };
    }

    internal class AdjusterUpgradeModel
    {
        public string Id
        {
            get; set;
        }
    }
}
