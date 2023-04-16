-----------------------------------------------------
-- 通用框架 (不仅针对此项目，未来任何项目都可以使用的通用代码)
require("frame.class")                     -- 面向对象编程 local customClass = class("customClass") local a = customClass.new(...)
require("frame.entity.all")                -- 实体
require("frame.eventsystem")               -- 事件系统
require("frame.define.all")                -- 声明与注释
require("frame.lib.all")                   -- 第三方库
require("frame.util.all")                  -- 常用帮助方法
local profiler = require("frame.profiler") -- 性能监控
-----------------------------------------------------

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

LuaProfiler = profiler.new()
