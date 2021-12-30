using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TriangleNet.Voronoi;
using System.Linq;

namespace WorldGenerator {
    public struct WorldConfig {
        public readonly string worldName;
        public readonly int seed;
        public readonly Material topSideMaterial;
        public readonly Material undersideMaterial;
        public readonly float size;
        public readonly float uvScale;
        public readonly int pointCount;
        public readonly float clipPercentile;
        public readonly List<GameObject> navAgents;

        public WorldConfig(string worldName, int seed, Material topSideMaterial, Material undersideMaterial, float size,  float uvScale, int pointCount, float clipPercentile, List<GameObject> navAgents) {
            this.worldName = worldName;
            this.seed = seed;
            this.topSideMaterial = topSideMaterial;
            this.undersideMaterial = undersideMaterial;
            this.size = size;
            this.uvScale = uvScale;
            this.pointCount = pointCount;
            this.clipPercentile = clipPercentile;
            this.navAgents = navAgents;
        }
    }

    public class Generator {
        public static List<Island> generateWorld(WorldConfig config) {
            DebugLoggingTimer timer = new DebugLoggingTimer();
            timer.begin("generateWorld");

            // create 2d voronoi-derived graph, with centers, edges and corners
            Graph graph = generateGraph(config.seed, config.size, config.pointCount);

            timer.logEventComplete("generateGraph()");

            WorldGenElevation elevationGenerator = new WorldGenElevation(graph, config.clipPercentile);
            elevationGenerator.generateElevations();
            timer.logEventComplete("generateElevations()");

            List<Island> islands = findIslands(graph);
            Debug.Log("island count " + islands.Count);
            timer.logEventComplete("findIslands()");

            World world = new World(config.seed, config.size, graph, islands);
            timer.logEventComplete("instantiateWorld()");

            // WorldGenBiomes.separateTheLandFromTheWater(world, new PerlinIslandShape(seed, worldSize));
            return islands;
        }

        public static List<GameObject> createIslandObjects(WorldConfig config, List<Island> islands) {
            DebugLoggingTimer timer = new DebugLoggingTimer();
            timer.begin("createIslandObjects");

            List<GameObject> islandObjects = new List<GameObject>(islands.Count);
            foreach (var island in islands) {
                timer.begin($"starting island {island.islandId}");
                // initialise gameobject
                GameObject gameObj = new GameObject();
                gameObj.name = $"Island {island.islandId}";
                gameObj.layer = LayerMask.NameToLayer("Terrain");

                WorldGenMesh.triangulate(gameObj, config.topSideMaterial, island.centers, config.uvScale);
                timer.logEventComplete("triangulated topside for island " + island.islandId);

                island.topsideBounds = CalculateBounds(island.center, gameObj);
                timer.logEventComplete("generated topside bounds for island " + island.islandId);

                // first pass at adding a nav mesh
                foreach (GameObject agentType in config.navAgents) {
                    NavMeshSurface navMeshSurface = gameObj.AddComponent<NavMeshSurface>();
                    navMeshSurface.agentTypeID = agentType.GetComponent<NavMeshAgent>().agentTypeID;
                    navMeshSurface.overrideVoxelSize = true;
                    navMeshSurface.voxelSize = 0.5f;
                    navMeshSurface.BuildNavMesh();
                }
                timer.logEventComplete("built NavMeshes for island " + island.islandId);

                WorldGenElevation.generateIslandUndersideElevations(config.seed, island);
                timer.logEventComplete("generated underside for island " + island.islandId);

                WorldGenMesh.triangulate(gameObj, config.undersideMaterial, island.undersideCoords, config.uvScale);
                timer.logEventComplete("triangulated underside for island " + island.islandId);

                island.totalBounds = CalculateBounds(island.center, gameObj);
                timer.logEventComplete("generated total bounds for island " + island.islandId);

                islandObjects.Add(gameObj);
            }
            timer.end();

            return islandObjects;
        }

        private static Bounds CalculateBounds(Vector3 center, GameObject gameObj) {
            Quaternion currentRotation = Quaternion.Euler(0f, 0f, 0f);
            Bounds bounds = new Bounds(center, Vector3.zero);
            foreach (Renderer renderer in gameObj.GetComponentsInChildren<Renderer>()) {
                bounds.Encapsulate(renderer.bounds);
            }
            Vector3 localCenter = bounds.center - gameObj.transform.position;
            bounds.center = localCenter;
            return bounds;
        }

        private static List<Vector3> getPointsForIslandUndersideGraph(Island island) {
            List<Vector3> points = island.centers.Select(center => new Vector3((float)center.coord.x, 0, (float)center.coord.y)).ToList();
            points.AddRange(island.corners.Where(corner => corner.isIslandRim).Select(corner => new Vector3((float)corner.coord.x, (float)corner.coord.elevation, (float)corner.coord.y)));
            return points;
        }

        private static List<Island> findIslands(Graph graph) {
            DebugLoggingTimer timer = new DebugLoggingTimer();
            timer.begin("findIslands");
            List<Center> landCenters = graph.centers.Where(center => !center.isClipped).ToList();
            List<Island> islands = new List<Island>();
            timer.logEventComplete("perpare landCenters");
            while (landCenters.Count > 0) {
                Center startCenter = landCenters[0];
                List<Center> islandCenters = new List<Center>();
                islandCenters.Add(startCenter);
                List<Center> queue = startCenter.neighbours.Where(center => !center.isClipped).ToList();
                timer.logEventComplete("perpare queue");
                while (queue.Count > 0) {
                    Center next = queue[0];
                    queue.Remove(next);
                    foreach (Center neigh in next.neighbours) {
                        if (!neigh.isClipped && !islandCenters.Contains(neigh)) {
                            islandCenters.Add(neigh);
                            queue.Add(neigh);
                        }
                    }
                }
                timer.logEventComplete("queue complete");
                foreach (var center in islandCenters) {
                    landCenters.Remove(center);
                }
                if (islandCenters.Count > 3) {
                    // filter out single-center islands for being too small
                    islands.Add(createIsland(islands.Count, islandCenters));
                }
                timer.logEventComplete("createIsland island");
            }
            timer.end();
            return islands;
        }

        private static Island createIsland(int islandIndex, List<Center> islandCenters) {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (Center center in islandCenters) {
                Coord coord = center.coord;
                if (coord.x < minX) {
                    minX = coord.x;
                }
                if (coord.x > maxX) {
                    maxX = coord.x;
                }
                if (coord.y < minY) {
                    minY = coord.y;
                }
                if (coord.y > maxY) {
                    maxY = coord.y;
                }
            }
            Rect rect = new Rect((float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY));
            return new Island(islandIndex, islandCenters, rect);
        }

        private static Graph generateGraph(int seed, float size, int pointCount) {
            VoronoiBase voronoi = WorldGeneratorUtils.generateVoronoi(seed, size, pointCount);

            List<Corner> corners = WorldGeneratorUtils.createCorners(voronoi.Vertices);

            List<Center> centers = WorldGeneratorUtils.createCenters(voronoi.Faces, corners);

            List<Edge> edges = WorldGeneratorUtils.createEdges(voronoi, centers, corners);

            WorldGeneratorUtils.recenterCorners(corners);

            return new Graph(seed, size, centers, corners, edges);
        }
    }
}
