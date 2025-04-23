// Assets/Tests/PlayMode/InjectorTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Linq; // For Linq operations if needed
using Breadcrumbs.DependencyInjection;
using System; // For Exception checks

public class InjectorTests {
    private GameObject providerGo;
    private GameObject injectableFieldsGo;
    private GameObject injectableMethodsGo;
    private GameObject injectablePropertiesGo;
    private GameObject injectableMissingGo;
    private GameObject nonRelevantGo;
    private GameObject injectorGo; // Keep track of the injector itself

    private TestProvider testProvider;
    private TestInjectableFields testInjectableFields;
    private TestInjectableMethods testInjectableMethods;
    private TestInjectableProperties testInjectableProperties;
    private TestInjectableMissingDependency testInjectableMissing;

    // Helper to create GameObjects with components
    private T CreateTestComponent<T>(string name) where T : Component {
        var go = new GameObject(name);
        return go.AddComponent<T>();
    }

    [UnitySetUp]
    public IEnumerator SetUp() {
        // Clean up potential leftovers from previous runs
        var existingInjector = Injector.TryGetInstance();
        if (existingInjector != null) {
            UnityEngine.Object.Destroy(existingInjector.gameObject);
        }
        // Destroy any test objects that might linger
        var testObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
            .Where(mb => mb.GetType().Name.StartsWith("Test") || mb is NonRelevantMonoBehaviour)
            .Select(mb => mb.gameObject)
            .Distinct();
        foreach (var obj in testObjects) {
            UnityEngine.Object.Destroy(obj);
        }

        yield return null; // Wait a frame for destruction

        // Reset instance variables for the next test
        providerGo = null;
        injectableFieldsGo = null;
        injectableMethodsGo = null;
        injectablePropertiesGo = null;
        injectableMissingGo = null;
        nonRelevantGo = null;
        injectorGo = null;

        testProvider = null;
        testInjectableFields = null;
        testInjectableMethods = null;
        testInjectableProperties = null;
        testInjectableMissing = null;
    }

    [UnityTearDown]
    public IEnumerator TearDown() {
        // Destroy objects created during the test
        if (injectorGo != null) UnityEngine.Object.Destroy(injectorGo);
        if (providerGo != null) UnityEngine.Object.Destroy(providerGo);
        if (injectableFieldsGo != null) UnityEngine.Object.Destroy(injectableFieldsGo);
        if (injectableMethodsGo != null) UnityEngine.Object.Destroy(injectableMethodsGo);
        if (injectablePropertiesGo != null) UnityEngine.Object.Destroy(injectablePropertiesGo);
        if (injectableMissingGo != null) UnityEngine.Object.Destroy(injectableMissingGo);
        if (nonRelevantGo != null) UnityEngine.Object.Destroy(nonRelevantGo);

        // Explicitly try to destroy the singleton instance again, as it might persist
        var existingInjector = Injector.TryGetInstance();
        if (existingInjector != null) {
             UnityEngine.Object.Destroy(existingInjector.gameObject);
        }

        yield return null; // Wait a frame for destruction
    }

    // --- Core Injection Tests ---

    [UnityTest]
    public IEnumerator Injector_Awake_InjectsFieldsCorrectly() {
        // Arrange: Create provider and injectable *before* Injector wakes up
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableFields = CreateTestComponent<TestInjectableFields>("InjectableFields");
        injectableFieldsGo = testInjectableFields.gameObject;

        // Act: Create the Injector, triggering its Awake
        injectorGo = new GameObject("Injector");
        injectorGo.AddComponent<Injector>();
        yield return null; // Wait for Awake to run

        // Assert
        Assert.IsNotNull(testInjectableFields.ServiceAInstance, "ServiceA field should be injected.");
        Assert.AreEqual("Data From Provider", testInjectableFields.ServiceAInstance.GetData(), "ServiceA data mismatch.");
        Assert.IsNotNull(testInjectableFields.GetServiceB(), "ServiceB private field should be injected.");
        Assert.AreEqual(999, testInjectableFields.GetServiceB().Value, "ServiceB value mismatch.");
        Assert.IsTrue(testProvider.ProvideServiceACalled, "[Provide] ServiceA method was not called.");
        Assert.IsTrue(testProvider.ProvideServiceBCalled, "[Provide] ServiceB method was not called.");
    }

