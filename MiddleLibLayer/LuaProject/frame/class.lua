--- lua 侧面向对象
--- @param classname string 类名称
--- @param super table 父类名称
--- 调用 .new(...) 实例化 ... 为 :ctor 的初始化参数
---@return LuaObject
function class(classname, super)
    local superType = type(super)
    local cls

    if superType ~= "function" and superType ~= "table" then
        superType = nil
        super = nil
    end

    if superType == "function" or (super and super.__ctype == 1) then
        -- inherited from native C++ Object
        cls = {}

        if superType == "table" then
            -- copy fields from super
            for k, v in pairs(super) do
                if k ~= "OnDispose" then
                    cls[k] = v
                end
            end
            cls.__create = super.__create
            cls.super    = super
        else
            cls.__create = super
            cls.ctor = function() end
        end

        cls.__cname = classname
        cls.__ctype = 1

        function cls.new(...)
            local instance = cls.__create(...)
            -- copy fields from class to native object
            for k, v in pairs(cls) do instance[k] = v end
            instance.class = cls
            instance:ctor(...)
            return instance
        end
    else
        -- inherited from Lua Object
        if super then
            cls = {}
            for k, v in pairs(super) do
                if k ~= "OnDispose" then
                    cls[k] = v
                end
            end
            cls = setmetatable(cls, getmetatable(super))
            cls.super = super
        else
            cls = { ctor = function() end }
        end

        cls.__cname = classname
        cls.__ctype = 2 -- lua
        if cls.__index == super then
            cls.__index = cls
        end

        function cls.new(...)
            local instance = setmetatable({}, cls)
            instance.class = cls
            if instance.ctor == nil then
                error("ctor is nil", cls.__cname)
            end
            instance:ctor(...)
            return instance
        end
    end

    return cls
end
