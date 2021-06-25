using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;

namespace TurboEdition.Items
{
    public abstract class Item
    {
        public abstract ItemDef itemDef { get; set; }

        /// <summary>
        /// For the love of god PLEASE use this as minimally as possible for hooks, use itemBehaviors wherever possible
        /// </summary>
        public virtual void Initialize()
        {
        }

        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }
    }
}