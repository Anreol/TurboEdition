using System.Reflection;
using R2API;
using RoR2;
using UnityEngine;

namespace Anreol.TurboEdition
{
    internal static class MeleeArmor
    {
        internal static GameObject MeleeArmorPrefab;
        internal static ItemIndex MeleeArmorItemIndex;

        private const string PrefabPath = null;
        private const string IconPath = null;

        internal static void Init()
        {
            // First registering your AssetBundle into the ResourcesAPI with a modPrefix that'll also be used for your prefab and icon paths
            // note that the string parameter of this GetManifestResourceStream call will change depending on
            // your namespace and file name
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomItem.rampage"))
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider(TurboEdition.assetPrefix.TrimEnd(':'), bundle);
                ResourcesAPI.AddProvider(provider);

                MeleeArmorPrefab = bundle.LoadAsset<GameObject>("Assets/Import/belt/belt.prefab");
            }

            defineItem();
            AddLanguageTokens();
        }

        private static void defineItem()
        {
            ItemDef itemdef = new ItemDef
            {
                // TODO get shortIdentifier + "_Whatever" working, doesnt currently exist in this context
                name = "MELEEARMOR_NAME",
                nameToken = "MELEEARMOR_NAME", //? Still needed if we are assigning name in the line above?
                pickupToken = "MELEEARMOR_PICKUP",
                descriptionToken = "MELEEARMOR_DESC",
                //loreToken = "CARNIVOROUSSLUG_LORE",
                tier = ItemTier.NoTier,
                //You can create your own icons and prefabs through assetbundles, but to keep this boilerplate brief, we'll be using question marks.
                pickupIconPath = "@Combiner:Assets/Combiner/texCarnivorousSlugIcon.png",
                pickupModelPath = "Prefabs/PickupModels/PickupMystery",
                //Can remove determines if a shrine of order, or a printer can take this item, generally true, except for NoTier items.
                canRemove = false,
                //Hidden means that there will be no pickup notification, and it won't appear in the inventory at the top of the screen. This is useful for certain noTier helper items, such as the DrizzlePlayerHelper.
                hidden = false
            };

            var itemDisplayRules = new ItemDisplayRule[1]; // keep this null if you don't want the item to show up on the survivor 3d model. You can also have multiple rules !
            itemDisplayRules[0].followerPrefab = MeleeArmorPrefab; // the prefab that will show up on the survivor
            itemDisplayRules[0].childName = "Chest"; // this will define the starting point for the position of the 3d model, you can see what are the differents name available in the prefab model of the survivors
            itemDisplayRules[0].localScale = new Vector3(0.15f, 0.15f, 0.15f); // scale the model
            itemDisplayRules[0].localAngles = new Vector3(0f, 180f, 0f); // rotate the model
            itemDisplayRules[0].localPos = new Vector3(-0.35f, -0.1f, 0f); // position offset relative to the childName, here the survivor Chest

            var meleeArmor = new R2API.CustomItem(itemdef, itemDisplayRules);

            MeleeArmorItemIndex = ItemAPI.Add(meleeArmor); // ItemAPI sends back the ItemIndex of your item
        }


        //This will be deprecated soon, probably
        //TODO Find a way to load stuff from a .txt like vanilla game, easier to manage and easier to edit
        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("BISCOLEASH_NAME", "Bisco's Leash");                    //Item Name
            LanguageAPI.Add("BISCOLEASH_PICKUP", "Gain Rampage stack on kill");     //Short Item description
            LanguageAPI.Add("BISCOLEASH_DESC",                                      //Detailed Item description. Used in logbook.
                "Grants <style=cDeath>RAMPAGE</style> on kill. \n<style=cDeath>RAMPAGE</style> : Specifics rewards for reaching kill streaks. \nIncreases <style=cIsUtility>movement speed</style> by <style=cIsUtility>1%</style> <style=cIsDamage>(+1% per item stack)</style> <style=cStack>(+1% every 20 Rampage Stacks)</style>. \nIncreases <style=cIsUtility>damage</style> by <style=cIsUtility>2%</style> <style=cIsDamage>(+2% per item stack)</style> <style=cStack>(+2% every 20 Rampage Stacks)</style>.");
            LanguageAPI.Add("BISCOLEASH_LORE",                                      //This is where we make the shitposts
                "You are right about one thing.\nI do need capital. And votes.\nWanna know why?\n'I have a dream'\n\nWhat?\n\nThat one day every person in this nation will control their OWN destiny.\nA land of the TRULY free, damnit.\nA nation of ACTION, not words.\nRuled by STRENGHT, not committee.\nWhere the law changes to suit the individual, not the other way around\nWhere power and justice are back where they belong: in the hands of the people!\nWhere every man is free to think -- to act -- for himself!\n" +
                "Fuck all these limp-dick lawyers and chicken-shit bureaucrats.\nFuck their 24/7 internet spew of trivia and celebrity bullshit.\nFuck 'American Pride.' Fuck the media!\nFuck all of it!\nAmerica is diseased. Rotten to the core.\nThere's no saving it -- we need to pull it out by the roots.\nWipe the slate clean. BURN IT DOWN!\nAnd from the ashes a new America will be born.\nEvolved, but untamed!\nThe weak will be purged, and the strongest will thrive -- free to live as they see fit,\nthey'll make America great again!\n\nWhat the hell are you talking about...");
        }
    }
}