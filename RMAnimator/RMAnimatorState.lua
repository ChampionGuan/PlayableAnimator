---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/10/9 11:38
---

---@class RMAnimatorStateInfo
---@field name String
---@field tag String
---@field isLooping Boolean
---@field length Float
---@field normalizedTime Float
---@field speed Float
---@field repeatedCount Int
---@field formSubStateMachineName String
---@field transitions RMAnimatorStateTransition[]

---@class RMAnimatorStateStatusType
local EInternalStatusType = {
    Exit = 0,
    PrepExit = 1,
    Enter = 2,
    PrepEnter = 3,
}

---@class RMAnimatorState
---@field name String
---@field tag String
---@field defaultSpeed Float
---@field speedParameterName String
---@field speedParameterActive Boolean
---@field motion RMMotion
---@field transitions RMAnimatorStateTransition[]
---@field speed Float
---@field formSubStateMachineName String
---@field _status RMAnimatorStateStatusType
---@field _toDestinationStateFunc fun(...)
---@field _stateNotifyFunc fun(...)
local RMAnimatorState = XECS.class("RMAnimatorState")

function RMAnimatorState:ctor(ctrl, name, tag, defaultSpeed, speedParameterName, speedParameterActive, formSubStateMachineName, motion, transitions)
    self._ctrl = ctrl
    self._status = EInternalStatusType.Exit
    self.name = name
    self.tag = tag
    self.defaultSpeed = defaultSpeed
    self.speedParameterName = speedParameterName
    self.speedParameterActive = speedParameterActive
    self.formSubStateMachineName = formSubStateMachineName
    self.motion = motion
    self.transitions = transitions or {}
end

function RMAnimatorState:OnEnter(enterTime, prevState)
    if self._status ~= EInternalStatusType.Exit then
        return
    end
    self._internalWeight = -self._ctrl.context.fOne
    self._prevState = prevState
    self._entryTimeOffset = self._ctrl.context.fZero
    self._entryTime = enterTime
    self._runningTime = enterTime
    self._runningPrevTime = enterTime
    self._prevLength = self:Length()
    self._status = EInternalStatusType.PrepEnter
    self._stateNotifyFunc(RMStateNotifyType.PrepEnter, self.name)
end

function RMAnimatorState:OnUpdate(deltaTime, weight)
    if self._status == EInternalStatusType.Exit then
        return
    end

    if self._status == EInternalStatusType.PrepEnter then
        self._status = EInternalStatusType.Enter
        self._stateNotifyFunc(RMStateNotifyType.Enter, self.name)
        self.motion:OnEnter(self._entryTime)
    end

    deltaTime = deltaTime * self.speed
    if self._prevLength ~= self:Length() then
        self._entryTimeOffset = (self._entryTime + self._entryTimeOffset) / self._prevLength * self:Length() - self._entryTime
        self._prevLength = self:Length()
    end

    if self._internalWeight ~= weight then
        self._internalWeight = weight
        self.motion:SetWeight(self._internalWeight)
    end

    self._entryTime = self._entryTime + deltaTime
    self._runningPrevTime = self._runningTime
    self._runningTime = self._entryTime + self._entryTimeOffset
    self.motion:SetTime(self._runningTime)

    if self._status == EInternalStatusType.PrepExit then
        self._fadeOutTick = self._fadeOutTick - deltaTime
        if self._fadeOutTick <= self._ctrl.context.fZero then
            self._status = EInternalStatusType.Exit
            self._stateNotifyFunc(RMStateNotifyType.Exit, self.name)
            self.motion:OnExit()
        end
    end

    if self._ctrl:IsReachingThreshold(self._runningTime, self._runningPrevTime, self:Length(), self:Length()) then
        self._stateNotifyFunc(RMStateNotifyType.Complete, self.name)
    end

    if self._status == EInternalStatusType.PrepEnter or self._status == EInternalStatusType.Enter then
        self:_CheckToDestinationState()
    end
end

function RMAnimatorState:Evaluate()
    return self.motion:Evaluate()
end

