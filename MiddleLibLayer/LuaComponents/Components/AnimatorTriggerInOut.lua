Behavior()

Property = {
    keycode = {
        type = "string",
        value = "h"
    },
    triggerInAnim = {
        type = "string",
        value = "GearDown"
    },
    triggerOutAnim = {
        type = "string",
        value = "GearUp"
    }
}

local M = class("AnimatorTriggerInOut")

function M:ctor()
    self.actionName = "AnimatorTriggerInOut"
    self.isTrigger = false
    self.com = ComponentAPI.GetNativeComponent(self.gameObject, "Animator")

    self.OnKeyPerformedCb = function(context)
        self:OnKeyPerformed(context)
    end

    self.OnKeyCanceledCb = function(context)
        self:OnKeyCanceled(context)
    end
end

function M:OnStarted()
    if self.IsLocalPlayer then
        InputAPI.RegisterKeyInput(self.actionName, self.keycode, self.OnKeyPerformedCb, self.OnKeyCanceledCb)
    end
end

function M:OnDestroyed()
    if self.IsLocalPlayer then
        InputAPI.UnregisterKeyInput(self.actionName, self.keycode, self.OnKeyPerformedCb, self.OnKeyCanceledCb)
    end
end

function M:OnKeyPerformed(context)
    if self.isTrigger then
        self.com:SetTrigger(self.triggerOutAnim)
    else
        self.com:SetTrigger(self.triggerInAnim)
    end

    self.isTrigger = not self.isTrigger
end

function M:OnKeyCanceled(context)
end

return M
