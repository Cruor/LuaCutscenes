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

    -- This is the old way of doing choices.
    -- Using choiceDialog instead (shown below) is recommended as it is much simpler for bigger conversations.
    if choice("CH6_THEO_ASK_VACATION", "CH6_THEO_ASK_FAMILY") == 1 then
        say("CH6_THEO_SAY_VACATION")

    else
        say("CH6_THEO_SAY_FAMILY")
    end

    jump()
    wait(2)

    -- The new way of doing choices, recommended.
    -- It accepts a table containing information about all possible choices and their requirements.
    choiceDialog({
        -- The most basic dialog choice.
        -- The dialog key DIALOG_1 will be used for displaying the choice, then when you choose it, the dialog DIALOG_1_SAY will be shown.
        { "DIALOG_1" },
        {
            -- This dialog requires that you had already chosen "DIALOG_1" before.
            "DIALOG_1_PART_2",
            requires = { "DIALOG_1" }
        },
        {
            -- This dialog will always be available, no matter how many times it has been chosen.
            "REPEATING",
            repeatable = true
        },
        {
            -- 'requires' can also be a function returning a bool. If this function returns false, the choice will not be available.
            "RANDOM",
            requires = function ()
                return math.random() > 0.5
            end,
        },
        {
            -- There can be multiple requirements for the same choice.
            -- All conditions need to be met at the same for this choice to show up.
            "AFTER_1_AND_RANDOM",
            requires = { "DIALOG_1", "RANDOM", function ()
                return math.random() > 0.5
            end},
        },
        {
            -- This choice will not display a dialog, and instead run the code in 'onChosen'
            "DIALOG_WITH_EFFECT",
            onChosen = function ()
                jump(0.2)
                wait()
                waitUntilOnGround()
            end
        },
        {
            --[[
            This choice will be available after either DIALOG_1 or DIALOG_WITH_EFFECT gets used.
            ctx is a table containing information about the current choice dialog.
            ctx.usedDialogs is a table which stores which dialogs you already used, in the format {
                ["DIALOG_KEY"] = true,
                ...
            }]]
            "AFTER_1_OR_EFFECT",
            requires = function (ctx)
                return ctx.usedDialogs["DIALOG_1"] or ctx.usedDialogs["DIALOG_WITH_EFFECT"]
            end
        },
        {
            -- this choice will still display a dialog like normal, but also run code in 'onEnd' after the dialog finishes.
            "DIALOG_CLOSE",
            onEnd = function ()
                -- call this function to close the choice dialog and allow the cutscene to continue.
                closeChoiceDialog()
            end
        }
    })

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