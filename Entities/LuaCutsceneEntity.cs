﻿using System;
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
        private static LuaTable cutsceneHelper = Everest.LuaLoader.Require($"{LuaCutscenesMod.Instance.Metadata.Name}:/Assets/LuaCutscenes/cutscene_helper") as LuaTable;

        private string filename;
        private string argumentsString;
        private bool unskippable;

        public bool Finished;

        private LuaTable cutsceneEnv;

        private IEnumerator onBeginRoutine;
        private LuaFunction onEndFunction;

        private LuaFunction onEnterFunction;
        private LuaFunction onStayFunction;
        private LuaFunction onLeaveFunction;

        private EntityData data;
        private Player player;
        private LuaCutsceneTrigger cutsceneTrigger;

        // TODO - Figure out why errors are weird on bad Lua code
        public void LoadCutscene(string filename, Player player, EntityData data)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var cutsceneData = new Dictionary<object, object>
                {
                    { "player", player },
                    { "cutsceneEntity", this },
                    { "cutsceneTrigger", cutsceneTrigger },
                    { "modMetaData", LuaCutscenesMod.Instance.Metadata },
                    { "arguments", LuaHelper.LoadArgumentsString(argumentsString) }
                };

                LuaTable dataTable = LuaHelper.DictionaryToLuaTable(cutsceneData);

                try
                {
                    object[] cutsceneResult = (cutsceneHelper["getCutsceneData"] as LuaFunction).Call(new object[] { filename, dataTable });

                    if (cutsceneResult != null)
                    {
                        cutsceneEnv = cutsceneResult.ElementAtOrDefault(0) as LuaTable;

                        onBeginRoutine = LuaHelper.LuaCoroutineToIEnumerator(cutsceneResult.ElementAtOrDefault(1) as LuaCoroutine);
                        onEndFunction = cutsceneResult.ElementAtOrDefault(2) as LuaFunction;

                        onEnterFunction = cutsceneResult.ElementAtOrDefault(3) as LuaFunction;
                        onStayFunction = cutsceneResult.ElementAtOrDefault(4) as LuaFunction;
                        onLeaveFunction = cutsceneResult.ElementAtOrDefault(5) as LuaFunction;
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

        public static void WarmUp()
        {
            Logger.Log("Lua Cutscenes", "Warming up cutscenes");

            LuaCutsceneEntity warmupEntity = new LuaCutsceneEntity(null, null, new EntityData());
            warmupEntity.LoadCutscene("Assets/LuaCutscenes/warmup_cutscene", null, null);

            Coroutine beginCoroutine = new Coroutine(warmupEntity.onBeginWrapper(null));

            warmupEntity.OnEnter(null);
            warmupEntity.OnStay(null);
            warmupEntity.OnLeave(null);

            try
            {
                while (!beginCoroutine.Finished)
                {
                    beginCoroutine.Update();
                }
            }
            catch
            {
                // Do nothing
                // The cutscene will crash because the level doesn't exist
            }
        }

        public LuaCutsceneEntity(LuaCutsceneTrigger cutsceneTrigger, Player player, EntityData data, bool fadeInOnSkip = true, bool endingChapterAfter = false, bool unskippable = false) : base(fadeInOnSkip, endingChapterAfter)
        {
            this.player = player;
            this.data = data;
            this.cutsceneTrigger = cutsceneTrigger;
            this.unskippable = unskippable;
            this.Finished = false;

            filename = data.Attr("filename", "");
            argumentsString = data.Attr("arguments", "");

            LoadCutscene(filename, player, data);
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

                if (unskippable)
                {
                    level.InCutscene = false;
                    level.CancelCutscene();
                }
            }
        }

        public override void OnEnd(Level level)
        {
            try
            {
                onEndFunction?.Call(new object[] { level, WasSkipped });
                this.Finished = true;
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to call OnEnd");
                Logger.LogDetailed(e);
            }
        }

        public void OnEnter(Player player)
        {
            try
            {
                onEnterFunction?.Call(new object[] { player });
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to call OnEnter");
                Logger.LogDetailed(e);
            }
        }

        public void OnStay(Player player)
        {
            try
            {
                onStayFunction?.Call(new object[] { player });
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to call OnStay");
                Logger.LogDetailed(e);
            }
        }

        public void OnLeave(Player player)
        {
            try
            {
                onLeaveFunction?.Call(new object[] { player });
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to call OnLeave");
                Logger.LogDetailed(e);
            }
        }
    }
}
