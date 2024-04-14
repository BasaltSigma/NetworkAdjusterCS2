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
                m_SetUpgradeFlags = new CompositionFlags()
                {
                    m_General = CompositionFlags.General.Edge,
                    m_Right = CompositionFlags.Side.WideSidewalk | CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.Lowered
                },
            },
            new AdjusterUpgradeModel
            {
                Id = "UnderpassAdjustment",
                m_SetUpgradeFlags = new CompositionFlags()
                {
                    m_General = CompositionFlags.General.Edge,
                    m_Right = CompositionFlags.Side.WideSidewalk | CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.Lowered
                },
            },
            new AdjusterUpgradeModel
            {
                Id = "SlopeAdjustment",
                m_SetUpgradeFlags = new CompositionFlags()
                {
                    m_General = CompositionFlags.General.Edge,
                    m_Right = CompositionFlags.Side.WideSidewalk | CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.Lowered
                },
            }
        };
    }

    internal class AdjusterUpgradeModel
    {
        public string Id
        {
            get; set;
        }

        public CompositionFlags m_SetUpgradeFlags
        {
            get; set;
        }
    }
}
