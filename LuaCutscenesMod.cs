using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Celeste.Mod.LuaCutscenes
{
    public class LuaCutscenesMod : EverestModule
    {
        public static LuaCutscenesMod Instance;

        public override Type SettingsType => null;

        public LuaCutscenesMod()
        {
            Instance = this;
        }

        public override void Load()
        {

        }

        public override void Unload()
        {

        }
    }
}
