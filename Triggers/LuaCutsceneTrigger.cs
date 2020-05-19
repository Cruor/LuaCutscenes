using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LuaCutscenes
{
    [CustomEntity("luaCutscenes/luaCutsceneTrigger")]
    class LuaCutsceneTrigger : Trigger
    {
        private bool played;
        private bool onlyOnce;
        private EntityData data;

        private LuaCutsceneEntity cutsceneEntity;

        public LuaCutsceneTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            this.data = data;

            onlyOnce = data.Bool("onlyOnce", true);

            played = false;
        }

        public override void OnEnter(Player player)
        {
            cutsceneEntity?.OnEnter(player);

            if (onlyOnce && played)
            {
                return;
            }

            played = true;
            Scene.Add(cutsceneEntity = new LuaCutsceneEntity(this, player, data));
            cutsceneEntity.OnEnter(player);

            base.OnEnter(player);
        }

        public override void OnStay(Player player)
        {
            cutsceneEntity?.OnStay(player);

            base.OnStay(player);
        }

        public override void OnLeave(Player player)
        {
            cutsceneEntity?.OnLeave(player);

            base.OnLeave(player);
        }
    }
}