    [UnityTest]
    public IEnumerator Injector_Awake_InjectsMethodsCorrectly() {
        // Arrange
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableMethods = CreateTestComponent<TestInjectableMethods>("InjectableMethods");
        injectableMethodsGo = testInjectableMethods.gameObject;

        // Act
        injectorGo = new GameObject("Injector");
        injectorGo.AddComponent<Injector>();
        yield return null;

        // Assert
        Assert.IsTrue(testInjectableMethods.InjectionMethodCalled, "Injection method should have been called.");
        Assert.IsNotNull(testInjectableMethods.ReceivedServiceA, "ServiceA parameter should be received.");
        Assert.AreEqual("Data From Provider", testInjectableMethods.ReceivedServiceA.GetData());
        Assert.IsNotNull(testInjectableMethods.ReceivedServiceB, "ServiceB parameter should be received.");
        Assert.AreEqual(999, testInjectableMethods.ReceivedServiceB.Value);
    }

    [UnityTest]
    public IEnumerator Injector_Awake_InjectsPropertiesCorrectly() {
        // Arrange
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableProperties = CreateTestComponent<TestInjectableProperties>("InjectableProperties");
        injectablePropertiesGo = testInjectableProperties.gameObject;

        // Act
        injectorGo = new GameObject("Injector");
        injectorGo.AddComponent<Injector>();
        yield return null;

        // Assert
        Assert.IsNotNull(testInjectableProperties.ServiceAProp, "ServiceA property should be injected.");
        Assert.AreEqual("Data From Provider", testInjectableProperties.ServiceAProp.GetData());
        Assert.IsNotNull(testInjectableProperties.ServiceBProp, "ServiceB property should be injected.");
        Assert.AreEqual(999, testInjectableProperties.ServiceBProp.Value);
    }

    // skip :P
    /*
    [UnityTest]
    public IEnumerator Injector_ManualRegister_InjectsCorrectly() {
        // Arrange: Create Injector first
        injectorGo = new GameObject("Injector");
        var injectorInstance = injectorGo.AddComponent<Injector>();
        yield return null; // Let Injector Awake finish (it won't find providers yet)

        // Arrange: Manually register a dependency
        var manualService = new ServiceA { Data = "Manually Registered" };
        injectorInstance.Register<IServiceA>(manualService);

        // Arrange: Create the injectable *after* manual registration
        testInjectableFields = CreateTestComponent<TestInjectableFields>("InjectableFields");
        injectableFieldsGo = testInjectableFields.gameObject;

        // Act: Manually trigger injection for the new object
        injectorInstance.Inject(testInjectableFields);
        yield return null;

        // Assert
        Assert.IsNotNull(testInjectableFields.ServiceAInstance, "Manually registered ServiceA should be injected.");
        Assert.AreSame(manualService, testInjectableFields.ServiceAInstance, "Injected instance should be the manually registered one.");
        Assert.AreEqual("Manually Registered", testInjectableFields.ServiceAInstance.GetData());
        // ServiceB should not be injected as it wasn't manually registered and no provider ran for this object
        Assert.IsNull(testInjectableFields.GetServiceB(), "ServiceB should not be injected in this scenario.");
    }
    // */


    // --- Error Handling and Edge Cases ---

    [UnityTest]
    public IEnumerator Injector_Awake_Throws_WhenProviderReturnsNull() {
        // Arrange: Create the provider that returns null *before* Injector
        testProvider = CreateTestComponent<TestProvider>("ProviderWithNull");
        providerGo = testProvider.gameObject;

        // Act & Assert: Expect an exception during Injector's Awake
        LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Provided instance is null.*Method: ProvideNullString.*Return type: System.String"));

        injectorGo = new GameObject("Injector");
        injectorGo.AddComponent<Injector>(); // This Awake should trigger the exception

        yield return null; // Allow Awake to run and log the exception
    }


    [UnityTest]
    public IEnumerator Injector_Awake_Throws_WhenDependencyIsMissing() {
        // Arrange: Create provider (doesn't provide INotProvidedService) and the injectable that needs it
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableMissing = CreateTestComponent<TestInjectableMissingDependency>("InjectableMissing");
        injectableMissingGo = testInjectableMissing.gameObject;

        // Act & Assert: Expect an exception during Injector's Awake when injecting
        LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Can't resolve field = INotProvidedService"));

        injectorGo = new GameObject("Injector");
        injectorGo.AddComponent<Injector>(); // This Awake should trigger the exception during injection phase

        yield return null; // Allow Awake to run and log the exception
    }

