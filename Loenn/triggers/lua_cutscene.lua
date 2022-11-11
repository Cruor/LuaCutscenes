local luaCutscene = {}

luaCutscene.name = "luaCutscenes/luaCutsceneTrigger"
luaCutscene.placements = {
    name = "cutscene",
    data = {
        onlyOnce = false,
        filename = "",
        arguments = "",
        unskippable = false
    }
}

return luaCutscene