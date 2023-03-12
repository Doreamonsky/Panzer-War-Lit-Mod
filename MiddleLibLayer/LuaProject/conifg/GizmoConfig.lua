--- 坦克工坊的 Gzimo 自定义
--- 比如插槽大小，坐标轴大小

GizmoConfig = {}

local this = GizmoConfig
this.configName = "Gizmo"

CameraControllerType = enum({
    "Keypad",
    "Joystick"
})

this.config = {
    ["SlotScale"] = 1,
    ["AxisScale"] = 1,
    ["CameraMoveScale"] = 1,
    ["EnableGrid"] = true,
    ["CameraControllerType"] = CameraControllerType.Keypad,
    ["EnablePressSelect"] = true,
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

    print("成功: 初始化 Gizmo 配置文件")
end

this.saveConfig = function()
    WriteConfig(this.configName, json.encode(this.config))
end

return this
