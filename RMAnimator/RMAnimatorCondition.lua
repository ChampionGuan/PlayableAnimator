﻿---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/10/9 11:37
---

---@class RMAnimatorConditionType
RMAnimatorConditionType = {
    If = 1,
    IfNot = 2,
    Greater = 3,
    Less = 4,
    Equals = 6,
    NotEqual = 7,
}

---@class RMAnimatorCondition
---@field type RMAnimatorConditionType
---@field parameterName String
---@field threshold Float
local RMAnimatorCondition = XECS.class("RMAnimatorCondition")

function RMAnimatorCondition:ctor(type, parameterName, threshold)
    self.type = type
    self.parameterName = parameterName
    self.threshold = threshold
end

---@param ctrl RMAnimatorController
function RMAnimatorCondition:IsMeet(ctrl)
    if not self.parameterName then
        return false
    end
    if not self._parameter or self._parameter.ctrl ~= ctrl then
        ---@type RMAnimatorParameter
        self._parameter = ctrl:GetParameter(self.parameterName)
    end
    if not self._parameter then
        return
    end

    if self._parameter.type == RMAnimatorParameterType.Trigger then
        return self._parameter.value
    end
    if self.type == RMAnimatorConditionType.If then
        return self._parameter.type == RMAnimatorParameterType.Bool and self._parameter.value
    elseif self.type == RMAnimatorConditionType.IfNot then
        return self._parameter.type == RMAnimatorParameterType.Bool and not self._parameter.value
    elseif self.type == RMAnimatorConditionType.Equals then
        return self._parameter.type == RMAnimatorParameterType.Int and self._parameter.value == self.threshold
    elseif self.type == RMAnimatorConditionType.NotEqual then
        return self._parameter.type == RMAnimatorParameterType.Int and self._parameter.value ~= self.threshold
    elseif self.type == RMAnimatorConditionType.Greater then
        return (self._parameter.type == RMAnimatorParameterType.Float and self._parameter.value > self.threshold) or
                (self._parameter.type == RMAnimatorParameterType.Int and self._parameter.value > self.threshold)
    elseif self.type == RMAnimatorConditionType.Less then
        return (self._parameter.type == RMAnimatorParameterType.Float and self._parameter.value < self.threshold) or
                (self._parameter.type == RMAnimatorParameterType.Int and self._parameter.value < self.threshold)
    end
    return false
end

function RMAnimatorCondition:DeepCopy()
    return RMAnimatorCondition.new(self.type, self.parameterName, self.threshold)
end

return RMAnimatorCondition