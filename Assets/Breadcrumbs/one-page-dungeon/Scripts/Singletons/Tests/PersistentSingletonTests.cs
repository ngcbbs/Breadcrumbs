// Assets/Tests/PlayMode/PersistentSingletonTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;
using Breadcrumbs.Singletons; // Namespace for PersistentSingleton
// Make sure your TestSingleton implementations are accessible (either in this file or referenced)

public class PersistentSingletonTests
{
    private const string TestSceneName = "PersistentSingletonTestScene"; // Name for a temporary scene

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Ensure no instances linger from previous tests if run manually
        var existingInstances = Object.FindObjectsOfType<TestSingletonImplementations>();
        foreach (var instance in existingInstances)
        {
            Object.Destroy(instance.gameObject);
        }
         var existingNoUnparent = Object.FindObjectsOfType<TestSingletonNoUnparent>();
        foreach (var instance in existingNoUnparent)
        {
            Object.Destroy(instance.gameObject);
        }
        // Wait a frame to ensure destruction completes
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
       // Clean up any created singletons after each test
       // Note: DontDestroyOnLoad objects persist, so finding them is important.
        var instance = TestSingletonImplementations.TryGetInstance();
        if (instance != null)
        {
            Object.Destroy(instance.gameObject);
        }
         var instanceNoUnparent = TestSingletonNoUnparent.TryGetInstance();
        if (instanceNoUnparent != null)
        {
             Object.Destroy(instanceNoUnparent.gameObject);
        }

