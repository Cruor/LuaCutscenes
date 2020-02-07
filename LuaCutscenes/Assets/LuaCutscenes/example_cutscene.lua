-- Global functions onBegin and onEnd can be defined, or returned in that order.

-- Coroutine that does all the cutscene magic.
-- This invloves stuff like walking, jumping, displaying text boxes, etc.
function onBegin()
    walk(24)
    walk(-24, true, 0.1)
    say("CH6_THEO_SAY_VACATION")
    jump()
    wait(2)

    enableMovement()
end

-- Function, no yielding actions allowed.
-- That means no walking, waiting etc.
-- Only "clean up" actions.
function onEnd(level, wasSkipped)
    -- Skipping cutscenes is rude, you know :/
    if wasSkipped then
        die()
    end
end