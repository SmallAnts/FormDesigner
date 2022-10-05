using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using Smart.FormDesigner.Services;

using static Smart.FormDesigner.Constants;

namespace Smart.FormDesigner.Serialization
{
    public class DefaultDesignerLoader : Component, IDesignerLoader, IDesignerSerializationService
    {
        #region 私有字段

        private ReferencedCollection referencedComponents = new ReferencedCollection();
        private Dictionary<string, IComponent> loadedComponents = new Dictionary<string, IComponent>();
        private Dictionary<Control, List<Control>> addedControls = new Dictionary<Control, List<Control>>();
        private Dictionary<string, List<Extender>> extenders = new Dictionary<string, List<Extender>>();
        private Hashtable lazyList = new Hashtable();
        private ArrayList initedObjects = new ArrayList();
        private DefaultEventBindingService eventBinding;
        private Control designedForm;
        private IDesignerHost designerHost;
        private bool versionWrited = false;
        private string currentVersion = VERSION_ID;

        #endregion

        #region 事件

        public event EventHandler<StoreEventArgs> ComponentStore;

        public event ComponentEventHandler ComponentLoaded;

        #endregion

        #region 属性

        [DefaultValue(LoadModes.Default), Description("获取或设置设计器的加载模式")]
        public LoadModes LoadMode { get; set; } = LoadModes.Default;

        [DefaultValue(false), Description("启用或禁用显示错误消息框")]
        public bool ShowErrorMessage { get; set; } = false;

        [DefaultValue(null)]
        public IDesignerHost DesignerHost
        {
            get
            {
                return this.designerHost;
            }
            set
            {
                if (this.designerHost != null)
                {
                    this.eventBinding.ServiceProvider = null;
                    this.designerHost.RemoveService(typeof(IDesignerSerializationService));
                    this.designerHost.RemoveService(typeof(IEventBindingService));
                    //this.designerHost.RemoveService(typeof(ComponentSerializationService));
                }
                this.designerHost = value;
                if (this.designerHost != null)
                {
                    this.eventBinding.ServiceProvider = this.designerHost;
                    this.designerHost.AddService(typeof(IEventBindingService), this.eventBinding);
                    this.designerHost.AddService(typeof(IDesignerSerializationService), this);
                    //this.designerHost.AddService(typeof(ComponentSerializationService));
                }
            }
        }

        [DefaultValue(null)]
        public IContainerService ContainerService { get; set; }

        #endregion

        public DefaultDesignerLoader()
        {
            this.ComponentStore += DesignerLoader_ControlStore;
            this.eventBinding = new DefaultEventBindingService();
        }

        #region IDesignerLoader 接口成员

        #region Load

        public void Load(Control parent, IReader reader)
        {
            this.Load(parent, reader, null, false);
        }

        public void Load(Control parent, string layout)
        {
            if (layout.Length == 0)
            {
                throw new ArgumentException();
            }
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(layout));
            using (var xmlFormReader = new XmlFormReader(stream))
            {
                this.Load(parent, xmlFormReader, null, false);
            }
        }

        public void Load(Control parent, IReader reader, Dictionary<string, IComponent> components, bool ignoreParent)
        {
            if (parent != null)
            {
                this.designedForm = parent;
                this.initedObjects.Clear();
                Control rootControl = null;
                if (this.designerHost != null)
                {
                    rootControl = (this.designerHost.RootComponent as Control);
                }
                reader.Read();
                reader.Attributes.TryGetValue(ATTR_VERSION, out this.currentVersion);
                if (reader.Name == NODE_OBJECT_COLLECTION)
                {
                    reader.Read();
                }
                parent = this.SubstRoot(rootControl, parent);
                if (!ignoreParent)
                {
                    this.LoadProperties(parent, reader);
                }

                this.loadedComponents.Clear();
                this.referencedComponents.Clear();
                this.lazyList.Clear();

                this.PrepareParent(reader, ignoreParent, parent);
                var stack = new Stack<Control>(); //后进先出列表
                stack.Push(parent);
                this.LoadControls(stack, reader);
                this.AddControls();
                this.LoadComponents(reader, components);
                this.SetReferences();
                this.SetExtendProviders();
                this.InvokeAddRange();
                foreach (ISupportInitialize supportInitialize in this.initedObjects)
                {
                    supportInitialize.EndInit();
                }
                this.AddBindings();
                if (this.designerHost == null)
                {
                    var form = parent.FindForm();
                    this.eventBinding.BindEvents(form ?? parent);
                }
                else
                {
                    this.eventBinding.RefreshEventData();
                }
                this.initedObjects.Clear();
            }
        }

        public void LoadRoot(Control parent, IReader reader)
        {
            this.Load(parent, reader, null, true);
        }

