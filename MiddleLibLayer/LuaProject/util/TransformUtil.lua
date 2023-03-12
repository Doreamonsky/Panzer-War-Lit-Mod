TransformUtil = {}

local this = TransformUtil

--- 序列化欧拉角 -> 四元数
--- @param vec SerializeVector3
this.SerializeVectorToQuaternion = function(vec)
    return Quaternion.Euler(vec.x, vec.y, vec.z)
end

--- 四元数 -> 序列化欧拉角
this.QuaternionToSeralizeVector = function(rot)
    local vec = rot.eulerAngles
    return SerializeVector3(vec.x, vec.y, vec.z)
end

--- 序列化坐标 -> Unity 坐标
--- @param vec SerializeVector3
this.SerializeVectorToUnityVector = function(vec)
    return Vector3(vec.x, vec.y, vec.z)
end

--- Unity 坐标 -> 序列化坐标
--- @param vec Vector3
this.UnityVectorToSerializeVector = function(vec)
    return SerializeVector3(vec.x, vec.y, vec.z)
end

return this
