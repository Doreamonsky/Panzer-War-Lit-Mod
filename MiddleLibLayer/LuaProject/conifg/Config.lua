--- 新增的 Config 统一在这里添加
require("conifg.GizmoConfig")
require("conifg.LuaModeConfig")

Config = {}

local this = Config

--- 加载所有的配置文件
--- lua 环境创建时候调用
this.loadAllConfigs = function()
    GizmoConfig.loadConfig()
    LuaModeConfig.loadConfig()
end

return this
