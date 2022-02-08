using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(CharacterBody))]
    internal class GrenadierPassiveController : NetworkBehaviour, IOnIncomingDamageServerReceiver
    {
        private bool hasEffectiveAuthority
        {
            get
            {
                return this.characterBody.hasEffectiveAuthority;
            }
        }

        //TODO: run on the client, check for body auth, doesn't have to be networked, bomblets will be projectiles so we let the projectile manager take care of networking.
        private CharacterBody characterBody;

        private bool[] localBlastArmorStates; //Used by the server for triggering the effect, used by the client to draw the HUD

        private float baseBlastArmorRechargeTime;
        private float blastArmorRechargeTime;

        [Header("Referenced Components")]
        public GenericSkill passiveSkillSlot;

        [Header("Skill Defs")]
        public SkillDef PassiveBlastArmor;

        private void Awake()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
        }

        private void OnEnable()
        {
            if (NetworkServer.active)
            {
                return;
            }
        }

        private void FixedUpdate()
        {
        }

        public void OnIncomingDamageServer(DamageInfo damageInfo) //This shouldnt need to be added directly as its built in the prefab, and HC should take it automatically
        {
            if (passiveSkillSlot)
            {
                if (passiveSkillSlot.skillDef == PassiveBlastArmor && !damageInfo.rejected && damageInfo.inflictor)
                {
                    damageInfo.damage /= 2;
                    if (characterBody.healthComponent.isHealthLow)
                        damageInfo.damage /= 2;
                    damageInfo.dotIndex = DotController.DotIndex.None;
                    damageInfo.damageType = DamageType.NonLethal;
                    damageInfo.procCoefficient = -255;
                }
            }
        }
    }
}