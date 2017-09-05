using System;
using myIoc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace WebApplication.Misc
{
    public class ControllerResolveException : Exception
    {
        public ControllerResolveException(String message) : base(message) {}
    }

    public class MyIocControllerFactory : IControllerFactory
    {
        interface IResolvableController
        {
            Controller ResolveController();
        }
        internal class ResolvableController<C> : IResolvableController
        {
            private MyIoc container;
            public ResolvableController(MyIoc container)
            {
                this.container = container;
            }
            public Controller ResolveController()
            {
                return this.container.Resolve<C>() as Controller;
            }
        }

        private Dictionary<String, IResolvableController> controllers = 
            new Dictionary<String, IResolvableController>();
        private MyIoc container;

        public MyIocControllerFactory(MyIoc container)
        {
            if(container == null)
            {
                throw new ArgumentNullException("Container must be non null.");
            }
            this.container = container;
        }

        public void RegisterController<C>(String controllerName)
        {
            container.Register<C, C>();
            controllers.Add(controllerName, 
                new ResolvableController<C>(this.container));
        }

        object IControllerFactory.CreateController(ControllerContext context)
        {
            TypeInfo info = context.ActionDescriptor.ControllerTypeInfo;
            string controllerName = info.Name;
            object createdController = null;
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentException("Controller name cannot be null or empty.");
            }
            if (controllers.ContainsKey(controllerName))
            {
                IResolvableController ctrlrResolver = null;
                controllers.TryGetValue(controllerName, out ctrlrResolver);
                createdController = ctrlrResolver.ResolveController();
            }
            else
            {
                //TODO It would be better to have a DefaultControllerFactory to default to...
                throw new ControllerResolveException(
                    String.Format("Couldn't find controller by name: {0}" , controllerName));
            }
            return createdController;
        }

        void IControllerFactory.ReleaseController(ControllerContext context, object controller)
        { 
            //Should myIoc be able to dispose any of its disposable injections?
            IDisposable disposable = controller as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

    }
}
