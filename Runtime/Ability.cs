using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Halluvision.GameplayTag;
using Halluvision.Coroutine;

namespace Halluvision.AbilitySystem
{
    public abstract class Ability : ScriptableObject
    {
        [Header("Common")]
        public string AbilityName = "New Ability";
        public float AbilityBaseCoolDown = 0f;
        public float InputBufferDuration = 0f;
        public float EnergyCost = 0;
        [ReadOnly]
        public float CoolDownTimeLeft = 0f;
        public bool NeedUpdate = true;
        public bool NeedFixedUpdate = false;
        [HideInInspector]
        public bool RunUpdateAnyway = false;
        public bool InitializeAfterAcquire = true;
        public bool NeedAllAllowedTags = false;
        public AudioClip AbilitySoundClip;
        
        [HideInInspector]
        public bool Initialized = false;
        [ReadOnly]
        public bool AbilityIsRunning = false;
        [HideInInspector]
        public bool AbilityIsAllowed;

        [Header("Gameplay Tags")]
        [GameplayTag]
        public int AbilityGameplayTag;
        [GameplayTag]
        public List<int> BlockedByAbilities;
        [GameplayTag]
        public List<int> AllowedByAbilities;
        [GameplayTag]
        public List<int> FinishAbilitiesOnStart;
        [GameplayTag]
        public List<int> FinishAbilitiesOnEnd;

        private CoroutineHandle inputBufferCoroutine = new CoroutineHandle();
        protected AbilitySystem _ownerSystem;
        protected AbilityStats _abilityStats;
        protected Transform _transform;
        protected Rigidbody _rigidbody;

        public void Initialize(AbilitySystem _owner)
        {
            Initialized = true;
            _ownerSystem = _owner;
            _ownerSystem.quitDelegate += ResetAbility;
            _transform = _ownerSystem.transform;
            _rigidbody = _owner.GetComponent<Rigidbody>();
            OnInitialized();
            RegisterInput();
        }

        protected virtual void OnInitialized() { }

        public bool CanUseAbility()
        {
            if (CoolDownTimeLeft > 0)
                return false;

            if (!HasEnoughEnergy())
                return false;

            if (IsAbilityBlockedByTags())
                return false;

            if (!IsAbilityAllowedByTags())
                return false;
            
            if (CheckAbilityConditions())
                return true;
            return false;
        }

        protected virtual bool CheckAbilityConditions() { return true; }

        protected bool HasEnoughEnergy()
        {
            if (EnergyCost > 0)
                return _abilityStats.Energy >= EnergyCost;
            else
                return true;
        }

        protected void ConsumeEnergy()
        {
            if (EnergyCost > 0)
                _abilityStats.ConsumeEnergy(EnergyCost);
        }

        public bool IsAbilityBlockedByTags()
        {
            foreach (var _tag in BlockedByAbilities)
            {
                if (_ownerSystem.GetRunningAbilityIDs().Contains(_tag))
                    return true;
            }
            return false;
        }

        public bool IsAbilityAllowedByTags()
        {
            if (NeedAllAllowedTags)
            {
                foreach (var _tag in AllowedByAbilities)
                {
                    if (!_ownerSystem.GetRunningAbilityIDs().Contains(_tag))
                        return false;
                }
                return true;
            }
            else
            {
                if (AllowedByAbilities.Count == 0)
                    return true;
                foreach (var _tag in AllowedByAbilities)
                {
                    if (_ownerSystem.GetRunningAbilityIDs().Contains(_tag))
                        return true;
                }
                return false;
            }
        }

        protected virtual void RegisterInput() { }

        protected virtual void BufferInput(InputAction.CallbackContext callback)
        {
            inputBufferCoroutine.Stop();
            inputBufferCoroutine = new CoroutineHandle(InputBufferTimer());
        }

        protected void RunAbility()
        {
            AbilityIsRunning = true;
            _ownerSystem.AddAbilityTag(AbilityGameplayTag);

            if (AbilityBaseCoolDown > 0)
                _ownerSystem.StartCoroutine(CoolDown());

            ConsumeEnergy();

            foreach (var _tag in FinishAbilitiesOnStart)
            {
                Ability _ability = _ownerSystem.GetAbilityByTag(_tag);
                if (_ability == null)
                    continue;
                if (_ability.AbilityIsRunning)
                    _ability.FinishAbility();
            }

            AbilityStarted();
        }

        protected virtual void AbilityStarted()
        { }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void RawUpdate() { }

        public bool FinishAbility()
        {
            if (AbilityIsRunning)
            {
                OnFinishAbility();
                AbilityEnded();
                return true;
            }
            else
                return false;
        }

        protected void AbilityEnded()
        {
            AbilityIsRunning = false;
            _ownerSystem.RemoveAbilityTag(AbilityGameplayTag);

            foreach (var _tag in FinishAbilitiesOnEnd)
            {
                var _ability = _ownerSystem.GetAbilityByTag(_tag);
                if (_ability.AbilityIsRunning)
                    _ability.FinishAbility();
            }
        }

        protected virtual void OnFinishAbility() { }

        public virtual void OnGameStateChanged(GameState newGameState) { }

        private IEnumerator CoolDown()
        {
            CoolDownTimeLeft = AbilityBaseCoolDown;
            while (CoolDownTimeLeft > 0)
            {
                yield return new WaitForEndOfFrame();
                CoolDownTimeLeft -= Time.deltaTime;
                CoolDownTimeLeft = Mathf.Max(CoolDownTimeLeft, 0);
            }
        }

        private IEnumerator InputBufferTimer()
        {
            float timer = 0;
            while (timer <= InputBufferDuration)
            {
                if (!AbilityIsRunning && AbilityIsAllowed)
                {
                    RunAbility();
                    yield break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
        }

        private void ResetAbility()
        {
            CoolDownTimeLeft = 0;
            FinishAbility();
        }

        public virtual void OnDrawGizmos()
        {

        }
    }
}