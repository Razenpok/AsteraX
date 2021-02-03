using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsteraX.Mediator.Assets.Scripts;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace AsteraX.Infrastructure
{
    public class GameSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure([NotNull] IContainerBuilder builder)
        {
            builder.RegisterContainer();
            builder.Register<ISender, Mediator>(Lifetime.Singleton);

            RegisterRequestHandlers(builder);

            var requestHandlerType = typeof(IRequestHandler<>);
            foreach (var monoBehaviour in FindObjectsOfType<MonoBehaviour>())
            {
                var type = monoBehaviour.GetType();
                var registration = builder.RegisterComponent(monoBehaviour);
                if (IsClassImplementingOpenGenericInterface(type, requestHandlerType))
                {
                    registration.AsImplementedInterfaces();
                }
            }
        }

        private static void RegisterRequestHandlers([NotNull] IContainerBuilder builder)
        {
            var requestHandlerType = typeof(IRequestHandler<>);
            var assembly = requestHandlerType.Assembly;
            var requestHandlerTypes = GetAllClassesImplementingOpenGenericInterface(requestHandlerType, assembly);
            foreach (var useCaseType in requestHandlerTypes)
            {
                builder.Register(useCaseType, Lifetime.Transient);
            }
        }

        [ItemNotNull]
        private static IEnumerable<Type> GetAllClassesImplementingOpenGenericInterface(Type openGenericType, Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => IsClassImplementingOpenGenericInterface(t, openGenericType));
        }

        private static bool IsClassImplementingOpenGenericInterface(Type type, Type openGenericType)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                return false;
            }
            
            var baseType = type.BaseType;
            
            foreach (var @interface in type.GetInterfaces())
            {
                if (baseType != null && baseType.IsGenericType &&
                    openGenericType.IsAssignableFrom(baseType.GetGenericTypeDefinition()))
                {
                    return true;
                }

                if (@interface.IsGenericType &&
                    openGenericType.IsAssignableFrom(@interface.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}