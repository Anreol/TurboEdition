# Turbo Edition Alpha
> **Note:** This is a very heavy **WIP** mod, **issues and bugs** can arise.
> The mod has been tested and *most features* are multiplayer compatible.

Turbo Edition is a content pack mod, that for now, it adds a couple of weird items and equipments.
Based off Risk Of Rain's Turbo Edition, it has a few returning items and new ones, but the goal of this mod is not to be a direct port, so keep in mind that some items will never come back, but it adds new ones, (and new features! *Soon!* )
Keep in mind that this is a single man made mod, so if you expected something big, I'm sorry to break it to you, but I cannot do everything at once!

Installation looks like
- plugins
	- TurboEdition
		- TurboEdition.dll
		- language AND assetbundles folders
## Why?
I'm pretty bad at coming up with names for what I make, and I followed Turbo Edition's development and just liked what it was.
I also find *"Turbo"* a funny word that goes along pretty well with a lot of other words. You can probably think of a few ones.

## Features
- 15 Awful items to ruin your host with.
- 1 Equipment that probably do something useful.
- 2 Artifacts to make your run harder, for some reason.

Unfinished content will be marked with a WIP icon, but keep in mind that with mods, anything can go wrong anytime, especially when you are loading 500 different mods at once!

## Planned Features

- Finish currently shipped content. That is the equipments and one artifact, along with probably models.
- Add a new Shrine, this turned out to have a very heavy refactoring so I didn't end up finishing it in time, maybe you know what it is about already!
- Survivors. I have up to 5 in mind, but the first one will probably be a simple one.
- More worthwhile items and equipment: Hopoo is adding over 60 items at once with the DLC, and there's countless mods already out there, meaning that items aren't really attracting me, so coming up with good items will be harder!

## Credits
- Bubbet & Moffein for giving me a hand in a few things and showing me how dumb some of my practices are.
- Kevin from HP support, learned a awful ton of Unity and proper practices thanks to my time spent in the SS2 team. Also, how the mod loads items is taken from there!
- Special thanks to Gnome, for introducing me to modding.
- Special thanks to JoeyAsagiri for making me want to make a mod myself. And by making the original mod!

I'm open to contributions, specifically models, as they take me more time than what I wished. You can contact me on your platform of choice, but if you choose Steam, and you have a suspicious profile, I'll probably reject you.

## Changelog
`0.0.5`
- Plugin should no longer report debug logs to console unless its a debug assembly
- Plugin now marks the game as modded
- Balance changes
	- Fan of Blades
		- Projectile speed 100 -> 250
		- Lifetime 10s -> 5s
		- Capsule radius 0.4 -> 0.6
		- Increased gravity

	- UVB-51 Radio
		- Removed time of day of the current stage as buff requirement, now any kind of stage gives off both armor and health regen bonuses
		- Initial stack 15s -> 45s
		- Later stacks 10s -> 30s
		- Now grants +0.05 sprint bonus per stage.
		- Now grants +0.025 attack speed bonus per stage.
		- Final stage bonus x1.25 -> x1.15
		
*Changes meant to make the item more reliable, and to aid the player in scouting the stage at arrival.*
*Sprint and attack speed bonuses are minimal as to not out-weight right away the items they are based off (Soda and syringes), but should become pretty significant post-loop. Final stage bonus decreased as the buff gives more than just defense now.*
	- Magnetic Belt
		- Initial stack radius 8m -> 16m
		- Later stacks 4m -> 8m
		- Initial stack duplication chance 0.5% -> 1%
		- Later stack duplication chances 0.4% -> 0.5%
		- Cloned pickups now have doubled acceleration and maximum speed.

*I initially took the radius values from the Warbanner but divided by half, as I thought it would be a good starting point, but as I played on my own, and as other players reported, the item struggles to be noticeable at all.*
*It now should grab monster tooths for melee and ranged survivors alike. Duplication chance increased as I thought it could be nice to have, but mostly remains for clover purposes, speed increased to guarantee that the player that duplicated them gets them.*
- Artifacts
	- Artifact of Worms
		- 🌧 Elite Worms if Honor is active can now be enabled/disabled with a newly added rule
		- Each worm segment will now spawn half less meatballs than the default worms.
		- Each worm segment now has a extra second of impact cooldown than the default worms.
		- Director maximum number to spawn in a wave 6 -> 3
		- Now scales slightly less over time.
		- Minimum and maximum combat rerolls interval modifications -1 -> -0.5. Minimum reroll minimum and minimum reroll maximum stay at 10.
		- Can now have up to 30 additional money waves. Previously it kept generating new money waves infinitely.

*I've modified the way that worms spawn, so it might not behave the same way as it did before. For example, the director maximum number in a wave change, was because of that*
*Other than that, this mainly aims to make worms not melt your performance by stage 3 due to the ridiculous high amount of them spawning. You could also say it also helps a bit with players getting 12 fire stacks out of nowhere and dying.*
*Worms will keep spawning post-teleporter event for now, but I'd like to know the general consensus on this.*
- Fixes
	- Fixed Artifact of Worms modifying the original worm character spawn cards
	

`0.0.4`
- Hotfix
	- Added the new placeholder model to items that I accidentally skipped over
	
`0.0.3`
- Misc changes
	- Added a better placeholder model for the items that need it, it might be a little small in-game
	- Added camera parameters to the sandbag
	- Removed comment from Typewriter and added it to its lore. Item description now indicates what it does.

`0.0.2`
- Fast patch
	- Removed the color changing from Chromatic Lenses for now
	- Removed placeholder tokens in the Spanish localization

`0.0.1`
- Initial Release
	- Happy New Years!

## Rainfusion
You can find the original Turbo Edition by JoeyAsagiri on [rainfusion.ml](https://rainfusion.ml/) and a direct download can be found [here](https://cdn.rainfusion.ml/download-mod/18f68f57-bcfd-4979-873c-6df90c33e353/turbo_edition_0.3.1.zip). The original repository can be found [here](https://github.com/JoeySmulders/RoR-Turbo-Edition).
