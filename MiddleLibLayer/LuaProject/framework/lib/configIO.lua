function GetLuaDataFolder()
    local path = AssetBundleManager.GetModDir().FullName
    return path
end

--- 读取配置
--- 存在文件则返回 string , 不存在则返回 nil
function ReadConfig(configName)
    local file, err = io.open(GetLuaDataFolder() .. "/" .. configName .. ".conf", "r")

    if file ~= nil then
        local txt = file:read("*a")
        file:close()

        return txt
    else
        return nil
    end
end

--- 写入配置
--- 写入配置文件
function WriteConfig(configName, config)
    local file, err = io.open(GetLuaDataFolder() .. "/" .. configName .. ".conf", "w")

    if file ~= nil then
        file:write(config)
        file:close()
    end
end
