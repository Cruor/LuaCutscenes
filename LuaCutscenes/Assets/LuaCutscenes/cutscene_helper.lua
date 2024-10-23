local cutsceneHelper = {}

local celesteMod = require("#celeste.mod")
local systemException = require("#System.Exception")

local function threadProxyResume(self, ...)
    local thread = self.value

    if coroutine.status(thread) == "dead" then
        return false, nil
    end

    local success, message = coroutine.resume(thread)

    -- The error message should be returned as an exception and not a string
    if not success then
        local errorMessage = string.format("%s\n%s", message, debug.traceback(thread))

        return success, systemException(errorMessage)
    end

    return success, message
end

local function prepareCutscene(env, func, filename)
    local success, onBegin, onEnd = pcall(func)

    if success then
        local onEnter = env.onEnter
        local onStay = env.onStay
        local onLeave = env.onLeave

        onEnd = onEnd or env.onEnd
        onBegin = onBegin or env.onBegin

        onBegin = onBegin and celesteMod.LuaCoroutine({value = coroutine.create(onBegin), resume = threadProxyResume})

        return onBegin, onEnd, onEnter, onStay, onLeave

    else
        local errorMessage = string.format("Failed to load cutscene '%s' in Lua: %s", filename, onBegin)

        celesteMod.logger.log(celesteMod.logLevel.error, "Lua Cutscenes", errorMessage)

        return success
    end
end

local function prepareTalker(env, func, filename)
    local success, onTalk, onEnd = pcall(func)

    if success then
        onTalk = onTalk or env.onTalk
        onEnd = onEnd or env.onEnd

        onTalk = onTalk and celesteMod.LuaCoroutine({value = coroutine.create(onTalk), resume = threadProxyResume})

        return onTalk, onEnd

    else
        local errorMessage = string.format("Failed to load cutscene '%s' in Lua: %s", filename, onTalk)

        celesteMod.logger.log(celesteMod.logLevel.error, "Lua Cutscenes", errorMessage)

        return success
    end
end

function cutsceneHelper.readFile(filename, modName)
    return celesteMod[modName].LuaHelper.GetFileContent(filename)
end

local function addHelperFunctions(data, env)
    local helperContent = cutsceneHelper.readFile(data.modMetaData.Name .. ":/Assets/LuaCutscenes/helper_functions", data.modMetaData.Name)
    local helperFunctions = load(helperContent, "helper_functions", nil, env)()

    for k, v in pairs(helperFunctions) do
        env[k] = v
    end

    env.helpers = helperFunctions
end

function cutsceneHelper.getLuaEnv(data)
    local env = data or {}

    setmetatable(env, {__index = _G})

    return env
end

function cutsceneHelper.getLuaData(filename, data, preparationFunc)
    preparationFunc = preparationFunc or function() end

    local env = cutsceneHelper.getLuaEnv(data)
    local content = cutsceneHelper.readFile(filename, data.modMetaData.Name)

    if content then
        addHelperFunctions(data, env)

        local func = load(content, filename, nil, env)

        return env, preparationFunc(env, func, filename)
    end
end

function cutsceneHelper.getCutsceneData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareCutscene)
end

function cutsceneHelper.getTalkerData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareTalker)
end

return cutsceneHelper
