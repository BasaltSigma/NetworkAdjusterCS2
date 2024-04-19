﻿using Colossal.UI.Binding;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdjusterCS2.Code
{
    internal partial class AdjusterUISystem : UISystemBase
    {
        private ValueBinding<bool> _ToolEnabled;
        protected readonly AdjusterTool _Tool = AdjusterTool.instance;

        protected override void OnCreate()
        {
            base.OnCreate();
            AddBinding(_ToolEnabled = new ValueBinding<bool>(Mod.MOD_UI, "NAT_ToolEnabled", false));
            AddBinding(new TriggerBinding(Mod.MOD_UI, "NAT_EnableToggle", NAT_EnableToggle));
        }

        private void NAT_EnableToggle()
        {
            Mod.log.Info("Main tool button has been toggled");
            
        }

        protected override void OnUpdate()
        {
            _ToolEnabled.Update(_Tool.Enabled);
        }
    }
}