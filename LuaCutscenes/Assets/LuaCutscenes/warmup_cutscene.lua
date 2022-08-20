local modName = modMetaData.Name
local methodCacheTargets = {
	["#Celeste.Audio"] = "everything",
	["#Celeste.Textbox"] = "everything",
	["#Celeste.MiniTextbox"] = "everything",
	["#Celeste.Postcard"] = "everything",
	["#Celeste.Dialog"] = "everything",
	["#Celeste.BadelineOldsite"] = "everything",
	["#Celeste.Level"] = "everything",
	["#Celeste.Session"] = "everything",
	["#Celeste.Player"] = "everything",
	["#Celeste.Mod." .. modName] = "everything",
	["#Celeste.Mod." .. modName .. ".MethodWrappers"] = "everything",
	["#Celeste.Mod.Logger"] = "everything",
	["#Monocle.Engine"] = "everything",
	["#Monocle.Scene"] = "everything",
}

local function performCaching()
	for name, methods in pairs(methodCacheTargets) do
		local module = require(name)

		if type(methods) == "table" then
			for _, method in ipairs(methods) do
				log(string.format("Caching member %s of %s", method, name))

				local cached = module[method]
			end

		elseif methods == "everything" then
			log(string.format("Caching all members of %s", name))

			local target = module["SomeRandomMethodName"]
		end
	end
end

function onBegin()
	log("Warming up cutscenes, hello from Lua onBegin")

	performCaching()
end

function onEnd()
	log("Warming up cutscenes, hello from Lua onEnd")
end

function onEnter()
	log("Warming up cutscenes, hello from Lua onEnter")
end

function onStay()
	log("Warming up cutscenes, hello from Lua onStay")
end

function onLeave()
	log("Warming up cutscenes, hello from Lua onLeave")
end