using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Gameplay.Character;

namespace GamePortfolio.UI.Character
{
    public class CharacterCustomizationUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button headPrevButton;
        [SerializeField] private Button headNextButton;
        [SerializeField] private TMP_Text headOptionText;
        
        [SerializeField] private Button bodyPrevButton;
        [SerializeField] private Button bodyNextButton;
        [SerializeField] private TMP_Text bodyOptionText;
        
        [SerializeField] private Button handsPrevButton;
        [SerializeField] private Button handsNextButton;
        [SerializeField] private TMP_Text handsOptionText;
        
        [SerializeField] private Button colorPrevButton;
        [SerializeField] private Button colorNextButton;
        [SerializeField] private TMP_Text colorOptionText;
        
        [Header("Character Preview")]
        [SerializeField] private Transform characterPreviewParent;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private Button resetRotationButton;
        
        [Header("Save Button")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        
        private CharacterCustomization customizationSystem;
        private GameObject previewCharacter;
        private bool isDragging = false;
        private Vector3 lastMousePosition;
        private Quaternion initialRotation;
        
        private void Start()
        {
            FindCustomizationSystem();
            SetupButtons();
            SetupCharacterPreview();
            UpdateAllOptionTexts();
        }
        
        private void Update()
        {
            HandleCharacterRotation();
        }
        
        private void FindCustomizationSystem()
        {
            customizationSystem = FindObjectOfType<CharacterCustomization>();
            
            if (customizationSystem == null)
            {
                Debug.LogError("CharacterCustomizationUI: CharacterCustomization component not found!");
            }
        }
        
        private void SetupButtons()
        {
            if (headPrevButton != null)
                headPrevButton.onClick.AddListener(() => CyclePrevOption(CustomizationType.Head));
            
            if (headNextButton != null)
                headNextButton.onClick.AddListener(() => CycleNextOption(CustomizationType.Head));
            
            if (bodyPrevButton != null)
                bodyPrevButton.onClick.AddListener(() => CyclePrevOption(CustomizationType.Body));
            
            if (bodyNextButton != null)
                bodyNextButton.onClick.AddListener(() => CycleNextOption(CustomizationType.Body));
            
            if (handsPrevButton != null)
                handsPrevButton.onClick.AddListener(() => CyclePrevOption(CustomizationType.Hands));
            
            if (handsNextButton != null)
                handsNextButton.onClick.AddListener(() => CycleNextOption(CustomizationType.Hands));
            
            if (colorPrevButton != null)
                colorPrevButton.onClick.AddListener(() => CyclePrevOption(CustomizationType.Color));
            
            if (colorNextButton != null)
                colorNextButton.onClick.AddListener(() => CycleNextOption(CustomizationType.Color));
            
            if (resetRotationButton != null)
                resetRotationButton.onClick.AddListener(ResetCharacterRotation);
            
            if (saveButton != null)
                saveButton.onClick.AddListener(SaveCustomization);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(CancelCustomization);
        }
        
        private void SetupCharacterPreview()
        {
            if (characterPreviewParent == null)
                return;
            
            previewCharacter = characterPreviewParent.GetChild(0)?.gameObject;
            
            if (previewCharacter != null)
            {
                initialRotation = previewCharacter.transform.rotation;
            }
        }
        
        private void HandleCharacterRotation()
        {
            if (characterPreviewParent == null || previewCharacter == null)
                return;
            
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.IsChildOf(characterPreviewParent) || hit.transform == characterPreviewParent)
                    {
                        isDragging = true;
                        lastMousePosition = Input.mousePosition;
                    }
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
            
            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                previewCharacter.transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime);
                lastMousePosition = Input.mousePosition;
            }
        }
        
        private void ResetCharacterRotation()
        {
            if (previewCharacter != null)
            {
                previewCharacter.transform.rotation = initialRotation;
            }
        }
        
        private void CycleNextOption(CustomizationType type)
        {
            if (customizationSystem == null)
                return;
            
            int currentIndex = customizationSystem.GetCurrentSelection(type);
            int maxOptions = customizationSystem.GetOptionCount(type);
            
            int nextIndex = (currentIndex + 1) % maxOptions;
            customizationSystem.ApplyCustomization(type, nextIndex);
            
            UpdateOptionText(type);
        }
        
        private void CyclePrevOption(CustomizationType type)
        {
            if (customizationSystem == null)
                return;
            
            int currentIndex = customizationSystem.GetCurrentSelection(type);
            int maxOptions = customizationSystem.GetOptionCount(type);
            
            int prevIndex = (currentIndex - 1 + maxOptions) % maxOptions;
            customizationSystem.ApplyCustomization(type, prevIndex);
            
            UpdateOptionText(type);
        }
        
        private void UpdateOptionText(CustomizationType type)
        {
            if (customizationSystem == null)
                return;
            
            int currentIndex = customizationSystem.GetCurrentSelection(type);
            int maxOptions = customizationSystem.GetOptionCount(type);
            string optionName = customizationSystem.GetOptionName(type, currentIndex);
            
            string displayText = $"{optionName} ({currentIndex + 1}/{maxOptions})";
            
            switch (type)
            {
                case CustomizationType.Head:
                    if (headOptionText != null)
                        headOptionText.text = displayText;
                    break;
                
                case CustomizationType.Body:
                    if (bodyOptionText != null)
                        bodyOptionText.text = displayText;
                    break;
                
                case CustomizationType.Hands:
                    if (handsOptionText != null)
                        handsOptionText.text = displayText;
                    break;
                
                case CustomizationType.Color:
                    if (colorOptionText != null)
                        colorOptionText.text = displayText;
                    break;
            }
        }
        
        private void UpdateAllOptionTexts()
        {
            UpdateOptionText(CustomizationType.Head);
            UpdateOptionText(CustomizationType.Body);
            UpdateOptionText(CustomizationType.Hands);
            UpdateOptionText(CustomizationType.Color);
        }
        
        private void SaveCustomization()
        {
            // In a full implementation, this would save the customization to player preferences
            // or a character data file
            
            // For now, just close the customization UI
            gameObject.SetActive(false);
        }
        
        private void CancelCustomization()
        {
            // In a full implementation, this would revert changes and restore previous customization
            
            // For now, just close the customization UI
            gameObject.SetActive(false);
        }
    }
}
