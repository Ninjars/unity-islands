using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldGenerator;

namespace Game {
    public class WorldData {
        public readonly string worldString;
        public readonly int randomSeedValue;
        public readonly List<IslandData> islands;
        public readonly List<GameObject> islandObjects;

        public WorldData(string worldString, int randomSeedValue, List<IslandData> islands, List<GameObject> islandObjects) {
            this.worldString = worldString;
            this.randomSeedValue = randomSeedValue;
            this.islands = islands;
            this.islandObjects = islandObjects;
        }
    }

    public class IslandData {
        public readonly List<WorldGenerator.ConnectedCoord> undersideCoords;

        public readonly List<WorldGenerator.Center> centers;
		public readonly List<WorldGenerator.Corner> corners;
        public readonly List<Vector3> centerVerts;
		public readonly List<Vector3> cornerVerts;
        public readonly Vector3 center;
        public readonly Bounds topsideBounds;
        public readonly Bounds totalBounds;

        public IslandData(Vector3 center, Bounds topSideBounds, Bounds totalBounds, List<ConnectedCoord> undersideCoords, List<Center> centers, List<Corner> corners) {
            this.undersideCoords = undersideCoords;
            this.centers = centers;
            this.corners = corners;
            this.center = center;
            this.topsideBounds = topSideBounds;
            this.totalBounds = totalBounds;

            centerVerts = centers.Select(c => c.coord.toVector3()).ToList();
            cornerVerts = corners.Select(c => c.coord.toVector3()).ToList();
        }
    }
}
