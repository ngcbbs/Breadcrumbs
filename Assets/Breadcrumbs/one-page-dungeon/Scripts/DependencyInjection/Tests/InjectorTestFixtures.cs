// Assets/Tests/PlayMode/InjectorTestFixtures.cs (or inside PersistentSingletonTests.cs)
using UnityEngine;
using Breadcrumbs.DependencyInjection; // For Inject/Provide attributes
using Breadcrumbs.Singletons; // For IDependencyProvider if needed (though Injector finds it)
using System; // For Exception

// --- Dependencies ---
public interface IServiceA {
    string GetData();
}

public class ServiceA : IServiceA {
    public string Data { get; set; } = "Default ServiceA Data";
    public string GetData() => Data;
}

public class ServiceB {
    public int Value { get; set; } = 100;
}

public interface INotProvidedService { } // Intentionally not provided

// --- Provider ---
public class TestProvider : MonoBehaviour, IDependencyProvider {
    public bool ProvideServiceACalled = false;
    public bool ProvideServiceBCalled = false;

    [Provide]
    public IServiceA ProvideServiceA() {
        ProvideServiceACalled = true;
        return new ServiceA { Data = "Data From Provider" };
    }

    [Provide]
    public ServiceB ProvideServiceB() {
        ProvideServiceBCalled = true;
        return new ServiceB { Value = 999 };
    }

    // Method to test null return
    [Provide]
    public string ProvideNullString() {
        return null; // This should cause an exception during registration
    }
}


// --- Injectables ---
public class TestInjectableFields : MonoBehaviour {
    [Inject] public IServiceA ServiceAInstance;
    [Inject] private ServiceB _serviceBInstance; // Private field
    [Inject] public string AlreadySetField = "Initial Value"; // Test warning

    public ServiceB GetServiceB() => _serviceBInstance; // Accessor for testing
}

public class TestInjectableMethods : MonoBehaviour {
    public bool InjectionMethodCalled = false;
    public IServiceA ReceivedServiceA = null;
    public ServiceB ReceivedServiceB = null;

    [Inject]
    public void Initialize(IServiceA serviceA, ServiceB serviceB) {
        InjectionMethodCalled = true;
        ReceivedServiceA = serviceA;
        ReceivedServiceB = serviceB;
    }
}

public class TestInjectableProperties : MonoBehaviour {
    [Inject]
    public IServiceA ServiceAProp { get; private set; }

    [Inject]
    public ServiceB ServiceBProp { get; set; } // Public setter for testing
}

public class TestInjectableMissingDependency : MonoBehaviour {
    [Inject] public INotProvidedService MissingService; // This should fail injection
}

// Class that is neither provider nor injectable
public class NonRelevantMonoBehaviour : MonoBehaviour {
    public int SomeValue = 5;
}