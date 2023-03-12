--------------------- Native --------------------
--- 寻找 Index 位置，若找到则返回位置，否则返回 false
table.indexOf = function(tab, value)
    for index, val in pairs(tab) do
        if value == val then
            return index
        else
            return false
        end
    end
end

--- 枚举
function enum(tbl)
    local length = #tbl
    for i = 1, length do
        local v = tbl[i]
        tbl[v] = i
    end

    return tbl
end

traceback = function(err)
    print(debug.traceback())
    error("lua error " .. err)
end
-----------------------------------------------------
-- 通用框架 (不仅针对此项目，未来任何项目都可以使用的通用代码)
require("framework.Class") -- 面向对象编程 local customClass = class("customClass") local a = customClass.new(...)
require("framework.lib.base64") -- base64
require("framework.lib.configIO") -- IO 相关
require("framework.Entity") -- 实体
require("framework.EventSystem") -- 事件系统

inspect = require("framework.lib.inspect") -- 方便 Debug Table
json = require("framework.lib.dkjson") -- Json 序列化，反序列化
-----------------------------------------------------

-----------------------------------------------------
-- 项目相关
require("LibVersion") -- Lua C# 交互版本
require("util.Util")
require("conifg.Config")
require("GlobalDefine") -- 方便调用 C#
require("events.EventDefine") -- 事件定义
-----------------------------------------------------
