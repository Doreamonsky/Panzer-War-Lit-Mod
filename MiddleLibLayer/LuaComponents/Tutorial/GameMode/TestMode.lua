local TestMode = class("TestMode")

GameMode()

Property = {
    enemyCount = {
        type = "number",
        value = 5
    },
    spawnInterval = {
        type = "number",
        value = 30 
    }
}


function TestMode:GetGameModeName(lang)
    self.hello = {}
    return "Lua 测试模式"
end

function TestMode:OnStartMode()
    print("Enter a test game mode")
end

function TestMode:OnExitMode()
    print("Exit a test game mode")
end

function TestMode:IsProxyBattle()
    return true
end

return TestMode
