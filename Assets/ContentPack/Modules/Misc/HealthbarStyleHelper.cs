using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.UI
{
    public static class HealthbarStyleHelper
    {
        //Should keep all local health bar data instances, regardless of player or splitscreen.
        public static List<HealthBarData> barDataInstances = new List<HealthBarData>();

        [SystemInitializer]
        public static void Initialize()
        {
            On.RoR2.UI.HealthBar.CheckInventory += (orig, instance) =>
            {
                orig(instance);
                if (!instance.source.body || !instance.source.body.inventory)
                {
                    return;
                }
                //Mimics CheckInventory() from HealthBar
                foreach (HealthBarData barData in barDataInstances)
                {
                    if (barData.watcher == instance.source)
                    {
                        barData.CheckInventory(ref instance);
                    }
                }
            };
            On.RoR2.UI.HealthBar.UpdateBarInfos += (orig, instance) =>
            {
                orig(instance);
                if (instance.source != null)
                {
                    var healthBarValues = instance.source.GetHealthBarValues();
                    foreach (HealthBarData barData in barDataInstances)
                    {
                        if (barData.watcher == instance.source)
                        {
                            barData.UpdateBarInfo(ref barData.barInfo, healthBarValues, ref instance);
                        }
                    }
                }
            };

            IL.RoR2.UI.HealthBar.ApplyBars += ApplyBar;
        }

        public static void ApplyBar(ILContext il)
        {
            var c = new ILCursor(il);

            var cls = -1;
            FieldReference fld = null;
            c.GotoNext(
                x => x.MatchLdloca(out cls),
                x => x.MatchLdcI4(0),
                x => x.MatchStfld(out fld)
            );

            //Patch this.barAllocator.AllocateElements to add our active bar count alongside the original HealthBar.BarInfoCollection.GetActiveCount
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<HealthBar.BarInfoCollection>(nameof(HealthBar.BarInfoCollection.GetActiveCount))
            );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<int, HealthBar, int>>((i, healthBarSource) =>
            {
                i += barDataInstances.Count(barInfo => barInfo.barInfo.enabled && barInfo.watcher == healthBarSource.source);
                return i;
            });

            c.Index = il.Instrs.Count - 2;
            c.Emit(OpCodes.Ldloca, cls);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloca, cls);
            c.Emit(OpCodes.Ldfld, fld);
            c.EmitDelegate<Func<HealthBar, int, int>>((healthBarSource, i) =>
            {
                foreach (HealthBarData barInfo in barDataInstances)
                {
                    if (barInfo.watcher == healthBarSource.source)
                    {
                        barInfo.HandleBar(ref barInfo.barInfo, ref i, ref healthBarSource);
                    }
                }
                return i;
            });
            c.Emit(OpCodes.Stfld, fld);
        }

        /// <summary>
        /// Something akin to a StatBar, instead of having an already existing object manage it, it manages itself, akin to Items or Equipment...
        /// </summary>
        public abstract class HealthBarData
        {
            //Whoever will be observing it
            public HealthComponent watcher;

            //Struct, has to be updated whenever it comes from the outside
            public HealthBar.BarInfo barInfo;

            public HealthBarStyle.BarStyle CachedStyle
            {
                get
                {
                    if (_cachedStyle == null)
                    {
                        _cachedStyle = GetStyle();
                        return (HealthBarStyle.BarStyle)_cachedStyle;
                    }
                    return (HealthBarStyle.BarStyle)_cachedStyle;
                }
                set
                {
                    _cachedStyle = value;
                }
            }

            public abstract HealthBarStyle.BarStyle GetStyle();

            private HealthBarStyle.BarStyle? _cachedStyle;

            public virtual void UpdateBarInfo(ref HealthBar.BarInfo info, HealthComponent.HealthBarValues healthBarValues, ref HealthBar healthBarInstance)
            {
                var style = CachedStyle;
                ApplyStyle(ref info, ref style);
            }

            /// <summary>
            /// Only called whenever the inventory changes! Don't do stuff here that requires constant updating, but only once!
            /// </summary>
            /// <param name="healthBar"></param>
            public virtual void CheckInventory(ref HealthBar healthBar)
            { }

            public virtual void HandleBar(ref HealthBar.BarInfo barInfo, ref int currentBarIndex, ref HealthBar healthBarInstance)
            {
                if (barInfo.enabled)
                {
                    Image image = healthBarInstance.barAllocator.elements[currentBarIndex];
                    image.type = barInfo.imageType;
                    image.sprite = barInfo.sprite;
                    image.color = barInfo.color;
                    SetRectPosition((RectTransform)image.transform, barInfo.normalizedXMin, barInfo.normalizedXMax, barInfo.sizeDelta);
                    currentBarIndex++;
                }
            }

            private static void ApplyStyle(ref HealthBar.BarInfo barInfo, ref HealthBarStyle.BarStyle barStyle)
            {
                barInfo.enabled &= barStyle.enabled;
                barInfo.color = barStyle.baseColor;
                barInfo.sprite = barStyle.sprite;
                barInfo.imageType = barStyle.imageType;
                barInfo.sizeDelta = barStyle.sizeDelta;
            }

            private static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
            {
                rectTransform.anchorMin = new Vector2(xMin, 0f);
                rectTransform.anchorMax = new Vector2(xMax, 1f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
            }
        }
    }
}