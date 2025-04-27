using Breadcrumbs.Common.Service;
using UnityEngine;

public class TestManager : MonoBehaviour {
    private IRenderingOptimizerService _renderingOptimizer;

    private void Start() {
        _renderingOptimizer = new RenderingOptimizerBuilder()
            .WithOcclusionCulling(true)
            .WithStaticBatching(true)
            .WithDynamicBatching(true)
            .WithTargetFrameRate(60)
            .WithTargetResolution(new Vector2Int(1920, 1080))
            .WithDynamicResolution(true)
            .Build(gameObject); // Attach to this GameObject

        // Example: Access current FPS
        Debug.Log($"Initial FPS: {_renderingOptimizer.CurrentFPS}");
    }
    
    /*
     void Update()
     {
         // _renderingOptimizer는 RenderingOptimizer 컴포넌트에서 업데이트 되고 있음.
         // Example: Manually trigger frame rate monitoring or resolution adjustment
         _renderingOptimizer.MonitorFrameRate();
         _renderingOptimizer.UpdateDynamicResolution();
     }
     // */
}