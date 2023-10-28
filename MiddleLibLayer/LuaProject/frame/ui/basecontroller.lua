---@class BaseController
---@field view BaseView
local BaseController = class("BaseController")

local M = BaseController

function M:Awake()
    error("override me plz")
end

function M:Destroy()
    error("override me plz")
end

return M
