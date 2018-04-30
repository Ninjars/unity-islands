using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
public class TerrainNode {
        private readonly WorldManager worldManager;
        private readonly int index;
        public readonly Vector3 position;

        public readonly bool isSolid;

        private float _radius = -1;
		public float radius {
			get {
				if (_radius < 0) {
					worldManager.getRadiusOfNode(index);
				}
				return _radius;
			 }
		}
		private List<TerrainNode> _neighbouringNodes;
		public List<TerrainNode> neighbouringNodes {
			get {
				if (_neighbouringNodes == null) {
					_neighbouringNodes = worldManager.getNeighbouringSolidNodes(index);
				}
				return _neighbouringNodes;
			}
		}

        public TerrainNode(WorldManager world, int index, Vector3 position, bool isSolid) {
			this.worldManager = world;
			this.index = index;
			this.position = position;
			this.isSolid = isSolid;
		}
	}
}
