module LuaCutscenesLuaCutsceneTrigger

using ..Ahorn, Maple

@mapdef Trigger "luaCutscenes/luaCutsceneTrigger" LuaCutsceneTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, onlyOnce::Bool=true, filename::String="")

const placements = Ahorn.PlacementDict(
    "Lua Cutscenes (Lua Cutscene)" => Ahorn.EntityPlacement(
        LuaCutsceneTrigger,
        "rectangle"
    )
)

end