        public void LoadRoot(Control parent, string layout)
        {
            if (layout.Length == 0)
            {
                throw new ArgumentException();
            }
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(layout)))
            using (var xmlFormReader = new XmlFormReader(stream))
            {
                this.Load(parent, xmlFormReader, null, true);
            }
        }

        private void LoadControls(Stack<Control> owners, IReader reader)
        {
            while (true)
            {
                if (reader.State == ReaderState.StartElement)
                {
                    var ownerControl = owners.Peek();
                    if (!this.addedControls.ContainsKey(ownerControl))
                    {
                        this.addedControls[ownerControl] = new List<Control>();
                    }

                    bool isFinded = false;
                    if (this.GetOrCreateObject(ownerControl, reader, ref isFinded) is Control control)
                    {
                        this.LoadProperties(control, reader);
                        if (!isFinded) // 是新创建的组件
                        {
                            this.addedControls[ownerControl].Add(control);
                            // tableLayoutPanel 特殊处理
                            if (ownerControl is TableLayoutPanel tableLayoutPanel)
                            {
                                tableLayoutPanel.ColumnCount = tableLayoutPanel.ColumnStyles.Count;
                                tableLayoutPanel.RowCount = tableLayoutPanel.RowStyles.Count;
                            }
                        }
                        owners.Push(control);
                    }
                    else // 组件为空的情况处理
                    {
                        if (!this.SkipControl(reader, true))
                        {
                            break;
                        }
                    }
                }
                else if (reader.State == ReaderState.EndElement)
                {
                    owners.Pop();
                    if (!reader.Read())
                    {
                        break;
                    }
                    if (owners.Count == 0)
                    {
                        break;
                    }
                }
                else if (!reader.Read())
                {
                    break;
                }
            }
        }

        private void LoadComponents(IReader reader, Dictionary<string, IComponent> components)
        {
            while (reader.State != ReaderState.EOF)
            {
                while (reader.State != ReaderState.StartElement && reader.State != ReaderState.Value)
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }
                if (reader.State != ReaderState.StartElement && reader.State != ReaderState.Value)
                {
                    break;
                }
                reader.Attributes.TryGetValue(ATTR_NAME, out string name);
                if (name != null)
                {
                    IComponent component = null;
                    components?.TryGetValue(name, out component);
                    object obj = this.LoadObject(reader, component, true);
                    if (obj != null && obj is IComponent && this.ContainerService != null)
                    {
                        this.ContainerService.Add(obj as IComponent, name);
                    }
                }

            }
        }

        private void LoadProperties(object obj, IReader reader)
        {
            var properties = TypeDescriptor.GetProperties(obj);
            while (reader.Read())
            {
                if (reader.State == ReaderState.EndElement || reader.Attributes.ContainsKey(ATTR_NAME))
                {
                    break;
                }

                string name = reader.Name;
                if (name == PROP_NAME_CONTROLS)
                {
                    continue;
                }

                if (name != PROP_NAME_NAME || !(obj is Control control) || control.Name == "")
                {
                    if (reader.Name == NODE_EVENT)
                    {
                        reader.Attributes.TryGetValue(ATTR_EVENT_NAME, out string eventName);
                        this.eventBinding.AddEvent(obj, eventName, reader.Value);
                    }
                    else
                    {
                        var propertyDescriptor = properties.Find(name, false);
                        if (propertyDescriptor != null)
                        {
                            object value = this.LoadValue(propertyDescriptor.PropertyType, propertyDescriptor, obj, reader, false);
                            if (value != null)
                            {
                                propertyDescriptor.SetValue(obj, value);
                            }
                        }
                        else
                        {
                            bool flag = false;
                            reader.Attributes.TryGetValue(ATTR_PROVIDER, out string provider);
                            reader.Attributes.TryGetValue(ATTR_PROP_TYPE, out string propertyType);
                            string property = $"Set{reader.Name}";
                            if (provider != null && propertyType != null)
                            {
                                var type = this.CreateType(propertyType);
                                if (type != null)
                                {
                                    flag = true;
                                    object value = this.LoadValue(type, null, obj, reader, false);
                                    if (value != null)
                                    {
                                        this.extenders.TryGetValue(provider, out List<Extender> extenderList);
                                        if (extenderList == null)
                                        {
                                            extenderList = new List<Extender>();
                                        }
                                        extenderList.Add(new Extender
                                        {
                                            Control = obj,
                                            Value = value,
                                            Property = property
                                        });
                                        this.extenders[provider] = extenderList;
                                    }
                                }
                            }
                            if (!flag && reader.State == ReaderState.StartElement)
                            {
                                this.SkipControl(reader, false);
                            }
                        }
                    }
                }
            }

            if (this.ComponentLoaded != null && obj is IComponent component)
            {
                this.ComponentLoaded(this, new ComponentEventArgs(component));
            }
        }

        private object LoadObject(IReader reader, object obj, bool readName)
        {
            reader.Attributes.TryGetValue(ATTR_TYPE, out string objTypeName);
            string objName;
            if (readName)
            {
                reader.Attributes.TryGetValue(ATTR_NAME, out objName);
                if (objName == null)
                {
                    objName = reader.Name;
                }
            }
            else
            {
                obj = this.CreateObject(objTypeName, null, true);
                objName = (obj as IComponent).Site.Name;
            }

            object result;

            if (objTypeName?.IndexOf(TYPE_ARRAY_STRING) != -1)
            {
                reader.Attributes.TryGetValue(PROP_NAME_LENGTH, out string len);
                if (len != null)
                {
                    result = this.LoadStrings(reader, Convert.ToInt32(len));
                    return result;
                }
            }

            if (obj == null)
            {
                if (objName != string.Empty && !this.NeedCreateNew(objTypeName))
                {
                    obj = this.FindComponent(objName);
                }
                if (obj == null)
                {
                    obj = this.CreateObject(objTypeName, objName, true);
                }
            }

            if (!string.IsNullOrEmpty(objName) && obj is IComponent component)
            {
                this.loadedComponents[objName] = component;
            }
            if (obj is ISupportInitialize supportInitialize && !this.initedObjects.Contains(obj))
            {
                this.initedObjects.Add(obj);
                supportInitialize.BeginInit();
            }
            this.LoadProperties(obj, reader);
            result = obj;
            return result;
        }

        private object LoadStrings(IReader reader, int length)
        {
            string[] array = new string[length];
            for (int i = 0; i < length; i++)
            {
                reader.Read();
                array.SetValue(reader.Value, i);
            }
            while (reader.State != ReaderState.EndElement)
            {
                if (!reader.Read())
                {
                    break;
                }
            }
            return array;
        }

        private object LoadBinary(TypeConverter converter, IReader reader)
        {
            byte[] array = Convert.FromBase64String(reader.Value);
            object result;
            if (converter.CanConvert(typeof(byte[])))
            {
                result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, array);
            }
            else
            {
                var binaryFormatter = new BinaryFormatter();
                var serializationStream = new MemoryStream(array);
                result = binaryFormatter.Deserialize(serializationStream);
            }
            return result;
        }

        private object LoadConstructor(IReader reader)
        {
            while (reader.Read())
            {
                if (reader.State == ReaderState.Value && reader.Name == ATTR_TYPE)
                {
                    object obj = this.CreateObject(reader.Value, null, false);
                    this.LoadProperties(obj, reader);
                    return obj;
                }
            }
            return null;
        }

        private object LoadInstanceDescriptor(IReader reader, bool needLazyLoad)
        {
            while (reader.Read())
            {
                if (reader.State == ReaderState.Value && reader.Name == NODE_DATA)
                {
                    byte[] buffer = Convert.FromBase64String(reader.Value);
                    var binaryFormatter = new BinaryFormatter();
                    var serializationStream = new MemoryStream(buffer);
                    var memberInfo = (MemberInfo)binaryFormatter.Deserialize(serializationStream);
                    object[] array = null;
                    bool flag = false;
                    if (memberInfo is MethodBase @base)
                    {
                        var parameters = @base.GetParameters();
                        array = new object[parameters.Length];
                        int i = 0;
                        while (i < parameters.Length)
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }
                            if ((reader.State == ReaderState.Value || reader.State == ReaderState.StartElement) && reader.Name == NODE_PARAM)
                            {
                                if (reader.Attributes.Count != 0)
                                {
                                    reader.Attributes.TryGetValue(ATTR_MODE, out string mode);
                                    if (mode != null && mode == ATTR_MODE_REFERENCE)
                                    {
                                        flag = true;
                                        array[i++] = reader.Value;
                                        continue;
                                    }
                                }
                                object obj = (reader.Attributes.ContainsKey(ATTR_NULL)) ? null : this.LoadValue(parameters[i].ParameterType, null, null, reader, false);
                                array[i++] = obj;
                            }
                            if (i == parameters.Length)
                            {
                                break;
                            }
                        }
                    }
                    if (flag || needLazyLoad)
                    {
                        var instanceDescriptorLoader = new InstanceDescriptorLoader(memberInfo, array);
                        int num = 0;
                        while (reader.Read())
                        {
                            if (reader.State == ReaderState.StartElement)
                            {
                                num++;
                            }
                            else if (reader.State == ReaderState.EndElement)
                            {
                                if (num == 0)
                                {
                                    break;
                                }
                                num--;
                            }
                        }
                        return instanceDescriptorLoader;
                    }
                    var instanceDescriptor = new InstanceDescriptor(memberInfo, array);
                    var result = instanceDescriptor.Invoke();
                    this.LoadProperties(result, reader);
                    return result;
                }
            }
            return null;
        }

        private void LoadList(IList list, string typeName, object control, IReader reader, PropertyDescriptor pd)
        {
            bool flag = false;
            while (reader.Read())
            {
                if (reader.State == ReaderState.EndElement)
                {
                    break;
                }

                Type type;
                if (typeName != null)
                {
                    type = Type.GetType(typeName);
                }
                else
                {
                    if (reader.Attributes.TryGetValue(ATTR_TYPE, out typeName) && typeName == null)
                    {
                        continue;
                    }
                    type = Type.GetType(typeName);
                }
                object obj = null;

                if (typeof(IList).IsAssignableFrom(type))
                {
                    obj = Activator.CreateInstance(type);
                    reader.Attributes.TryGetValue(ATTR_TYPE, out typeName);
                    this.LoadList((IList)obj, typeName, null, reader, null);
                }
                else
                {
                    obj = this.LoadValue(type, pd, control, reader, true);
                }

                if (obj != null)
                {
                    if (pd != null && obj is InstanceDescriptorLoader instanceDescriptorLoader)
                    {
                        object value = pd.GetValue(control);
                        if (value.GetType().IsDataCollection())
                        {
                            if (!this.lazyList.ContainsKey(value))
                            {
                                this.lazyList.Add(value, new ArrayList());
                            }
                            ((ArrayList)this.lazyList[value]).Add(instanceDescriptorLoader);
                        }
                    }
                    else
                    {
                        list.Add(obj);
                        flag = true;
                    }
                }
            }
            if (flag && this.designerHost != null && pd != null)
            {
                var componentChangeService = this.designerHost.GetService<IComponentChangeService>();
                componentChangeService.OnComponentChanging(control, pd);
                componentChangeService.OnComponentChanged(control, pd, null, list);
            }
        }

        private object LoadCollectionItem(IReader reader)
        {
            bool isItem = reader.Name.StartsWith("Item");
            if (isItem)
            {
                reader.Read();
            }
            object result = this.LoadObject(reader, null, true);
            if (isItem)
            {
                reader.Read();
            }
            return result;
        }

        private object LoadArray(Type arrayType, PropertyDescriptor p, object control, IReader reader)
        {
            var arrayList = new ArrayList();
            reader.Attributes.TryGetValue(ATTR_TYPE, out string typeName);
            this.LoadList(arrayList, typeName, control, reader, p);
            var array = (Array)Activator.CreateInstance(arrayType, new object[]
            {
                arrayList.Count
            });
            arrayList.CopyTo(array);
            return array;
        }

        private object LoadValue(Type propType, PropertyDescriptor p, object control, IReader reader, bool loadListInvoke)
        {
            string name = reader.Name;
            object obj = null;
            int num = reader.Attributes.Count;
            if (num != 0 && reader.Attributes.ContainsKey(ATTR_PROVIDER))
            {
                num--;
            }
            if (num != 0 && reader.Attributes.ContainsKey(ATTR_PROP_TYPE))
            {
                num--;
            }
            object result;
            if (num != 0)
            {
                reader.Attributes.TryGetValue(ATTR_MODE, out string mode);
                if (mode != null)
                {
                    if (mode == ATTR_MODE_BINARY)
                    {
                        obj = this.LoadBinary(TypeDescriptor.GetConverter(propType), reader);
                    }
                    else if (mode == ATTR_MODE_INSTANCE_DESCRIPTOR)
                    {
                        obj = this.LoadInstanceDescriptor(reader, this.LazyLoadInstance(propType));
                    }
                    else if (mode == ATTR_MODE_CONSTRUCTOR)
                    {
                        obj = this.LoadConstructor(reader);
                    }
                    else if (mode == ATTR_MODE_REFERENCE)
                    {
                        if (control != null)
                        {
                            if (p.PropertyType.IsDataCollection())
                            {
                                object value = p.GetValue(control);
                                if (!this.lazyList.ContainsKey(value))
                                {
                                    this.lazyList.Add(value, new ArrayList());
                                }
                                ((ArrayList)this.lazyList[value]).Add(reader.Value);
                            }
                            else
                            {
                                var property = new ComponentProperty(control, p);
                                this.referencedComponents.Add(reader.Value, property);
                            }
                        }
                    }
                }
                else if (reader.Attributes.ContainsKey(ATTR_CONTENT))
                {
                    var properties = TypeDescriptor.GetProperties(control);
                    var propertyDescriptor = properties.Find(name, false);
                    obj = propertyDescriptor.GetValue(control);
                    this.LoadProperties(obj, reader);
                }
                else if (reader.Attributes.ContainsKey(ATTR_COLLECTION))
                {
                    if (control == null || p == null || reader.State != ReaderState.StartElement || propType.IsArray)
                    {
                        if (propType.IsArray)
                        {
                            result = this.LoadArray(propType, p, control, reader);
                            return result;
                        }
                        return null;
                    }
                    else
                    {
                        reader.Attributes.TryGetValue(ATTR_TYPE, out string typeName);
                        object value2 = p.GetValue(control);
                        var list = value2 as IList;
                        if (list == null)
                        {
                            var property2 = value2.GetType().GetProperty("List", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
                            if (property2 != null)
                            {
                                list = (IList)property2.GetValue(value2, new object[0]);
                            }
                        }
                        if (list != null)
                        {
                            this.ClearList(list);
                            this.LoadList(list, typeName, control, reader, p);
                        }
                    }
                }
                else if (reader.Attributes.ContainsKey(ATTR_CONTROL))
                {
                    var stack = new Stack<Control>();
                    bool flag = false;
                    if (reader.State == ReaderState.Value)
                    {
                        result = null;
                        return result;
                    }
                    if (loadListInvoke)
                    {
                        result = this.LoadCollectionItem(reader);
                        return result;
                    }
                    if (p != null)
                    {
                        obj = p.GetValue(control);
                    }
                    if (obj != null)
                    {
                        reader.Read();
                        if (reader.State != ReaderState.Value)
                        {
                            this.LoadProperties(obj, reader);
                        }
                    }
                    else
                    {
                        if (!reader.Attributes.ContainsKey(ATTR_TYPE))
                        {
                            reader.Read();
                        }
                        bool flag2 = false;
                        obj = this.GetOrCreateObject(null, reader, ref flag2);
                        if (obj != null)
                        {
                            this.LoadProperties(obj, reader);
                        }
                        else
                        {
                            int num2 = 0;
                            while (reader.Read())
                            {
                                if (reader.State == ReaderState.EndElement)
                                {
                                    if (num2 <= 0)
                                    {
                                        break;
                                    }
                                    num2--;
                                }
                                else if (reader.State == ReaderState.StartElement)
                                {
                                    num2++;
                                }
                            }
                        }
                        flag = true;
                    }
                    if (obj is Control c)
                    {
                        stack.Push(c);
                        this.LoadControls(stack, reader);
                    }
                    else
                    {
                        reader.Read();
                    }
                    if (p == null)
                    {
                        result = obj;
                        return result;
                    }
                    if (flag)
                    {
                        p.SetValue(control, obj);
                    }
                    return null;
                }
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(propType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    if (this.currentVersion == null)
                    {
                        obj = converter.ConvertFrom(null, CultureInfo.CurrentCulture, reader.Value);
                    }
                    else
                    {
                        obj = converter.ConvertFrom(null, CultureInfo.InvariantCulture, reader.Value);
                    }
                }
                if (obj == null && propType == typeof(object))
                {
                    obj = reader.Value;
                }
            }
            result = obj;
            return result;
        }

        private void AddControls()
        {
            foreach (var dictionaryEntry in this.addedControls)
            {
                var control = dictionaryEntry.Key;
                foreach (var value in dictionaryEntry.Value)
                {
                    control.Controls.Add(value);
                }
            }
            this.addedControls.Clear();
        }

        private void AddBindings()
        {
            foreach (DictionaryEntry dictionaryEntry in this.lazyList)
            {
                if (dictionaryEntry.Key is ControlBindingsCollection controlBindingsCollection)
                {
                    foreach (InstanceDescriptorLoader idl in (dictionaryEntry.Value as ArrayList))
                    {
                        if (this.CreateInstance(idl) is Binding binding)
                        {
                            controlBindingsCollection.Add(binding);
                        }
                    }
                }
            }
        }

        private void ClearList(IList list)
        {
            if (this.designerHost == null)
            {
                list.Clear();
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var current = list[i];
                    if (!(current is IComponent component) || component.Site == null || component.Site.Container != this.designerHost)
                    {
                        list.Remove(current);
                        i--;
                    }
                }
            }
        }

        private bool SkipControl(IReader reader, bool readNext)
        {
            int num = 1;
            while (reader.Read())
            {
                if (reader.State == ReaderState.EndElement)
                {
                    if (num == 1)
                    {
                        return !readNext || reader.Read();
                    }
                    num--;
                }
                else if (reader.State == ReaderState.StartElement)
                {
                    num++;
                }
            }
            return false;
        }

        #endregion

        #region Store

        public void Store(IComponent[] _components, IWriter writer)
        {
            if (_components.Length != 0)
            {
                this.BeforeWriting();
                var componentList = new List<IComponent>(_components);

                if (this.ContainerService != null)
                {
                    foreach (var item in this.ContainerService.Components)
                    {
                        if (!componentList.Contains(item.Value))
                        {
                            componentList.Add(item.Value);
                        }
                    }
                }

                if (_components[0] is Control control)
                {
                    this.designedForm = control;
                }

                if (componentList.Count > 1)
                {
                    var hashtable = new Hashtable
                    {
                        [ATTR_VERSION] = this.currentVersion
                    };
                    writer.WriteStartElement(NODE_OBJECT_COLLECTION, hashtable);
                    this.versionWrited = true;
                }

                var rootControl = this.designerHost.RootComponent as Control;
                var controlList = componentList.Where(c=>c is Control).ToList();

                foreach (IComponent component in controlList)
                {
                    if (component is Control c && !c.HaveParentInList(controlList))
                    {
                        this.StoreControl(component, rootControl, writer);
                    }
                }

                if (componentList.Count > 1)
                {
                    writer.WriteEndElement(NODE_OBJECT_COLLECTION);
                }
                writer.Flush();
                this.AfterWriting();
            }
        }

        public string Store(Control[] parents)
        {
            var memoryStream = new MemoryStream();
            using (var xmlFormWriter = new XmlFormWriter(memoryStream))
            {
                //designerLoader  designerLoader  = new designerLoader ();
                this.Store(parents, xmlFormWriter);
                string str = Encoding.UTF8.GetString(memoryStream.ToArray());
                return str;
            }

        }

        private void StoreHead(object control, IWriter writer)
        {
            var hashtable = new Hashtable();
            string text = string.Empty;
            if (control is IComponent icp && icp.Site != null)
            {
                text = icp.Site.Name;
            }
            if (text == string.Empty)
            {
                text = this.GetObjectName(control);
            }
            hashtable[ATTR_NAME] = text;
            hashtable[ATTR_TYPE] = control.GetType().AssemblyQualifiedName;
            if (!this.versionWrited)
            {
                hashtable[ATTR_VERSION] = 1.ToString();
                this.versionWrited = true;
            }
            writer.WriteStartElement(NODE_OBJECT, hashtable);
        }

        private void StoreBinary(string name, byte[] value, IWriter writer, IComponent provider)
        {
            var hashtable = new Hashtable
            {
                [ATTR_MODE] = ATTR_MODE_BINARY
            };
            if (provider != null)
            {
                hashtable[ATTR_PROVIDER] = provider.Site.Name;
                hashtable[ATTR_PROP_TYPE] = value.GetType().AssemblyQualifiedName;
            }
            writer.WriteValue(name, Convert.ToBase64String(value), hashtable);
        }

        private void StoreInstanceDescriptor(string name, InstanceDescriptor id, object value, Control rootControl, IWriter writer, IComponent provider)
        {
            var hashtable = new Hashtable();
            if (provider != null)
            {
                hashtable[ATTR_PROVIDER] = provider.Site.Name;
                hashtable[ATTR_PROP_TYPE] = value.GetType().AssemblyQualifiedName;
            }
            if (id.Arguments.Count == 0 && id.MemberInfo.Name == ".ctor")
            {
                hashtable[ATTR_MODE] = ATTR_MODE_CONSTRUCTOR;
                writer.WriteStartElement(name, hashtable);
                writer.WriteValue(ATTR_TYPE, value.GetType().AssemblyQualifiedName, null);
            }
            else
            {
                var binaryFormatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, id.MemberInfo);
                string value2 = Convert.ToBase64String(memoryStream.ToArray());
                hashtable[ATTR_MODE] = ATTR_MODE_INSTANCE_DESCRIPTOR;
                writer.WriteStartElement(name, hashtable);
                writer.WriteValue(NODE_DATA, value2, null);
                foreach (object current in id.Arguments)
                {
                    if (current == null)
                    {
                        hashtable.Clear();
                        hashtable[ATTR_NULL] = "1";
                        writer.WriteValue(NODE_PARAM, string.Empty, hashtable);
                    }
                    else
                    {
                        this.StoreValue(NODE_PARAM, current, rootControl, writer);
                    }
                }
            }
            if (!id.IsComplete)
            {
                this.StoreProperties(value, rootControl, writer);
            }
            writer.WriteEndElement(name);
        }

        private int StoreValue(string name, object value, Control rootControl, IWriter writer)
        {
            return this.StoreValue(name, value, rootControl, writer, null);
        }

        private int StoreValue(string name, object value, Control rootControl, IWriter writer, IComponent provider)
        {
            int result;
            if (value == null)
            {
                result = 0;
            }
            else
            {
                object[] customAttributes = value.GetType().GetCustomAttributes(typeof(BinarySerializationAttribute), false);
                if (customAttributes.Length > 0)
                {
                    var binaryFormatter = new BinaryFormatter();
                    var memoryStream = new MemoryStream();
                    binaryFormatter.Serialize(memoryStream, value);
                    this.StoreBinary(name, memoryStream.ToArray(), writer, provider);
                    result = 1;
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(value);
                    if (value is IComponent)
                    {
                        string text = this.ComponentName(value as IComponent);
                        if (text != null && !(value is ToolStripItem))
                        {
                            var hashtable = new Hashtable
                            {
                                [ATTR_MODE] = ATTR_MODE_REFERENCE
                            };
                            writer.WriteValue(name, text, hashtable);
                        }
                        else
                        {
                            this.StoreObjectAsProperty(name, value, rootControl, writer, provider);
                        }
                    }
                    else if (converter.CanConvert(typeof(string)))
                    {
                        string value2 = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(string));
                        Hashtable hashtable = null;
                        if (provider != null)
                        {
                            hashtable = new Hashtable
                            {
                                [ATTR_PROVIDER] = provider.Site.Name,
                                [ATTR_PROP_TYPE] = value.GetType().AssemblyQualifiedName
                            };
                        }
                        writer.WriteValue(name, value2, hashtable);
                    }
                    else if (converter.CanConvert(typeof(InstanceDescriptor)))
                    {
                        var id = (InstanceDescriptor)converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(InstanceDescriptor));
                        this.StoreInstanceDescriptor(name, id, value, rootControl, writer, provider);
                    }
                    else if (converter.CanConvert(typeof(byte[])))
                    {
                        byte[] value3 = (byte[])converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(byte[]));
                        this.StoreBinary(name, value3, writer, provider);
                    }
                    else
                    {
                        if (value is IList list)
                        {
                            result = this.StoreList(name, list, rootControl, writer);
                            return result;
                        }
                        if (!value.GetType().IsSerializable)
                        {
                            result = this.StoreObjectAsProperty(name, value, rootControl, writer, provider);
                            return result;
                        }
                        var binaryFormatter = new BinaryFormatter();
                        var memoryStream = new MemoryStream();
                        binaryFormatter.Serialize(memoryStream, value);
                        this.StoreBinary(name, memoryStream.ToArray(), writer, provider);
                    }
                    result = 1;
                }
            }
            return result;
        }

        private int StoreList(string name, IList value, Control rootControl, IWriter writer)
        {
            if (value.Count == 0) return 0;

            int result = 0;
            var hashtable = new Hashtable
            {
                [ATTR_COLLECTION] = STR_TRUE,
                [ATTR_TYPE] = value[0].GetType().AssemblyQualifiedName
            };

            writer.WriteStartElement(name, hashtable);
            int num = 0;
            foreach (object current in value)
            {
                if (current.GetType().Name == "DesignerToolStripControlHost") continue;

                result += this.StoreValue($"Item{num}", current, rootControl, writer);
                num++;
            }
            writer.WriteEndElement(name);
            return result;
        }

        private int StoreObjectAsProperty(string propName, object value, Control rootControl, IWriter writer, IComponent provider)
        {
            int result;
            if (value is Control control)
            {
                if (this.designerHost != null)
                {
                    var rootCompoent = this.designerHost.RootComponent as Control;
                    while (control != null && control != rootCompoent)
                    {
                        control = control.Parent;
                    }
                    if (control == null)
                    {
                        return 0;
                    }
                }
            }

            var hashtable = new Hashtable
            {
                [ATTR_CONTROL] = STR_TRUE
            };
            if (provider != null)
            {
                hashtable[ATTR_PROVIDER] = provider.Site.Name;
                hashtable[ATTR_PROP_TYPE] = value.GetType().AssemblyQualifiedName;
            }

            writer.WriteStartElement(propName, hashtable);
            int num = this.StoreControl(value, rootControl, writer);
            writer.WriteEndElement(propName);

            result = num;
            return result;
        }

        internal int StoreMember(object control, PropertyDescriptor prop, Control rootControl, IWriter writer)
        {
            int num = 0;
            var attributes = prop.Attributes;
            var designerSerializationVisibilityAttribute = (DesignerSerializationVisibilityAttribute)attributes[typeof(DesignerSerializationVisibilityAttribute)];
            var designerSerializationVisibility = designerSerializationVisibilityAttribute.Visibility;
            if (designerSerializationVisibility == DesignerSerializationVisibility.Hidden && attributes[typeof(ExtenderProvidedPropertyAttribute)] != null && (prop.Name == "Row" || prop.Name == "Column"))
            {
                designerSerializationVisibility = DesignerSerializationVisibility.Visible;
            }
            switch (designerSerializationVisibility)
            {
                case DesignerSerializationVisibility.Visible:
                    {
                        IComponent provider = null;
                        object value;
                        if (prop.Name == PROP_NAME_VISIBLE && control is Control control2 && control2.Parent != null && !control2.Parent.Visible)
                        {
                            value = prop.GetValue(control);
                        }
                        else
                        {
                            value = prop.GetValue(control);
                            if (attributes[typeof(ExtenderProvidedPropertyAttribute)] is ExtenderProvidedPropertyAttribute appAttr &&
                                appAttr.Provider != null && value != null)
                            {
                                if (appAttr.Provider is IComponent component && component.Site != null)
                                {
                                    provider = component;
                                }
                            }
                        }
                        if (!prop.IsReadOnly)
                        {
                            num += this.StoreValue(prop.Name, value, rootControl, writer, provider);
                        }
                        break;
                    }
                case DesignerSerializationVisibility.Content:
                    {
                        object value = prop.GetValue(control);
                        if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                        {
                            num += this.StoreList(prop.Name, (IList)value, rootControl, writer);
                        }
                        else if (prop.PropertyType.IsDataCollection())
                        {
                            var property = value.GetType().GetProperty("List", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
                            var value2 = (IList)property.GetValue(value, new object[0]);
                            num += this.StoreList(prop.Name, value2, rootControl, writer);
                        }
                        else
                        {
                            num += this.StoreObjectAsProperty(prop.Name, value, rootControl, writer, null);
                        }
                        break;
                    }
            }
            return num;
        }

        private int StoreProperties(object control, Control rootControl, IWriter writer)
        {
            int num = 0;
            var properties = TypeDescriptor.GetProperties(control);
            foreach (PropertyDescriptor propertyDescriptor in properties)
            {
                try
                {
                    if (propertyDescriptor.ShouldSerializeValue(control) && propertyDescriptor.Name != PROP_NAME_CONTROLS)
                    {
                        num += this.StoreMember(control, propertyDescriptor, rootControl, writer);
                    }
                }
                catch (Exception ex)
                {
                    string text = $"序列化属性{propertyDescriptor.Name}发生异常，{ex.Message}";
                    if (this.ShowErrorMessage)
                    {
                        MessageBox.Show(text);
                    }
                }
            }

            var eventDatas = this.eventBinding.GetEventDatas(control);
            if (eventDatas != null)
            {
                var hashtable = new Hashtable();
                foreach (var eventData in eventDatas)
                {
                    hashtable[ATTR_EVENT_NAME] = eventData.EventName;
                    writer.WriteValue(NODE_EVENT, eventData.MethodName, hashtable);
                }
            }
            return num;
        }

        private void StoreTail(object control, IWriter writer)
        {
            writer.WriteEndElement(NODE_OBJECT);
        }

        internal int StoreControl(object obj, Control rootControl, IWriter writer)
        {
            if (obj.GetType().Name == "TransparentToolStrip") return 0;

            int num = 0;
            this.StoreHead(obj, writer);
            num += this.StoreProperties(obj, rootControl, writer);
            if (obj is Control control)
            {
                var rootParent = this.SubstRoot(rootControl, control);
                bool flag = false;

                foreach (var invocation in this.ComponentStore.GetInvocationList())
                {
                    var eventArgs=new StoreEventArgs(rootParent);
                    invocation.DynamicInvoke(new object[] { this, eventArgs });
                    if (!eventArgs.Cancel)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    var controlsCopy = rootParent.Controls.ToArray();
                    for (int i = 0; i < controlsCopy.Length; i++)
                    {
                        var controlTmp = controlsCopy[i];
                        if (controlTmp != null)
                        {
                            if (this.designerHost != null)
                            {
                                if (controlTmp.Site == null || controlTmp.Site.GetService(typeof(IDesignerHost)) != this.designerHost)
                                {
                                    continue;
                                }
                            }
                            num += this.StoreControl(controlTmp, rootControl, writer);
                        }
                    }
                }
            }
            this.StoreTail(obj, writer);
            return num;
        }

        #endregion

        #region Event

        public void SetEventSource(object eventSource)
        {
            this.eventBinding.Source = eventSource;
        }

        public void BindEvents(object eventSource)
        {
            this.eventBinding.BindEvents(eventSource);
        }

        public void UnbindEvents(object eventSource)
        {
            this.eventBinding.UnbindEvents(eventSource);
        }

        public void RefreshEventData()
        {
            this.eventBinding.RefreshEventData();
        }

        #endregion

        #endregion

        internal void BeforeWriting()
        {
            this.versionWrited = false;
        }

        internal void AfterWriting()
        {
            this.versionWrited = false;
        }

        private Type CreateType(string type)
        {
            int num = type.IndexOf(",");
            string typeName = type.Substring(0, num);
            string assemblyName = type.Substring(num + 1).Trim();

            Type result = null ;
            if (this.designerHost != null)
            {
                if (this.designerHost.GetService<ITypeResolutionService>() is ITypeResolutionService typeResolutionService)
                {
                    result = typeResolutionService.GetType(typeName, false, false);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            Assembly assembly = null ;
            try
            {
                assembly = Assembly.Load(assemblyName);
                result = assembly.GetType(typeName);
            }
            catch (Exception ex)
            {
                if (this.ShowErrorMessage)
                {
                    MessageBox.Show($"从程序集 {assemblyName} 创建类型 {type} 失败");
                }
            }

            return result;
        }

        private object CreateObject(string typeName, string objName, bool createComponent)
        {
            var type = this.CreateType(typeName);
            if (type == null)
            {
                return null;
            }

            object result;
            object obj = null;
            if (this.designerHost != null && createComponent && typeof(IComponent).IsAssignableFrom(type))
            {
                obj = this.designerHost.CreateComponent(type, objName);
            }
            if (obj == null)
            {
                try
                {
                    obj = Activator.CreateInstance(type);
                    if (obj is Control control)
                    {
                        control.Name = objName;
                    }
                }
                catch (Exception ex)
                {
                    string error = $"创建类型 {type} 失败，{ex.Message}";
                    if (this.ShowErrorMessage)
                    {
                        MessageBox.Show(error);
                    }
                    obj = null;
                }
            }
            if (obj != null && objName != null && obj is IComponent component)
            {
                if (!this.loadedComponents.ContainsKey(objName))
                {
                    this.loadedComponents.Add(objName, component);
                }
            }
            if (obj != null && obj is ISupportInitialize supportInitialize)
            {
                this.initedObjects.Add(obj);
                supportInitialize.BeginInit();
            }
            result = obj;
            return result;
        }

        private object CreateInstance(InstanceDescriptorLoader idl)
        {
            for (int i = 0; i < idl.Arguments.Length; i++)
            {
                if (idl.Arguments[i] is string name)
                {
                    if (name == DESIGNED_FORM)
                    {
                        idl.Arguments[i] = this.designedForm;
                    }
                    else
                    {
                        this.loadedComponents.TryGetValue(name, out IComponent component);
                        idl.Arguments[i] = component;
                    }
                }
            }
            var instanceDescriptor = new InstanceDescriptor(idl.MemberInfo, idl.Arguments);
            return instanceDescriptor.Invoke();
        }

        private string ComponentName(IComponent component)
        {
            if (component.Site != null && component.Site.Container == this.designerHost)
            {
                return component.Site.Name;
            }

            if (this.ContainerService != null)
            {
                foreach (var item in this.ContainerService.Components)
                {
                    if (item.Value == component)
                    {
                        return item.Key;
                    }
                }
            }

            return component == this.designedForm ? DESIGNED_FORM : null;
        }

        private void DesignerLoader_ControlStore(object sender, StoreEventArgs e)
        {
            if (e.Component is BindingNavigator)
            {
                e.Cancel = true;
            }
            else if (!(e.Component is ContainerControl) || e.Component is Form || this.designerHost != null && e.Component == this.designerHost.RootComponent)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = e.Component.GetType().ToString().IndexOf("DesignSurface") == -1;
            }
        }

        private IComponent FindComponent(string name)
        {
            if (this.designerHost == null || name == null)
            {
                return null;
            }

            var container = this.designerHost.GetService<IContainer>();
            foreach (IComponent component in container.Components)
            {
                if (component.Site.Name == name)
                {
                    return component;
                }
            }
            if (this.ContainerService != null)
            {
                foreach (var item in this.ContainerService.Components)
                {
                    if (item.Key == name)
                    {
                        return item.Value;
                    }
                }
            }
            return null;
        }

        private object GetOrCreateObject(Control parent, IReader reader, ref bool finded)
        {
            reader.Attributes.TryGetValue(ATTR_TYPE, out string typeName);
            reader.Attributes.TryGetValue(ATTR_NAME, out string name);
            if (name == null)
            {
                name = reader.Name;
            }

            finded = false;
            // 允许重复，直接创建新对象返回
            if (this.LoadMode == LoadModes.Duplicate)
            {
                if (this.designerHost != null && parent.FindFirst(name) != null)
                {
                    name = null;
                }
                return this.CreateObject(typeName, name, true);
            }

            object obj = null;
            if (parent != null && name != null && name != string.Empty)
            {
                obj = parent.FindFirst(name);
            }

            //  找到对象，直接返回。
            if (obj != null)
            {
                finded = true;
                return obj;
            }

            // 处理不存在的情况
            bool createFlag = this.LoadMode != LoadModes.ModifyExisting;
            // 如果是 ModifyExisting 并且 type 不是 IComponent 时创建对象
            if (this.LoadMode == LoadModes.ModifyExisting)
            {
                var type = this.CreateType(typeName);
                if (type != null)
                {
                    createFlag = !typeof(IComponent).IsAssignableFrom(type);
                }
            }
            if (createFlag) // 创建对象
            {
                obj = this.CreateObject(typeName, name, true);
            }

            return obj;
        }

        private string GetObjectName(object control)
        {
            string result;
            if (control is Control ctl)
            {
                result = ctl.Name;
            }
            else
            {
                if (this.ContainerService != null)
                {
                    foreach (var item in this.ContainerService.Components)
                    {
                        if (item.Value == control)
                        {
                            return item.Key;
                        }
                    }
                }
                if (this.designerHost != null)
                {
                    var nameCreationService = this.designerHost.GetService<INameCreationService>();
                    var container =this.designerHost.GetService<IContainer>();
                    result = nameCreationService.CreateName(container, control.GetType());
                }
                else if (control is DataTable table)
                {
                    result = table.TableName;
                }
                else if (control is DataColumn column)
                {
                    result = column.ColumnName;
                }
                else
                {
                    result = string.Empty;
                }
            }
            return result;
        }

        private void InvokeAddRange()
        {
            foreach (DictionaryEntry dictionaryEntry in this.lazyList)
            {
                object key = dictionaryEntry.Key;
                var method = key.GetType().GetMethod(ADD_RANGE_METHOD_NAME);
                if (method == null) continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType.IsArray)
                {
                    var arrayList = dictionaryEntry.Value as ArrayList;
                    var values = new ArrayList();
                    foreach (object current in arrayList)
                    {
                        var obj = current;
                        if (current is string str)
                        {
                            this.loadedComponents.TryGetValue(str, out IComponent component);
                            obj = component;
                        }
                        else if (current is InstanceDescriptorLoader loader)
                        {
                            obj = this.CreateInstance(loader);
                        }

                        values.Add(obj);
                    }
                    var array = (Array)Activator.CreateInstance(parameters[0].ParameterType, new object[]
                    {
                        values.Count
                    });
                    values.CopyTo(array);
                    method.Invoke(key, new object[] { array });
                }
            }
        }

        private bool LazyLoadInstance(Type type)
        {
            return typeof(DataRelation).IsAssignableFrom(type) || typeof(Constraint).IsAssignableFrom(type);
        }

        private bool NeedCreateNew(string typeName)
        {
            bool result = false;
            var type = this.CreateType(typeName);
            if (type != null)
            {
                result = (typeof(DataTable).IsAssignableFrom(type) || typeof(DataColumn).IsAssignableFrom(type));
            }
            return result;
        }

        /// <summary>
        /// 当 this.LoadMode == LoadModes.EraseForm 时清除表单组件
        /// </summary>
        private void PrepareParent(IReader reader, bool ignoreParent, Control parent)
        {
            if (this.LoadMode == LoadModes.EraseForm)
            {
                if (this.designerHost != null)
                {
                    var container = this.designerHost.GetService<IContainer>();
                    var selectionService = this.designerHost.GetService<ISelectionService>();
                    selectionService.SetSelectedComponents(null);
                    foreach (IComponent component in container.Components)
                    {
                        if (component != this.designerHost.RootComponent)
                        {
                            container.Remove(component);
                            if (component != null)
                            {
                                component.Dispose();
                            }
                        }
                    }
                    if (this.ContainerService != null)
                    {
                        this.ContainerService.Clear();
                    }
                }
                else
                {
                    foreach (Control control in parent.Controls)
                    {
                        control.Dispose();
                    }
                }
            }
        }

        private void SetExtendProviders()
        {
            foreach (var extenderList in this.extenders)
            {
                IComponent component;
                if (this.designerHost != null)
                {
                    var container = this.designerHost.GetService<IContainer>();
                    component = container.Components[extenderList.Key];
                }
                else
                {
                    this.loadedComponents.TryGetValue(extenderList.Key, out component);
                }

                if (component == null) continue;

                foreach (var extender in extenderList.Value)
                {
                    var method = component.GetType().GetMethod(extender.Property);
                    if (method != null)
                    {
                        method.Invoke(component, new object[]
                        {
                                extender.Control,
                                extender.Value
                        });
                    }
                }

            }
        }

        private void SetReferences()
        {
            foreach (var referencedItem in this.referencedComponents)
            {
                if (this.loadedComponents.TryGetValue(referencedItem.Key, out IComponent obj))
                {
                    if (referencedItem.Key == DESIGNED_FORM)
                    {
                        obj = this.designedForm;
                    }
                    else
                    {
                        if (this.designerHost != null)
                        {
                            var container = this.designerHost.GetService<IContainer>();
                            var component = container.Components[referencedItem.Key];
                            if (component != null)
                            {
                                obj = component;
                            }
                        }
                        if (obj == null)
                        {
                            continue;
                        }
                    }
                }
                foreach (var componentProperty in referencedItem.Properties)
                {
                    componentProperty.SetProperty(obj);
                }
            }
        }

        private Control SubstRoot(Control rootControl, Control control)
        {
            if (rootControl == null || rootControl == control)
            {
                return control;
            }

            for (var parent = control.Parent; parent != null; parent = parent.Parent)
            {
                if (parent == rootControl)
                {
                    return control;
                }
            }
            return rootControl;

        }

        public ICollection Deserialize(object serializationData)
        {
            return null;
        }

        public object Serialize(ICollection objects)
        {
            return null;
        }

    }

}
