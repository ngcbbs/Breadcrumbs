using UnityEngine;
using UnityEngine.UI;

namespace Breadcrumbs.Player {
    public class InputSettingsMenu : MonoBehaviour {
        [SerializeField] private Button resetAllButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private RebindingUI[] rebindingUIElements;

        private void Start() {
            if (resetAllButton != null) {
                resetAllButton.onClick.AddListener(OnResetAllButtonClicked);
            }

            if (saveButton != null) {
                saveButton.onClick.AddListener(OnSaveButtonClicked);
            }
        }

        private void OnDestroy() {
            if (resetAllButton != null) {
                resetAllButton.onClick.RemoveListener(OnResetAllButtonClicked);
            }

            if (saveButton != null) {
                saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            }
        }

        private void OnResetAllButtonClicked() {
            InputManager.Instance.ResetAllBindings();
            UpdateAllRebindingUIElements();
        }

        private void OnSaveButtonClicked() {
            InputManager.Instance.SaveBindingOverrides();
        }

        public void UpdateAllRebindingUIElements() {
            foreach (var rebindingUI in rebindingUIElements) {
                if (rebindingUI != null) {
                    rebindingUI.UpdateBindingDisplayText();
                }
            }
        }
    }
}