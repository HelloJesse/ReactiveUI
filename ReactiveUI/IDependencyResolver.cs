﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveUI
{
    public interface IDependencyResolver : IDisposable
    {
        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>. Must return <c>null</c>
        /// if the service is not available (must not throw).
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>The requested object, if found; <c>null</c> otherwise.</returns>
        object GetService(Type serviceType, string contract = null);

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>. Must return an empty
        /// collection if the service is not available (must not return <c>null</c> or throw).
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>. The sequence
        /// should be empty (not <c>null</c>) if no objects of the given type are available.</returns>
        IEnumerable<object> GetServices(Type serviceType, string contract = null);
    }

    public interface IMutableDependencyResolver : IDependencyResolver
    {
        void Register(Func<object> factory, Type serviceType, string contract = null);
    }

    public static class DependencyResolverMixins
    {
        public static T GetService<T>(this IDependencyResolver This, string contract = null)
        {
            return (T)This.GetService(typeof(T), contract);
        }

        public static IEnumerable<T> GetServices<T>(this IDependencyResolver This, string contract = null)
        {
            return This.GetServices(typeof(T), contract).Cast<T>();
        }

        public static void InitializeResolver(this IMutableDependencyResolver resolver)
        {
            new Registrations().Register((f,t) => resolver.Register(f,t));

            var namespaces = new[] { 
                "ReactiveUI.Xaml", 
                "ReactiveUI.Mobile", 
                "ReactiveUI.NLog", 
                "ReactiveUI.Gtk", 
                "ReactiveUI.Cocoa", 
                "ReactiveUI.Android",
            };

            var assmName = new AssemblyName(typeof(FuncDependencyResolver).AssemblyQualifiedName);
            namespaces.ForEach(ns => {
                var targetType = ns + ".Registrations";
                string fullName = targetType + ", " + assmName.FullName.Replace(assmName.Name, ns);

                var registerTypeClass = Reflection.ReallyFindType(fullName, false);
                if (registerTypeClass == null) return;

                var registerer = (IWantsToRegisterStuff)Activator.CreateInstance(registerTypeClass);
                registerer.Register((f, t) => resolver.Register(f, t));
            });
        }
    }
}
