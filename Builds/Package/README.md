# Turbo Edition
### **Note:** This is a very heavy **WIP** mod, **issues and bugs** can arise.
The mod has been tested and while *most features* are multiplayer compatible, some things may not work.

Turbo Edition is a content pack mod, that for now, it adds a couple of weird items and equipments.
Based off Risk Of Rain's Turbo Edition, it has a few returning items and new ones, but the goal of this mod is not to be a direct port, so keep in mind that some items will never come back, but it adds new ones, (and new features! *Soon!* )
Keep in mind that this is a single man made mod, so if you expected something big, I'm sorry to break it to you, but I cannot do everything at once.

Installation looks like
- plugins
	- TurboEdition
		- TurboEdition.dll
		- language, assetbundles and soundbank folders
## Why?
I'm pretty bad at coming up with names for what I make, and I followed Turbo Edition's development and just liked what it was.
I also find *"Turbo"* a funny word that goes along pretty well with a lot of other words. You can probably think of a few ones.

## Features
You can click each category to access its page on the [wiki](https://thunderstore.io/package/Anreol/TurboEdition/wiki/)

- 17 [Awful items to ruin your host with.](https://thunderstore.io/package/Anreol/TurboEdition/wiki/668-items/) (Some of them are currently disabled)
- 3 [Equipments that probably do something useful.](https://thunderstore.io/package/Anreol/TurboEdition/wiki/669-equipment/)
- 3 [Artifacts to make your run harder, for some reason.](https://thunderstore.io/package/Anreol/TurboEdition/wiki/670-artifacts)
	- One of them available as a wave in Simulacrum

Keep in mind that with mods, anything can go wrong anytime, especially when you are loading 500 different mods at once, let me know if something breaks.

## Contributors
- Some items modelled by Big Nut.
- Imp Trasher models and animations by DotFlare.
- Expansion Icon by SOM.
- Grenadier model by Anon.
	- Texturing & animation by DotFlare.
- Assistance from Moffein.
- Kevin from HP support, for experience in SS2 and some part of the codebase.
- Special thanks to /vm/ for playtesting and /agdg/ for assistance.
- Special thanks to Gnome, for introducing me to modding.
- Special thanks to JoeyAsagiri for making me want to make a mod myself. And by making the original mod.

## Contact
For any issues, please create a report [in my Github page](https://github.com/Anreol/TurboEdition/issues).
I ***will not*** accept *any other means of communication*.


## Changelog
Click [here](https://rentry.org/TurboEditionChangelog) to access the [full Changelog](https://rentry.org/TurboEditionChangelog)

### Latest release

`0.1.10` Dedicated server optimizations
- Fixes
	- Hopefully fixed an error that happened due to modifying the mod's healthbar collection while it was being iterated on
	- Hopefully fixed a NRE due to a IL hook related to MarkAsUnableToBeLocked
	- Updated scriptable object name lookups in PackMagnetBodyBehavior to fix a NRE

- Dedicated server optimizations
	- Healthbar code will now not run if the game is ran as dedicated server or in batch mode
	- Temporary visual effect code will now not run if the game is ran as dedicated server or in batch mode

- Updated BepInEx's plugin version to the current version (prev 0.1.6) which otherwise would lead to possible net version mismatches

## Rainfusion
You can find the original Turbo Edition by JoeyAsagiri on [rainfusion.ml](https://rainfusion.ml/) and a direct download can be found [here](https://cdn.rainfusion.ml/download-mod/18f68f57-bcfd-4979-873c-6df90c33e353/turbo_edition_0.3.1.zip). The original repository can be found [here](https://github.com/JoeySmulders/RoR-Turbo-Edition).
