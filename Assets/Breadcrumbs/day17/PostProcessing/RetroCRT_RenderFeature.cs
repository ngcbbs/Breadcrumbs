using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class RetroCRT_RenderFeature : ScriptableRendererFeature {
    // 인스펙터에 표시할 설정
    [SerializeField] private RetroCrtSettings settings;
    [SerializeField] private Shader shader;
    private RetroCRT_RenderPass _retroCrtPass;
    private Material _material;

    public override void Create() {
        if (shader == null)
            return;
        _material = new Material(shader);
        _retroCrtPass = new RetroCRT_RenderPass(_material, settings) {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (_retroCrtPass == null)
            return;
        var stack = VolumeManager.instance.stack;
        var retroCrtEffect = stack.GetComponent<RetroCRT_Effect>();
        if (retroCrtEffect == null || !retroCrtEffect.active)
            return;
        VolumeManager.instance.CheckStack(stack); // 에디터 전용.
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (renderingData.cameraData.cameraType == CameraType.SceneView && sceneView != null &&
            sceneView.sceneViewState.showImageEffects) {
            // 씬뷰에 post processing 이팩트 표시 설정이 켜진경우 적용.
            renderer.EnqueuePass(_retroCrtPass);
        }
        else if (renderingData.cameraData.cameraType == CameraType.Game) {
            renderer.EnqueuePass(_retroCrtPass);
        }
    }

    protected override void Dispose(bool disposing) {
        if (Application.isPlaying)
            Destroy(_material);
        else
            DestroyImmediate(_material);
    }
}

[Serializable]
public class RetroCrtSettings {
    public Color tintColor;
    public float intensity;
}
