local profiler = class("profiler")

function profiler:ctor()
    self.last_time = nil
    self.frame_start_time = nil
    self.frame_end_time = nil
end

function profiler:EnableProfile()
    debug.sethook(self.Hook, "cr")
end

function profiler:DisableProfile()
    debug.sethook(nil)
end

function profiler:Hook(event)
    if event == "call" then
        if self.frame_start_time == nil then
            self.frame_start_time = os.clock()
        end
    elseif event == "return" then
        self.frame_end_time = os.clock()
    end
end

function profiler:OnProfile()
    if self.frame_start_time and self.frame_end_time then
        local delta_time = self.frame_end_time - self.frame_start_time
        print("上一帧所有函数耗时: " .. delta_time .. " 秒")
    end

    self.frame_start_time = nil
    self.frame_end_time = nil
end

return profiler
