using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Smart.FormDesigner.Serialization
{
    internal class ComponentProperty
    {
        protected PropertyDescriptor property;
        protected object component;

        public ComponentProperty(object component, PropertyDescriptor property)
        {
            this.component = component;
            this.property = property;
        }

        public virtual void SetProperty(object value)
        {
            if (property.PropertyType.IsArray)
            {
                var arrayList = new ArrayList();
                if (this.property.GetValue(this.component) is Array array)
                {
                    arrayList.AddRange(array);
                }
                arrayList.Add(value);
                var instance = (Array)Activator.CreateInstance(this.property.PropertyType, new object[1] { arrayList.Count });
                arrayList.CopyTo(instance);
                try
                {
                    this.property.SetValue(this.component, instance);
                }
                catch
                {
                    // TODO: SetProperty 错误处理
                }
            }
            else if (typeof(IList).IsAssignableFrom(property.PropertyType))
            {
                object listValue = this.property.GetValue(this.component);
                if (listValue == null || this.TryAddRange(listValue, value))
                {
                    return;
                }
                if (listValue is IList list)
                {
                    if (list.IsFixedSize) return;

                    var property = listValue.GetType().GetProperty("List", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
                    if (property != null)
                    {
                        list = (IList)property.GetValue(listValue, new object[0]);
                    }
                    try
                    {
                        list.Add(value);
                    }
                    catch
                    {
                    }
                }
            }
            else if (this.property.GetValue(this.component) == value)
            {
                return;
            }
            else
            {
                this.property.SetValue(this.component, value);
            }
        }

        public virtual object GetProperty()
        {
            if (property.PropertyType.IsArray)
            {
                return null;
            }
            else if (typeof(IList).IsAssignableFrom(property.PropertyType))
            {
                return null;
            }
            else
            {
                return this.property.GetValue(this.component);
            }

        }

        private bool TryAddRange(object list, object item)
        {
            try
            {
                foreach (var method in list.GetType().GetMethods())
                {
                    if (method.Name == "AddRange")
                    {
                        var parameters1 = method.GetParameters();
                        if (parameters1.Length == 1 && parameters1[0].ParameterType.IsArray)
                        {
                            var instance = (Array)Activator.CreateInstance(parameters1[0].ParameterType, new object[1] { 1 });
                            instance.SetValue(item, 0);
                            object[] parameters2 = new object[1] { instance };
                            method.Invoke(list, parameters2);
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

    }
}
