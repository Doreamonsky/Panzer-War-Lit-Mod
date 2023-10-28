---@class LuaUIManager
---@field createResourceDelegate function
---@field removeResourceDelegate function
local LuaUIManager = {}
local M = LuaUIManager

M.createResourceDelegate = nil
M.removeResourceDelegate = nil

M.Index = 0
M.RefMap = {}

---@param controller BaseController
---@param view BaseView
function M.CreateUI(view, controller, ...)
    M.Index = M.Index + 1
    M.RefMap[M.Index] = {
        view = nil,
        controller = nil,
        resourceRef = nil
    }

    M.createResourceDelegate(function(instance, resourceRef)
        if M.RefMap[M.Index] ~= nil then
            local root = instance.transform
            view:InitUI(root)
            controller.view = view

            M.RefMap[M.Index].view = view
            M.RefMap[M.Index].controller = controller
            M.RefMap[M.Index].resourceRef = resourceRef

            controller:Awake()
        else
            M.removeResourceDelegate(resourceRef)
        end
    end, ...)

    return M.Index
end

function M.RemoveUI(index)
    local ref = M.RefMap[index]

    if ref ~= nil then
        ---@type BaseController
        local controller = ref.controller
        controller:Destroy()

        M.removeResourceDelegate(ref.resourceRef)
        M.RefMap[index] = nil
    end
end

return M
