using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text;

namespace Smart.FormDesigner.Services
{
    public class BaseEventBindingService : IEventBindingService
    {
        private Hashtable _eventProperties;
        public virtual IServiceProvider ServiceProvider { get; set; }

        protected void ClearData()
        {
            this._eventProperties?.Clear();
        }

        protected string CreateUniqueMethodName(IComponent component, EventDescriptor e)
        {
            return $"{component.ToString().Split(' ')[0]}_{e.Name}";
        }
        protected virtual ICollection GetCompatibleMethods(EventDescriptor e)
        {
            return new string[0];
        }

        protected string GetEventDescriptorHashCode(EventDescriptor eventDesc)
        {
            var stringBuilder = new StringBuilder(eventDesc.Name);
            stringBuilder.Append(eventDesc.EventType.GetHashCode().ToString());
            foreach (Attribute attribute in eventDesc.Attributes)
            {
                stringBuilder.Append(attribute.GetHashCode().ToString());
            }
            return stringBuilder.ToString();
        }

        protected object GetService(Type serviceType) => this.ServiceProvider?.GetService(serviceType);

        protected bool ShowCode() => false;

        protected bool ShowCode(int lineNumber) => false;

        protected bool ShowCode(object component, EventDescriptor e, string methodName) => false;

        #region 虚方法

        public virtual void FreeMethod(object component, EventDescriptor e, string methodName)
        {
        }
        public virtual void UseMethod(object component, EventDescriptor e, string methodName)
        {
        }
        protected virtual void ValidateMethodName(string methodName)
        {
        }

        #endregion

        #region IEventBindingService 接口成员

        string IEventBindingService.CreateUniqueMethodName(IComponent component, EventDescriptor e)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            return this.CreateUniqueMethodName(component, e);
        }

        ICollection IEventBindingService.GetCompatibleMethods(EventDescriptor e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            return this.GetCompatibleMethods(e);
        }

        EventDescriptor IEventBindingService.GetEvent(PropertyDescriptor property)
        {
            if (property is EventPropertyDescriptor epd)
            {
                return epd.Event;
            }
            return null;
        }

