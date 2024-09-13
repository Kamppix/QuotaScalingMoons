## v0.2.3

- Added optional limiters for all quota scaling config variables
- Made turning off `EnableMoonBalancing` disable scrap value adjustment based on the moon's average scrap value
- Made small changes to some config names and descriptions

## v0.2.1

- Changed apparatice and beehive value scaling to not take the moon's average scrap value into account
- Fixed apparatice value being incorrect when adjusted (it never was) and reverted `EnableApparaticeValueAdjust` default value to `true`

## v0.2.0

- Made every moon use Artifice enemy spawn chance multiplier curves to balance enemy spawning
- Set the default value of `EnableApparaticeValueAdjust` to `false` (for now)

## v0.1.1

- Added boolean config options for all patches
- Made the mod work in multiplayer

## v0.1.0

- Initial beta release born from an all-nighter (full of bugs and stuff)
