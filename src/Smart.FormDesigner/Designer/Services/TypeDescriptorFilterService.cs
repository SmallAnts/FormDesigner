using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Smart.FormDesigner.Services
{
    public class TypeDescriptorFilterService : ITypeDescriptorFilterService
    {
        private IDesignerHost _host;

        internal event EventHandler<FilterEventArgs> FilterAttribute;
        internal event EventHandler<FilterEventArgs> FilterEvnt;
        internal event EventHandler<FilterEventArgs> FilterProperty;

        public TypeDescriptorFilterService(IDesignerHost host)
        {
            this._host = host;
        }

        public bool FilterAttributes(IComponent component, IDictionary attributes)
        {
            bool flag = false;
            var designer = this._host.GetDesigner(component);
            if (designer is IDesignerFilter designerFilter)
            {
                designerFilter.PreFilterAttributes(attributes);
                designerFilter.PostFilterAttributes(attributes);
                flag = true;
            }
            if (this.FilterAttribute != null && !(component is DesignSurface))
            {
                var filterEventArgs = new FilterEventArgs()
                {
                    Data = attributes,
                    Caching = true
                };
                this.FilterAttribute(component, filterEventArgs);
                return filterEventArgs.Caching;
            }
            return flag;
        }

        public bool FilterEvents(IComponent component, IDictionary events)
        {
            bool flag = false;
            var designer = this._host.GetDesigner(component);
            if (designer is IDesignerFilter designerFilter)
            {
                designerFilter.PreFilterEvents(events);
                designerFilter.PostFilterEvents(events);
                flag = true;
            }
            if (this.FilterEvnt != null && !(component is DesignSurface))
            {
                var filterEventArgs = new FilterEventArgs()
                {
                    Data = events,
                    Caching = true
                };
                this.FilterEvnt(component, filterEventArgs);
                return filterEventArgs.Caching;
            }
            return flag;
        }

        public bool FilterProperties(IComponent component, IDictionary properties)
        {
            bool flag = false;
            var designer = this._host.GetDesigner(component);
            if (designer is IDesignerFilter designerFilter)
            {
                designerFilter.PreFilterProperties(properties);
                designerFilter.PostFilterProperties(properties);
                flag = true;
            }
            if (this.FilterProperty != null && !(component is DesignSurface))
            {
                var filterEventArgs = new FilterEventArgs()
                {
                    Data = properties,
                    Caching = true
                };
                this.FilterProperty(component, filterEventArgs);
                return filterEventArgs.Caching;
            }
            return flag;
        }

    }
}
