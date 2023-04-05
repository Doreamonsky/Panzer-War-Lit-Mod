Behavior()
Property = {}
local GetChildrenComponentTutorial = class("GetChildrenComponentTutorial")

function GetChildrenComponentTutorial:OnStarted()
    local renderers = ComponentAPI.GetNativeComponentsInChildren(self.gameObject, "MeshRenderer")
    local arrayCnt = renderers.Length

    for index = 0, arrayCnt - 1 do
        renderers[index].enabled = false
    end
end

return GetChildrenComponentTutorial
