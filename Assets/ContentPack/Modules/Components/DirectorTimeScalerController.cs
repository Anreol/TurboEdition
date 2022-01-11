using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(CombatDirector))]
    class DirectorTimeScalerController : MonoBehaviour
    {
        [Header("Difficulty Scaling")]
        [Tooltip("Number to multiply the difficulty's scalingValue by.")]
        public float baseDifficultyScaleValue = 0.5f;

        [Tooltip("Should it automatically use the scalingValue given by difficultyDefs, or the ones specified in this component. Modded difficulty defs will fallback to true.")]
        public bool useAutomaticDifficultyScalingValue = true;
        public float easyDifficultyScale = 1f;
        public float normalDifficultyScale = 2f;
        public float hardDifficultyScale = 3f;

        [Header("Time Scaling")]
        [Tooltip("Amount of seconds for this component to apply the scaling to its combat director.")]
        public float baseIntervalApplyTimer;
        [Tooltip("Minimum amount of seconds that baseIntervalApplyTimer can be.")]
        public float baseMinIntervalApplyTimer;
        [Tooltip("Amount of seconds to take away from baseIntervalApplyTimer when used.")]
        public float intervalDecreaseTime;
        [Tooltip("Amount of seconds it takes to use intervalDecreaseTime on baseIntervalApplyTimer.")]
        public float intervalDecreaseApplyTimer;
        [Tooltip("Should intervalDecreaseApplyTimer fasten with the current difficultyScaleValue. Calculation is intervalDecreaseApplyTimer / DifficultyScaleValue.")]
        public bool intervalDecreaseApplySpeedUpWithDifficulty = true;

        [Header("Combat Director's Money Waves")]
        [Tooltip("Should it generate new money waves whenever this components applies its scaling.")]
        public bool useMoneyWaveGenerator = false;
        [Tooltip("Maximum amount of Money Waves to generate, -1 for infinite.")]
        public int moneyWaveNumberToGenerate = -1;
        [Tooltip("Minimum amount of time for the generated Money Wave to activate.")]
        public float moneyWaveTimerMin = 1f;
        [Tooltip("Maximum amount of time for the generated Money Wave to activate.")]
        public float moneyWaveTimerMax = 1f;

        [Header("Combat Director's Reroll Timers")]
        [Tooltip("Should it speed up or slow down the Combat Director's reroll timers whenever this components applies its scaling.")]
        public bool useSpeedUpRerolls = false;
        [Tooltip("Amount of time to modify the Min Reroll Spawn Interval for. Negative speeds up.")]
        public float modMinRerollInterval = -1f;
        [Tooltip("Amount of time to modify the Max Reroll Spawn Interval for. Negative speeds up.")]
        public float modMaxRerollInterval = -1f;
        [Tooltip("Minimum amount of seconds the Combat Director's MIN reroll timer can have. This component won't decrease it past this value.")]
        public float minimumMinCombatRerollSpawn = 10f;
        [Tooltip("Minimum amount of seconds the Combat Director's MAX reroll timer can have. This component won't decrease it past this value.")]
        public float minimumMaxCombatRerollSpawn = 15f;

        [Header("Misc Options")]
        [Tooltip("Amount to sum to the Combat Director's Credit whenever this component applies its scaling. Zero to disable.")]
        public int modCreditAdd = 0;
        [Tooltip("Amount to sum to the Combat Director's Credit Multiplier whenever this component applies its scaling. Zero to disable.")]
        public float modCreditMultiplier = 0.05f;

        private CombatDirector[] _combatDirectors;
        private float _difficultyScaleValue;
        private int _moneyWavesGenerated;
        private float _baseIntervalApplyTimer;
        private float _intervalApplyTimer;
        private float _intervalDecreaseApplyTimer;
        private void Awake()
        {
            if (!NetworkServer.active) //uNet Weaver doesnt like [Server] Tags on something that isnt a network behavior
                return;
            _combatDirectors = gameObject.GetComponents<CombatDirector>();
            _difficultyScaleValue = baseDifficultyScaleValue * GetDifficultyScale();
            if (_difficultyScaleValue <= 0f) //Someone forgot to add a tag AND a scaling value...
                _difficultyScaleValue = 1f;
            _baseIntervalApplyTimer = baseIntervalApplyTimer;
            _intervalApplyTimer = _baseIntervalApplyTimer;
            _intervalDecreaseApplyTimer = intervalDecreaseApplySpeedUpWithDifficulty ? intervalDecreaseApplyTimer / _difficultyScaleValue : _difficultyScaleValue;
        }
        private void FixedUpdate()
        {
            if (!NetworkServer.active) //uNet Weaver doesnt like [Server] Tags on something that isnt a network behavior
                return;
            this._intervalApplyTimer -= Time.fixedDeltaTime;
            if (_baseIntervalApplyTimer >= baseMinIntervalApplyTimer)
                this._intervalDecreaseApplyTimer -= Time.fixedDeltaTime;
            
            if (_intervalApplyTimer <= 0f)
            {
                ApplyScale();
                _intervalApplyTimer = Mathf.Max(_baseIntervalApplyTimer, baseMinIntervalApplyTimer);
            }
            if (_intervalDecreaseApplyTimer <= 0f && _baseIntervalApplyTimer >= baseMinIntervalApplyTimer)
            {
                _baseIntervalApplyTimer -= intervalDecreaseTime;

                _intervalDecreaseApplyTimer = intervalDecreaseApplySpeedUpWithDifficulty ? intervalDecreaseApplyTimer / _difficultyScaleValue : _difficultyScaleValue;
            }
        }
        private float GetDifficultyScale()
        {
            if (useAutomaticDifficultyScalingValue)
                return DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;
            switch (DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).serverTag)
            {
                case "dz":
                    return easyDifficultyScale;
                case "rs":
                    return normalDifficultyScale;
                case "mn":
                    return hardDifficultyScale;
                default: //Someone forgot to tag their difficulty
                    return DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;
            }
        }
        private void ApplyScale()
        {
            foreach (CombatDirector item in _combatDirectors)
            {
                if (item.enabled)
                {
                    if (useMoneyWaveGenerator && (_moneyWavesGenerated <= moneyWaveNumberToGenerate || moneyWaveNumberToGenerate == -1) )
                    {
                        if (moneyWaveNumberToGenerate > -1)
                            _moneyWavesGenerated++;
                        RangeFloat rangeFloat = new RangeFloat
                        {
                            min = moneyWaveTimerMin,
                            max = moneyWaveTimerMax
                        };
                        HG.ArrayUtils.ArrayAppend(ref item.moneyWaveIntervals, rangeFloat); 
                    }
                    if (useSpeedUpRerolls && (item.minRerollSpawnInterval > minimumMinCombatRerollSpawn || item.maxRerollSpawnInterval > minimumMaxCombatRerollSpawn) )
                    {
                        item.minRerollSpawnInterval = Mathf.Max(item.minRerollSpawnInterval += modMinRerollInterval, minimumMinCombatRerollSpawn);
                        item.maxRerollSpawnInterval = Mathf.Max(item.maxRerollSpawnInterval += modMaxRerollInterval, minimumMaxCombatRerollSpawn);
                    }
                    item.monsterCredit += modCreditAdd;
                    item.creditMultiplier += modCreditMultiplier;
                }
            }
        }
    }
}
