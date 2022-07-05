using System;
using System.Collections.Generic;
using UnityEngine;
using Halluvision.GameplayTag;
using Halluvision.Coroutine;

namespace Halluvision.AbilitySystem
{
    [RequireComponent(typeof(AbilityStats))]
    public class AbilitySystem : MonoBehaviour
    {
        //Defining Delegate
        public delegate void OnQuitDelegate();
        public OnQuitDelegate quitDelegate;
        
        [SerializeField]
        private List<Ability> _playerAbilityReferences;
        [SerializeField, GameplayTag]
        private List<int> _acquiredAbilityTags; // Save this
        [SerializeField]
        private bool _useCloneAbilities;
        [SerializeField]
        private bool _initializeAbilitiesOnAwake = true;
        [SerializeField, GameplayTag, ReadOnly]
        private List<int> _abilityTagsPreview;

        private List<int> _runningAbilityTags;
        private Dictionary<int, Ability> _allAbilitiesDic;
        private Dictionary<Type, Ability> _typeAbilitiesDic;

        private List<Ability> _playerAbilities = new List<Ability>();
        private bool _lockAbilities = false;
        private AbilityStats _abilityStats;
        public AbilityStats Stats { get { return _abilityStats; } }

        private void Awake()
        {
            _allAbilitiesDic = new Dictionary<int, Ability>();
            _typeAbilitiesDic = new Dictionary<Type, Ability>();
            _runningAbilityTags = new List<int>();
            
            _abilityStats = GetComponent<AbilityStats>();

            if (_initializeAbilitiesOnAwake)
                GetAbilitiesFromTags();

            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void Start()
        {
            if (!_initializeAbilitiesOnAwake)
                GetAbilitiesFromTags();
        }

        private void GetAbilitiesFromTags()
        {
            foreach (var _ability in _playerAbilityReferences)
            {
                if (_acquiredAbilityTags.Contains(_ability.AbilityGameplayTag))
                {
                    if (_useCloneAbilities)
                    {
                        Ability ability = Instantiate<Ability>(_ability);
                        AcquireAbility(ability);
                    }
                    else
                        AcquireAbility(_ability);
                }
            }
        }

        private void Update()
        {
            if (GameStateManager.Instance.CurrentGameState == GameState.Pause || _lockAbilities)
                return;

            foreach (Ability _ability in _playerAbilities)
            {
                _ability.AbilityIsAllowed = _ability.CanUseAbility();

                if (_ability.AbilityIsAllowed || _ability.AbilityIsRunning || _ability.RunUpdateAnyway)
                {
                    if (_ability.NeedUpdate)
                        _ability.Update();
                }
            }
        }

        private void FixedUpdate()
        {
            foreach (Ability _ability in _playerAbilities)
            {

                if (_ability.AbilityIsAllowed || _ability.AbilityIsRunning)
                    if (_ability.NeedFixedUpdate)
                        _ability.FixedUpdate();
            }
        }

        public bool HasAbility(Ability _ability)
        {
            return _playerAbilities.Contains(_ability);
        }

        public List<int> GetRunningAbilityIDs()
        {
            return _runningAbilityTags;
        }

        public bool AddAbilityTag(int _tag)
        {
            if (_runningAbilityTags.Contains(_tag))
                return false;
            else
            {
                _runningAbilityTags.Add(_tag);
                _abilityTagsPreview = _runningAbilityTags;
            }
            return true;
        }

        public bool RemoveAbilityTag(int _tag)
        {
            if (!_runningAbilityTags.Contains(_tag))
                return false;
            else
            {
                _runningAbilityTags.Remove(_tag);
                _abilityTagsPreview = _runningAbilityTags;
            }
            return true;
        }

        public bool IsAbilityRunning(int _tag)
        {
            if (_runningAbilityTags.Contains(_tag))
                return true;
            else
                return false;
        }

        public bool HasAbilityTag(int _tag)
        {
            if (_runningAbilityTags.Contains(_tag))
                return true;
            else
                return false;
        }

        private void AcquireAbility(Ability _ability)
        {
            if (!_playerAbilities.Contains(_ability))
            {
                _playerAbilities.Add(_ability);
                UpdateStaticAbilities();
                if (_ability.InitializeAfterAcquire)
                    _ability.Initialize(this);
            }
        }

        public void AcquireAbility(int _abilityTag)
        {
            if (_acquiredAbilityTags.Contains(_abilityTag))
            {
                Debug.Log("Ability exist.");
                return;
            }

            foreach (var _ability in _playerAbilityReferences)
            {
                if (_ability.AbilityGameplayTag == _abilityTag)
                {
                    _acquiredAbilityTags.Add(_abilityTag);
                    AcquireAbility(_ability);
                }
            }
        }

        private void LooseAbility(Ability _ability)
        {
            if (_playerAbilities.Contains(_ability))
                _playerAbilities.Remove(_ability);
        }

        private void UpdateStaticAbilities()
        {
            foreach (Ability _ability in _playerAbilities)
            {
                if (!_allAbilitiesDic.ContainsKey(_ability.AbilityGameplayTag))
                    _allAbilitiesDic.Add(_ability.AbilityGameplayTag, _ability);

                if (!_typeAbilitiesDic.ContainsKey(_ability.GetType()))
                    _typeAbilitiesDic.Add(_ability.GetType(), _ability);
            }
        }

        public T GetAbilityByType<T>()
        {
            if (_typeAbilitiesDic.ContainsKey(typeof(T)))
            {
                return (T)(_typeAbilitiesDic[typeof(T)] as object);
            }
            return default(T);
        }

        public Ability GetAbilityByTag(int _abilityID)
        {
            if (_allAbilitiesDic.ContainsKey(_abilityID))
                return _allAbilitiesDic[_abilityID];
            else
                return null;
        }

        public void LockAbilities(bool _lock)
        {
            _lockAbilities = _lock;
        }

        public void FinishAllAbilities()
        {
            foreach (var _ability in _playerAbilities)
            {
                if (_ability.AbilityIsRunning)
                    _ability.FinishAbility();
            }
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            foreach (var _ability in _playerAbilities)
            {
                if (_ability.AbilityIsRunning)
                    _ability.OnGameStateChanged(newGameState);
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Ability _ability in _playerAbilities)
            {
                if (_ability.AbilityIsRunning)
                    _ability.OnDrawGizmos();
            }
        }

#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            quitDelegate?.Invoke();
        }
#endif
    }
}