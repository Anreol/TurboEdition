using RoR2;
using UnityEngine;

using Buff = TurboEdition.Buffs.Buff;

namespace TurboEdition
{
    //Collection of Buffs and their logic
    internal class TurboBuffManager : MonoBehaviour
    {
        public IStatBuffBehavior[] statBuffBehaviors = new IStatBuffBehavior[] { };

        //public Buff[] detectedBuffs = new Buff[] { };
        private CharacterBody body;

        private void Awake()
        {
            body = gameObject.GetComponent<CharacterBody>();
            Debug.LogWarning("Hello! " + body);
        }

        public void CheckForBuffs()
        {
            foreach (var buffRef in InitBuffs.buffList)
            {
                int count = body.GetBuffCount(buffRef.Key);
                Debug.LogWarning("Check check! " + count);
                buffRef.Value.AddBehavior(ref body, count);
                buffRef.Value.UpdateBuff(ref body, count);
            }
            statBuffBehaviors = GetComponents<IStatBuffBehavior>();
        }

        public void OnBuffFinalStackLost(Buff buff)
        {
            /*int i = System.Array.IndexOf(detectedBuffs, buff);
            if (i > -1)
                HG.ArrayUtils.ArrayRemoveAtAndResize(ref detectedBuffs, detectedBuffs.Length, i);*/
        }

        public void RunStatRecalculationsStart()
        {
            foreach (var statBehavior in statBuffBehaviors)
                statBehavior.RecalculateStatsStart();
            foreach (var buffRef in InitBuffs.buffList)
                buffRef.Value.RecalcStatsStart(ref body);
        }

        public void RunStatRecalculationsEnd()
        {
            foreach (var statBehavior in statBuffBehaviors)
                statBehavior.RecalculateStatsEnd();
            foreach (var buffRef in InitBuffs.buffList)
                buffRef.Value.RecalcStatsEnd(ref body);
        }
    }
}