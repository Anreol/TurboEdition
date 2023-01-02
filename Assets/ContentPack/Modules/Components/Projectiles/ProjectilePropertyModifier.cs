using RoR2.Projectile;
using UnityEngine;

namespace TurboEdition.Projectiles
{
    internal class ProjectilePropertyModifier : MonoBehaviour
    {
        public ProjectileImpactExplosion projectileImpactExplosion;
        public ProjectileExplosion projectileExplosion;
        private ProjectileDamage projectileDamage;

        private void Start()
        {
            projectileDamage = GetComponent<ProjectileDamage>();
        }

        public void SetExplosionChildDamageCoeff(float coefficient)
        {
            if (projectileExplosion)
            {
                projectileExplosion.childrenDamageCoefficient = coefficient;
            }
            if (projectileImpactExplosion)
            {
                projectileImpactExplosion.childrenDamageCoefficient = coefficient;
            }
        }

        public void SetExplosionDamageCoeff(float coefficient)
        {
            if (projectileExplosion)
            {
                projectileExplosion.blastDamageCoefficient = coefficient;
            }
            if (projectileImpactExplosion)
            {
                projectileImpactExplosion.blastDamageCoefficient = coefficient;
            }
        }

        public void IncreaseDamage(float addDamage)
        {
            if (projectileDamage)
            {
                projectileDamage.damage += addDamage;
            }
        }

        public void SetDamage(float newDamage)
        {
            if (projectileDamage)
            {
                projectileDamage.damage = newDamage;
            }
        }

        public void IncreaseExplosionDamageCoeff(float coefficient)
        {
            if (projectileExplosion)
            {
                projectileExplosion.blastDamageCoefficient += coefficient;
            }
            if (projectileImpactExplosion)
            {
                projectileImpactExplosion.blastDamageCoefficient += coefficient;
            }
        }

        public void IncreaseExplosionRadius(float addRadius)
        {
            if (projectileExplosion)
            {
                projectileExplosion.blastRadius += addRadius;
            }
            if (projectileImpactExplosion)
            {
                projectileImpactExplosion.blastRadius += addRadius;
            }
        }

        public void SetExplosionChildCount(int newCount)
        {
            if (projectileExplosion)
            {
                projectileExplosion.childrenCount = newCount;
            }
            if (projectileImpactExplosion)
            {
                projectileImpactExplosion.childrenCount = newCount;
            }
        }

        public void IncreaseExplosionChildCount(int newCount)
        {
            if (projectileExplosion)
            {
                projectileExplosion.childrenCount += newCount;
            }
            if (projectileImpactExplosion)
            {
                projectileImpactExplosion.childrenCount += newCount;
            }
        }
    }
}