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
        private string argumentsString;
        private bool onlyOnce;
        private bool unskippable;

        private bool wasSkipped;

        private LuaTable cutsceneEnv;
        private IEnumerator onTalkRoutine;
        private LuaFunction onEndFunction;

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
                    { "arguments", LuaHelper.LoadArgumentsString(argumentsString) }
                };

                LuaTable dataTable = LuaHelper.DictionaryToLuaTable(cutsceneData);

                try
                {
                    object[] cutsceneResult = (cutsceneHelper["getTalkerData"] as LuaFunction).Call(new object[] { filename, dataTable });

                    if (cutsceneResult != null)
                    {
                        cutsceneEnv = cutsceneResult.ElementAtOrDefault(0) as LuaTable;

                        onTalkRoutine = LuaHelper.LuaCoroutineToIEnumerator(cutsceneResult.ElementAtOrDefault(1) as LuaCoroutine);
                        onEndFunction = cutsceneResult.ElementAtOrDefault(2) as LuaFunction;
                    }
                    else
                    {
                        Logger.Log("Lua Cutscenes", $"Failed to load cutscene, target file does not exist: \"{filename}\"");
                    }

                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to execute talker in C#: {e}");
                }
            }
        }

        public LuaTalker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            this.data = data;

            onlyOnce = data.Bool("onlyOnce", true);
            filename = data.Attr("filename", "");
            argumentsString = data.Attr("arguments", "");
            unskippable = data.Bool("unskippable", false);

            wasSkipped = false;

            Add(talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), data.Nodes.First() + offset - Position, onTalk)
            {
                Enabled = true,
                PlayerMustBeFacing = false
            });
        }

        private void skipCutscene(Level level)
        {
            wasSkipped = true;

            onEnd(level);
        }

        private IEnumerator onBeginWrapper(Level level)
        {
            yield return onTalkRoutine;

            onEnd(level);
        }

        private void onTalk(Player player)
        {
            LoadTalker(filename, player);
 
            if (onTalkRoutine != null) {
                Level level = SceneAs<Level>();

                if (!unskippable)
                {
                    level.StartCutscene(skipCutscene);
                }

                Add(new Coroutine(onBeginWrapper(level)));

                if (onlyOnce)
                {
                    shouldDisable = true;
                }
            }
        }

        private void onEnd(Level level)
        {
            onEndFunction?.Call(new object[] { level, wasSkipped });

            level.EndCutscene();
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
