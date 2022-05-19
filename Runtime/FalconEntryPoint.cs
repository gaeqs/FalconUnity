using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Falcon {
    public class FalconEntryPoint : MonoBehaviour {
        [SerializeField] private uint device = 0;

        [Space] public float workspaceRadius = 1.0f;

        private readonly ConcurrentQueue<FalconForceGenerator> _forcesQueue =
            new ConcurrentQueue<FalconForceGenerator>();

        private readonly List<FalconForceGeneratorInstance> _forces =
            new List<FalconForceGeneratorInstance>();

        private Vector3 _nextTickForce = Vector3.zero;

        private Thread _hapticThread;
        private bool _running;

        public event Action<FalconEntryPoint, Vector3> OnTick;

        public Vector3 GetPosition() {
            return _getPosition(device).toVector() * 10 * workspaceRadius;
        }

        public void ApplyForce(Vector3 force) {
            _nextTickForce += force;
        }

        public void ApplyForce(FalconForceGenerator generator) {
            _forcesQueue.Enqueue(generator);
        }

        public bool IsMainButtonPressed() {
            return _isButtonPressed(device, 0);
        }

        public bool IsButtonPressed(int id) {
            return _isButtonPressed(device, id);
        }

        private void Awake() {
            if (_connect(device)) {
                Debug.Log($"Device {device} connected.");
                _running = true;
                _hapticThread = new Thread(StartThread);
                _hapticThread.Start();
            }
            else {
                Debug.LogError($"Failed to connect to device {device}.");
                _running = false;
            }
        }


        private void OnDestroy() {
            _running = false;
            if (_close(device)) {
                Debug.Log($"Device {device} closed.");
            }
            else {
                Debug.LogError($"Failed to close device {device}.");
            }
        }

        private void StartThread() {
            while (_running) {
                OnTick?.Invoke(this, GetPosition());

                var time = Stopwatch.GetTimestamp() * 1_000_000L / Stopwatch.Frequency;

                while (_forcesQueue.TryDequeue(out var generator)) {
                    _forces.Add(new FalconForceGeneratorInstance(generator, time));
                }

                var finalForce = _nextTickForce;
                for (var i = _forces.Count - 1; i >= 0; i--) {
                    var force = _forces[i];
                    if (force.End <= time) {
                        _forces.RemoveAt(i);
                    }
                    else {
                        finalForce += force.Evaluate(time);
                    }
                }

                _applyForce(device, finalForce.toFVector());
                _nextTickForce = Vector3.zero;
            }
        }


        private readonly struct FalconForceGeneratorInstance {
            public readonly FalconForceGenerator Force;
            public readonly long Start;
            public readonly long End;

            public FalconForceGeneratorInstance(FalconForceGenerator force, long start) {
                Force = force;
                Start = start;

                var lastX = force.forceX.length > 0 ? force.forceX.keys[force.forceX.length - 1].time : 0;
                var lastY = force.forceY.length > 0 ? force.forceY.keys[force.forceY.length - 1].time : 0;
                var lastZ = force.forceZ.length > 0 ? force.forceZ.keys[force.forceZ.length - 1].time : 0;

                var last = (long)(Math.Max(lastX, Math.Max(lastY, lastZ)) * 1_000_000L);
                End = start + last;
            }

            public Vector3 Evaluate(long time) {
                var normalizedTime = time - Start;
                var timeInSeconds = normalizedTime / 1_000_000.0f;
                return Force.Evaluate(timeInSeconds);
            }
        }

        [DllImport("FalconUnity.dll", EntryPoint = "connect_haptic")]
        private static extern bool _connect(uint device);

        [DllImport("FalconUnity.dll", EntryPoint = "close_haptic")]
        private static extern bool _close(uint device);

        [DllImport("FalconUnity.dll", EntryPoint = "is_device_connected")]
        private static extern bool _isConnected(uint device);

        [DllImport("FalconUnity.dll", EntryPoint = "get_position")]
        private static extern FalconUtils.FVector _getPosition(uint device);

        [DllImport("FalconUnity.dll", EntryPoint = "apply_force")]
        private static extern bool _applyForce(uint device, FalconUtils.FVector vector);

        [DllImport("FalconUnity.dll", EntryPoint = "is_button_pressed")]
        private static extern bool _isButtonPressed(uint device, int buttonId);
    }
}