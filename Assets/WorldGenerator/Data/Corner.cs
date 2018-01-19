using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WorldGenerator {
    public class Corner {
        private int index;
        private Coord coord;

        public Corner(int i, Coord coord) {
            this.index = i;
            this.coord = coord;
        }
    }
}
