using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Smart.FormDesigner.Internal
{
    internal class MegaAction
    {
        private DesignerHost _host;
        private List<DesignerAction> _actions;
        private ObjectHolder _objects;
        private Dictionary<string, string> _removedControls;

        public MegaAction(DesignerHost host)
        {
            this._host = host;
            this._actions = new List<DesignerAction>();
            this._objects = new ObjectHolder();
        }

        public void StartActions()
        {
            var componentChangeService = this._host.GetService<IComponentChangeService>();
            componentChangeService.ComponentAdded += new ComponentEventHandler(this.ComponentAdded);
            componentChangeService.ComponentChanged += new ComponentChangedEventHandler(this.ComponentChanged);
            componentChangeService.ComponentChanging += new ComponentChangingEventHandler(this.ComponentChanging);
            componentChangeService.ComponentRemoving += new ComponentEventHandler(this.ComponentRemoving);
        }
        public void StopActions()
        {
            var componentChangeService = this._host.GetService<IComponentChangeService>();
            componentChangeService.ComponentAdded -= new ComponentEventHandler(this.ComponentAdded);
            componentChangeService.ComponentChanging -= new ComponentChangingEventHandler(this.ComponentChanging);
            componentChangeService.ComponentChanged -= new ComponentChangedEventHandler(this.ComponentChanged);
            componentChangeService.ComponentRemoving -= new ComponentEventHandler(this.ComponentRemoving);
            this._objects.Clear();
        }

        public void Undo()
        {
            for (int i = this._actions.Count - 1; i >= 0; i--)
            {
                this._actions[i].Undo();
            }

            if (this._removedControls != null)
            {
                foreach (var item in this._removedControls)
                {
                    var container = this._host.GetService<IContainer>();
                    var control = container.Components[item.Key as string] as Control;
                    var parent = container.Components[item.Value as string] as Control;
                    if (control != null && parent != null && control.Parent == null)
                    {
                        control.Parent = parent;
                    }
                }
            }
        }
        public void Redo()
        {
            for (int i = 0; i < this._actions.Count; i++)
            {
                this._actions[i].Redo();
            }
        }

        public void LoadProperties(object component, Hashtable props, DesignerHost host)
        {
            var properties = TypeDescriptor.GetProperties(component);
            var enumerator = props.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string text = (string)enumerator.Key;
                if (text != "Image" || !props.ContainsKey("ImageList"))
                {
                    var propertyDescriptor = properties.Find(text, false);
                    if (propertyDescriptor != null)
                    {
                        if (propertyDescriptor.Name == "Parent" && host != null)
                        {
                            host.Parents[component] = enumerator.Value;
                        }
                        else if (propertyDescriptor.IsReadOnly && propertyDescriptor.SerializationVisibility == DesignerSerializationVisibility.Content)
                        {
                            object value = propertyDescriptor.GetValue(component);
                            var properties2 = TypeDescriptor.GetProperties(enumerator.Value);
                            foreach (PropertyDescriptor propertyDescriptor2 in properties2)
                            {
                                if (propertyDescriptor2.IsBrowsable && !propertyDescriptor2.IsReadOnly)
                                {
                                    object value2 = propertyDescriptor2.GetValue(enumerator.Value);
                                    propertyDescriptor2.SetValue(value, value2);
                                }
                            }
                        }
                        else
                        {
                            propertyDescriptor.SetValue(component, enumerator.Value);
                        }
                    }
                }
            }
        }

        public Hashtable StoreProperties(object control, DesignerHost host, PropertyDescriptor propDescriptor)
        {
            var hashtable = new Hashtable();
            var properties = TypeDescriptor.GetProperties(control);
            foreach (PropertyDescriptor propertyDescriptor in properties)
            {
                if (propertyDescriptor.IsBrowsable && (!propertyDescriptor.IsReadOnly || propertyDescriptor.SerializationVisibility == DesignerSerializationVisibility.Content))
                {
                    object obj = propertyDescriptor.GetValue(control);
                    if (obj != null)
                    {
                        if (!(control is DesignSurface) && propertyDescriptor.Name == "Parent" && host != null)
                        {
                            var parents = host.Parents;
                            if (obj != parents[control])
                            {
                                object value = obj;
                                if (parents[control] != null)
                                {
                                    obj = parents[control];
                                }
                                parents[control] = value;
                            }
                        }
                        if (!hashtable.Contains(propertyDescriptor.Name))
                        {
                            hashtable.Add(propertyDescriptor.Name, obj);
                        }
                    }
                }
            }
            if (propDescriptor != null && propDescriptor.Name != "Parent")
            {
                object value2 = propDescriptor.GetValue(control);
                if (value2 != null)
                {
                    hashtable[propDescriptor.Name] = value2;
                }
            }
            return hashtable;
        }

        private void ComponentAdded(object sender, ComponentEventArgs e)
        {
            var value = new AddAction(this._host, e.Component, this);
            this._actions.Add(value);
        }

        private void ComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            this._objects.Add(e.Component, this.StoreProperties(e.Component, this._host, null));
        }

        private void ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            var propData = this._objects.Get(e.Component);
            if (propData != null)
            {
                var value = new ChangeAction(this._host, e.Component, propData.Properties, this.StoreProperties(e.Component, this._host, e.Member as PropertyDescriptor), this);
                this._actions.Add(value);
            }
        }

        private void ComponentRemoving(object sender, ComponentEventArgs e)
        {
            var value = new RemoveAction(this._host, e.Component, this);
            this._actions.Add(value);
            var parent = e.Component as Control;
            if (parent != null && parent.Controls.Count != 0)
            {
                if (this._removedControls == null)
                {
                    this._removedControls = new Dictionary<string, string>();
                }
                foreach (Control control in parent.Controls)
                {
                    this._removedControls[control.Name] = parent.Name;
                }
            }
        }

        private class PropData
        {
            public object Component { get; set; }

            public Hashtable Properties { get; set; }

            public PropData(object component, Hashtable properties)
            {
                this.Component = component;
                this.Properties = properties;
            }
        }

        private class ObjectHolder : Collection<PropData>
        {
            public void Add(object component, Hashtable properties)
            {
                this.Add(new PropData(component, properties));
            }

            public PropData Get(object component)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    var propData = this[i];
                    if (propData.Component == component)
                    {
                        this.RemoveAt(i);
                        return propData;
                    }
                }
                return null;
            }

        }

    }

}
