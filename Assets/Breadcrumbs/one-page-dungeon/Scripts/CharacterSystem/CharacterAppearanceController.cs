using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class CharacterAppearanceController : MonoBehaviour {
        [SerializeField]
        private CharacterCustomization customization;

        // 렌더러 참조
        [SerializeField]
        private SkinnedMeshRenderer bodyRenderer;
        [SerializeField]
        private SkinnedMeshRenderer headRenderer;
        [SerializeField]
        private SkinnedMeshRenderer hairRenderer;

        // 게임오브젝트 참조
        [SerializeField]
        private Transform hairParent;
        [SerializeField]
        private Transform accessoriesParent;

        // 프리팹 참조
        [SerializeField]
        private GameObject[] hairStylePrefabs;
        [SerializeField]
        private GameObject[] faceShapePrefabs;
        [SerializeField]
        private GameObject[] accessoryPrefabs;

        // 블렌드쉐이프 이름
        private readonly string[] bodyBlendShapes = {
            "Height", "Weight", "Muscularity"
        };

        private readonly string[] faceBlendShapes = {
            "JawWidth", "CheekboneHeight", "CheekboneWidth",
            "EyeSize", "EyeSeparation", "EyeHeight",
            "BrowHeight", "BrowWidth", "NoseLength",
            "NoseBridge", "NoseWidth", "NoseTip",
            "MouthWidth", "MouthDepth", "ChinHeight",
            "ChinWidth", "ChinDepth"
        };

        // 현재 인스턴스화된 헤어 및 액세서리
        private GameObject currentHair;
        private List<GameObject> currentAccessories = new List<GameObject>();

        private void Start() {
            // 기본 초기화가 필요한 경우
            if (customization == null) {
                customization = new CharacterCustomization();
            }

            UpdateCharacterAppearance();
        }

        // 전체 외형 업데이트
        public void UpdateCharacterAppearance() {
            UpdateBodyShape();
            UpdateFaceShape();
            UpdateHair();
            UpdateSkin();
            UpdateAccessories();
            UpdateDecals();
        }

        // 신체 형태 업데이트
        private void UpdateBodyShape() {
            if (bodyRenderer == null) return;

            // 블렌드쉐이프 업데이트
            SetBlendShape(bodyRenderer, "Height", customization.height);
            SetBlendShape(bodyRenderer, "Weight", customization.weight);
            SetBlendShape(bodyRenderer, "Muscularity", customization.muscularity);

            // 재질 색상 적용
            Material bodyMaterial = bodyRenderer.material;
            bodyMaterial.SetColor("_SkinColor", customization.skinColor);
        }

        // 얼굴 형태 업데이트
        private void UpdateFaceShape() {
            if (headRenderer == null) return;

            // 얼굴 형태 변경 (프리팹 교체)
            if (faceShapePrefabs != null && customization.faceShape < faceShapePrefabs.Length) {
                // (실제 구현에서는 여기서 적절한 얼굴 프리팹 적용)
            }

            // 얼굴 디테일 블렌드쉐이프 적용
            SetBlendShape(headRenderer, "JawWidth", customization.jawWidth);
            SetBlendShape(headRenderer, "CheekboneHeight", customization.cheekboneHeight);
            SetBlendShape(headRenderer, "CheekboneWidth", customization.cheekboneWidth);
            SetBlendShape(headRenderer, "EyeSize", customization.eyeSize);
            SetBlendShape(headRenderer, "EyeSeparation", customization.eyeSeparation);
            // ... 나머지 얼굴 특성들

            // 눈 색상 적용
            Material headMaterial = headRenderer.material;
            headMaterial.SetColor("_EyeColor", customization.eyeColor);
        }

        // 헤어 스타일 업데이트
        private void UpdateHair() {
            if (hairParent == null || hairStylePrefabs == null) return;

            // 기존 헤어 제거
            if (currentHair != null) {
                Destroy(currentHair);
                currentHair = null;
            }

            // 새 헤어 인스턴스화
            if (customization.hairStyle >= 0 && customization.hairStyle < hairStylePrefabs.Length) {
                GameObject hairPrefab = hairStylePrefabs[customization.hairStyle];
                if (hairPrefab != null) {
                    currentHair = Instantiate(hairPrefab, hairParent);

                    // 머리카락 색상 설정
                    if (hairRenderer != null) {
                        Material hairMaterial = hairRenderer.material;
                        hairMaterial.SetColor("_HairColor", customization.hairColor);
                    } else {
                        // 인스턴스화된 헤어의 렌더러 찾기
                        Renderer[] renderers = currentHair.GetComponentsInChildren<Renderer>();
                        foreach (var renderer in renderers) {
                            renderer.material.SetColor("_Color", customization.hairColor);
                        }
                    }
                }
            }
        }

        // 피부색 업데이트
        private void UpdateSkin() {
            if (bodyRenderer != null) {
                bodyRenderer.material.SetColor("_SkinColor", customization.skinColor);
            }

            if (headRenderer != null) {
                headRenderer.material.SetColor("_SkinColor", customization.skinColor);
            }
        }

        // 액세서리 업데이트
        private void UpdateAccessories() {
            if (accessoriesParent == null || accessoryPrefabs == null) return;

            // 기존 액세서리 제거
            foreach (var accessory in currentAccessories) {
                Destroy(accessory);
            }

            currentAccessories.Clear();

            // 기본 의상 설정
            // (실제 구현시에는 의상 프리팹 적용)

            // 액세서리 추가
            foreach (int accessoryId in customization.accessories) {
                if (accessoryId >= 0 && accessoryId < accessoryPrefabs.Length) {
                    GameObject accessoryPrefab = accessoryPrefabs[accessoryId];
                    if (accessoryPrefab != null) {
                        GameObject newAccessory = Instantiate(accessoryPrefab, accessoriesParent);
                        currentAccessories.Add(newAccessory);

                        // 액세서리 색상 설정
                        Renderer[] renderers = newAccessory.GetComponentsInChildren<Renderer>();
                        foreach (var renderer in renderers) {
                            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                            renderer.GetPropertyBlock(propBlock);

                            propBlock.SetColor("_PrimaryColor", customization.primaryColor);
                            propBlock.SetColor("_SecondaryColor", customization.secondaryColor);
                            propBlock.SetColor("_AccentColor", customization.accentColor);

                            renderer.SetPropertyBlock(propBlock);
                        }
                    }
                }
            }
        }

        // 문신/흉터 업데이트
        private void UpdateDecals() {
            if (bodyRenderer == null) return;

            // 문신/흉터 텍스처 생성 및 적용
            // (실제 구현에서는 여기서 동적 텍스처 생성 또는 
            // 다수의 디케일 마스크를 적용하는 로직이 필요함)
            foreach (var decal in customization.decals) {
                // 문신 타입에 따른 처리
                switch (decal.type) {
                    case CustomizationDecal.DecalType.Tattoo:
                        ApplyTattoo(decal);
                        break;
                    case CustomizationDecal.DecalType.Scar:
                        ApplyScar(decal);
                        break;
                    case CustomizationDecal.DecalType.Makeup:
                        ApplyMakeup(decal);
                        break;
                    case CustomizationDecal.DecalType.Warpaint:
                        ApplyWarpaint(decal);
                        break;
                }
            }
        }

        // 실제 디케일 적용 메서드들 (실제 구현 필요)
        private void ApplyTattoo(CustomizationDecal decal) { }
        private void ApplyScar(CustomizationDecal decal) { }
        private void ApplyMakeup(CustomizationDecal decal) { }
        private void ApplyWarpaint(CustomizationDecal decal) { }

        // 블렌드쉐이프 설정 헬퍼 메서드
        private void SetBlendShape(SkinnedMeshRenderer renderer, string shapeName, float value) {
            int index = renderer.sharedMesh.GetBlendShapeIndex(shapeName);
            if (index != -1) {
                renderer.SetBlendShapeWeight(index, value * 100f); // 0-100 스케일로 변환
            }
        }

        // 현재 커스터마이징 객체 가져오기
        public CharacterCustomization GetCustomization() {
            return customization;
        }

        // 커스터마이징 값 설정
        public void SetCustomization(CharacterCustomization newCustomization) {
            if (newCustomization != null) {
                customization.CopyFrom(newCustomization);
                UpdateCharacterAppearance();
            }
        }

        // 특정 커스터마이징 값 변경
        public void SetHeight(float value) {
            customization.height = Mathf.Clamp01(value);
            UpdateBodyShape();
        }

        public void SetWeight(float value) {
            customization.weight = Mathf.Clamp01(value);
            UpdateBodyShape();
        }

        public void SetHairStyle(int styleIndex) {
            if (hairStylePrefabs != null && styleIndex >= 0 && styleIndex < hairStylePrefabs.Length) {
                customization.hairStyle = styleIndex;
                UpdateHair();
            }
        }

        public void SetHairColor(Color color) {
            customization.hairColor = color;
            UpdateHair();
        }

        public void SetSkinColor(Color color) {
            customization.skinColor = color;
            UpdateSkin();
        }

        public void SetEyeColor(Color color) {
            customization.eyeColor = color;
            UpdateFaceShape();
        }

        // 랜덤 외형 생성
        public void RandomizeAppearance() {
            customization.Randomize();
            UpdateCharacterAppearance();
        }

        // 프리셋 저장
        public void SaveCurrentPreset(string presetName) {
            customization.SavePreset(presetName);
        }

        // 프리셋 로드
        public void LoadPreset(string presetName) {
            if (customization.LoadPreset(presetName)) {
                UpdateCharacterAppearance();
            }
        }
    }
}