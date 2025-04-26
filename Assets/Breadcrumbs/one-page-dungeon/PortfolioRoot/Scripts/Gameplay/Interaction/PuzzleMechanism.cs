using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Interaction;

namespace GamePortfolio.Gameplay.Interaction
{
    /// <summary>
    /// A puzzle mechanism that requires multiple components to be activated in a specific order or pattern
    /// Can be used for lever sequences, pressure plates, rune combinations, etc.
    /// </summary>
    public class PuzzleMechanism : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        [SerializeField] private string puzzleName = "Puzzle";
        [SerializeField] private bool requireSpecificOrder = true;
        [SerializeField] private bool resetOnWrongInput = true;
        [SerializeField] private float resetDelay = 2f;
        [SerializeField] private int maximumAttempts = 0; // 0 = unlimited
        
        [Header("Component References")]
        [SerializeField] private List<ComponentReference> puzzleComponents = new List<ComponentReference>();
        
        [Header("Feedback")]
        [SerializeField] private GameObject successFeedback;
        [SerializeField] private GameObject failureFeedback;
        [SerializeField] private GameObject lockedFeedback;
        [SerializeField] private float feedbackDuration = 3f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent OnPuzzleSolved;
        [SerializeField] private UnityEvent OnPuzzleFailed;
        [SerializeField] private UnityEvent OnPuzzleLocked;
        [SerializeField] private UnityEvent OnPuzzleReset;
        
        // Runtime state
        private List<int> expectedSequence = new List<int>();
        private List<int> currentSequence = new List<int>();
        private bool isPuzzleSolved = false;
        private bool isPuzzleLocked = false;
        private int currentAttempts = 0;
        private Coroutine feedbackCoroutine;
        
        // Events
        public event Action OnPuzzleStateChanged;
        
        private void Awake()
        {
            // Initialize the expected sequence
            BuildExpectedSequence();
            
            // Subscribe to component events
            for (int i = 0; i < puzzleComponents.Count; i++)
            {
                int componentIndex = i; // Capture index for lambda
                
                // Get the component
                ComponentReference reference = puzzleComponents[i];
                
                // Set up the listener based on component type
                if (reference.Component != null)
                {
                    // Lever
                    LeverMechanism lever = reference.Component.GetComponent<LeverMechanism>();
                    if (lever != null)
                    {
                        lever.OnLeverStateChanged += (isOn) => {
                            if (isOn)
                            {
                                HandleComponentActivated(componentIndex);
                            }
                        };
                        continue;
                    }
                    
                    // Other component types could be added here...
                    // For example, pressure plates, rune stones, etc.
                }
            }
            
            // Initialize feedback objects
            if (successFeedback != null) successFeedback.SetActive(false);
            if (failureFeedback != null) failureFeedback.SetActive(false);
            if (lockedFeedback != null) lockedFeedback.SetActive(false);
        }
        
        /// <summary>
        /// Build the expected sequence based on configuration
        /// </summary>
        private void BuildExpectedSequence()
        {
            expectedSequence.Clear();
            
            for (int i = 0; i < puzzleComponents.Count; i++)
            {
                // If the component is part of the sequence, add its order
                if (puzzleComponents[i].IsPartOfSequence)
                {
                    // Components with order 0 are ignored in the sequence
                    if (puzzleComponents[i].ActivationOrder > 0)
                    {
                        expectedSequence.Add(i);
                    }
                }
            }
            
            // Sort by activation order if specific order is required
            if (requireSpecificOrder && expectedSequence.Count > 0)
            {
                expectedSequence.Sort((a, b) => 
                    puzzleComponents[a].ActivationOrder.CompareTo(puzzleComponents[b].ActivationOrder));
            }
        }
        
