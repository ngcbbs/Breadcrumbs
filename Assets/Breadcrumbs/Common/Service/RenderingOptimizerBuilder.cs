using UnityEngine;

namespace Breadcrumbs.Common.Service {
    public class RenderingOptimizerBuilder {
        private bool _useOcclusionCulling = true;
        private bool _useStaticBatching = true;
        private bool _useDynamicBatching = true;
        private int _targetFrameRate = 60;
        private Vector2Int _targetResolution = new Vector2Int(1920, 1080);
        private bool _dynamicResolution = true;

        public RenderingOptimizerBuilder WithOcclusionCulling(bool enabled) {
            _useOcclusionCulling = enabled;
            return this;
        }

        public RenderingOptimizerBuilder WithStaticBatching(bool enabled) {
            _useStaticBatching = enabled;
            return this;
        }

        public RenderingOptimizerBuilder WithDynamicBatching(bool enabled) {
            _useDynamicBatching = enabled;
            return this;
        }

        public RenderingOptimizerBuilder WithTargetFrameRate(int frameRate) {
            _targetFrameRate = Mathf.Max(1, frameRate); // Ensure frame rate is positive
            return this;
        }

        public RenderingOptimizerBuilder WithTargetResolution(Vector2Int resolution) {
            _targetResolution = new Vector2Int(
                Mathf.Max(1, resolution.x),
                Mathf.Max(1, resolution.y)
            );
            return this;
        }

        public RenderingOptimizerBuilder WithDynamicResolution(bool enabled) {
            _dynamicResolution = enabled;
            return this;
        }

        public IRenderingOptimizerService Build(GameObject targetGameObject) {
            var service = targetGameObject.AddComponent<RenderingOptimizer>();
            service.Initialize(
                _useOcclusionCulling,
                _useStaticBatching,
                _useDynamicBatching,
                _targetFrameRate,
                _targetResolution,
                _dynamicResolution
            );
            return service;
        }
    }
}