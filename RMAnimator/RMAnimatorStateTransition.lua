---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/10/9 11:40
---

---@class RMAnimatorStateTransition
---@field solo Boolean
---@field mute Boolean
---@field hasExitTime Boolean
---@field hasFixedDuration Boolean
---@field offset Float
---@field duration Float
---@field exitTime Float
---@field destinationStateName String
---@field conditions RMAnimatorCondition[]
local RMAnimatorStateTransition = XECS.class("RMAnimatorStateTransition")

function RMAnimatorStateTransition:ctor(hasExitTime, hasFixedDuration, offset, duration, exitTime, destinationStateName, solo, mute, conditions)
    self.solo = solo
    self.mute = mute
    self.hasExitTime = hasExitTime
    self.hasFixedDuration = hasFixedDuration
    self.offset = offset
    self.duration = duration
    self.exitTime = exitTime
    self.destinationStateName = destinationStateName
    self.conditions = conditions or {}
end

function RMAnimatorStateTransition:AddConditions(conditions)
    if not conditions then
        return
    end
    for _, t in ipairs(conditions) do
        table.insert(self.conditions, t)
    end
end

function RMAnimatorStateTransition:ClearConditions()
    self.conditions = {}
end

function RMAnimatorStateTransition:AddCondition(condition)
    if not condition then
        return
    end
    for _, t in ipairs(self.conditions) do
        if t == condition then
            return
        end
    end
    table.insert(self.conditions, condition)
end

function RMAnimatorStateTransition:RemoveCondition(condition)
    if not condition then
        return
    end
    for i, t in ipairs(self.conditions) do
        if t == condition then
            table.remove(self.conditions, i)
            break
        end
    end
end

function RMAnimatorStateTransition:DeepCopy()
    local conditions = {}
    for _, condition in ipairs(self.conditions) do
        table.insert(conditions, condition:DeepCopy())
    end
    return RMAnimatorStateTransition.new(self.hasExitTime, self.hasFixedDuration, self.offset, self.duration, self.exitTime, self.destinationStateName, self.solo, self.mute, conditions)
end

---@param ctrl RMAnimatorController
---@param onlySolo Boolean
---@param time Float
---@param prevTime Float
---@param length Float
function RMAnimatorStateTransition:CanToDestinationState(ctrl, onlySolo, time, prevTime, length)
    if onlySolo and not self.solo then
        return false
    end
    if self.mute then
        return false
    end

    if self.hasExitTime then
        local result, dValue = ctrl:IsReachingThreshold(time, prevTime, length, self.exitTime * length)
        if result and self:_CheckConditions(ctrl, time, prevTime) then
            return true, dValue
        else
            return false, ctrl.context.fZero
        end
    else
        return self:_CheckConditions(ctrl, time, prevTime), ctrl.context.fZero
    end
end

function RMAnimatorStateTransition:_CheckConditions(ctrl, time, prevTime)
    if time == prevTime then
        return false
    end
    for _, condition in ipairs(self.conditions) do
        if not condition:IsMeet(ctrl) then
            return false
        end
    end
    return true
end

return RMAnimatorStateTransition