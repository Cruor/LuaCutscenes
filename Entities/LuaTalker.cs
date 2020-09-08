using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monocle;
using NLua;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LuaCutscenes
{
    [CustomEntity("luaCutscenes/luaTalker")]
    class LuaTalker : Actor
    {
        private static LuaTable cutsceneHelper = Everest.LuaLoader.Require($"{LuaCutscenesMod.Instance.Metadata.Name}:/Assets/LuaCutscenes/cutscene_helper") as LuaTable;

        private string filename;
        private bool onlyOnce;

        private LuaTable cutsceneEnv;
        private IEnumerator onTalkRoutine;

        private TalkComponent talker;
        private EntityData data;

        private bool shouldDisable = false;

        // TODO - Figure out why errors are weird on bad Lua code
        public void LoadTalker(string filename, Player player)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var cutsceneData = new Dictionary<object, object>
                {
                    { "player", player },
                    { "talker", this },
                    { "modMetaData", LuaCutscenesMod.Instance.Metadata },
                };

                LuaTable dataTable = LuaHelper.DictionaryToLuaTable(cutsceneData);

                try
                {
                    object[] cutsceneResult = (cutsceneHelper["getTalkerData"] as LuaFunction).Call(new object[] { filename, dataTable });

                    if (cutsceneResult != null)
                    {
                        cutsceneEnv = cutsceneResult.ElementAtOrDefault(0) as LuaTable;

                        onTalkRoutine = LuaHelper.LuaCoroutineToIEnumerator(cutsceneResult.ElementAtOrDefault(1) as LuaCoroutine);
                    }
                    else
                    {
                        Logger.Log("Lua Cutscenes", $"Failed to load cutscene, target file does not exist: \"{filename}\"");
                    }

                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to execute cutscene in C#: {e}");
                }
            }
        }

        public LuaTalker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            this.data = data;

            onlyOnce = data.Bool("onlyOnce", true);
            filename = data.Attr("filename", "");

            Add(talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), data.Nodes.First() + offset - Position, onTalk)
            {
                Enabled = true,
                PlayerMustBeFacing = false
            });
        }

        private void onTalk(Player player)
        {
            if (onTalkRoutine == null)
            {
                LoadTalker(filename, player);
            }
 
            if (onTalkRoutine != null) {
                Add(new Coroutine(onTalkRoutine));

                if (onlyOnce)
                {
                    shouldDisable = true;
                }
            }
        }

        public override void Update()
        {
            if (shouldDisable && talker.Enabled)
            {
                talker.Enabled = false;
            }

            base.Update();
        }
    }
}
