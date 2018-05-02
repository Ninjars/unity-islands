using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game {
	public class WorldManager : MonoBehaviour {

		public static WorldManager instance;
		public GameObject agent; // todo: allow creation of configurable spawns
		public int agentSpawnCount = 20;
		public bool debugDrawDelauney = false;
        public bool debugDrawCornerConnections = false;
		public bool debugDrawDownlopes = true;


        private WorldGenerator.World world;
		private List<TerrainNode> terrainNodes;

        public List<TerrainNode> solidTerrainNodes;

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

			terrainNodes = world.centers.Select(center => new TerrainNode(this, center.index, new Vector3(center.coord.x, center.scaledElevation(world.verticalScale), center.coord.y), !center.isClipped)).ToList();
			solidTerrainNodes = terrainNodes.Where(node => node.isSolid).ToList();
			centralNode = world.indexOfClosestCenter(0, new Vector3(world.size/2, 0, world.size/2), true);
		}

		void Start() {
			for (int i = 0; i < agentSpawnCount; i++) {
				Vector3 position = solidTerrainNodes[gameRandom.Next(solidTerrainNodes.Count)].position + Vector3.up;
				Instantiate(agent, position, UnityEngine.Random.rotation);
			}
		}

        internal Vector3 findSurfacePosition(Vector2 targetXY) {
			RaycastHit hit;
			Vector3 origin = new Vector3(targetXY.x, world.verticalScale, targetXY.y);
			if (Physics.Raycast(origin, Vector3.down, out hit, (int) world.verticalScale)) {
				return hit.point;
			} else {
				Debug.Log("cast from " + origin + " failed to hit geometry");
				return origin;
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
			float smallestRadius = 10000;
			foreach (WorldGenerator.Center c in center.neighbours) {
				var distance = Vector3.Distance(center.coord.toVector3(), c.coord.toVector3());
				if (distance < smallestRadius) {
					smallestRadius = distance;
				}
			}
			return smallestRadius / 2;
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
		}
	}
}
