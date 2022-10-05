using Smart.FormDesigner.Services;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Smart.FormDesigner.Internal
{
    internal class DesignerSite : ISite, IServiceContainer
    {
        private DesignerHost _designerHost;
        private NestedContainer _nestedContainer;
        private ServiceContainer _serviceContainer;
        private DictionaryService _dictionaryService;

        public bool DesignMode { get; set; }

        public string Name { get; set; }

        public IComponent Component { get; private set; }

        public IContainer Container
        {
            get
            {
                return this._designerHost.Container;
            }
        }

        public IDesigner Designer { get; set; }

        public DesignerSite(DesignerHost designer, IComponent component)
        {
            this._designerHost = designer;
            this._serviceContainer = new ServiceContainer(designer);
            this.Component = component;
            this.DesignMode = true;
        }

        #region IServiceProvider 接口成员

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDictionaryService))
            {
                if (this._dictionaryService == null)
                {
                    this._dictionaryService = new DictionaryService();
                }
                return this._dictionaryService;
            }
            else if (serviceType == typeof(INestedContainer))
            {
                if (this._nestedContainer == null)
                {
                    this._nestedContainer = new NestedContainer(this.Component, this._designerHost);
                }
                return this._nestedContainer;
            }
            else
            {
                object service = this._serviceContainer.GetService(serviceType);
                if (service != null)
                {
                    return service;
                }
                else
                {
                    return this._designerHost.GetService(serviceType);
                }
            }
        }

        #endregion

        #region IServiceContainer 接口成员

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            this.AddService(serviceType, callback, false);
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            this.AddService(serviceType, serviceInstance, false);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            this._serviceContainer.AddService(serviceType, callback, promote);
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            this._serviceContainer.AddService(serviceType, serviceInstance, promote);
        }

        public void RemoveService(Type serviceType)
        {
            this.RemoveService(serviceType, false);
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            this._serviceContainer.RemoveService(serviceType, promote);
        }

        #endregion

    }
}
