using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
public class WorldManager : MonoBehaviour {

	public static WorldManager instance;
	public bool debugDrawDelauney = false;
	public bool debugDrawCornerConnections = false;
	public bool debugDrawDownlopes = true;

	private WorldGenerator.World world;


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
	}
	
	void Update () {
		
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
