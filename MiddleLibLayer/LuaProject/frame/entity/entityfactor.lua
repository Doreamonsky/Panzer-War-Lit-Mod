-- Entity Factory 需要 Update 接入游戏的生命周期，才可以正常使用

local EntityFactory = {}

--- @type table<number,Entity>
EntityFactory.entities = {}

--- @param entityType Class
function EntityFactory.AddEntity(entity)
    table.insert(EntityFactory.entities, entity)
    entity:awake()

    return entity
end

function EntityFactory.UpdateEntity()
    for k, v in pairs(EntityFactory.entities) do
        v:update()
    end
end

--- @param entity Entity
function EntityFactory.RemoveEntity(entity)
    local index = table.indexOf(EntityFactory.entities, entity)
    entity:dispose()

    if index then
        table.remove(EntityFactory.entities, index)
    end
end

return EntityFactory
