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
        private bool oncePerSession;
        private bool unskippable;
        private EntityData data;
        private EntityID id;

        private LuaCutsceneEntity cutsceneEntity;

        public LuaCutsceneTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            this.data = data;
            this.id = id;

            onlyOnce = data.Bool("onlyOnce", true);
            oncePerSession = data.Bool("oncePerSession", false);
            unskippable = data.Bool("unskippable", false);

            played = false;
        }

        public override void OnEnter(Player player)
        {
            if (onlyOnce && played)
            {
                return;
            }

            if (cutsceneEntity == null || cutsceneEntity.Finished)
            {
                Scene.Add(cutsceneEntity = new LuaCutsceneEntity(this, player, data, unskippable: unskippable));
            }

            played = true;
            cutsceneEntity?.OnEnter(player);

            if (oncePerSession)
            {
                RemoveSelf();
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }

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
