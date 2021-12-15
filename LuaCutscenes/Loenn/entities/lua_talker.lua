local luaTalker = {}

luaTalker.name = "luaCutscenes/luaTalker"
luaTalker.depth = 0
luaTalker.nodeLimits = {1, 1}
luaTalker.placements = {
    name = "talker",
    data = {
        width = 8,
        height = 8,
        onlyOnce = false,
        filename = "",
        arguments = "",
        unskippable = false
    }
}

luaTalker.borderColor = {0.0, 1.0, 1.0, 1.0}
luaTalker.fillColor = {0.0, 1.0, 1.0, 0.4}
luaTalker.nodeTexture = "objects/LuaCutscenes/hover_idle"

return luaTalker