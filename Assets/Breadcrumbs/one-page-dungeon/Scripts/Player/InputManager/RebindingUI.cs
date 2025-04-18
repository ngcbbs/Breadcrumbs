using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Breadcrumbs.Player {
    public class RebindingUI : MonoBehaviour {
        [SerializeField] private string actionName;
        [SerializeField] private int bindingIndex;
        [SerializeField] private TMP_Text bindingDisplayNameText;
        [SerializeField] private Button rebindButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject waitingForInputUI;

        private void Start() {
            if (rebindButton != null) {
                rebindButton.onClick.AddListener(OnRebindButtonClicked);
            }

            if (resetButton != null) {
                resetButton.onClick.AddListener(OnResetButtonClicked);
            }

            UpdateBindingDisplayText();
        }

        private void OnDestroy() {
            if (rebindButton != null) {
                rebindButton.onClick.RemoveListener(OnRebindButtonClicked);
            }

            if (resetButton != null) {
                resetButton.onClick.RemoveListener(OnResetButtonClicked);
            }
        }

        public void UpdateBindingDisplayText() {
            if (bindingDisplayNameText != null) {
                string displayString = InputManager.Instance.GetBindingDisplayString(actionName, bindingIndex);
                bindingDisplayNameText.text = displayString;
            }
        }

        private void OnRebindButtonClicked() {
            if (waitingForInputUI != null) {
                waitingForInputUI.SetActive(true);
            }

            InputManager.Instance.StartRebinding(
                actionName, 
                bindingIndex,
                OnRebindComplete,
                OnRebindCancelled
            );
        }

        private void OnRebindComplete() {
            if (waitingForInputUI != null) {
                waitingForInputUI.SetActive(false);
            }
            UpdateBindingDisplayText();
        }

        private void OnRebindCancelled() {
            if (waitingForInputUI != null) {
                waitingForInputUI.SetActive(false);
            }
        }

        private void OnResetButtonClicked() {
            InputManager.Instance.ResetBinding(actionName, bindingIndex);
            UpdateBindingDisplayText();
        }
    }
}