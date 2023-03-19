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


function TestMode:getModeName(lang)
    return "Lua 测试模式"
end

function TestMode:onStartMode()
    print("Enter a test game mode")
end

function TestMode:onExitMode()
    print("Exit a test game mode")
end

function TestMode:isProxyBattle()
    return true
end

return TestMode
