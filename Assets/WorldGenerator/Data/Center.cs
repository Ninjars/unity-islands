using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class Center {
		private int index;
		private Coord coord;

		public Center(int i, Coord coord)
		{
			this.index = i;
			this.coord = coord;
		}
	}
}
