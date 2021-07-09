using RoR2;
using UnityEngine;

using Buff = TurboEdition.Buffs.Buff;

namespace TurboEdition
{
    //Collection of Buffs and their logic
    internal class TurboBuffManager : MonoBehaviour
    {
        public IStatBuffBehavior[] statBuffBehaviors = new IStatBuffBehavior[] { };
        private CharacterBody body;

        private void Awake()
        {
            body = gameObject.GetComponent<CharacterBody>();
        }

        public void CheckForBuffs()
        {
            foreach (var buffRef in InitBuffs.buffList)
            {
                int count = body.GetBuffCount(buffRef.Key);
                buffRef.Value.AddBehavior(ref body, count);
                buffRef.Value.UpdateBuff(ref body, count);
            }
            statBuffBehaviors = GetComponents<IStatBuffBehavior>();
        }

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