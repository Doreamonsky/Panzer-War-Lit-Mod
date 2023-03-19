Behavior()
Property = {
    ssss = {
        type = "number",
        value = 15
    },
}
local HelloWorld = class("HelloWorld")

function HelloWorld:OnStarted()
    print("Hello World! 你好世界！")
end

function HelloWorld:OnUpdated()
    print(self.a)
end

function HelloWorld:OnDestroyed()
    print("I am destroyed 我被销毁了")
end

return HelloWorld
