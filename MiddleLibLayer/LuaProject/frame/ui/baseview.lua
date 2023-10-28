---@class BaseView
local BaseView = class("BaseView")

local M = BaseView

function M:InitUI(root)
    error("override me plz")
end

return M
