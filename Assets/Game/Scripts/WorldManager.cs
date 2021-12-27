using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Game {
    public class WorldManager : MonoBehaviour, WorldProvider {
        public bool debugDrawDelauney = false;
        public bool debugDrawCornerConnections = false;
        public bool debugDrawDownlopes = false;

        private WorldData worldData;

        public WorldData generateWorld(WorldGenerator.WorldConfig config) {
            List<WorldGenerator.Island> islands = WorldGenerator.Generator
                .generateWorld(config)
                .OrderByDescending(island => island.simpleExtents.max.magnitude)
                .ToList();

            List<GameObject> islandObjects = WorldGenerator.Generator.createIslandObjects(config, islands);

            worldData = new WorldData(
                config.worldName,
                config.seed,
                islands
                    .Select(island => new IslandData(island.center, island.topsideBounds, island.totalBounds, island.undersideCoords, island.centers, island.corners))
                    .ToList(),
                islandObjects
            );
            return worldData;
        }

        public WorldData getWorldData() {
            return worldData;
        }

        void OnDrawGizmos() {
            if (worldData == null) {
                return;
            }
            if (debugDrawDelauney) {
                foreach (IslandData island in worldData.islands) {
                    foreach (WorldGenerator.Center center in island.centers) {
                        foreach (WorldGenerator.Center neigh in center.neighbours) {
                            if (neigh.index > center.index) {
                                Debug.DrawLine(
                                    center.coord.toVector3(),
                                    neigh.coord.toVector3()
                                );
                            }
                        }
                    }
                }
            }
            if (debugDrawDownlopes) {
                foreach (IslandData island in worldData.islands) {
                    foreach (WorldGenerator.Corner corner in island.corners) {
                        if (corner.downslope != null) {
                            Debug.DrawLine(
                                corner.coord.toVector3(),
                                corner.downslope.coord.toVector3(),
                                Color.red
                            );
                        }
                    }
                }
            }
            if (debugDrawCornerConnections) {
                foreach (IslandData island in worldData.islands) {
                    foreach (WorldGenerator.Corner corner in island.corners) {
                        foreach (WorldGenerator.Center center in corner.GetTouches()) {
                            Debug.DrawLine(
                                center.coord.toVector3(),
                                corner.coord.toVector3(),
                                Color.green
                            );
                        }
                    }
                }
            }
        }
    }
}
