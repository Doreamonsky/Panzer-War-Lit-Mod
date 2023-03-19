-- 生命周期需要游戏侧自己维护 update 以及 dispose
-- 可以理解为 mono 对象
--- @class Entity
local Entity = class("Entity")

function Entity:ctor(...)
    self:awake(...)
end

function Entity:awake(...)
    print("you have not implment awake")
end

function Entity:update()
    print("you have not implment update")
end

function Entity:dispose()
    print("you have not implment dispose")
end

return Entity

