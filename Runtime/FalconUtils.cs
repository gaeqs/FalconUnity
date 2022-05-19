using System.Runtime.InteropServices;
using UnityEngine;

namespace Falcon {
    public static class FalconUtils {
        
        public const int BUTTON_CENTER = 0;
        public const int BUTTON_LEFT = 1;
        public const int BUTTON_TOP = 2;
        public const int BUTTON_RIGHT = 3;

        
        [StructLayout(LayoutKind.Sequential)]
        public struct FVector {
            public double x;
            public double y;
            public double z;
        }

        public static Vector3 toVector(this FVector vec) {
            return new Vector3((float)vec.y, (float)vec.z, (float)vec.x);
        }

        public static FVector toFVector(this Vector3 vec) {
            var vector = new FVector {
                x = vec.z,
                y = vec.x,
                z = vec.y
            };
            return vector;
        }

        public static Vector3 mul(this Vector3 a, Vector3 b) {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}