using UnityEngine;

namespace Breadcrumbs.Common.Service {
    public interface IRenderingOptimizerService {
        void OptimizeRenderingSettings();
        void MonitorFrameRate();
        void UpdateDynamicResolution();
        void AdjustResolution();
        float CurrentFPS { get; }
        Vector2Int CurrentResolution { get; }
    }
}