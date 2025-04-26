using System;
using GamePortfolio.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// UI component for a single keybinding entry
    /// </summary>
    public class KeybindingUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text actionNameText;
        [SerializeField] private Text keyCodeText;
        [SerializeField] private Button remapButton;
        [SerializeField] private GameObject listeningPanel;
        [SerializeField] private Text listeningText;
        
        private string actionName;
        private KeyCode currentKey;
        private bool isListening = false;
        private Action<string, KeyCode> onKeyChanged;
        
        // Property to get action name
        public string ActionName => actionName;
        
        private void Awake()
        {
            // Set up remap button
            if (remapButton != null)
            {
                remapButton.onClick.AddListener(StartRemapping);
            }
            
            // Hide listening panel initially
            if (listeningPanel != null)
            {
                listeningPanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            // Check for key press when listening
            if (isListening)
            {
                CheckForKeyPress();
            }
        }
        
        /// <summary>
        /// Initialize the keybinding UI
        /// </summary>
        public void Initialize(string action, KeyCode key, Action<string, KeyCode> callback)
        {
            actionName = action;
            currentKey = key;
            onKeyChanged = callback;
            
            // Set action name
            if (actionNameText != null)
            {
                // Convert action name to display name (e.g., "MoveForward" to "Move Forward")
                string displayName = FormatActionName(actionName);
                actionNameText.text = displayName;
            }
            
            // Set key code
            if (keyCodeText != null)
            {
                keyCodeText.text = FormatKeyCode(currentKey);
            }
        }
        
        /// <summary>
        /// Update the key code display
        /// </summary>
        public void UpdateKeyCode(KeyCode newKey)
        {
            currentKey = newKey;
            
            if (keyCodeText != null)
            {
                keyCodeText.text = FormatKeyCode(currentKey);
            }
        }
        
        /// <summary>
        /// Start remapping the key
        /// </summary>
        private void StartRemapping()
        {
            isListening = true;
            
            // Show listening panel
            if (listeningPanel != null)
            {
                listeningPanel.SetActive(true);
            }
            
            // Update listening text
            if (listeningText != null)
            {
                listeningText.text = "Press any key...";
            }
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }
        
        /// <summary>
        /// Stop remapping the key
        /// </summary>
        private void StopRemapping()
        {
            isListening = false;
            
            // Hide listening panel
            if (listeningPanel != null)
            {
                listeningPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Check for key press when listening for remapping
        /// </summary>
        private void CheckForKeyPress()
        {
            // Check for escape to cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopRemapping();
                return;
            }
            
            // Check all key codes
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                // Skip mouse buttons, modifiers, and joystick buttons
                if (IsValidKeyForBinding(key) && Input.GetKeyDown(key))
                {
                    // Set new key
                    KeyCode oldKey = currentKey;
                    UpdateKeyCode(key);
                    
                    // Notify callback
                    onKeyChanged?.Invoke(actionName, key);
                    
                    // Stop listening
                    StopRemapping();
                    return;
                }
            }
        }
        
        /// <summary>
        /// Check if a key is valid for binding
        /// </summary>
        private bool IsValidKeyForBinding(KeyCode key)
        {
            // Skip system keys
            if (key == KeyCode.Escape || key == KeyCode.Print || key == KeyCode.SysReq ||
                key == KeyCode.Break || key == KeyCode.Menu || key == KeyCode.AltGr)
                return false;
                
            // Skip mouse movement
            if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1 || key == KeyCode.Mouse2 ||
                key == KeyCode.Mouse3 || key == KeyCode.Mouse4 || key == KeyCode.Mouse5 ||
                key == KeyCode.Mouse6)
                return false;
                
            // Skip joystick axes
            if (key.ToString().Contains("JoystickButton"))
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Format action name for display
        /// </summary>
        private string FormatActionName(string actionName)
        {
            // Insert spaces before capital letters
            string displayName = System.Text.RegularExpressions.Regex.Replace(
                actionName, 
                "([a-z])([A-Z])", 
                "$1 $2");
                
            // Capitalize first letter
            if (displayName.Length > 0)
            {
                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);
            }
            
            return displayName;
        }
        
        /// <summary>
        /// Format key code for display
        /// </summary>
        private string FormatKeyCode(KeyCode key)
        {
            string keyString = key.ToString();
            
            // Handle special cases
            switch (key)
            {
                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    return keyString.Replace("Alpha", "");
                    
                case KeyCode.Keypad0:
                case KeyCode.Keypad1:
                case KeyCode.Keypad2:
                case KeyCode.Keypad3:
                case KeyCode.Keypad4:
                case KeyCode.Keypad5:
                case KeyCode.Keypad6:
                case KeyCode.Keypad7:
                case KeyCode.Keypad8:
                case KeyCode.Keypad9:
                    return keyString.Replace("Keypad", "Num ");
                    
                case KeyCode.KeypadPeriod:
                    return "Num .";
                case KeyCode.KeypadDivide:
                    return "Num /";
                case KeyCode.KeypadMultiply:
                    return "Num *";
                case KeyCode.KeypadMinus:
                    return "Num -";
                case KeyCode.KeypadPlus:
                    return "Num +";
                case KeyCode.KeypadEnter:
                    return "Num Enter";
                case KeyCode.KeypadEquals:
                    return "Num =";
                    
                case KeyCode.LeftShift:
                    return "L Shift";
                case KeyCode.RightShift:
                    return "R Shift";
                case KeyCode.LeftControl:
                    return "L Ctrl";
                case KeyCode.RightControl:
                    return "R Ctrl";
                case KeyCode.LeftAlt:
                    return "L Alt";
                case KeyCode.RightAlt:
                    return "R Alt";
                    
                case KeyCode.BackQuote:
                    return "`";
                case KeyCode.Minus:
                    return "-";
                case KeyCode.Equals:
                    return "=";
                case KeyCode.LeftBracket:
                    return "[";
                case KeyCode.RightBracket:
                    return "]";
                case KeyCode.Backslash:
                    return "\\";
                case KeyCode.Semicolon:
                    return ";";
                case KeyCode.Quote:
                    return "'";
                case KeyCode.Comma:
                    return ",";
                case KeyCode.Period:
                    return ".";
                case KeyCode.Slash:
                    return "/";
                    
                default:
                    return keyString;
            }
        }
        
        /// <summary>
        /// Handle click on keybinding entry
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // Start remapping on right click
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                StartRemapping();
            }
        }
    }
}