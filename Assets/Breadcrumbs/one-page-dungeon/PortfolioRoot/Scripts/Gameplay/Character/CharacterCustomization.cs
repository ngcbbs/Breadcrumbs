using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Gameplay.Character {
    public class CharacterCustomization : MonoBehaviour {
        [Header("Customization Settings")]
        [SerializeField]
        private Transform headAttachPoint;
        [SerializeField]
        private Transform bodyAttachPoint;
        [SerializeField]
        private Transform leftHandAttachPoint;
        [SerializeField]
        private Transform rightHandAttachPoint;

        [Header("Customization Options")]
        [SerializeField]
        private List<CustomizationSet> headOptions = new List<CustomizationSet>();
        [SerializeField]
        private List<CustomizationSet> bodyOptions = new List<CustomizationSet>();
        [SerializeField]
        private List<CustomizationSet> handOptions = new List<CustomizationSet>();
        [SerializeField]
        private List<CustomizationSet> colorOptions = new List<CustomizationSet>();

        private Dictionary<CustomizationType, int> currentSelections = new Dictionary<CustomizationType, int>();
        private Dictionary<CustomizationType, GameObject> currentAttachments = new Dictionary<CustomizationType, GameObject>();

        private Dictionary<CustomizationType, Material> currentMaterials = new Dictionary<CustomizationType, Material>();
        private List<SkinnedMeshRenderer> characterMeshes = new List<SkinnedMeshRenderer>();

        public event Action<CustomizationType, int> OnCustomizationChanged;

        private void Awake() {
            Initialize();
        }

        private void Initialize() {
            foreach (CustomizationType type in Enum.GetValues(typeof(CustomizationType))) {
                currentSelections[type] = 0;
            }

            FindAndCacheRenderers();
            ApplyInitialCustomizations();
        }

        private void FindAndCacheRenderers() {
            characterMeshes.Clear();

            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (var renderer in renderers) {
                characterMeshes.Add(renderer);
            }
        }

        private void ApplyInitialCustomizations() {
            ApplyCustomization(CustomizationType.Head, 0);
            ApplyCustomization(CustomizationType.Body, 0);
            ApplyCustomization(CustomizationType.Hands, 0);
            ApplyCustomization(CustomizationType.Color, 0);
        }

        public void ApplyCustomization(CustomizationType type, int optionIndex) {
            switch (type) {
                case CustomizationType.Head:
                    ApplyHeadOption(optionIndex);
                    break;
                case CustomizationType.Body:
                    ApplyBodyOption(optionIndex);
                    break;
                case CustomizationType.Hands:
                    ApplyHandOption(optionIndex);
                    break;
                case CustomizationType.Color:
                    ApplyColorOption(optionIndex);
                    break;
            }

            currentSelections[type] = optionIndex;
            OnCustomizationChanged?.Invoke(type, optionIndex);
        }

        private void ApplyHeadOption(int optionIndex) {
            if (headAttachPoint == null || optionIndex < 0 || optionIndex >= headOptions.Count)
                return;

            RemoveCurrentAttachment(CustomizationType.Head);

            CustomizationSet option = headOptions[optionIndex];

            if (option.prefab != null) {
                GameObject headObj = Instantiate(option.prefab, headAttachPoint.position, headAttachPoint.rotation,
                    headAttachPoint);
                currentAttachments[CustomizationType.Head] = headObj;
            }

            if (option.material != null) {
                foreach (var mesh in characterMeshes) {
                    if (mesh.name.ToLower().Contains("head")) {
                        Material[] materials = mesh.materials;
                        for (int i = 0; i < materials.Length; i++) {
                            materials[i] = option.material;
                        }

                        mesh.materials = materials;
                    }
                }

                currentMaterials[CustomizationType.Head] = option.material;
            }
        }

        private void ApplyBodyOption(int optionIndex) {
            if (bodyAttachPoint == null || optionIndex < 0 || optionIndex >= bodyOptions.Count)
                return;

            RemoveCurrentAttachment(CustomizationType.Body);

            CustomizationSet option = bodyOptions[optionIndex];

            if (option.prefab != null) {
                GameObject bodyObj = Instantiate(option.prefab, bodyAttachPoint.position, bodyAttachPoint.rotation,
                    bodyAttachPoint);
                currentAttachments[CustomizationType.Body] = bodyObj;
            }

            if (option.material != null) {
                foreach (var mesh in characterMeshes) {
                    if (mesh.name.ToLower().Contains("body") || mesh.name.ToLower().Contains("torso")) {
                        Material[] materials = mesh.materials;
                        for (int i = 0; i < materials.Length; i++) {
                            materials[i] = option.material;
                        }

                        mesh.materials = materials;
                    }
                }

                currentMaterials[CustomizationType.Body] = option.material;
            }
        }

        private void ApplyHandOption(int optionIndex) {
            if (leftHandAttachPoint == null || rightHandAttachPoint == null ||
                optionIndex < 0 || optionIndex >= handOptions.Count)
                return;

            RemoveCurrentAttachment(CustomizationType.Hands);

            CustomizationSet option = handOptions[optionIndex];

            if (option.prefab != null) {
                GameObject leftHandObj = Instantiate(option.prefab, leftHandAttachPoint.position, leftHandAttachPoint.rotation,
                    leftHandAttachPoint);
                GameObject rightHandObj = Instantiate(option.prefab, rightHandAttachPoint.position, rightHandAttachPoint.rotation,
                    rightHandAttachPoint);

                currentAttachments[CustomizationType.Hands] = leftHandObj;
            }

            if (option.material != null) {
                foreach (var mesh in characterMeshes) {
                    if (mesh.name.ToLower().Contains("hand") || mesh.name.ToLower().Contains("arm")) {
                        Material[] materials = mesh.materials;
                        for (int i = 0; i < materials.Length; i++) {
                            materials[i] = option.material;
                        }

                        mesh.materials = materials;
                    }
                }

                currentMaterials[CustomizationType.Hands] = option.material;
            }
        }

        private void ApplyColorOption(int optionIndex) {
            if (optionIndex < 0 || optionIndex >= colorOptions.Count)
                return;

            CustomizationSet option = colorOptions[optionIndex];

            if (option.material != null) {
                foreach (var mesh in characterMeshes) {
                    Material[] materials = mesh.materials;
                    Material baseMaterial = option.material;

                    for (int i = 0; i < materials.Length; i++) {
                        if (mesh.name.ToLower().Contains("head") && currentMaterials.ContainsKey(CustomizationType.Head)) {
                            materials[i] = currentMaterials[CustomizationType.Head];
                        } else if ((mesh.name.ToLower().Contains("body") || mesh.name.ToLower().Contains("torso")) &&
                                   currentMaterials.ContainsKey(CustomizationType.Body)) {
                            materials[i] = currentMaterials[CustomizationType.Body];
                        } else if ((mesh.name.ToLower().Contains("hand") || mesh.name.ToLower().Contains("arm")) &&
                                   currentMaterials.ContainsKey(CustomizationType.Hands)) {
                            materials[i] = currentMaterials[CustomizationType.Hands];
                        } else {
                            materials[i] = baseMaterial;
                        }
                    }

                    mesh.materials = materials;
                }
            }
        }

        private void RemoveCurrentAttachment(CustomizationType type) {
            if (currentAttachments.ContainsKey(type) && currentAttachments[type] != null) {
                Destroy(currentAttachments[type]);
                currentAttachments.Remove(type);
            }
        }

        public int GetCurrentSelection(CustomizationType type) {
            return currentSelections.ContainsKey(type) ? currentSelections[type] : 0;
        }

        public int GetOptionCount(CustomizationType type) {
            switch (type) {
                case CustomizationType.Head:
                    return headOptions.Count;
                case CustomizationType.Body:
                    return bodyOptions.Count;
                case CustomizationType.Hands:
                    return handOptions.Count;
                case CustomizationType.Color:
                    return colorOptions.Count;
                default:
                    return 0;
            }
        }

        public string GetOptionName(CustomizationType type, int index) {
            switch (type) {
                case CustomizationType.Head:
                    return index >= 0 && index < headOptions.Count ? headOptions[index].name : string.Empty;
                case CustomizationType.Body:
                    return index >= 0 && index < bodyOptions.Count ? bodyOptions[index].name : string.Empty;
                case CustomizationType.Hands:
                    return index >= 0 && index < handOptions.Count ? handOptions[index].name : string.Empty;
                case CustomizationType.Color:
                    return index >= 0 && index < colorOptions.Count ? colorOptions[index].name : string.Empty;
                default:
                    return string.Empty;
            }
        }
    }

    [Serializable]
    public class CustomizationSet {
        public string name;
        public GameObject prefab;
        public Material material;
    }

    public enum CustomizationType {
        Head,
        Body,
        Hands,
        Color
    }
}