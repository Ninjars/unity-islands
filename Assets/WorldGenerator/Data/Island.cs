using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
public class Island {

			public List<Center> centers {
				get;
				private set;
			}

			public Island(List<Center> centers) {
				this.centers = centers;
			}
	}
}
