--- lua 侧面向对象
--- @param classname string 类名称
--- @param super table 父类名称
--- 调用 .new(...) 实例化 ... 为 :ctor 的初始化参数
function class(classname, super)
    local cls

    if type(super) ~= "table" then
        super = nil
    end

    if super then
        cls = {}
        setmetatable(cls, { __index = super })
        cls.super = super
    else
        cls = {
            ctor = function()
            end
        }
    end

    cls.__cname = classname
    cls.__index = cls

    --- @param params ... 构造函数 
    function cls.new(...)
        local instance = setmetatable({}, cls)
        instance.class = cls
        instance:ctor(...)
        return instance
    end

    return cls
end
