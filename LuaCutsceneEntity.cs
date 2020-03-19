using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod;
using Monocle;
using NLua;

namespace Celeste.Mod.LuaCutscenes
{
    class LuaCutsceneEntity : CutsceneEntity
    {
        private static LuaFunction cutsceneLoader = Everest.LuaLoader.Require($"{LuaCutscenesMod.Instance.Metadata.Name}:/Assets/cutscene_helper") as LuaFunction;

        private string filename;

        private IEnumerator onBeginRoutine;
        private LuaFunction onEndFunction;
        private LuaTable cutsceneEnv;

        private LuaFunction onEnterFunction;
        private LuaFunction onStayFunction;
        private LuaFunction onLeaveFunction;

        private EntityData data;
        private Player player;
        private LuaCutsceneTrigger cutsceneTrigger;

        // Figure out why errors are weird on bad Lua code
        private void loadCutscene(string filename, Player player, EntityData data)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var cutsceneData = new Dictionary<object, object>
                {
                    { "player", player },
                    { "cutsceneEntity", this },
                    { "cutsceneTrigger", cutsceneTrigger },
                    { "modMetaData", LuaCutscenesMod.Instance.Metadata },
                };

                LuaTable dataTable = LuaHelper.DictionaryToLuaTable(cutsceneData);

                try
                {
                    object[] cutsceneResult = cutsceneLoader.Call(new object[] { filename, dataTable });

                    cutsceneEnv = cutsceneResult.ElementAtOrDefault(0) as LuaTable;
                    onBeginRoutine = LuaHelper.LuaCoroutineToIEnumerator(cutsceneResult.ElementAtOrDefault(1) as LuaCoroutine);
                    onEndFunction = cutsceneResult.ElementAtOrDefault(2) as LuaFunction;

                    onEnterFunction = cutsceneResult.ElementAtOrDefault(3) as LuaFunction;
                    onStayFunction = cutsceneResult.ElementAtOrDefault(4) as LuaFunction;
                    onLeaveFunction = cutsceneResult.ElementAtOrDefault(5) as LuaFunction;
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to load cutscene in C#: {e}");
                }
            }
        }

        public LuaCutsceneEntity(LuaCutsceneTrigger cutsceneTrigger, Player player, EntityData data, bool fadeInOnSkip = true, bool endingChapterAfter = false) : base(fadeInOnSkip, endingChapterAfter)
        {
            this.player = player;
            this.data = data;
            this.cutsceneTrigger = cutsceneTrigger;

            filename = data.Attr("filename", "");

            loadCutscene(filename, player, data);
        }

        private IEnumerator onBeginWrapper(Level level)
        {
            yield return onBeginRoutine;

            EndCutscene(level);
        }

        public override void OnBegin(Level level)
        {
            if (onBeginRoutine != null)
            {
                Add(new Coroutine(onBeginWrapper(level)));
            }
        }

        public override void OnEnd(Level level)
        {
            onEndFunction?.Call(new object[] { level, WasSkipped });
        }

        public void OnEnter(Player player)
        {
            onEnterFunction?.Call(new object[] { player });
        }

        public void OnStay(Player player)
        {
            onStayFunction?.Call(new object[] { player });
        }

        public void OnLeave(Player player)
        {
            onLeaveFunction?.Call(new object[] { player });
        }
    }
}
