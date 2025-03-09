using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class RetroCRT_RenderPass : ScriptableRenderPass
{
    // 패스 이름
    private const string PROFILER_TAG = "RetroCRT Effect";
    private const string PASS_NAME = "RetroCRT Pass";
    
    private readonly Material _material;
    private RetroCrtSettings _defaultRetroCrtSettings;
    
    // RenderGraph 관련 변수
    private static readonly int TintColorPropertyId = Shader.PropertyToID("_TintColor");
    private static readonly int IntensityPropertyId = Shader.PropertyToID("_Intensity");
    private static readonly int TempColorTintTextureId = Shader.PropertyToID("_TempColorTint");

    public RetroCRT_RenderPass(Material material, RetroCrtSettings settings)
    {
        _material = material;
        _defaultRetroCrtSettings = settings;
    }
    
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();
        
        if (resourceData.isActiveTargetBackBuffer)
            return;

        var stack = VolumeManager.instance.stack;
        var retroCrtEffect = stack.GetComponent<RetroCRT_Effect>();

        if (retroCrtEffect == null || (!retroCrtEffect.tintColor.overrideState && !retroCrtEffect.intensity.overrideState))
            return;

        var descriptor = renderGraph.GetTextureDesc(resourceData.activeColorTexture);

        var srcCameraColor = resourceData.activeColorTexture;
        var dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
            new RenderTextureDescriptor(descriptor.width, descriptor.height), "_TempColorTint", false);

        if (!srcCameraColor.IsValid() || !dst.IsValid())
            return;
        
        _material.SetColor(TintColorPropertyId,
            retroCrtEffect.tintColor.overrideState ? retroCrtEffect.tintColor.value : Color.white);
        _material.SetFloat(IntensityPropertyId, 
            retroCrtEffect.intensity.overrideState ? retroCrtEffect.intensity.value : 0f);

        renderGraph.AddCopyPass(srcCameraColor, dst);
        var blitRetroCrt = new RenderGraphUtils.BlitMaterialParameters(
            dst, srcCameraColor, _material, 0
        );
        renderGraph.AddBlitPass(blitRetroCrt, PASS_NAME);
    }
}