-- TODO - Fixed enums
-- TODO - Add rumble

local luanet = _G.luanet

local celeste = require("#celeste")
local celesteMod = celeste.mod
local csharpVector2 = require("#microsoft.xna.framework.vector2")
local engine = require("#monocle.engine")

local modName = modMetaData.Name

local helpers = {}

helpers.celeste = celeste
helpers.engine = engine

local function vector2(x, y)
    local typ = type(x)

    if typ == "table" and not y then
        return csharpVector2(x[1], x[2])

    elseif typ == "userdata" and not y then
        return x

    else
        return csharpVector2(x, y)
    end
end

-- Get the content of a file from Celeste Mods directory
function helpers.readCelesteAsset(filename)
    return celesteMod[modName].LuaHelper.GetFileContent(filename)
end

-- Run the file and return its result
-- Does NOT do caching like Lua `require`!
function helpers.loadCelesteAsset(filename)
    local content = helpers.readCelesteAsset(filename)

    if not content then
        celesteMod.logger.log(celesteMod.LogLevel.Error, "Lua Cutscenes", "Failed to require asset in Lua: file '" .. filename .. "' not found")

        return
    end

    local func = load(content, nil, nil, {})
    local success, result = pcall(func)

    if success then
        return result

    else
        celesteMod.logger.log(celesteMod.LogLevel.Error, "Lua Cutscenes", "Failed to require asset in Lua: " .. result)
    end
end

-- Log debug message
function helpers.log(message, tag)
    celesteMod.logger.log(celesteMod.LogLevel.Info, tag or "Lua Cutscenes", tostring(message))
end

-- Wait for duration amount of seconds
helpers.wait = coroutine.yield

function helpers.getLevel()
    return engine.Scene
end

helpers.getRoom = helpers.getLevel

function helpers.getSession()
    return engine.Scene.Session
end

-- Display minitextbox with dialog
function helpers.say(dialog)
    coroutine.yield(celeste.Textbox.Say(tostring(dialog), {}))
end

function helpers.walkTo(x, walkBackwards, speedMultiplier, keepWalkingIntoWalls)
    coroutine.yield(player:DummyWalkTo(x, walkBackwards or false, speedMultiplier or 1, keepWalkingIntoWalls or false))
end

function helpers.walk(x, walkBackwards, speedMultiplier, keepWalkingIntoWalls)
    helpers.walkTo(player.Position.X + x, walkBackwards, speedMultiplier, keepWalkingIntoWalls)
end

function helpers.runTo(x, fastAnimation)
    coroutine.yield(player:DummyRunTo(x, fastAnimation or false))
end

function helpers.run(x, fastAnimation)
    helpers.runTo(player.Position.X + x, fastAnimation)
end

function helpers.die(direction, evenIfInvincible, registerDeathInStats)
    player:Die(vector2(direction or {0, 0}), evenIfInvincible or false, registerDeathInStats or registerDeathInStats == nil)
end

function helpers.setPlayerState(state, locked)
    player.StateMachine.Locked = locked or false

    if type(state) == "string" then
        if not state:match("^St") then
            state = "St" .. state
        end

        player.StateMachine.State = celeste.Player[state]

    else
        player.StateMachine.State = state
    end
end

function helpers.getPlayerState()
    return player.StateMachine.State, player.StateMachine.Locked
end

function helpers.disableMovement()
    helpers.setPlayerState("Dummy", false)
end

function helpers.enableMovement()
    helpers.setPlayerState("Normal", false)
end

function helpers.jump(duration)
    player:Jump(true, true)
    player.AutoJump = true
    player.AutoJumpTimer = duration or 2.0
end

function helpers.changeRoom(name, spawnX, spawnY)
    local level = engine.Scene

    level.Session.Level = name
    level.Session.RespawnPoint = level:GetSpawnPoint(vector2(spawnX or level.Bounds.Left, spawnY or level.Bounds.Bottom))
    level.Session:UpdateLevelStartDashes()

    -- TODO - Test
    engine.Scene = celeste.LevelLoader(level.Session, level.Session.RespawnPoint)
end

function helpers.getRoomPosition(name)
    -- TODO - Implement
    -- If name is absent use current room
end

function helpers.teleportTo(x, y, room)
    player.Position = vector2(x, y)

    if room then
        helpers.changeRoom(room, x, y)
    end
end

function helpers.teleport(x, y, room)
    helpers.teleportTo(player.Position.X + x, player.Position.Y + y, name)
end

function helpers.completeArea(spotlightWipe, skipScreenWipe, skipCompleteScreen)
    engine.scene:CompleteArea(spotlightWipe or false, skipScreenWipe or false, skipCompleteScreen or false)
end

function helpers.playSound(name, position)
    if position then
        return celeste.Audio.Play(name, position)

    else
        return celeste.Audio.Play(name)
    end
end

function helpers.getEntities(name)
    return celeste.Mod[modName].MethodWrappers.GetEntities(name)
end

function helpers.getEntity(name)
    return celeste.Mod[modName].MethodWrappers.GetEntity(name)
end

function helpers.getFirstEntity(name)
    return celeste.Mod[modName].MethodWrappers.GetFirstEntity(name)
end

function helpers.getAllEntities(name)
    return celeste.Mod[modName].MethodWrappers.getAllEntities(name)
end

function helpers.giveFeather()
    player:StartStarFly()
end

function helpers.deathsInCurrentRoom()
    return engine.Scene.Session.DeathsInCurrentLevel
end

-- Play a given music track
-- Progress is optional, leave empty for no progress change
function helpers.playMusic(track, progress)
    engine.Scene.Session.Audio.Music.Event = celeste.SFX.EventnameByHandle(track)

    if progress then
        engine.Scene.Session.Audio.Music.Progress = progress
    end

    engine.Scene.Session.Audio:Apply()
