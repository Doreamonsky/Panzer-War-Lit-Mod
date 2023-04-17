Common()
Property = {}
local FollowCameraTutorial = class("FollowCameraTutorial")

function FollowCameraTutorial:ctor()
    self.vehicleList = {}
    self.timePassed = 0
    self.vehicleIndex = 1

    InputAPI.RegisterKeyInput("EnableFreeCamera", "F1", function()
            self.freeCamera = nil
            local freeCamera = SpawnVehicleAPI.CreateFreeCamera(Vector3.zero, Quaternion.identity)
            self.playerState = freeCamera.playerState
        end,
        function()
        end)

    InputAPI.RegisterKeyInput("ToggleFreeCamera", "F2", function()
        self.vehicleIndex = self.vehicleIndex + 1

        if self.vehicleIndex > #self.vehicleList then
            self.vehicleIndex = 1
        end
    end, function()
    end)

    GameAPI.RegisterVehicleLoadedEvent(function(vehicle)
        if VehicleAPI.IsTankVehicle(vehicle) then
            table.insert(self.vehicleList, vehicle)
        end
    end)

    GameAPI.RegisterVehicleGameObjectDestroyedEvent(function(vehicle)
        if VehicleAPI.IsTankVehicle(vehicle) then
            for k, v in pairs(self.vehicleList) do
                if v:GetIndex() == vehicle:GetIndex() then
                    table.remove(self.vehicleList, k)
                    return
                end
            end
        end
    end)
end

function FollowCameraTutorial:OnUpdated()
    if #self.vehicleList > 0 and self.playerState ~= nil then
        self.freeCamera = self.playerState.freeCamera

        if self.freeCamera ~= nil and not self.freeCamera:IsNull() then
            self.timePassed = self.timePassed + Time.deltaTime
            if self.timePassed >= 30 or self.vehicleIndex > #self.vehicleList then
                self.vehicleIndex = self.vehicleIndex % #self.vehicleList + 1
                self.timePassed = 0
            end

            local InstanceMesh = self.vehicleList[self.vehicleIndex].InstanceMesh
            if InstanceMesh ~= nil then
                self.freeCamera:MoveTo(InstanceMesh.transform.position + Vector3(0, 3, 0))
            end
        end
    end
end

return FollowCameraTutorial
