// Assets/Tests/PlayMode/TestSingletonImplementations.cs (or inside your test script)
using Breadcrumbs.Singletons;
using UnityEngine;

// Basic implementation for most tests
public class TestSingletonImplementations : PersistentSingleton<TestSingletonImplementations>
{
    public string TestData = "Initial";

    // Optional: Override Awake if needed, but remember base.Awake()
    // protected override void Awake() {
    //     base.Awake();
    //     // Additional setup for TestSingleton if needed
    // }
}

// Implementation to test autoUnparentOnAwake = false
public class TestSingletonNoUnparent : PersistentSingleton<TestSingletonNoUnparent>
{
    protected override void Awake()
    {
        autoUnparentOnAwake = false; // Set before base.Awake() or Initialize() is called
        base.Awake();
        Debug.Log($"TestSingletonNoUnparent Awake: autoUnparent={autoUnparentOnAwake}, Parent={transform.parent?.name ?? "None"}");
    }
}