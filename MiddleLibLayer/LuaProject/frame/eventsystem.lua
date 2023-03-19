EventSystem = {}

function EventSystem.AddListener(eventType, func, instance)
    if (eventType == nil or func == nil) then
        return
    end
    if (EventSystem[eventType] == nil) then
        local a = {}
        table.insert(a, { func, instance })
        EventSystem[eventType] = a
    else
        table.insert(EventSystem[eventType], { func, instance })
    end
end

function EventSystem.RemoveListener(eventType, func, instance)
    if (eventType == nil or func == nil) then
        return
    end
    local a = EventSystem[eventType]
    if (a ~= nil) then
        for k, v in pairs(a) do
            if (v[1] == func and v[2] == instance) then
                a[k] = nil
            end
        end
    end
end

function EventSystem.DispatchEvent(eventType, ...)
    if (eventType ~= nil) then
        local a = EventSystem[eventType]
        if (a ~= nil) then
            for k, v in pairs(a) do
                v[1](v[2], ...)
            end
        end
    end
end

return EventSystem