        // Ensure the test scene is unloaded if created
        // SceneManager.UnloadSceneAsync might be needed in more complex scenarios
        yield return null; // Wait a frame for destruction
    }

    // Test 1: Instance creation when none exists
    [UnityTest]
    public IEnumerator Instance_CreatesNew_WhenNoneExists()
    {
        // Arrange: Start with no instance

        // Act: Access the Instance property
        TestSingletonImplementations instance = TestSingletonImplementations.Instance;
        yield return null; // Wait a frame for Awake to potentially run

        // Assert
        Assert.IsNotNull(instance, "Instance should not be null after access.");
        Assert.AreEqual($"[{nameof(TestSingletonImplementations)}]", instance.gameObject.name, "GameObject should have the default name.");
        Assert.IsTrue(TestSingletonImplementations.HasInstance, "HasInstance should be true.");
        Assert.AreSame(instance, TestSingletonImplementations.TryGetInstance(), "TryGetInstance should return the same instance.");

        // Check if it's marked DontDestroyOnLoad (indirectly by checking scene root)
        Assert.AreEqual(null, instance.transform.root.gameObject.scene.name, "Instance GameObject should be in DontDestroyOnLoad scene root.");
        // Check default unparenting
        Assert.IsNull(instance.transform.parent, "Instance should have no parent by default.");
    }

    // Test 2: Instance finds existing one
    [UnityTest]
    public IEnumerator Instance_FindsExisting_WhenOneExists()
    {
        // Arrange: Manually create an instance beforehand
        var go = new GameObject("ManualTestSingleton");
        var manualInstance = go.AddComponent<TestSingletonImplementations>();
        manualInstance.TestData = "Manual";
        yield return null; // Allow Awake of manual instance if needed (though Instance getter might bypass it here)

        // Act: Access the Instance property
        TestSingletonImplementations accessedInstance = TestSingletonImplementations.Instance;
        yield return null;

        // Assert
        Assert.IsNotNull(accessedInstance, "Accessed instance should not be null.");
        Assert.AreSame(manualInstance, accessedInstance, "Instance getter should return the pre-existing instance.");
        Assert.AreEqual("Manual", accessedInstance.TestData, "Data should be from the manual instance.");
        Assert.AreEqual(1, Object.FindObjectsOfType<TestSingletonImplementations>().Length, "Should only be one instance in the scene.");
        // Check default unparenting happened to the manual instance
        Assert.IsNull(accessedInstance.transform.parent, "Existing instance should have been unparented.");
    }

    // Test 3: Second instance destroys itself
    [UnityTest]
    public IEnumerator SecondInstance_DestroysItself_WhenOneExists()
    {
        // Arrange: Create the first instance via the getter
        TestSingletonImplementations firstInstance = TestSingletonImplementations.Instance;
        firstInstance.TestData = "First";
        yield return null; // Ensure first instance's Awake runs

        // Arrange: Create a second instance manually
        var go2 = new GameObject("SecondTestSingleton");
        var secondInstanceAttempt = go2.AddComponent<TestSingletonImplementations>();
        // Let the second instance's Awake run
        yield return null;

        // Assert
        Assert.IsTrue(TestSingletonImplementations.HasInstance, "HasInstance should still be true.");
        Assert.IsNotNull(TestSingletonImplementations.Instance, "Singleton Instance should still exist.");
        Assert.AreSame(firstInstance, TestSingletonImplementations.Instance, "Singleton Instance should be the FIRST one.");
        Assert.AreEqual("First", TestSingletonImplementations.Instance.TestData, "Data should belong to the first instance.");

        // Check if the second GameObject was destroyed
        // Finding the specific component might return null if destroyed
        var foundInstances = Object.FindObjectsOfType<TestSingletonImplementations>();
        Assert.AreEqual(1, foundInstances.Length, "There should only be one TestSingleton component left.");
        Assert.AreSame(firstInstance, foundInstances[0], "The remaining instance should be the first one.");
        // Check if the GameObject itself is gone (might take a frame)
        Assert.IsNull(GameObject.Find("SecondTestSingleton"), "The second GameObject should have been destroyed.");

    }

    // Test 4: Persistence across scene loads
    [UnityTest]
    public IEnumerator Instance_Persists_AfterSceneLoad()
    {
        // Arrange: Create instance in the current scene
        TestSingletonImplementations initialInstance = TestSingletonImplementations.Instance;
        initialInstance.TestData = "Persisting";
        var initialGo = initialInstance.gameObject;
        yield return null; // Ensure Awake ran and DontDestroyOnLoad was called

        // Act: Load a new empty scene
        // Create a dummy scene to load into if needed, or use an existing empty one
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current scene (or load a different one)
        yield return null; // Wait for scene load to complete
        yield return null; // Extra frame just in case

        // Assert
        Assert.IsTrue(TestSingletonImplementations.HasInstance, "HasInstance should be true after scene load.");
        TestSingletonImplementations instanceAfterLoad = TestSingletonImplementations.Instance; // Access again
        Assert.IsNotNull(instanceAfterLoad, "Instance should not be null after scene load.");
        Assert.AreSame(initialInstance, instanceAfterLoad, "Instance after load should be the exact same object.");
        Assert.AreEqual("Persisting", instanceAfterLoad.TestData, "Data should persist across scenes.");
        // Verify it's the same GameObject
        Assert.IsTrue(initialGo != null && initialGo == instanceAfterLoad.gameObject, "GameObject reference should be the same.");
    }

     // Test 5: autoUnparentOnAwake = false keeps parent
    [UnityTest]
    public IEnumerator AutoUnparentFalse_KeepsParent_OnAwake()
    {
        // Arrange: Create a parent and add the specific singleton type as a child
        var parentGo = new GameObject("Parent");
        var childGo = new GameObject("ChildSingleton");
        childGo.transform.SetParent(parentGo.transform);
        // Add the component that sets autoUnparentOnAwake to false
        var instance = childGo.AddComponent<TestSingletonNoUnparent>();
        yield return null; // Allow Awake to run

        // Assert
        Assert.IsNotNull(TestSingletonNoUnparent.Instance, "Instance should be created.");
        Assert.AreSame(instance, TestSingletonNoUnparent.Instance, "Instance getter should return the correct instance.");
        Assert.IsNotNull(instance.transform.parent, "Instance should still have a parent.");
        Assert.AreSame(parentGo.transform, instance.transform.parent, "Instance parent should be the original parent GameObject.");

        // Cleanup the parent too
        Object.Destroy(parentGo);
        yield return null;
    }

     // Test 6: TryGetInstance returns null when no instance
    [UnityTest]
    public IEnumerator TryGetInstance_ReturnsNull_WhenNoneExists()
    {
        // Arrange: Ensure no instance exists (handled by SetUp)

        // Act
        var instance = TestSingletonImplementations.TryGetInstance();
        yield return null;

        // Assert
        Assert.IsNull(instance, "TryGetInstance should return null when no instance has been created.");
        Assert.IsFalse(TestSingletonImplementations.HasInstance, "HasInstance should be false.");
    }
}