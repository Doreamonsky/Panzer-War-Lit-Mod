local RotateComponent = class("RotateComponent")

Propery = {
    rotateSpeed = "number",
}


function RotateComponent:OnUpdated()
    local r = Vector3.up * Time.deltaTime * self.rotateSpeed
    self.transform:Rotate(r)
end

return RotateComponent
