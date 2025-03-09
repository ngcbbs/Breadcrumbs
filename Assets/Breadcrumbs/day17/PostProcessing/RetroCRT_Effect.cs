using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class RetroCRT_Effect : VolumeComponent
{
    // 효과에 사용할 파라미터 정의
    public ColorParameter tintColor = new ColorParameter(Color.white, true);
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
}
