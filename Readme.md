# Project Unity Islands
Initial vision was for procedurally generated islands with an ecosystem that the player would interact with, featuring a variety of agents with goals that they pursued irrespective of the player's own.

After initially working out how to procedurally generate islands and creating a sheep agent, the focus shifted to play around with agents and group behaviours. Flocks of sheep would roam the island "grazing" and the player could direct a sheep dog around that could "eat" the sheep. The idea here was to make things a bit technological, robo-sheep harvesting energy produced by the island substrate that they could use when flocking to defend themselves. Cos robots are cool, right? And low-poly style is about my level when it comes to modelling.

The sheep ended up being too derpy and cute, and led to shifting focus to Project Sheep Island to make a Christmas gift for a close friend in 2019.

## New Direction
Christmas 2021 I get the inspiration for a game that features, once again, sheep on floating islands - this time with a more organic theme.

The sheep come from a cottony plant that takes root on an island. In order to germinate the plant and grow it to fruiting the player needs to make the island habitable, taking it from barren rock to paradise garden. Once the criteria have been met the sheep plant buds and releases the sheep, who help bring the island up to the final level of development at which it reaches a steady state. The sheep release dandelion-like seeds to the winds, opening the way for the player to progress to the next island.

The game would be more a meditative experience than a puzzle game, with the focus to be given to pleasing interactions and visual results.

Basic development elements would be water and soil, the former generated by guiding rain clouds to dried out ponds and lakes and the latter coming first from "loam stones" scattered across the islands which the player can move and break down to create the first layers of top soil. 

Soil dampened by either proximity to collected water, repeated rain cloud passes or the player manually dragging water from ponds, produces basic vegetation with a simple looping lifecycle that ends up producing mulch. Mulch left in place will increase the fertility of the soil it's on as it degrades, or can be placed by the player onto stone to create more topsoil coverage. The player's decision here is how quickly to spread the mulch around to get more mulch production, weighed against their ability to keep it all damp enough to keep the basic vegetation growing and the need to make the soil more fertile to support more advanced vegetations that will produce the insects needed to develop the sheep plant.

Other stretch-goal features would include the presence of cute critters living in areas on the rock already which would vanish if their homes were covered by soil, giving the player a reason to not just paint the whole island in green but to have the option to explore what interactions with the indigenous life are possible. Perhaps they like to eat insects, so if you find a good balance they can co-exist peacefully with the sheep and both benefit from the terraforming the player performs. Also an advanced vegetation type could be trees, grown after the sheep have sprouted to aid in the formation of rain clouds to make the next island more hospitable - and also to give the sheep some shelter from the rain on their current island!

The sheep themselves need a bunch of work, with animations and a variety of states to display as well as improvements to the feet movement to make them better match the sheep's changes in position.

## Dependencies
- [Amplify Shader Editor](https://assetstore.unity.com/packages/tools/visual-scripting/amplify-shader-editor-68570)
- [Polaris Terrain Editor 2021](https://assetstore.unity.com/packages/tools/terrain/polaris-2021-low-poly-mesh-terrain-editor-196648)
- Unity Burst Compiler package - soft dependency for Polaris
- Unity Editor Coroutines package - soft dependency for Polaris
