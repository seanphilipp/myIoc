using myIoc;
using System;
using Xunit;

namespace myIocTest
{
    public class MyIocTest
    {
        public interface INonExistent { }
        public interface IExistent { }
        public class ExistentImpl : IExistent
        {
            public ExistentImpl()
            {

            }
        }
        public interface IExistentWithDeps { }
        public class ExistentWithDepsImpl : IExistentWithDeps
        {
            public IExistent dependency { get; }
            public ExistentWithDepsImpl(IExistent dependency)
            {
                this.dependency = dependency;
            }
        }

        MyIoc myIoc;

        public MyIocTest()
        {
            myIoc = new MyIoc();
        }

        [Fact]
        public void RegisteredTypesShouldExist()
        {
            //Register a type and verity that it is available in the container
            myIoc.Register<IExistent, ExistentImpl>();
            //Verify that the container has the type we registered
            bool exists = myIoc.Exists<IExistent>();
            Assert.True(exists);
        }

        [Fact]
        public void ResolveRegisteredTypeNoDependencies()
        {
            //Register an IExistent type.
            myIoc.Register<IExistent, ExistentImpl>();
            ///Resolving the type should return an object of the same type.
            IExistent resolved = myIoc.Resolve<IExistent>();
            //Object should be nonnull and should be an instance of what was registered
            Assert.NotNull(resolved);
            Assert.Equal(resolved.GetType(), typeof(ExistentImpl));
            Assert.NotNull(resolved as IExistent);
        }

        [Fact]
        public void ResolveRegisteredTypeWithDependencies()
        {
            //Register an IExistent type.
            myIoc.Register<IExistent, ExistentImpl>();
            myIoc.Register<IExistentWithDeps, ExistentWithDepsImpl>();
            ///Resolving the type should return an object of the same type.
            IExistentWithDeps resolved = myIoc.Resolve<IExistentWithDeps>();
            //Object should be nonnull and should be an instance of what was registered
            Assert.NotNull(resolved);
            Assert.Equal(resolved.GetType(), typeof(ExistentWithDepsImpl));
            Assert.NotNull(resolved as IExistentWithDeps);
        }

        [Fact]
        public void UnregisteredTypesShouldThrowException()
        {
            Assert.Throws<UnregisteredTypeException>(() => myIoc.Resolve<IExistent>());
        }

        [Fact]
        public void RegisteredTypeLifecycleTransientDefault()
        {
            //Register an IExistent type.
            myIoc.Register<IExistent, ExistentImpl>();
            //Resolved should be a new instance.
            IExistent resolved = myIoc.Resolve<IExistent>();
            IExistent resolvedAgain = myIoc.Resolve<IExistent>();
            //The two objects should be different (transient) instances.
            Assert.False(resolved == resolvedAgain);
        }

        [Fact]
        public void RegisteredTypeLifecycleSingleton()
        {
            //Register an IExistent type.
            myIoc.Register<IExistent, ExistentImpl>(Lifecycle.Singleton);
            //Resolved should be a new instance.
            IExistent resolved = myIoc.Resolve<IExistent>();
            Assert.NotNull(resolved);
            IExistent resolvedAgain = myIoc.Resolve<IExistent>();
            Assert.NotNull(resolvedAgain);
            //The two objects should be different (transient) instances.
            Assert.True(resolved == resolvedAgain);
        }
        
    }

    
}
