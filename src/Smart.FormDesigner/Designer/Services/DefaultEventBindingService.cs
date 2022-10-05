using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Forms;

namespace Smart.FormDesigner.Services
{
    public class DefaultEventBindingService : BaseEventBindingService
    {
        private bool _loading = false;
        private Dictionary<object, List<EventData>> _events = new Dictionary<object, List<EventData>>();

        public object Source { get; set; }

        public List<EventData> GetEventDatas(object compoent)
        {
            if (this._events.ContainsKey(compoent))
            {
                return this._events[compoent];
            }
            return null;
        }

        internal void AddEvent(object component, string eventName, string methodName)
        {
            var eventDatas = this.GetEventDatas(component);
            if (eventDatas == null)
            {
                eventDatas = new List<EventData>();
                this._events.Add(component, eventDatas);
            }
            eventDatas.Add(new EventData(eventName, methodName));
        }

        private void LoadEventData(object s, EventArgs e)
        {
            this._loading = true;
            var arrayList = new ArrayList();
            foreach (var dictionaryEntry in this._events)
            {
                var events = TypeDescriptor.GetEvents(dictionaryEntry.Key);
                var key = (IComponent)dictionaryEntry.Key;
                if (key == null || key.Site == null)
                {
                    arrayList.Add(dictionaryEntry.Key);
                }
                else
                {
                    foreach (var eventData in dictionaryEntry.Value)
                    {
                        var eventDescriptor = events[eventData.EventName];
                        var eventProperty = ((IEventBindingService)this).GetEventProperty(eventDescriptor);
                        eventProperty.SetValue(dictionaryEntry.Key, eventData.MethodName);
                    }
                }
            }
            foreach (object key in arrayList)
            {
                this._events.Remove(key);
            }
            this._loading = false;
        }


        public void RefreshEventData()
        {
            this.ClearData();
            this.LoadEventData(this, new EventArgs());
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            this._events.Remove(e.Component);
        }

        public override IServiceProvider ServiceProvider
        {
            set
            {
                var serviceProvider = this.ServiceProvider;
                if (serviceProvider != null)
                {
                    if (serviceProvider.GetService(typeof(IComponentChangeService)) is IComponentChangeService service1)
                    {
                        service1.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    }
                }

                base.ServiceProvider = value;
                if (value == null)
                {
                    return;
                }
                if (value.GetService(typeof(IComponentChangeService)) is IComponentChangeService service2)
                {
                    service2.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                }
            }
        }

        private ICollection GetCompatibleMethods(object obj, EventDescriptor ed)
        {
            var arrayList = new ArrayList();
            var type = obj.GetType();
            var method1 = ed.EventType.GetMethod("Invoke");
            var parameters1 = method1.GetParameters();
            foreach (var method2 in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var parameters2 = method2.GetParameters();
                if (method1.ReturnType == method2.ReturnType && parameters1.Length == parameters2.Length)
                {
                    int index = 0;
                    bool flag = true;
                    foreach (var parameterInfo in parameters1)
                    {
                        if (parameterInfo.ParameterType != parameters2[index].ParameterType)
                        {
                            flag = false;
                            break;
                        }
                        ++index;
                    }
                    if (flag)
                    {
                        arrayList.Add(method2.Name);
                    }
                }
            }
            return (ICollection)arrayList;
        }

        protected override ICollection GetCompatibleMethods(EventDescriptor e)
        {
            if (this.Source == null)
            {
                for (var control = this.ServiceProvider.GetService<IDesignerHost>().RootComponent as Control;
                    control != null;
                    control = control.Parent)
                {
                    if (control is Form)
                    {
                        this.Source = control;
                        break;
                    }
                }
            }
            if (this.Source == null)
            {
                return new ArrayList();
            }
            return this.GetCompatibleMethods(this.Source, e);
        }

        public override void FreeMethod(object component, EventDescriptor e, string methodName)
        {
            if (this._loading)
            {
                return;
            }
            var eventDatas = this.GetEventDatas(component);
            if (eventDatas == null)
            {
                return;
            }
            foreach (var eventData in eventDatas)
            {
                if (eventData.EventName == e.Name && eventData.MethodName == methodName)
                {
                    eventDatas.Remove(eventData);
                    break;
                }
            }
        }

        public override void UseMethod(object component, EventDescriptor e, string methodName)
        {
            if (this._loading)
            {
                return;
            }

            this.FreeMethod(component, e, methodName);
            this.AddEvent(component, e.Name, methodName);
        }

        public void Clear()
        {
            this._events.Clear();
            this.ClearData();
        }

        private void UpdateEvents(object eventSource, bool adding)
        {
            if (eventSource == null)
            {
                return;
            }
            eventSource.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var dictionaryEntry in this._events)
            {
                if (dictionaryEntry.Value != null)
                {
                    var type = dictionaryEntry.Key.GetType();
                    foreach (var eventData in dictionaryEntry.Value)
                    {
                        var eventInfo = type.GetEvent(eventData.EventName);
                        if (eventInfo != null)
                        {
                            var methodDelegate = eventData.MethodDelegate;
                            if (methodDelegate == null)
                            {
                                methodDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, eventSource, eventData.MethodName, false, false);
                                eventData.MethodDelegate = methodDelegate;
                            }
                            if (methodDelegate != null)
                            {
                                eventInfo.RemoveEventHandler(dictionaryEntry.Key, methodDelegate);
                                if (adding)
                                {
                                    eventInfo.AddEventHandler(dictionaryEntry.Key, methodDelegate);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BindEvents(object eventSource)
        {
            this.UpdateEvents(eventSource, true);
        }

        public void UnbindEvents(object eventSource)
        {
            this.UpdateEvents(eventSource, false);
        }

        public class EventData
        {
            public string EventName { get; set; }
            public string MethodName { get; set; }
            public Delegate MethodDelegate { get; set; }

            public EventData(string eventName, string methodName)
            {
                this.EventName = eventName;
                this.MethodName = methodName;
            }

        }
    }

}
