using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.DependencyInjection {
    [DefaultExecutionOrder(-1000)]
    public class Injector : PersistentSingleton<Injector> {
        private const BindingFlags kBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly Dictionary<Type, object> _registry = new();

        protected override void Awake() {
            base.Awake();

            var targets = FindMonoBehaviours();
            var providers = targets.OfType<IDependencyProvider>();
            foreach (var provider in providers) {
                //Debug.Log($"Provider = {provider.GetType().Name}");
                Register(provider);
            }

            var injectables = targets.Where(IsInjectable);
            foreach (var injectable in injectables) {
                //Debug.Log($"Injectable = {injectable.GetType().Name}");
                Inject(injectable);
            }
        }

        private object Resolve(Type type) {
            _registry.TryGetValue(type, out var resolved);
            return resolved;
        }

        public Injector Register<T>(T obj) {
            _registry[typeof(T)] = obj;
            return this;
        }
        
        // hum...
        public Injector Register<T>(T obj, Action<T> resolver) {
            _registry[typeof(T)] = obj;
            return this;
        }

        private void Register(IDependencyProvider provider) {
            var methods = provider.GetType().GetMethods(kBindingFlags);

            foreach (var method in methods) {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                    continue;
                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null) {
                    _registry.Add(returnType, providedInstance);
                }
                else {
                    throw new Exception($"Provided instance is null.\nClass: {provider.GetType().Name}\nMethod: {method.Name}\nReturn type: {returnType}");
                }
            }
        }

        private void Inject(object obj) {
            var objType = obj.GetType();

            // 필드
            var injectableFields  = objType.GetFields(kBindingFlags)
                .Where(x => Attribute.IsDefined(x, typeof(InjectAttribute)));

            foreach (var field in injectableFields ) {
                if (field.GetValue(obj) != null) {
                    Debug.LogWarning($"[Injector] Field '{field.Name}' of class '{objType.Name}' is already set.");
                    continue;
                }
                var fieldType = field.FieldType;
                var resolved = Resolve(fieldType);
                if (resolved == null) {
                    throw new Exception($"Can't resolve field = {fieldType}");
                }

                field.SetValue(obj, resolved);
            }

            // 메소드
            var injectableMethods = objType.GetMethods(kBindingFlags)
                .Where(x => Attribute.IsDefined(x, typeof(InjectAttribute)));

            foreach (var method in injectableMethods) {
                var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var resolved = parameters.Select(Resolve).ToArray();
                if (resolved.Any(resolveInstance => resolveInstance == null)) {
                    throw new Exception($"Can't resolve method = {method.Name}");
                }

                method.Invoke(obj, resolved);
            }

            // 프로퍼티
            var injectableProperties = objType.GetProperties(kBindingFlags)
                .Where(x => Attribute.IsDefined(x, typeof(InjectAttribute)));

            foreach (var property in injectableProperties) {
                var type = property.PropertyType;
                var resolved = Resolve(type);
                if (resolved == null) {
                    throw new Exception($"Can't resolve property = {property.Name}");
                }

                property.SetValue(obj, resolved);
            }
        }


        #region Validation

        public void ValidateDependencies() {
            var targets = FindMonoBehaviours();
            var providers = targets.OfType<IDependencyProvider>();
            var providedDependencies = GetProvidedDependencies(providers);
            
            var invalidDependencies = targets
                .SelectMany(mb => mb.GetType().GetFields(kBindingFlags), (mb, field) => new {mb, field})
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject {t.mb.gameObject.name}");
            
            var invalidDependencyList = invalidDependencies.ToList();
            
            if (!invalidDependencyList.Any()) {
                Debug.Log("[Validation] All dependencies are valid.");
            } else {
                Debug.LogError($"[Validation] {invalidDependencyList.Count} dependencies are invalid:");
                foreach (var invalidDependency in invalidDependencyList) {
                    Debug.LogError(invalidDependency);
                }
            }
        }
        
        private HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyProvider> providers) {
            var providedDependencies = new HashSet<Type>();
            foreach (var provider in providers) {
                var methods = provider.GetType().GetMethods(kBindingFlags);

                foreach (var method in methods) {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                        continue;

                    var returnType = method.ReturnType;
                    providedDependencies.Add(returnType);
                }
            }

            return providedDependencies;
        }
        
        #endregion
        
        public void ClearDependencies() {
            foreach (var monoBehaviour in FindMonoBehaviours()) {
                var type = monoBehaviour.GetType();
                var injectableFields = type.GetFields(kBindingFlags)
                    .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                foreach (var injectableField in injectableFields) {
                    injectableField.SetValue(monoBehaviour, null);
                }
            }
            
            Debug.Log("[Injector] All injectable fields cleared.");
        }
        
        private static bool IsInjectable(MonoBehaviour obj) {
            var members = obj.GetType().GetMembers(kBindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
        
        private static MonoBehaviour[] FindMonoBehaviours() {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
    }
}
