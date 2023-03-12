--- 坦克工坊的 Gzimo 自定义
--- 比如插槽大小，坐标轴大小

LuaModeConfig = {}

local this = LuaModeConfig
this.configName = "LuaMode"

this.config = {
    ["MultiTurretGUI"] = true
}

this.loadConfig = function()
    local config = ReadConfig(this.configName)

    if config ~= nil then
        local dict = json.decode(config)

        if dict ~= nil then
            for k, v in pairs(dict) do
                this.config[k] = v
            end
        end
    end
end

this.saveConfig = function()
    WriteConfig(this.configName, json.encode(this.config))
end

return this
