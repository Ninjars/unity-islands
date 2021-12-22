using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game {
	[RequireComponent(typeof(WorldGenerator.WorldGenerator))]
	public class WorldManager : MonoBehaviour {

		public static WorldManager instance;
		public GameObject agent; // todo: allow creation of configurable spawns
		public int agentSpawnCount = 20;
		public bool debugDrawDelauney = false;
        public bool debugDrawCornerConnections = false;
		public bool debugDrawDownlopes = false;
		public bool debugDrawTerrainNodes = false;
		public bool debugShowNodeEnergy = false;

        private WorldGenerator.World world;
		private List<TerrainNode> terrainNodes;
        private List<TerrainNode> solidTerrainNodes;
        private System.Random gameRandom;
        private int centralNode;

		void Awake () {
			if (instance == null) {
				instance = this;
			} else {
				Destroy(gameObject);
				Debug.LogWarning("attempted to instantiate second WorldManager");
				return;
			}
			var worldGenerator = GetComponent<WorldGenerator.WorldGenerator>();
			world = worldGenerator.generateWorld();
			gameRandom = new System.Random(world.seed);

			terrainNodes = world.centers.Select(center => createTerrainNode(center)).ToList();
			solidTerrainNodes = terrainNodes.Where(node => node.isSolid).ToList();
			centralNode = world.indexOfClosestCenter(0, new Vector3(world.size/2, 0, world.size/2), true);
		}

		private TerrainNode createTerrainNode(WorldGenerator.Center center) {
			var node = new TerrainNode(this, center.index, new Vector3(center.coord.x, center.scaledElevation(world.verticalScale), center.coord.y), !center.isClipped);
			if (!center.isClipped) {
				float rate = 1 - center.coord.elevation;
				BackgroundEnergy energy = new BackgroundEnergy(node, 0, rate);
				node.addResource(energy);
			}
			return node;
		}

		void Start() {
			for (int i = 0; i < agentSpawnCount; i++) {
				Vector3 position = solidTerrainNodes[gameRandom.Next(solidTerrainNodes.Count)].getPosition() + Vector3.up;
				Instantiate(agent, position, UnityEngine.Random.rotation);
			}
		}

        internal Vector3 findSurfacePosition(Vector2 targetXY) {
			RaycastHit hit;
			Vector3 origin = new Vector3(targetXY.x, world.verticalScale, targetXY.y);
			if (Physics.Raycast(origin, Vector3.down, out hit, (int) world.verticalScale)) {
				return hit.point;
			} else {
				return Vector3.zero;
			}
        }

        internal List<TerrainNode> getNeighbouringSolidNodes(int index) {
			return world.centers[index].neighbours.Where(center => !center.isClipped).Select(center => terrainNodes[center.index]).ToList();
		}

        internal TerrainNode getClosestTerrainNode(Vector3 point) {
            return terrainNodes[world.indexOfClosestCenter(centralNode, point, true)];
        }

		internal System.Random GetRandom() {
			return gameRandom;
		}

        internal float getRadiusOfNode(int index) {
            WorldGenerator.Center center = world.centers[index];
			float radiusSum = 0;
			foreach (WorldGenerator.Center c in center.neighbours) {
				radiusSum += Vector3.Distance(center.coord.toVector3(), c.coord.toVector3());
			}
			return radiusSum / (2 * center.neighbours.Count);
        }

		void OnDrawGizmos() {
			if (world == null) {
				return;
			}
			var verticalScale = world.verticalScale;
			if (debugDrawDelauney) {
				foreach (WorldGenerator.Center center in world.centers) {
					foreach (WorldGenerator.Center neigh in center.neighbours) {
						if (neigh.index > center.index) {
							Debug.DrawLine(
								center.coord.toVector3(verticalScale * 1.01f), 
								neigh.coord.toVector3(verticalScale * 1.01f));
						}
					}
				}
			}
			if (debugDrawDownlopes) {
				foreach (WorldGenerator.Corner corner in world.corners) {
					if (corner.downslope != null) {
						Debug.DrawLine(
								corner.coord.toVector3(verticalScale * 1.01f), 
								corner.downslope.coord.toVector3(verticalScale * 1.01f),
							Color.red);
					}				
				}
			}
			if (debugDrawCornerConnections) {
				foreach (WorldGenerator.Corner corner in world.corners) {
					foreach (WorldGenerator.Center center in corner.GetTouches()) {
						Debug.DrawLine(
								center.coord.toVector3(verticalScale * 1.01f), 
								corner.coord.toVector3(verticalScale * 1.01f),
								Color.green);
					}
				}
			}
			if (debugDrawTerrainNodes) {
				Gizmos.color = Color.green;
				foreach (TerrainNode node in solidTerrainNodes) {
					Gizmos.DrawSphere(node.getPosition(), node.radius);
				}
			}
			if (debugShowNodeEnergy) {
				foreach (TerrainNode node in solidTerrainNodes) {
					var res = node.getResource(ResourceType.LOW_DENSITY_ENERGY);
					if (res != null) {
						DebugUtils.drawString(node.getPosition(), res.getCurrentValue().ToString());
					}
				}
			}
		}
	}
}
