local profiler = class("profiler")

function profiler:ctor()
    self.lastTime = nil
    self.frameStartTime = nil
    self.frameEndTime = nil
    self.averageCostTime = 0
    self.enableProfiler = false
end

function profiler:EnableProfile()
    self.enableProfiler = true
    self.hookHandler = function(event)
        self:Hook(event)
    end

    debug.sethook(self.hookHandler, "cr")
end

function profiler:DisableProfile()
    self.enableProfiler = false
    debug.sethook(nil)
end

function profiler:Hook(event)
    if event == "call" then
        if self.frameStartTime == nil then
            self.frameStartTime = os.clock()
        end
    elseif event == "return" then
        if self.frameStartTime ~= nil then
            local now = os.clock()
            self.averageCostTime = self.averageCostTime + (now - self.frameStartTime)
            self.frameStartTime = now
        end
    end
end

function profiler:OnProfile()
    local costTime = self.averageCostTime
    self.frameStartTime = nil
    self.frameEndTime = nil
    self.averageCostTime = 0

    return costTime
end

return profiler
