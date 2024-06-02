using System.Collections.Generic;
using System.Linq;
using Model.Runtime.Projectiles;
using UnityEngine;
using Model;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        List<Vector2Int> _targetOutOfRange = new List<Vector2Int>();
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
            {
                return;
            }

            IncreaseTemperature();

            for (int i = 0; i < currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
           
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int targetPosition = _targetOutOfRange.Count > 0 ? _targetOutOfRange[0] : unit.Pos;
            if (IsTargetInRange(targetPosition)) 
            {
                return unit.Pos;
            }
            else return unit.Pos.CalcNextStepTowards(targetPosition);
            
            
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 1.4 (1st block, 4rd module)
            ///////////////////////////////////////
            List<Vector2Int> result = new List<Vector2Int>(GetAllTargets());
            _targetOutOfRange.Clear();                      
            float minDistance = float.MaxValue;
            Vector2Int firstTarget = Vector2Int.zero;          
                        
            if (result.Count > 0)
            {
                foreach (Vector2Int target in GetAllTargets()) 
                {

                    if (minDistance >= DistanceToOwnBase(target))
                    {

                        minDistance = DistanceToOwnBase(target);
                        firstTarget = target;
                    }

                    if (minDistance < float.MaxValue)
                    {
                        if (IsTargetInRange(firstTarget))
                        {
                            result.Add(firstTarget);
                        }
                        _targetOutOfRange.Add(firstTarget);
                    }
                    else
                    {
                        Vector2Int enemyBase = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
                        result.Add(enemyBase);

                    }
                }

                
            }
            
            

            result.Clear();
            result.Add(firstTarget);
            
            return result;
                        
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}