        PropertyDescriptorCollection IEventBindingService.GetEventProperties(EventDescriptorCollection events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }
            var properties = new PropertyDescriptor[events.Count];
            if (this._eventProperties == null)
            {
                this._eventProperties = new Hashtable();
            }
            for (int index = 0; index < events.Count; ++index)
            {
                object descriptorHashCode = this.GetEventDescriptorHashCode(events[index]);
                properties[index] = (PropertyDescriptor)this._eventProperties[descriptorHashCode];
                if (properties[index] == null)
                {
                    properties[index] = new EventPropertyDescriptor(events[index], this);
                    this._eventProperties[descriptorHashCode] = properties[index];
                }
            }
            return new PropertyDescriptorCollection(properties);
        }

        PropertyDescriptor IEventBindingService.GetEventProperty(EventDescriptor e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            if (this._eventProperties == null)
            {
                this._eventProperties = new Hashtable();
            }
            object descriptorHashCode = this.GetEventDescriptorHashCode(e);
            var propertyDescriptor = (PropertyDescriptor)this._eventProperties[descriptorHashCode];
            if (propertyDescriptor == null)
            {
                propertyDescriptor = new EventPropertyDescriptor(e, this);
                this._eventProperties[descriptorHashCode] = propertyDescriptor;
            }
            return propertyDescriptor;
        }

        bool IEventBindingService.ShowCode() => this.ShowCode();

        bool IEventBindingService.ShowCode(int lineNumber) => this.ShowCode(lineNumber);

        bool IEventBindingService.ShowCode(IComponent component, EventDescriptor e)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            string methodName = (string)((IEventBindingService)this).GetEventProperty(e).GetValue(component);
            if (methodName == null)
            {
                return false;
            }
            return this.ShowCode(component, e, methodName);
        }

        #endregion

        private class EventPropertyDescriptor : PropertyDescriptor
        {
            private BaseEventBindingService _eventSvc;
            private EventDescriptor _eventDesc;
            private TypeConverter _converter;

            internal EventDescriptor Event => this._eventDesc;

            internal EventPropertyDescriptor(EventDescriptor eventDesc, BaseEventBindingService eventSvc)
              : base(eventDesc, null)
            {
                this._eventDesc = eventDesc;
                this._eventSvc = eventSvc;
            }

            public override bool CanResetValue(object component) => this.GetValue(component) != null;
            public override Type ComponentType => this._eventDesc.ComponentType;
            public override TypeConverter Converter => this._converter ?? (this._converter = new EventConverter(this._eventDesc));
            public override bool IsReadOnly => this.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
            public override Type PropertyType => this._eventDesc.EventType;

            public override object GetValue(object component)
            {
                if (component == null)
                {
                    throw new ArgumentNullException(nameof(component));
                }

                var site = this.GetCompoentSite(component);
                var dicService = this.GetDictionaryService(site);
                return dicService.GetValue(new ReferenceEventClosure(component, this))?.ToString();
            }
            public override void SetValue(object component, object value)
            {
                if (component == null)
                {
                    throw new ArgumentNullException(nameof(component));
                }
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException("试图设置只读事件。");
                }
                if (value != null && !(value is string))
                {
                    throw new ArgumentException($"不能设置值{value}。");
                }

                var site = this.GetCompoentSite(component);
                var dicService = this.GetDictionaryService(site);

                string methodName1 = (string)value;
                if (methodName1 != null && methodName1.Length == 0)
                {
                    methodName1 = null;
                }
                var referenceEventClosure = new ReferenceEventClosure(component, this);
                string methodName2 = dicService.GetValue(referenceEventClosure)?.ToString();
                if (ReferenceEquals(methodName2, methodName1)
                    || methodName2 != null && methodName1 != null && methodName2.Equals(methodName1))
                {
                    return;
                }

                if (methodName1 != null)
                {
                    this._eventSvc.ValidateMethodName(methodName1);
                }
                var changeService = site.GetService<IComponentChangeService>();
                if (changeService != null)
                {
                    try
                    {
                        changeService.OnComponentChanging(component, this);
                    }
                    catch (CheckoutException ex)
                    {
                        if (ex == CheckoutException.Canceled)
                        {
                            return;
                        }
                        throw ex;
                    }
                }
                if (methodName1 != null)
                {
                    this._eventSvc.UseMethod(component, this._eventDesc, methodName1);
                }
                if (methodName2 != null)
                {
                    this._eventSvc.FreeMethod(component, this._eventDesc, methodName2);
                }
                dicService.SetValue(referenceEventClosure, methodName1);
                changeService?.OnComponentChanged(component, this, methodName2, methodName1);
                this.OnValueChanged(component, EventArgs.Empty);
            }
            public override void ResetValue(object component) => this.SetValue(component, null);
            public override bool ShouldSerializeValue(object component) => this.CanResetValue(component);

            private ISite GetCompoentSite(object component)
            {
                ISite site = null;
                if (component is IComponent icp)
                {
                    site = icp.Site;
                }
                if (site == null)
                {
                    if (this._eventSvc.ServiceProvider.GetService(typeof(IReferenceService)) is IReferenceService refService)
                    {
                        if (refService.GetComponent(component) is IComponent icp2)
                        {
                            site = icp2.Site;
                        }
                    }
                }
                if (site == null)
                {
                    throw new InvalidOperationException($"组件 {component} 没有设置 Site 属性");
                }
                return site;
            }

            private IDictionaryService GetDictionaryService(ISite site)
            {
                if (site.GetService(typeof(IDictionaryService)) is IDictionaryService dicService)
                {
                    return dicService;
                }
                throw new InvalidOperationException("没有找到 IDictionaryService。");
            }

            private class EventConverter : TypeConverter
            {
                private EventDescriptor _evt;

                internal EventConverter(EventDescriptor evt)
                {
                    this._evt = evt;
                }

                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    if (sourceType == typeof(string))
                        return true;
                    return base.CanConvertFrom(context, sourceType);
                }

                public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                {
                    if (destinationType == typeof(string))
                        return true;
                    return base.CanConvertTo(context, destinationType);
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    if (value == null)
                        return value;
                    if (!(value is string))
                        return base.ConvertFrom(context, culture, value);
                    if (((string)value).Length == 0)
                        return null;
                    return value;
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    if (destinationType == typeof(string))
                        return value == null ? string.Empty : value;
                    return base.ConvertTo(context, culture, value, destinationType);
                }

                public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    string[] strArray = null;
                    if (context != null)
                    {
                        if (context.GetService(typeof(IEventBindingService)) is IEventBindingService service)
                        {
                            var compatibleMethods = service.GetCompatibleMethods(this._evt);
                            strArray = new string[compatibleMethods.Count];
                            int num = 0;
                            foreach (string str in compatibleMethods)
                            {
                                strArray[num++] = str;
                            }
                        }
                    }
                    return new StandardValuesCollection(strArray);
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return false;
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    return true;
                }
            }

            private class ReferenceEventClosure
            {
                private object reference;
                private EventPropertyDescriptor propertyDescriptor;

                public ReferenceEventClosure(object reference, EventPropertyDescriptor prop)
                {
                    this.reference = reference;
                    this.propertyDescriptor = prop;
                }

                public override int GetHashCode()
                {
                    return this.reference.GetHashCode() * this.propertyDescriptor.GetHashCode();
                }

                public override bool Equals(object otherClosure)
                {
                    if (!(otherClosure is ReferenceEventClosure))
                    {
                        return false;
                    }

                    var referenceEventClosure = (ReferenceEventClosure)otherClosure;

                    return referenceEventClosure.reference == this.reference
                        && referenceEventClosure.propertyDescriptor == this.propertyDescriptor;
                }
            }
        }
    }

}
