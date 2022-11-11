--- Showing off callbacks available in the talker and brief explanations.
-- You are not required to define anything you aren't using.
-- Leave callbacks global to automatically detect it on c# side.
-- This assumes you know your way around Lua, if you don't then <a href=https://www.lua.org/pil/5.1.html>please consider reading the Lua PIL</a>.
-- <h2>Setting up in map editor</h2>
-- In the case of this talker, the filename for the entity field is "Assets/LuaCutscenes/example_talker".
-- Please use your own folders for your own talkers, like any other Celeste asset collisions can happen.<br>
-- <h2>Using C# in talker</h2>
-- Any imports from C# can be done with prefixing # in the require string. For example `local celeste = require("#Celeste")`.<br>
-- Check out helper_functions.lua for examples on C# interaction.
-- @module example_talker

--- Coroutine that is called when talker is interacted with.
-- This invloves stuff like walking, jumping, displaying text boxes, etc.
function onTalk()
    walk(24)
    walk(-24, true, 0.1)

    if choice("CH6_THEO_ASK_VACATION", "CH6_THEO_ASK_FAMILY") == 1 then
        say("CH6_THEO_SAY_VACATION")
    else
        say("CH6_THEO_SAY_FAMILY")
    end

    jump()
    wait(2)

    enableMovement()
end

--- Callback for when the cutscene ends.
-- Function, no yielding actions allowed.
-- That means no walking, waiting etc.
-- Only "clean up" actions.
-- @tparam #Celeste.Level room Current room.
-- @bool wasSkipped If the cutscene was skipped.
function onEnd(room, wasSkipped)
    -- Skipping cutscenes is rude, you know :/
    if wasSkipped then
        die()
    end
end
