using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// sdf 테스트.. 값 전달은.. 일반적인 방법은 아니지만.. :) 
// note:
// - 쉐이더에서 배열의 크기를 지정하지 않으면 특정 랜더러에서 쉐이더 컴파일 자체가 되지 않음.
// - 쉐이더 프로퍼티를 사용하지 않아도 되긴 하는데.. :P
// - 카메라 방향이 좀 이상해 보이는데..(일단 패스~)

// 참고자료
// sdf - https://iquilezles.org/articles/distfunctions/

// 생각해 볼 점
// - sdf 를 사용한 메쉬 생성을 ai도움을 받아보면... (실패)
// - 참고사이트: https://github.com/praeclarum/SdfKit <- 요기를 참고해서 유니티용으로 만들어 볼까..

public class day2 : MonoBehaviour {
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [SerializeField] private Shader shader;
    [SerializeField] private Image sdfScreen;
    [SerializeField] private RawImage dataImage;

    [SerializeField] private List<Transform> trackingTargets = new();
    
    private Texture2D _dataTexture;
    private Material _material;
    

    private void Start() {
        // create dataTexture
        _dataTexture = new Texture2D(8, 8, TextureFormat.RGBAFloat, false) {
            name = "data texture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        _dataTexture.Apply(false, false);
        dataImage.texture = _dataTexture;
        
        _material = new Material(shader);
        _material.SetTexture(MainTex, _dataTexture);
        sdfScreen.material = _material;
    }
    
    private void OnDestroy() {
        Destroy(_dataTexture);
        Destroy(_material);
    }

    private void UpdateData() {
        var pixels = _dataTexture.GetPixelData<Color>(0);
        int max = Math.Min(pixels.Length, trackingTargets.Count * 2);
        for (int i = 0; i < trackingTargets.Count; i++) {
            var index = i * 2;
            var target = trackingTargets[i];
            var pos = transform.localToWorldMatrix * target.localPosition;
            pixels[index + 0] = new Color(target.localPosition.x, target.localPosition.y, target.localPosition.z, 1);
            pixels[index + 1] = new Color(target.localScale.x, target.localScale.y, target.localScale.z, 0.5f);
        }
        for (var i = max; i < pixels.Length; i++)
            pixels[i] = Color.black;
        _dataTexture.Apply();
        pixels.Dispose();
    }

    private void Update() {
        UpdateData();
    }
}
