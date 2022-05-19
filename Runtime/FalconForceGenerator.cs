using System;
using UnityEngine;

namespace Falcon {
    [Serializable]
    public class FalconForceGenerator {
        [Range(-1000, 1000)] public float scale = 100.0f;
        public AnimationCurve forceX = AnimationCurve.Linear(0, 0f, 1.0f, 0f);
        public AnimationCurve forceY = AnimationCurve.Linear(0, 0f, 1.0f, 0f);
        public AnimationCurve forceZ = AnimationCurve.Linear(0, 0f, 1.0f, 0f);
        public Quaternion rotation = Quaternion.identity;

        public Vector3 Evaluate(float time) {
            var x = forceX.Evaluate(time) * scale;
            var y = forceY.Evaluate(time) * scale;
            var z = forceZ.Evaluate(time) * scale;
            return rotation * new Vector3(x, y, z);
        }

        public FalconForceGenerator() {
        }

        private FalconForceGenerator(float scale,
            AnimationCurve forceX,
            AnimationCurve forceY,
            AnimationCurve forceZ,
            Quaternion rotation) {
            this.scale = scale;
            this.forceX = forceX;
            this.forceY = forceY;
            this.forceZ = forceZ;
            this.rotation = rotation;
        }

        public FalconForceGenerator WithRotation(Quaternion newRotation) {
            return new FalconForceGenerator(scale, forceX, forceY, forceZ, newRotation);
        }
    }
}