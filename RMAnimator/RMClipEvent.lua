﻿---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/11/10 10:39
---

---@class RMFrameEvent
---@field frameIndex number 第几帧
---@field time Fix 第几帧对应的时间点，单位秒（自动计算的）
---@field eventName string 事件名
---@field eventArgs table<string, any> 由事件双方约定好

---@class RMClipEvent
local RMClipEvent = XECS.class("RMClipEvent")

---@param ctrl RMAnimatorController
---@param frameEvents RMFrameEvent[]
---@param length Float
function RMClipEvent:ctor(ctrl, frameEvents, length)
    self._ctrl = ctrl
    self._frameEvents = frameEvents or {}

    for _, event in ipairs(self._frameEvents) do
        if not event.time then
            event.time = self._ctrl.context:GetTimeByFrame(event.frameIndex)
        end
    end
    self._length = length or (#self._frameEvents > 0 and self._frameEvents[#self._frameEvents].time or self._ctrl.context.fZero)
end

function RMClipEvent:Restart(startingTime, frameEventFunc)
    self._boundedTime = startingTime
    self._frameEventFunc = frameEventFunc
    self._frameIndex = 1
    for index, event in ipairs(self._frameEvents) do
        if event.time >= startingTime then
            self._frameIndex = index
            break
        end
    end
end

function RMClipEvent:Evaluate(deltaTime)
    if deltaTime <= self._ctrl.context.fZero then
        return
    end

    self._boundedTime = self._boundedTime + deltaTime
    while self._boundedTime > self._length do
        self._boundedTime = self._boundedTime - self._length
    end

    if self._boundedTime - deltaTime >= self._ctrl.context.fZero then
        self._frameIndex = self:_CheckEvent(self._frameIndex, self._boundedTime)
    else
        self._frameIndex = self:_CheckEvent(self._frameIndex, self._length)
        self._frameIndex = self:_CheckEvent(1, self._boundedTime)
    end
end

function RMClipEvent:_CheckEvent(startIndex, endingTime)
    local index, event = startIndex, nil
    while true do
        event = self._frameEvents[index]
        if not event or event.time > endingTime then
            break
        end
        index = index + 1
        self._frameEventFunc(event.eventName, event.eventArgs)
    end
    return index
end

return RMClipEvent