        /// <summary>
        /// Handle when a component is activated
        /// </summary>
        private void HandleComponentActivated(int componentIndex)
        {
            // Skip if puzzle is already solved or locked
            if (isPuzzleSolved || isPuzzleLocked)
                return;
                
            // Skip if component isn't part of sequence
            if (!puzzleComponents[componentIndex].IsPartOfSequence)
                return;
                
            // Check if this component is part of the expected sequence
            if (!expectedSequence.Contains(componentIndex))
                return;
                
            // Add to current sequence
            currentSequence.Add(componentIndex);
            
            // Play activation sound
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySfx("PuzzleComponentActivated");
            }
            
            // Check for specific order if required
            if (requireSpecificOrder)
            {
                // Check if current sequence matches expected sequence up to current point
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    if (i >= expectedSequence.Count || currentSequence[i] != expectedSequence[i])
                    {
                        // Sequence is wrong
                        HandleWrongSequence();
                        return;
                    }
                }
            }
            else
            {
                // Order doesn't matter, just check if component is in expected sequence
                // Since we already checked this above, we don't need to do anything here
            }
            
            // Check if the puzzle is solved
            CheckForSolution();
            
            // Notify state change
            OnPuzzleStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Handle when a wrong sequence is entered
        /// </summary>
        private void HandleWrongSequence()
        {
            // Increment attempt counter
            currentAttempts++;
            
            // Check if maximum attempts reached
            if (maximumAttempts > 0 && currentAttempts >= maximumAttempts)
            {
                // Lock the puzzle
                isPuzzleLocked = true;
                
                // Invoke locked event
                OnPuzzleLocked.Invoke();
                
                // Show locked feedback
                ShowFeedback(lockedFeedback);
                
                // Play locked sound
                if (AudioManager.HasInstance)
                {
                    AudioManager.Instance.PlaySfx("PuzzleLocked");
                }
                
                Debug.Log($"Puzzle {puzzleName} locked after {maximumAttempts} failed attempts.");
                return;
            }
            
            // Show failure feedback
            ShowFeedback(failureFeedback);
            
            // Play failure sound
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySfx("PuzzleFailed");
            }
            
            // Invoke failure event
            OnPuzzleFailed.Invoke();
            
            // Reset puzzle if configured to do so
            if (resetOnWrongInput)
            {
                StartCoroutine(ResetAfterDelay(resetDelay));
            }
            
            Debug.Log($"Puzzle {puzzleName} sequence incorrect. Attempt {currentAttempts}/{maximumAttempts}");
            
            // Notify state change
            OnPuzzleStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Check if the puzzle is solved
        /// </summary>
        private void CheckForSolution()
        {
            bool isSolved = false;
            
            if (requireSpecificOrder)
            {
                // For ordered sequence, current must match expected exactly
                isSolved = currentSequence.Count == expectedSequence.Count;
            }
            else
            {
                // For unordered, all expected components must be in current sequence
                // but the order doesn't matter
                isSolved = true;
                
                foreach (int expectedComponent in expectedSequence)
                {
                    if (!currentSequence.Contains(expectedComponent))
                    {
                        isSolved = false;
                        break;
                    }
                }
            }
            
            if (isSolved)
            {
                SolvePuzzle();
            }
        }
        
        /// <summary>
        /// Mark the puzzle as solved and trigger events
        /// </summary>
        private void SolvePuzzle()
        {
            // Skip if already solved
            if (isPuzzleSolved)
                return;
                
            isPuzzleSolved = true;
            
            // Show success feedback
            ShowFeedback(successFeedback);
            
            // Play success sound
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySfx("PuzzleSolved");
            }
            
            // Invoke success event
            OnPuzzleSolved.Invoke();
            
            Debug.Log($"Puzzle {puzzleName} solved!");
            
            // Notify state change
            OnPuzzleStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Reset the puzzle state
        /// </summary>
        public void ResetPuzzle()
        {
            // Reset sequence
            currentSequence.Clear();
            
            // Update components if needed
            foreach (ComponentReference reference in puzzleComponents)
            {
                // Reset levers
                LeverMechanism lever = reference.Component?.GetComponent<LeverMechanism>();
                if (lever != null && reference.ResetOnPuzzleReset)
                {
                    lever.ResetLever();
                }
                
                // Other component types could be reset here
            }
            
            // Invoke reset event
            OnPuzzleReset.Invoke();
            
            Debug.Log($"Puzzle {puzzleName} reset.");
            
            // Notify state change
            OnPuzzleStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Reset the puzzle after a delay
        /// </summary>
        private IEnumerator ResetAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetPuzzle();
        }
        
        /// <summary>
        /// Show feedback object for a duration
        /// </summary>
        private void ShowFeedback(GameObject feedback)
        {
            if (feedback == null)
                return;
                
            // Stop any existing feedback
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
            }
            
            // Start new feedback
            feedbackCoroutine = StartCoroutine(ShowFeedbackRoutine(feedback));
        }
        
        /// <summary>
        /// Coroutine to show feedback for a duration
        /// </summary>
        private IEnumerator ShowFeedbackRoutine(GameObject feedback)
        {
            // Hide all feedback objects
            if (successFeedback != null) successFeedback.SetActive(false);
            if (failureFeedback != null) failureFeedback.SetActive(false);
            if (lockedFeedback != null) lockedFeedback.SetActive(false);
            
            // Show the requested feedback
            feedback.SetActive(true);
            
            // Wait for duration
            yield return new WaitForSeconds(feedbackDuration);
            
            // Hide the feedback if puzzle isn't solved/locked (permanent feedback)
            if (feedback != successFeedback || !isPuzzleSolved)
            {
                if (feedback != lockedFeedback || !isPuzzleLocked)
                {
                    feedback.SetActive(false);
                }
            }
            
            feedbackCoroutine = null;
        }
        
        /// <summary>
        /// Force solve the puzzle
        /// </summary>
        public void ForceSolve()
        {
            // Set the current sequence to the expected sequence
            currentSequence.Clear();
            currentSequence.AddRange(expectedSequence);
            
            // Mark as solved
            SolvePuzzle();
            
            // Update components to solved state
            foreach (ComponentReference reference in puzzleComponents)
            {
                // Update levers
                LeverMechanism lever = reference.Component?.GetComponent<LeverMechanism>();
                if (lever != null && expectedSequence.Contains(puzzleComponents.IndexOf(reference)))
                {
                    lever.SetState(true);
                }
                
                // Other component types could be updated here
            }
        }
        
        /// <summary>
        /// Check if the puzzle is solved
        /// </summary>
        public bool IsSolved()
        {
            return isPuzzleSolved;
        }
        
        /// <summary>
        /// Check if the puzzle is locked
        /// </summary>
        public bool IsLocked()
        {
            return isPuzzleLocked;
        }
        
        /// <summary>
        /// Get the current number of attempts
        /// </summary>
        public int GetAttemptCount()
        {
            return currentAttempts;
        }
        
        /// <summary>
        /// Get the component sequence that has been activated so far
        /// </summary>
        public List<GameObject> GetCurrentSequenceComponents()
        {
            List<GameObject> components = new List<GameObject>();
            
            foreach (int index in currentSequence)
            {
                if (index >= 0 && index < puzzleComponents.Count && puzzleComponents[index].Component != null)
                {
                    components.Add(puzzleComponents[index].Component);
                }
            }
            
            return components;
        }
    }
    
    /// <summary>
    /// Reference to a puzzle component with activation order settings
    /// </summary>
    [System.Serializable]
    public class ComponentReference
    {
        [Tooltip("The GameObject component that is part of the puzzle")]
        public GameObject Component;
        
        [Tooltip("Whether this component is part of the activation sequence")]
        public bool IsPartOfSequence = true;
        
        [Tooltip("The order in which this component should be activated (0 = not in sequence)")]
        public int ActivationOrder = 0;
        
        [Tooltip("Whether to reset this component when the puzzle resets")]
        public bool ResetOnPuzzleReset = true;
    }
}