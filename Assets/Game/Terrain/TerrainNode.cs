using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
public class TerrainNode {
        private readonly WorldManager worldManager;
        private readonly int index;

		private List<TerrainNode> _neighbouringNodes;
		public List<TerrainNode> neighbouringNodes {
			get {if (_neighbouringNodes == null) {
				_neighbouringNodes = worldManager.getNeighbouringNodes(index);
				}
				return _neighbouringNodes;
			}
		}

        public TerrainNode(WorldManager world, int index) {
			this.worldManager = world;
			this.index = index;
		}
	}
}
