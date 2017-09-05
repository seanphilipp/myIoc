using System;
using System.Collections.Generic;
using System.Reflection;
using static myIoc.MyIoc;

namespace myIoc
{
    /// <summary>
    /// The Lifecycle selects the available lifecycles which the container
    /// will apply to its managed objects.
    /// </summary>
    public enum Lifecycle
    {
        Transient, Singleton
    }

    /// <summary>
    /// This exception is thrown when the container attempts to resolve
    /// a type which has not been registered successfully.
    /// </summary>
    public class UnregisteredTypeException : Exception
    {
        public UnregisteredTypeException(String message) : base(message)
        { }
    }

    /// <summary>
    /// MyIoc implements an inversion of control container for providing transient
    /// and singleton object lifecycle management. It also provides the facility
    /// to dependency inject while resolving its registered types.
    /// </summary>
    public class MyIoc
    {
        private Dictionary<Type, IRegisteredType> registeredTypes = 
            new Dictionary<Type, IRegisteredType>();

        /// <summary>
        /// An IRegistered type must be able to get an instance of
        /// itself, and it should remit its Lifecycle information.
        /// </summary>
        private interface IRegisteredType
        {
            Lifecycle Lifecycle { get; }
            Object GetInstance();
        }

        /// <summary>
        /// A TransientRegisteredType implements IRegisteredType
        /// and is registered with Lifecycle Transient. It will
        /// provide a new instance any time GetInstance() is called.
        /// </summary>
        /// <typeparam name="I">An interface type.</typeparam>
        /// <typeparam name="C">A class type.</typeparam>
        private class TransientRegisteredType<I, C> : IRegisteredType
        {
            private MyIoc parent; 
            public Lifecycle Lifecycle
            {
                get => Lifecycle.Transient;
            }

            public TransientRegisteredType(MyIoc parent)
            {
                this.parent = parent;
            }

            public Object GetInstance()
            {
                return parent.Init<I,C>();
            }
        }

        /// <summary>
        /// A SingletonRegisteredType implements IRegisteredType
        /// and is registered with Lifecycle Singleton. It will
        /// provide one single instance when GetInstance() is called.
        /// </summary>
        /// <typeparam name="I">An interface type.</typeparam>
        /// <typeparam name="C">A class type.</typeparam>
        private class SingletonRegisteredType<I, C> : IRegisteredType
        {
            private static volatile Object instance;
            private static object syncRoot = new Object();
            private MyIoc parent;
            public Lifecycle Lifecycle
            {
                get => Lifecycle.Singleton;
            }

            public SingletonRegisteredType(MyIoc parent)
            {
                this.parent = parent;
            }

            public Object GetInstance()
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = parent.Init<I, C>();
                        }
                    }
                }

                return instance;
            }
        }
        
        /// <summary>
        /// Registers an interface type to a class implementation type. Allows specification
        /// of the lifecycle.
        /// </summary>
        /// <typeparam name="I">An interface type.</typeparam>
        /// <typeparam name="C">A class type.</typeparam>
        /// <param name="lifecycle">A Lifecycle value. Default will be Transient.</param>
        public void Register<I, C>(Lifecycle lifecycle = Lifecycle.Transient)
        {
            if(lifecycle == Lifecycle.Transient)
            {
                registeredTypes.Add(typeof(I), new TransientRegisteredType<I, C>(this));
            } else
            {
                registeredTypes.Add(typeof(I), new SingletonRegisteredType<I, C>(this));
            }
        }

        /// <summary>
        /// Resolves the type I from the container.
        /// </summary>
        /// <typeparam name="I">An interface type.</typeparam>
        /// <returns>The class type to implement the interface.</returns>
        public I Resolve<I>()
        { 
            Object resolved = ResolveByType(typeof(I));
            return (I)resolved;
        }

        /// <summary>
        /// Resolves the type from a Type object by referenging the registeredTypes.
        /// </summary>
        /// <param name="interfaceType">A Type from the interface to resolve.</param>
        /// <returns>The class type to implement the interface, as an object.</returns>
        private Object ResolveByType(Type interfaceType)
        {
            Object resolved = null;
            registeredTypes.TryGetValue(interfaceType, out IRegisteredType resolvedType);
            if (resolvedType != null)
            {
                resolved = resolvedType.GetInstance();
            }
            else
            {
                throw new UnregisteredTypeException(
                    String.Format("The Type {0} could not be resolved by the container.",
                    resolvedType));
            }
            return resolved;
        }

        /// <summary>
        /// Informs if a type is registered.
        /// </summary>
        /// <typeparam name="I">An interface type.</typeparam>
        /// <returns>True if the type was registered, otherwise false.</returns>
        public bool Exists<I>()
        {
            return registeredTypes.ContainsKey(typeof(I));
        }

        /// <summary>
        /// Initializes the types through reflective calls to fully paramterized constructors.
        /// When paramters exist they are also resolved.
        /// </summary>
        /// <typeparam name="I">An interface type.</typeparam>
        /// <typeparam name="C">A class type.</typeparam>
        /// <returns>Returns an object that implements the type I.</returns>
        private I Init<I, C>()
        {
            //Find the constructor.
            TypeInfo classTypeInfo = IntrospectionExtensions.GetTypeInfo(typeof(C));            
            IEnumerable<ConstructorInfo> ctorInfos = classTypeInfo.DeclaredConstructors;
            ConstructorInfo ctor = null;
            using (IEnumerator<ConstructorInfo> enumerator = ctorInfos.GetEnumerator())
            {
                enumerator.Reset();
                if(enumerator.MoveNext())
                {
                    ctor = enumerator.Current;
                }
            }
            //Stop here if there wasn't a constructor
            if(ctor == null)
            {
                throw new Exception("Registered type doesn't present a constuctor for use.");
            }
            //If there are parameters to the constructor, we have to invoke the container
            //API to resolve them.
            Object[] parameters = new Object[ctor.GetParameters().Length];
            int i = 0;
            foreach (var param in ctor.GetParameters())
            {
                //Resolve dependencies via ioc container
                parameters[i++] = ResolveByType(param.ParameterType);
            }

            //Construct the type.
            return (I) Activator.CreateInstance(typeof(C), parameters);
        }
    }
}