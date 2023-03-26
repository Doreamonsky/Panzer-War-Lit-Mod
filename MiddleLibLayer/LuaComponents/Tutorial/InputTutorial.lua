Behavior()

-- 定义属性
-- Define properties
Property = {
    keycode = {
        type = "string", -- 类型：字符串 Type: string
        value = "w"      -- 键码值：W键 Key code value: W key
    }
}


-- 定义一个名为 InputTest 的类
-- Define a class called InputTest
local InputTutorial = class("InputTutorial")

function InputTutorial:ctor()
    self.actionName = "TestAPI"
end

-- 当 InputTest 开始时调用此函数
-- Call this function when InputTest starts
function InputTutorial:OnStarted()
    -- 定义键按下的回调函数
    -- Define the key press callback function
    self.OnKeyPerformedCb = function(context)
        self:OnKeyPerformed(context)
    end
    -- 定义键取消的回调函数
    -- Define the key cancel callback function
    self.OnKeyCanceledCb = function(context)
        self:OnKeyCanceled(context)
    end

    -- 注册键输入事件
    -- Register key input event
    InputAPI.RegisterKeyInput(self.actionName, self.keycode, self.OnKeyPerformedCb, self.OnKeyCanceledCb)
end

-- 当 InputTest 被销毁时调用此函数
-- Call this function when InputTest is destroyed
function InputTutorial:OnDestroyed()
    print("On Destroyed")
    -- 注销键输入事件
    -- Unregister key input event
    InputAPI.UnregisterKeyInput(self.actionName, self.keycode, self.OnKeyPerformedCb, self.OnKeyCanceledCb)
end

-- 键按下事件处理函数
-- Key press event handler
function InputTutorial:OnKeyPerformed(context)
    print("on key pressed") -- 输出“按键按下”
end

-- 键取消事件处理函数
-- Key cancel event handler
function InputTutorial:OnKeyCanceled(context)
    print("on key canceled") -- 输出“按键取消”
end

-- 返回 InputTest 类
-- Return InputTest class
return InputTutorial
