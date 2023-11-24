using EntityStates;
using EntityStates.AI.Walker;
using RoR2;
using RoR2.CharacterAI;
using TurboEdition.EntityStates.AI.Walker;

namespace TurboEdition.Buffs
{
    public class BuffPanicked : Buff
    {
        public override BuffDef buffDef { get; set; } = TEContent.Buffs.Panic;

        private bool forceStateExitOnBuffLose = false;

        public override void Initialize()
        {
        }

        public override void BuffStep(ref CharacterBody body, int stack)
        {
        }

        public override void OnBuffFirstStackGained(ref CharacterBody body)
        {
            BaseAI baseAI = body.master?.GetComponent<BaseAI>();
            if (baseAI != null)
            {
                for (int i = 0; i < body.timedBuffs.Count; i++)
                {
                    if (body.timedBuffs[i].buffIndex == buffDef.buffIndex)
                    {
                        ForceFlee forceFlee = new ForceFlee();
                        forceFlee.fleeDuration = body.timedBuffs[i].timer;
                        baseAI.stateMachine.SetNextState(forceFlee);

                        //Get ALL skills on cooldown
                        if (body.skillLocator.primary)
                            body.skillLocator.primary.DeductStock(1);
                        if (body.skillLocator.secondary)
                            body.skillLocator.secondary.DeductStock(1);
                        if (body.skillLocator.utility)
                            body.skillLocator.utility.DeductStock(1);
                        if (body.skillLocator.special)
                            body.skillLocator.special.DeductStock(1);

                        //Interrupt attack
                        EntityStateMachine.FindByCustomName(body.gameObject, "Weapon")?.SetNextStateToMain();

                        //Set state on hurt is what the game uses to set other machines other than body to Idle when stunned or frozen or shocked.
                        SetStateOnHurt setStateOnHurt = body.gameObject.GetComponent<SetStateOnHurt>();
                        if (setStateOnHurt != null)
                        {
                            for (int x = 0; x < setStateOnHurt.idleStateMachine.Length; x++)
                            {
                                setStateOnHurt.idleStateMachine[x].SetNextState(new Idle());
                            }
                        }
                        return;
                    }
                }
            }

            //Remove buff if its not AI
            body.RemoveBuff(buffDef.buffIndex);
        }

        public override void OnBuffLastStackLost(ref CharacterBody body)
        {
            BaseAI baseAI = body.master?.GetComponent<BaseAI>();
            if (baseAI != null && forceStateExitOnBuffLose)
            {
                baseAI.stateMachine.SetNextState(new LookBusy());
            }
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            if (body.skillLocator.primary)
                body.skillLocator.primary.cooldownScale += 0.25f;
            if (body.skillLocator.secondary)
                body.skillLocator.secondary.cooldownScale += 0.25f;
            if (body.skillLocator.utility)
                body.skillLocator.utility.cooldownScale += 0.25f;
            if (body.skillLocator.special)
                body.skillLocator.special.cooldownScale += 0.25f;
        }
    }
}