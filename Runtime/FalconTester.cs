using UnityEngine;

namespace Falcon {
    [RequireComponent(typeof(FalconEntryPoint))]
    public class FalconTester : MonoBehaviour {
        private FalconEntryPoint _entryPoint;

        private Vector3 _position = new Vector3();

        private void Awake() {
            _entryPoint = GetComponent<FalconEntryPoint>();
            _entryPoint.OnTick += FalconTick;
        }


        private void OnDestroy() {
            _entryPoint.OnTick -= FalconTick;
        }

        private void Update() {
            transform.position = _position.mul(new Vector3(1.0f, 1.0f, -1.0f));
        }

        private void FalconTick(FalconEntryPoint entryPoint, Vector3 position) {
            _position = position;
            _entryPoint.ApplyForce(-position * 10.0f);
        }
    }
}