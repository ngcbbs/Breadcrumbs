using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class CharacterCustomization {
        // 신체 비율
        [Header("신체 특성")]
        [Range(0, 1)]
        public float height = 0.5f;
        [Range(0, 1)]
        public float weight = 0.5f;
        [Range(0, 1)]
        public float muscularity = 0.5f;

        // 얼굴 특성
        [Header("얼굴 특성")]
        public int faceShape = 0;
        [Range(0, 1)]
        public float jawWidth = 0.5f;
        [Range(0, 1)]
        public float cheekboneHeight = 0.5f;
        [Range(0, 1)]
        public float cheekboneWidth = 0.5f;
        [Range(0, 1)]
        public float eyeSize = 0.5f;
        [Range(0, 1)]
        public float eyeSeparation = 0.5f;
        [Range(0, 1)]
        public float eyeHeight = 0.5f;
        [Range(0, 1)]
        public float browHeight = 0.5f;
        [Range(0, 1)]
        public float browWidth = 0.5f;
        [Range(0, 1)]
        public float noseLength = 0.5f;
        [Range(0, 1)]
        public float noseBridge = 0.5f;
        [Range(0, 1)]
        public float noseWidth = 0.5f;
        [Range(0, 1)]
        public float noseTip = 0.5f;
        [Range(0, 1)]
        public float mouthWidth = 0.5f;
        [Range(0, 1)]
        public float mouthDepth = 0.5f;
        [Range(0, 1)]
        public float chinHeight = 0.5f;
        [Range(0, 1)]
        public float chinWidth = 0.5f;
        [Range(0, 1)]
        public float chinDepth = 0.5f;

        // 머리카락
        [Header("머리카락")]
        public int hairStyle = 0;
        public Color hairColor = Color.black;

        // 눈
        [Header("눈")]
        public Color eyeColor = new Color(0.6f, 0.3f, 0.1f, 1.0f);

        // 피부
        [Header("피부")]
        public Color skinColor = Color.white;

        // 문신 및 흉터
        [Header("문신 및 흉터")]
        public List<CustomizationDecal> decals = new List<CustomizationDecal>();

        // 의상 및 액세서리
        [Header("의상 및 액세서리")]
        public int baseOutfit = 0;
        public List<int> accessories = new List<int>();

        // 색상 팔레트
        [Header("색상 팔레트")]
        public Color primaryColor = Color.blue;
        public Color secondaryColor = Color.red;
        public Color accentColor = Color.yellow;

        // 사전 정의된 프리셋
        [Serializable]
        public class CustomizationPreset {
            public string presetName;
            public CharacterCustomization presetData;
        }

        [Header("프리셋")]
        public List<CustomizationPreset> savedPresets = new List<CustomizationPreset>();

        // 프리셋 저장
        public void SavePreset(string presetName) {
            // 딥 카피 필요
            CharacterCustomization copy = this.Clone();

            CustomizationPreset preset = new CustomizationPreset {
                presetName = presetName,
                presetData = copy
            };

            // 같은 이름의 프리셋이 있으면 업데이트
            for (int i = 0; i < savedPresets.Count; i++) {
                if (savedPresets[i].presetName == presetName) {
                    savedPresets[i] = preset;
                    Debug.Log($"Updated preset: {presetName}");
                    return;
                }
            }

            // 새 프리셋 추가
            savedPresets.Add(preset);
            Debug.Log($"Saved new preset: {presetName}");
        }

        // 프리셋 로드
        public bool LoadPreset(string presetName) {
            foreach (var preset in savedPresets) {
                if (preset.presetName == presetName) {
                    // 딥 카피로 값 복사
                    CopyFrom(preset.presetData);
                    Debug.Log($"Loaded preset: {presetName}");
                    return true;
                }
            }

            Debug.Log($"Preset not found: {presetName}");
            return false;
        }

        // 랜덤 외형 생성
        public void Randomize() {
            // 랜덤 값으로 설정
            height = UnityEngine.Random.value;
            weight = UnityEngine.Random.value;
            muscularity = UnityEngine.Random.value;

            faceShape = UnityEngine.Random.Range(0, 10); // 가정: 10개의 얼굴 형태
            jawWidth = UnityEngine.Random.value;
            cheekboneHeight = UnityEngine.Random.value;
            // ... 나머지 얼굴 특성들

            hairStyle = UnityEngine.Random.Range(0, 20); // 가정: 20개의 헤어스타일
            hairColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            eyeColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            skinColor = new Color(
                UnityEngine.Random.Range(0.2f, 0.9f),
                UnityEngine.Random.Range(0.2f, 0.9f),
                UnityEngine.Random.Range(0.2f, 0.9f)
            );

            // 기타 특성들도 랜덤하게 설정
            Debug.Log("Generated random appearance");
        }

        // 복제
        public CharacterCustomization Clone() {
            // JSON으로 변환 후 다시 객체로 변환하여 딥 카피
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<CharacterCustomization>(json);
        }

        // 다른 객체에서 값 복사
        public void CopyFrom(CharacterCustomization other) {
            if (other == null) return;

            // JSON으로 변환 후 다시 현재 객체로 로드하여 딥 카피
            string json = JsonUtility.ToJson(other);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}