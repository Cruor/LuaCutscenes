local cutsceneHelper = {}

local celesteMod = require("#celeste.mod")

local function threadProxyResume(self, ...)
    return coroutine.resume(self.value, ...)
end

local function prepareCutsene(env, func)
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
        celesteMod.logger.log(celesteMod.logLevel.error, "Lua Cutscenes", "Failed to load cutscene in Lua: " .. onBegin)

        return success
    end
end

local function prepareTalker(env, func)
    local success, onTalk, onEnd = pcall(func)

    if success then
        onTalk = onTalk or env.onTalk
        onEnd = onEnd or env.onEnd

        onTalk = onTalk and celesteMod.LuaCoroutine({value = coroutine.create(onTalk), resume = threadProxyResume})

        return onTalk, onEnd

    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Lua Cutscenes", "Failed to load cutscene in Lua: " .. onTalk)

        return success
    end
end

local environmentCode = [[

for k, v in pairs(helpers) do
    _ENV[k] = v
end

]]

function cutsceneHelper.readFile(filename, modName)
    return celesteMod[modName].LuaHelper.GetFileContent(filename)
end

local function addEnvironmentRequire(content, data)
    local helperFunctionsContent = cutsceneHelper.readFile(data.modMetaData.Name .. ":/Assets/LuaCutscenes/helper_functions", data.modMetaData.Name)

    return helperFunctionsContent .. environmentCode .. content
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
        content = addEnvironmentRequire(content, data)

        local func = load(content, nil, nil, env)

        return env, preparationFunc(env, func)
    end
end

function cutsceneHelper.getCutsceneData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareCutsene)
end

function cutsceneHelper.getTalkerData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareTalker)
end

return cutsceneHelper