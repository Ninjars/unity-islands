using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

	public enum ResourceType {
		LOW_DENSITY_ENERGY,
	}
	public interface IResource {
		ResourceType getType();
		float getCurrentValue();
		TerrainNode getNode();
		
		/// returns the amount actually harvested from this resource, which may be less than or equal to the target amount
		float harvest(float targetAmount);
	}
}