end

function helpers.getMusic()
    return celeste.Audio.CurrentMusic
end

-- Sets music progression
function helpers.setMusicProgression(progress)
    engine.Scene.Session.Audio.Music.Progress = progress
end

-- Gets the current music progression
function helpers.getMusicProgression()
    return engine.Scene.Session.Audio.Music.progress
end

function helpers.setMusicLayer(layer, value)
    if type(layer) == "table" then
        for _, index in ipairs(layer) do
            engine.Scene.Session.Audio.Music:Layer(index, value)
        end

    else
        engine.Scene.Session.Audio.Music:Layer(layer, value)
    end

    engine.Scene.Session.Audio:Apply()
end

function helpers.setSpawnPoint(target, absolute)
    local session = engine.Scene.Session
    local ct = cutsceneTrigger

    target = target or {0, 0}
    target = absolute and target or vector2(ct.Position.X + ct.Width / 2 + target[1], ct.Position.Y + ct.Height / 2 + target[2])

    if session.RespawnPoint and (session.RespawnPoint.X ~= target.X or session.RespawnPoint.Y ~= target.Y) then
        session.HitCheckpoint = true
        session.RespawnPoint = target
        session:UpdateLevelStartDashes()
    end
end

function helpers.shake(direction, duration)
    if direction and duration then
        engine.Scene:DirectionalShake(direction, duration)

    else
        engine.Scene:Shake(direction)
    end
end

-- If string name use preset from PlayerInventory, otherwise use passed in value
function helpers.setInventory(inventory)
    if type(inventory) == "string" then
        engine.Scene.Session.Inventory = celeste.PlayerInventory[inventory]

    else
        engine.Scene.Session.Inventory = inventory
    end
end

-- If name is given get inventory by name, otherwise the current player inventory
function helpers.getInventory(inventory)
    if inventory then
        return celeste.PlayerInventory[inventory]

    else
        return engine.Scene.Session.Inventory
    end
end

-- Offset the camera by x and y like in camera offset trigger, or set it to a existing offset struct
function helpers.setCameraOffset(x, y)
    engine.Scene.CameraOffset = y and vector2(x * 48, y * 32) or x
end

-- Get the current offset struct
function helpers.getCameraOffset()
    return engine.Scene.CameraOffset
end

function helpers.setFlag(flag, value)
    engine.Scene.Session:SetFlag(flag, value)
end

function helpers.getFlag(flag)
    return engine.Scene.Session:GetFlag(flag)
end

-- TODO - Accept table?
-- TODO - Unhaunt.
function helpers.spawnBadeline(x, y, relativeToPlayer)
    local position = (relativeToPlayer or relativeToPlayer == nil) and vector2(player.Position.X + x, player.Position.Y + y) or vector2(x, y)
    local badeline = celeste.BadelineOldsite(position, 1)

    engine.Scene:Add(badeline)

    return badeline
end

function helpers.endCutscene()
    cutsceneEntity:EndCutscene(engine.Scene)
end

function helpers.setBloomStrength(amount)
    engine.Scene.Bloom.Strength = amount
end

function helpers.getBloomStrength()
    return engine.Scene.Bloom.Strength
end

function helpers.setDarkness(amount)
    engine.Scene.Bloom.Strength = amount
end

function helpers.getDarkness()
    return engine.Scene.Bloom.Strength
end

function helpers.setCoreMode(mode)
    if type(mode) == "string" then
        engine.Scene.CoreMode = engine.Scene.Session.CoreModes[mode]

    else
        engine.Scene.CoreMode = mode
    end
end

function helpers.getCoreMode()
    return engine.Scene.CoreMode
end

-- Map coordinates
-- End position, controll
function helpers.cassetteFlyTo(endX, endY, controllX, controllY)
    playSound("event:/game/general/cassette_bubblereturn", vector2(engine.Scene.Camera.Position.X + 160, engine.Scene.Camera.Position.Y + 90))

    if endX and endY and controllX and controllY then
        player:StartCassetteFly(vector2(endX, endY), vector2(controllX, controllY))

    else
        player:StartCassetteFly(vector2(endX, endY), vector2(endX, endY))
    end
end

-- Relative to player
-- End position, controll
function helpers.cassetteFly(endX, endY, controllX, controllY)
    local playerX = player.Position.X
    local playerY = player.Position.Y

    controllX = controllX or endX
    controllY = controllY or endY

    helpers.cassetteFlyTo(endX + playerX, endY + playerY, controllX + playerX, controllY + playerY)
end

function helpers.setLevelFlag()
    -- TODO - Implement
end

function helpers.getLevelFlag()
    -- TODO - Implement
end

function helpers.giveKey()
    local level = engine.Scene
    local key = celeste.Key(player, Celeste.EntityID("unknown", 1073741823 + math.random(0, 10000)))

    level:Add(key)
    level.Session.Keys:Add(key.ID)
end

function helpers.setWind(pattern)
    local windController = helpers.getFirstEntity("WindController")
    local level = engine.Scene

    if type(pattern) == "string" then
        pattern = windController.Patterns[pattern]
    end

    if windController then
        windController:SetPattern(pattern)

    else
        windController = celeste.WindController(pattern)
        level.Add(windController)
    end
end

-- Requires reflection :(
function helpers.getWind()
    local windController = helpers.getFirstEntity("WindController")

    if windController then
        return windController.startPattern
    end
end

-- Requires Enums
function helpers.rumble(...)
    -- TODO - Implement
end

function helpers.makeUnskippable()
    engine.Scene.InCutscene = false
    engine.Scene:CancelCutscene()
end

function helpers.enableRetry()
    engine.Scene.CanRetry = true
end

function helpers.disableRetry()
    engine.Scene.CanRetry = false
end