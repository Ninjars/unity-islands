using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class BackgroundEnergy : IResource {
        private readonly TerrainNode node;
        private readonly float accumulationRate;

        private float lastValue;
        private float lastValueCheck;

        public BackgroundEnergy(TerrainNode parentNode, float initialValue, float accumulationRate) {
            this.node = parentNode;
            lastValue = initialValue;
            lastValueCheck = Time.time;
            this.accumulationRate = accumulationRate;
		}
        public ResourceType getType() {
            return ResourceType.LOW_DENSITY_ENERGY;
        }
        public float getCurrentValue() {
            return lastValue + accumulationRate * (Time.time - lastValueCheck);
        }

        public TerrainNode getNode() {
            return node;
        }

        public float harvest(float targetAmount) {
            float currentAmount = getCurrentValue();
            float harvestedAmount;
            if (currentAmount > targetAmount) {
                harvestedAmount = targetAmount;
            } else {
                harvestedAmount = currentAmount;
            }
            lastValue = currentAmount - harvestedAmount;
            lastValueCheck = Time.time;
            return harvestedAmount;
        }
    }
}
