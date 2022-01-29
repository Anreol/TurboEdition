# Turbo Edition Alpha
### **Note:** This is a very heavy **WIP** mod, **issues and bugs** can arise.
The mod has been tested and while *most features* are multiplayer compatible, some things may not work.

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
- More worthwhile items and equipment: Hopoo is adding over 40 items at once with the DLC, and there's countless mods already out there, meaning that items aren't really attracting me, so coming up with good items will be harder!

## Credits
- Bubbet & Moffein for giving me a hand in a few things and showing me how dumb some of my practices are.
- Kevin from HP support, learned a awful ton of Unity and proper practices thanks to my time spent in the SS2 team. Also, how the mod loads items is taken from there!
- Special thanks to Gnome, for introducing me to modding.
- Special thanks to JoeyAsagiri for making me want to make a mod myself. And by making the original mod!

I'm open to contributions, specifically models, as they take me more time than what I wished. You can contact me on your platform of choice, but if you choose Steam, and you have a suspicious profile, I'll probably reject you.

## Changelog
`0.0.5`

And like that, January is gone.
- Plugin should no longer report debug logs to console unless its a debug assembly
- Plugin now marks the game as modded
- Added stock images to most items. Only the placeholder lunar and the placeholder equipment has the old ones.
- Assetbundle is now chunk compressed, for smaller file size (14mb -> 2mb) and for faster load times.
- Disabled unfinished content (except unfinished artifact, that's 100% up to the player)
	- Disabled typewriter as I come up with a better concept.
	- Disabled Bloody cross and big scythe as they are still unfinished.
	- These items are just hidden or disabled from being dropped. Third-party mods might give these items to enemies or something else regardless of availability.

### Balance changes
- Voice Modulator
	- Now panics enemies
		- Panic forces enemies to run around and no longer attack you.
		- Increases afflicted enemies' cooldowns by x0.5
		- Removes a stack from all their abilities when afflicted.
		- Forces them to sprint.
		- Adds movement speed bonus.
		- Won't attack as long as they are panicked.
	- Stack radius 2m -> 3.5m
	- Initial debuff duration 6 -> 10
		- Debuff duration no longer increases with stacks.
		- Champions and bosses no longer candidates for debuff.
		- Additionally, bigger enemies have less chances of getting afflicted by the debuff.
	- Guaranteed trigger On combat enter -> On safety exit
	- Now can also trigger depending on % damage taken, similar to old stealthkit.
		- Getting to safety recharges both triggers.
		
*This shifts the behavior into a slightly more defense-wise item: Safety instead of combat makes triggering easier for range characters as they no longer have to wait to be next to the enemy to start shooting, and as a bonus, it triggers when taking heavy damage.*

*As this makes enemies ignore you completely, bosses won't be affected and bigger enemies have less chance of affliction, it might help with any kind trash mob and wisp, but no way it will give you a free pass for all those titans and vagrants.*
- Fan of Blades
	- Projectile speed 100 -> 250
	- Lifetime 10s -> 5s
	- Capsule radius 0.4 -> 0.6
	- Increased gravity & decreased force.
	- Damage 250% -> 285%
	- Now affected by aim direction.
		
*Still most useful at point blank angle, but now at least your knives wont instantly be stuck in the ground. Allows pretty much anybody to snipe any enemy on utility skill use, even melee survivors.*
- UVB-51 Radio
	- Item entirely reworked
	- Reveals 10% of the stage's interactables, +5 interactables per additional item stack for one minute when entering the stage.
		- Team Wide stacking.
		- Wont reveal teleporter, barrels, vehicles, bazaar upgrades and proxy interactions.
	- Revealed things (From this item or radars) have a 25% chance of revealing the contents inside, +5% per stack.
		- Depending on each player.
		- You have to be within 12 meters of the interactor, and it scans for them every 10 seconds.
		- Succesfully revealing the contents extends the indicator for 30 more seconds.
		- Affected by luck
	- Radio towers should now scan the whole map like the Scanner equipment if whoever uses it has the radio.
		
*The previous concept seemed unreliable for a green item, as its purpose was to aid the player in scouting the stage at the start, which the defense or attack buffs didn't help at all with.*

*The old concept will return in the future as a different item*
- Blood Economy
	- Amount of damage accumulated needed to be rewarded 650 -> 250
		
*Too hard to pull off very early in the run and very unrewarding for a green, while the amount accumulated is still unknown to the player, I plan to address that sometime in the future when I get more assets.*
- Magnetic Belt
	- Initial stack radius 8m -> 16m
	- Later stacks 4m -> 8m
	- Initial stack duplication chance 0.5% -> 1%
	- Later stack duplication chances 0.4% -> 0.5%
	- Cloned pickups now have doubled acceleration and maximum speed.

*I initially took the radius values from the Warbanner but divided by half, as I thought it would be a good starting point, but as I played on my own, and as other players reported, the item struggles to be noticeable at all.*
*It now should grab monster tooths for melee and ranged survivors alike. Duplication chance increased as I thought it could be nice to have, but mostly remains for clover purposes, speed increased to guarantee that the player that duplicated them gets them.*
- Nanomachines
	- Initial stack buffs 1 times -> 2 times
- Super Stickies
	- Is now functional.
	- Initial damage 180%x3 -> 540%
	- Stacking changed from +1 explosion per stack to +180% damage

*Changes made to don't stress as much on performance*
- Artifacts
	- Artifact of Worms
		- 🌧 Elite Worms if Honor is active can now be enabled/disabled with a newly added rule
		- Each worm segment will now spawn half less meatballs than the default worms.
		- Each worm segment now has a extra second of impact cooldown than the default worms.
		- Director maximum number to spawn in a wave 6 -> 3
		- Now scales slightly less over time.
		- Minimum and maximum combat rerolls interval modifications -1 -> -0.5. Minimum reroll minimum and minimum reroll maximum stay at 10.
		- Can now have up to 30 additional money waves. Previously it kept generating new money waves infinitely.

*I've modified the way that worms spawn, so it might not behave the same way as it did before. For example, the director maximum number in a wave change.*

*Other than that, this mainly aims to make worms not melt your performance by stage 3 due to the ridiculous high amount of them spawning. You could also say it also helps a bit with players getting 12 fire stacks out of nowhere and dying.*

*Worms will keep spawning post-teleporter event for now, but I'd like to know the general consensus on this.*
- Fixes
	- Fixed equipments not working, at all.
	- Fixed Artifact of Worms modifying the original worm character spawn cards.
	- Fixed Fan of Blades not having ghosts.
	- Playing Cards is now server only. Should fix unintended behavior.
	- Nanomachines is now server only.
	- Added an extra condition to the Ejection Lever, it now should let players trigger it if the teleporter is the state previous to stage exit.
	- Initial radius of Voice Modulator should now be fixed.
	- Super stickies should be now properly affected by luck.
	- Engineer turrets now will never inherit extra items from Red Tape Roll
	- Fixed some logic in Red Tape Roll that let unintended items to be given
	- Fixed logic so now Red Tape Roll truly works with mechanical summons only, before it gave items to summons like squids or beetles.
		- This can be reverted at any time so the item can copy items to any summon, but for now i'll leave it here.
	
- Known issues
	- Impale or stick projectiles (knifes and stickybombs) sometimes fly in the opposite direction at high speeds after hitting an enemy.
	- Super stickies have broken materials in their particle systems, I am currently unable to fix this
	- Knives spawn with the wrong orientation, this is purely visual.
	- Enemies affected by panic won't run away if they got debuffed the moment they spawned.
	
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
