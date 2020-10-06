using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using System.Reflection;
using Microsoft.Xna.Framework;
using Celeste.Mod.Helpers;
using NLua;
using System.Collections;

namespace Celeste.Mod.LuaCutscenes
{
    static class MethodWrappers
    {
        private static MethodInfo getEntityMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetEntity");
        private static MethodInfo getEntitiesMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetEntities");
        private static MethodInfo entitiesFindFirstMethodInfo = Engine.Scene.Entities.GetType().GetMethod("FindFirst");
        private static MethodInfo entitiesFindAll = Engine.Scene.Entities.GetType().GetMethod("FindAll");

        public static Type GetTypeFromString(string name, string prefix="Celeste.")
        {
            return FakeAssembly.GetFakeEntryAssembly().GetType(prefix + name);
        }

        public static object GetEntity(string name, string prefix = "Celeste.")
        {
            return GetEntity(GetTypeFromString(name, prefix));
        }

        public static object GetEntity(Type type)
        {
            try
            { 
                return getEntityMethodInfo.MakeGenericMethod(new Type[] { type }).Invoke(Engine.Scene.Tracker, null);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: '{type}' is not trackable");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: {e}");
            }

            return null;
        }

        public static LuaTable GetEntities(string name, string prefix = "Celeste.")
        {
            return GetEntities(GetTypeFromString(name, prefix));
        }

        public static LuaTable GetEntities(Type type)
        {
            try
            {
                return LuaHelper.ListToLuaTable(getEntitiesMethodInfo.MakeGenericMethod(new Type[] { type }).Invoke(Engine.Scene.Tracker, null) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: '{type}' is not trackable");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: {e}");
            }

            return null;
        }

        // Do not confuse with GetEntity, this also works on non Tracked entities
        public static object GetFirstEntity(string name, string prefix = "Celeste.")
        {
            return GetFirstEntity(GetTypeFromString(name, prefix));
        }

        // Do not confuse with GetEntity, this also works on non Tracked entities
        public static object GetFirstEntity(Type type)
        {
            try
            {
                return entitiesFindFirstMethodInfo.MakeGenericMethod(new Type[] { type }).Invoke(Engine.Scene.Entities, null);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: Requested type does not exist");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: {e}");
            }

            return null;
        }

        // Do not confuse with GetEntity, this also works on non Tracked entities
        public static object GetAllEntities(string name, string prefix = "Celeste.")
        {
            return GetAllEntities(GetTypeFromString(name, prefix));
        }

        // Do not confuse with GetEntities, this also works on non Tracked entities
        public static object GetAllEntities(Type type)
        {
            try
            {
                return LuaHelper.ListToLuaTable(entitiesFindAll.MakeGenericMethod(new Type[] { type }).Invoke(Engine.Scene.Entities, null) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: Requested type does not exist");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: {e}");
            }

            return null;
        }

        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType=Player.IntroTypes.Transition, Vector2? nearestSpawn=null)
        {
            Level level = scene as Level;

            if (level != null)
            {
                level.OnEndOfFrame += () =>
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }

        public static void InstantTeleport(Scene scene, Player player, string room, bool sameRelativePosition, float positionX, float positionY)
        {
            Level level = scene as Level;

            if (level == null)
            {
                return;
            }

            if (String.IsNullOrEmpty(room))
            {
                Vector2 playerRelativeOffset = (new Vector2(positionX, positionY) - player.Position);

                player.Position = new Vector2(positionX, positionY);
                level.Camera.Position += playerRelativeOffset;
                player.Hair.MoveHairBy(playerRelativeOffset);

            }
            else
            {
                level.OnEndOfFrame += delegate
                {
                    Vector2 levelOffset = level.LevelOffset;
                    Vector2 playerOffset = player.Position - level.LevelOffset;
                    Vector2 cameraOffset = level.Camera.Position - level.LevelOffset;
                    Facings facing = player.Facing;

                    level.Remove(player);
                    level.UnloadLevel();
                    level.Session.Level = room;
                    level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                    level.Session.FirstLevel = false;
                    level.LoadLevel(Player.IntroTypes.Transition);

                    if (sameRelativePosition)
                    {
                        level.Camera.Position = level.LevelOffset + cameraOffset;
                        level.Add(player);
                        player.Position = level.LevelOffset + playerOffset;
                        player.Facing = facing;
                        player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                    }
                    else
                    {
                        Vector2 playerRelativeOffset  = (new Vector2(positionX, positionY) - level.LevelOffset - playerOffset);

                        level.Camera.Position = level.LevelOffset + cameraOffset + playerRelativeOffset;
                        level.Add(player);
                        player.Position = new Vector2(positionX, positionY);
                        player.Facing = facing;
                        player.Hair.MoveHairBy(level.LevelOffset - levelOffset + playerRelativeOffset);
                    }

                    if (level.Wipe != null)
                    {
                        level.Wipe.Cancel();
                    }
                };
            }
        }
    }
}