function RMAnimatorState:OnExit(fadeOutDuration, nextState)
    if self._status == EInternalStatusType.Exit then
        return
    end

    self._nextState = nextState
    self._fadeOutTick = fadeOutDuration
    self._status = fadeOutDuration > self._ctrl.context.fZero and EInternalStatusType.PrepExit or EInternalStatusType.Exit
    self._stateNotifyFunc(RMStateNotifyType.PrepExit, self.name)

    if self._status == EInternalStatusType.Exit then
        self._stateNotifyFunc(RMStateNotifyType.Exit, self.name)
        self.motion:OnExit()
    end
end

function RMAnimatorState:OnDestroy()
    self.motion:OnDestroy()
end

function RMAnimatorState:SetSpeed(speed)
    self.speed = speed
    self.defaultSpeed = speed
    self.speedParameterActive = false
end

function RMAnimatorState:AddTransitions(transitions)
    if not transitions then
        return
    end
    for _, t in ipairs(transitions) do
        table.insert(self.transitions, t)
    end
end

function RMAnimatorState:ClearTransitions()
    self.transitions = {}
end

function RMAnimatorState:AddTransition(transition)
    if not transition then
        return
    end
    for _, t in ipairs(self.transitions) do
        if t == transition then
            return
        end
    end
    table.insert(self.transitions, transition)
end

function RMAnimatorState:RemoveTransition(transition)
    if not transition then
        return
    end
    for i, t in ipairs(self.transitions) do
        if t == transition then
            table.remove(self.transitions, i)
            break
        end
    end
end

function RMAnimatorState:DeepCopy()
    local transitions = {}
    for _, t in ipairs(self.transitions) do
        table.insert(transitions, t:DeepCopy())
    end
    return RMAnimatorState.new(self._ctrl, self.name, self.tag, self.defaultSpeed, self.speedParameterName, self.speedParameterActive, self.formSubStateMachineName, self.motion:DeepCopy(), transitions)
end

---@return Float
function RMAnimatorState:Length()
    return self.motion.length > self._ctrl.context.fZero and self.motion.length or self._ctrl.context.fOne
end

---@return Float
function RMAnimatorState:NormalizedTime()
    return self.motion.normalizedTime
end

---@return Float
function RMAnimatorState:RepeatedCount()
    return self.motion.repeatedCount
end

---@return Float
function RMAnimatorState:IsLoop()
    return self.motion.isLoop
end

---@return boolean
function RMAnimatorState:IsValid(ctrl)
    return self._ctrl == ctrl and self._isValid
end

---@param ctrl RMAnimatorController
function RMAnimatorState:Rebuild(ctrl, toDestinationStateAction, stateNotifyFunc, eventNotifyFunc)
    self.motion:Rebuild(ctrl, eventNotifyFunc)

    self._isValid = true
    self._ctrl = ctrl
    self._stateNotifyFunc = stateNotifyFunc
    self._toDestinationStateFunc = toDestinationStateAction
    self.speed = self.defaultSpeed or ctrl.context.fOne

    if self.speedParameterActive then
        local parameter = ctrl:GetParameter(self.speedParameterName)
        if parameter then
            parameter.onValueChanged:RemoveListener(self, self._OnParameterValueChanged)
            parameter.onValueChanged:AddListener(self, self._OnParameterValueChanged)
            self.speed = self.defaultSpeed * parameter.value
        end
    end
end

function RMAnimatorState:_CheckToDestinationState()
    if self._status == EInternalStatusType.Exit or not self._toDestinationStateFunc then
        return
    end

    local onlySolo = false
    for _, transition in ipairs(self.transitions) do
        if transition.sole then
            onlySolo = true
            break
        end
    end

    for _, transition in ipairs(self.transitions) do
        local result, dValue = transition:CanToDestinationState(self._ctrl, onlySolo, self._runningTime, self._runningPrevTime, self:Length())
        if result then
            self._toDestinationStateFunc(transition.destinationStateName, dValue / self.speed, transition.offset,
                    transition.hasFixedDuration and transition.duration / self:Length() or transition.duration)
            break
        end
    end
end

---@param parameter RMAnimatorParameter
function RMAnimatorState:_OnParameterValueChanged(parameter)
    if not parameter or self.speedParameterName ~= parameter.name or not self.speedParameterActive then
        return
    end
    self.speed = self.defaultSpeed * parameter.value
end

return RMAnimatorState