    [UnityTest]
    public IEnumerator Injector_Awake_Warns_WhenFieldAlreadySet() {
        // Arrange
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableFields = CreateTestComponent<TestInjectableFields>("InjectableFields");
        injectableFieldsGo = testInjectableFields.gameObject;
        // Ensure the field has its initial value
        Assert.AreEqual("Initial Value", testInjectableFields.AlreadySetField);

        // Act & Assert: Expect the warning during Injector's Awake
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("\\[Injector\\] Field 'AlreadySetField' of class 'TestInjectableFields' is already set."));

        injectorGo = new GameObject("Injector");
        injectorGo.AddComponent<Injector>();
        yield return null;

        // Assert: Verify the field was NOT overwritten
        Assert.AreEqual("Initial Value", testInjectableFields.AlreadySetField, "Field should not be overwritten by injector.");
    }

    // --- Utility Method Tests ---

    [UnityTest]
    public IEnumerator ValidateDependencies_Succeeds_WhenAllValid() {
        // Arrange
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableFields = CreateTestComponent<TestInjectableFields>("InjectableFields");
        injectableFieldsGo = testInjectableFields.gameObject;
        nonRelevantGo = CreateTestComponent<NonRelevantMonoBehaviour>("NonRelevant").gameObject; // Add unrelated object

        injectorGo = new GameObject("Injector");
        var injectorInstance = injectorGo.AddComponent<Injector>();
        yield return null; // Let Awake run

        // Act & Assert: Expect the success log
        LogAssert.Expect(LogType.Log, "[Validation] All dependencies are valid.");
        injectorInstance.ValidateDependencies();
        yield return null;
    }

    [UnityTest]
    public IEnumerator ValidateDependencies_Fails_WhenDependencyMissing() {
        // Arrange: Provider exists, but injectable needs something not provided
        testProvider = CreateTestComponent<TestProvider>("Provider"); // Doesn't provide INotProvidedService
        providerGo = testProvider.gameObject;
        testInjectableMissing = CreateTestComponent<TestInjectableMissingDependency>("InjectableMissing");
        injectableMissingGo = testInjectableMissing.gameObject;

        injectorGo = new GameObject("Injector");
        var injectorInstance = injectorGo.AddComponent<Injector>();
        // Don't wait for Awake's injection phase to throw, test Validate directly
        // yield return null;

        // Act & Assert: Expect error logs
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[Validation\\] .* dependencies are invalid:"));
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[Validation\\] TestInjectableMissingDependency is missing dependency INotProvidedService on GameObject InjectableMissing"));

        injectorInstance.ValidateDependencies();
        yield return null;
    }

     [UnityTest]
    public IEnumerator ClearDependencies_SetsInjectableFieldsToNull()
    {
        // Arrange: Setup and inject normally first
        testProvider = CreateTestComponent<TestProvider>("Provider");
        providerGo = testProvider.gameObject;
        testInjectableFields = CreateTestComponent<TestInjectableFields>("InjectableFields");
        injectableFieldsGo = testInjectableFields.gameObject;

        injectorGo = new GameObject("Injector");
        var injectorInstance = injectorGo.AddComponent<Injector>();
        yield return null; // Let Awake run injection

        // Arrange: Verify injection happened
        Assert.IsNotNull(testInjectableFields.ServiceAInstance, "Pre-condition failed: ServiceA should be injected.");
        Assert.IsNotNull(testInjectableFields.GetServiceB(), "Pre-condition failed: ServiceB should be injected.");

        // Act: Clear dependencies
        LogAssert.Expect(LogType.Log, "[Injector] All injectable fields cleared.");
        injectorInstance.ClearDependencies();
        yield return null;

        // Assert: Check fields are now null
        Assert.IsNull(testInjectableFields.ServiceAInstance, "ServiceA field should be cleared.");
        Assert.IsNull(testInjectableFields.GetServiceB(), "ServiceB field should be cleared.");
        // Note: ClearDependencies only targets fields in the current implementation
    }
}