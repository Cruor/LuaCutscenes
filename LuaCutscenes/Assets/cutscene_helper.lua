-- TODO - Return env in getCutscene

local celesteMod = require("#celeste.mod")

local function threadProxyResume(self, ...)
    return coroutine.resume(self.value, ...)
end

local function prepareForCSharp(env, func)
    local success, onBegin, onEnd = pcall(func)

    if success then
        onEnd = onEnd or env.onEnd
        onBegin = onBegin or env.onBegin

        onBegin = onBegin and celesteMod.LuaCoroutine({value = coroutine.create(onBegin), resume = threadProxyResume})

        return onBegin, onEnd

    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Lua Cutscenes", "Failed to load cutscene: " .. onBegin)

        return success, onBegin
    end
end

local environmentCode = [[

for k, v in pairs(helpers) do
    _ENV[k] = v
end

]]

local function readFile(filename, modName)
    return celesteMod[modName].LuaHelper.GetFileContent(filename)
end

local function addEnvironmentRequire(content, data)
    local helperFunctionsContent = readFile(data.modMetaData.Name .. ":/Assets/helper_functions", data.modMetaData.Name)

    return helperFunctionsContent .. environmentCode .. content
end

local function getCutsceneEnv(data)
    local env = data

    setmetatable(env, {__index = _G})

    return env
end

local function getCutscene(filename, data)
    local env = getCutsceneEnv(data)
    local content = readFile(filename, data.modMetaData.Name)

    if content then
        content = addEnvironmentRequire(content, data)

        local func = load(content, nil, nil, env)

        return env, prepareForCSharp(env, func)
    end
end

return getCutscene