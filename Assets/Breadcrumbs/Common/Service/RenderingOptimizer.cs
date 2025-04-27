using UnityEngine;

namespace Breadcrumbs.Common.Service {
    public class RenderingOptimizer : MonoBehaviour, IRenderingOptimizerService {
        private bool _useOcclusionCulling;
        private bool _useStaticBatching;
        private bool _useDynamicBatching;
        private int _targetFrameRate;
        private Vector2Int _targetResolution;
        private bool _dynamicResolution;

        private float _frameTimeAccumulator = 0f;
        private int _frameCount = 0;
        private float _currentFPS = 0f;
        private Vector2Int _currentResolution;
        private float _resolutionScale = 1f;
        private const float kFPSCheckInterval = 1f;       // FPS check interval (seconds)
        private const float kMinResolutionScale = 0.5f;   // Minimum resolution scale
        private const float kMaxResolutionScale = 1f;     // Maximum resolution scale
        private const float kResolutionAdjustStep = 0.1f; // Resolution adjustment step

        public float CurrentFPS => _currentFPS;
        public Vector2Int CurrentResolution => _currentResolution;

        public void Initialize(
            bool occlusionCulling,
            bool staticBatching,
            bool dynamicBatching,
            int frameRate,
            Vector2Int resolution,
            bool dynamicRes
        ) {
            _useOcclusionCulling = occlusionCulling;
            _useStaticBatching = staticBatching;
            _useDynamicBatching = dynamicBatching;
            _targetFrameRate = frameRate;
            _targetResolution = resolution;
            _dynamicResolution = dynamicRes;
        }

        private void Awake() {
            // Apply rendering optimizations
            OptimizeRenderingSettings();
        }

        private void Start() {
            // Set initial resolution
            _currentResolution = _targetResolution;
            if (_dynamicResolution) {
                AdjustResolution();
            } else {
                Screen.SetResolution(_targetResolution.x, _targetResolution.y, true);
            }

            // Set target frame rate
            Application.targetFrameRate = _targetFrameRate;
        }

        private void Update() {
            // Monitor frame rate
            MonitorFrameRate();

            // Adjust resolution dynamically if enabled
            if (_dynamicResolution) {
                UpdateDynamicResolution();
            }
        }

        public void OptimizeRenderingSettings() {
            // Enable/disable Occlusion Culling
            if (_useOcclusionCulling) {
                Camera.main.useOcclusionCulling = true;
                Debug.Log("Occlusion Culling enabled.");
            } else {
                Camera.main.useOcclusionCulling = false;
            }

            // Enable Static Batching
            if (_useStaticBatching) {
                StaticBatchingUtility.Combine(gameObject);
                Debug.Log("Static Batching enabled.");
            }

            // Dynamic Batching (configured in Player Settings)
            if (_useDynamicBatching) {
                Debug.Log("Dynamic Batching is enabled (ensure it's enabled in Player Settings).");
            }

            // Disable VSync for frame rate control
            QualitySettings.vSyncCount = 0;
        }

        public void MonitorFrameRate() {
            _frameTimeAccumulator += Time.unscaledDeltaTime;
            _frameCount++;

            // Calculate FPS every FPS_CHECK_INTERVAL seconds
            if (_frameTimeAccumulator >= kFPSCheckInterval) {
                _currentFPS = _frameCount / _frameTimeAccumulator;
                _frameTimeAccumulator = 0f;
                _frameCount = 0;

                Debug.Log($"Current FPS: {_currentFPS:F2}");
            }
        }

        public void UpdateDynamicResolution() {
            // Adjust resolution scale based on FPS
            if (_currentFPS < _targetFrameRate * 0.9f && _resolutionScale > kMinResolutionScale) {
                _resolutionScale = Mathf.Max(kMinResolutionScale, _resolutionScale - kResolutionAdjustStep);
            } else if (_currentFPS > _targetFrameRate * 1.1f && _resolutionScale < kMaxResolutionScale) {
                _resolutionScale = Mathf.Min(kMaxResolutionScale, _resolutionScale + kResolutionAdjustStep);
            }

            // Apply new resolution
            AdjustResolution();
        }

        public void AdjustResolution() {
            int newWidth = Mathf.RoundToInt(_targetResolution.x * _resolutionScale);
            int newHeight = Mathf.RoundToInt(_targetResolution.y * _resolutionScale);

            // Apply resolution if changed
            if (newWidth != _currentResolution.x || newHeight != _currentResolution.y) {
                Screen.SetResolution(newWidth, newHeight, true);
                _currentResolution = new Vector2Int(newWidth, newHeight);
                Debug.Log($"Resolution adjusted to: {newWidth}x{newHeight} (Scale: {_resolutionScale:F2})");
            }
        }
    }
}