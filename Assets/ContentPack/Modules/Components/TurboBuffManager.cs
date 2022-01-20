using RoR2;
using UnityEngine;

//using System.Linq;

namespace TurboEdition
{
    //Collection of Buffs and their logic
    internal class TurboBuffManager : MonoBehaviour
    {
        public IStatBuffBehavior[] statBuffBehaviors = new IStatBuffBehavior[] { };
        private BuffDef[] activeBuffsList = new BuffDef[InitBuffs.buffList.Count];
        private CharacterBody body;

        private void Awake()
        {
            body = gameObject.GetComponent<CharacterBody>();
        }

        public void CheckForBuffs()
        {
            int i = 0; //I cant be bothered to transform the below into a for...
            foreach (var buffRef in InitBuffs.buffList)
            {
                int count = body.GetBuffCount(buffRef.Key);
                if (count > 0 && activeBuffsList[i] == null)
                {
                    activeBuffsList[i] = buffRef.Value.buffDef;
                    buffRef.Value.OnBuffFirstStackGained(ref body);
                }
                else if (activeBuffsList[i] && count < 1)
                {
                    buffRef.Value.OnBuffLastStackLost(ref body);
                    activeBuffsList[i] = null;
                }
                buffRef.Value.AddBehavior(ref body, count);
                buffRef.Value.BuffStep(ref body, count);
                i++;
            }
            statBuffBehaviors = GetComponents<IStatBuffBehavior>();
        }

        //Calculation done in pickups to avoid rehooking
        public void RunStatRecalculationsStart(CharacterBody self)
        {
            foreach (var statBehavior in statBuffBehaviors)
                statBehavior.RecalculateStatsStart(ref self);
            foreach (var buffRef in InitBuffs.buffList)
            {
                if (body.HasBuff(buffRef.Key))
                {
                    buffRef.Value.RecalcStatsStart(ref self);
                }
            }
        }

        public void RunStatRecalculationsEnd()
        {
            foreach (var statBehavior in statBuffBehaviors)
                statBehavior.RecalculateStatsEnd();
            foreach (var buffRef in InitBuffs.buffList)
            {
                if (body.HasBuff(buffRef.Key))
                {
                    buffRef.Value.RecalcStatsEnd(ref body);
                }
            }
        }
    }
}