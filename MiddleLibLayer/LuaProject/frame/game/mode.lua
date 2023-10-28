---@class Mode
local Mode = class("Mode")

local M = Mode

function M:ctor()
end

function M:GetGameModeName(lang)
    error("override me plz")
end

function M:OnStartMode()
    error("override me plz")
end

function M:OnUpdated()
    error("override me plz")
end

function M:OnExitMode()
    error("override me plz")
end

function M:IsProxyBattle()
    error("override me plz")
end

function M:GetMapMode()
    return MODE_INFINITE
end

return M
