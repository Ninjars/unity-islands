using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
	public class TerrainNode {
        private readonly WorldManager worldManager;
        private readonly int nodeIndex;
        private readonly Vector3 position;

        public bool isSolid { get; private set; }
        private Dictionary<ResourceType, IResource> resources = new Dictionary<ResourceType, IResource>();

        private float _radius = -1;
		public float radius {
			get {
				if (_radius < 0) {
					_radius = worldManager.getRadiusOfNode(nodeIndex);
				}
				return _radius;
			 }
		}
		private List<TerrainNode> _neighbouringNodes;
		public List<TerrainNode> neighbouringNodes {
			get {
				if (_neighbouringNodes == null) {
					_neighbouringNodes = worldManager.getNeighbouringSolidNodes(nodeIndex);
				}
				return _neighbouringNodes;
			}
		}

        public TerrainNode(WorldManager world, int index, Vector3 position, bool isSolid) {
			this.worldManager = world;
			this.nodeIndex = index;
			this.position = position;
			this.isSolid = isSolid;
		}

		public Vector2 getRandomPoint2D(float factor = 1) {
			float x = radius * 2 * (float)worldManager.GetRandom().NextDouble() - radius;
			float y = radius * 2 * (float)worldManager.GetRandom().NextDouble() - radius;
			return new Vector2(position.x + x * factor, position.z + y * factor);
		}

		public Vector3 getRandomPoint3D(float factor = 1) {
			for (int i = 0; i < 25; i++) {
				Vector3 pos = worldManager.findSurfacePosition(getRandomPoint2D(factor));
				if (!pos.Equals(Vector3.zero)) {
					return pos;
				}
			}
			return position;
		}

        internal void addResource(IResource resource) {
            resources.Add(resource.getType(), resource);
        }

		public IResource getResource(ResourceType type) {
			return resources[type];
		}

        internal Vector3 getPosition()  {
            return position;
        }
    }
}
