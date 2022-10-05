using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
using Smart.FormDesigner.Serialization;
using Smart.FormDesigner.Services;
using static Smart.FormDesigner.Constants;

namespace Smart.FormDesigner
{

    [ToolboxBitmap(typeof(Designer))]
    public class Designer : Component
    {
        private bool _active = false;
        private Dictionary<object, Dictionary<string, object>> _changedDefaults;
        private Dictionary<string, IComponent> _designedComponents = null;
        private DesignerHost _designerHost = null;
        private IDesignerLoader _designerLoader = null;
        private License _license = null;
        private List<Control> _pastedControls = new List<Control>();
        private Size _pasteShift = new Size(0, 0);

        #region 事件

        public event EventHandler DirtyChanged;

        public event KeyEventHandler KeyUp
        {
            add
            {
                var eventFilter = GetEventFilterService();
                eventFilter.KeyUp += value;
            }
            remove
            {
                var eventFilter = GetEventFilterService();
                eventFilter.KeyUp -= value;
            }
        }
        public event KeyEventHandler KeyDown
        {
            add
            {
                var eventFilter = GetEventFilterService();
                eventFilter.KeyDown += value;
            }
            remove
            {
                var eventFilter = GetEventFilterService();
                eventFilter.KeyDown -= value;
            }
        }
        public event EventHandler DoubleClick
        {
            add
            {
                var eventFilter = GetEventFilterService();
                eventFilter.DoubleClick += value;
            }
            remove
            {
                var eventFilter = GetEventFilterService();
                eventFilter.DoubleClick -= value;
            }
        }
        public event MouseEventHandler MouseUp
        {
            add
            {
                var eventFilter = GetEventFilterService();
                eventFilter.MouseUp += value;
            }
            remove
            {
                var eventFilter = GetEventFilterService();
                eventFilter.MouseUp -= value;
            }
        }
        public event MouseEventHandler MouseDown
        {
            add
            {
                var eventFilter = GetEventFilterService();
                eventFilter.MouseDown += value;
            }
            remove
            {
                var eventFilter = GetEventFilterService();
                eventFilter.MouseDown -= value;
            }
        }

        private EventService GetEventFilterService()
        {
            return this._designerHost.GetService<EventService>();
        }

        #endregion

        #region 设计器选项

        [Category("设计器选项")]
        [DefaultValue(true), Description("是否启用智能标记")]
        public bool UseSmartTags
        {
            get { return this.GetDesignerOption<bool>(DESIGNER_OPTION_USESMARTTAGS); }
            set { this.SetDesignerOption(DESIGNER_OPTION_USESMARTTAGS, value); }
        }

        [Category("设计器选项")]
        [DefaultValue(true), Description("是否自动打开对象绑定智能标记")]
        public bool ObjectBoundSmartTagAutoShow
        {
            get { return this.GetDesignerOption<bool>(DESIGNER_OPTION_OBSTAS); }
            set { this.SetDesignerOption(DESIGNER_OPTION_OBSTAS, value); }
        }

        [Category("设计器选项")]
        [DefaultValue(true), Description("启用或禁用设计器中的对齐线")]
        public bool UseSnapLines
        {
            get { return this.GetDesignerOption<bool>(DESIGNER_OPTION_USESNAPLINES); }
            set { this.SetDesignerOption(DESIGNER_OPTION_USESNAPLINES, value); }
        }

        [Category("设计器选项")]
        [DefaultValue(true), Description("对齐到网格")]
        public bool SnapToGrid
        {
            get { return this.GetDesignerOption<bool>(DESIGNER_OPTION_SNAPTOGRID); }
            set { this.SetDesignerOption(DESIGNER_OPTION_SNAPTOGRID, value); }
        }

        [Category("设计器选项")]
        [DefaultValue(false), Description("是否显示风格")]
        public bool ShowGrid
        {
            get { return this.GetDesignerOption<bool>(DESIGNER_OPTION_SHOWGRID); }
            set { this.SetDesignerOption(DESIGNER_OPTION_SHOWGRID, value); }
        }

        [Category("设计器选项")]
        [DefaultValue(524296), Description("设计器网格大小")]
        public Size GridSize
        {
            get { return this.GetDesignerOption<Size>(DESIGNER_OPTION_GRIDSIZE); }
            set { this.SetDesignerOption(DESIGNER_OPTION_GRIDSIZE, value); }
        }

        private void SetDesignerOption<T>(string name, T value)
        {
            var designerOptionService = this._designerHost.GetService<DesignerOptionService>();
            var propertyDescriptor = designerOptionService.Options.Properties[name];
            propertyDescriptor.SetValue(null, value);
        }

        private T GetDesignerOption<T>(string name)
        {
            var designerOptionService = this._designerHost.GetService<DesignerOptionService>();
            var propertyDescriptor = designerOptionService.Options.Properties[name];
            return (T)propertyDescriptor.GetValue(null);
        }

        #endregion

        #region 属性

        [Browsable(false)]
        [DefaultValue(false)]
        public bool Active
        {
            get
            {
                return this._active;
            }
            set
            {
                if (this._active != value)
                {
                    this._active = value;
                    if (this._designerLoader != null)
                    {
                        this._designerLoader.DesignerHost = (value ? this._designerHost : null);
                    }

                    this.EnableEdit(value);
                }
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public Dictionary<string, IComponent> DesignedComponents
        {
            get
            {
                return this._designedComponents;
            }
            set
            {
                if (this._designedComponents != null)
                {
                    bool flag = false;
                    if (this._designerLoader != null && this._designerLoader.DesignerHost == null)
                    {
                        flag = true;
                        this._designerLoader.DesignerHost = this._designerHost;
                    }

                    var container = this._designerHost.GetService<IContainerService>();
                    foreach (var item in this._designedComponents)
                    {
                        var component = container.Components.FirstOrDefault(c => c.Value == item.Value);

                        if (component.Key != null)
                        {
                            container.Remove(component.Key);
                        }
                    }

                    if (flag)
                    {
                        this._designerLoader.DesignerHost = null;
                    }
                }

                this._designedComponents = value;
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public SortedList<string, string> DesignedContainers => this._designerHost?.DesignedContainers;

        [DefaultValue(null)]
        public Control DesignContainer
        {
            get { return this._designerHost.DesignContainer; }
            set { this._designerHost.DesignContainer = value; }
        }

        [DefaultValue(null)]
        public Control DesignedForm
        {
            get { return this._designerHost.DesignedForm; }
            set
            {
                this._designerHost.DesignedForm = value;
                if (this._designerLoader != null)
                {
                    this._designerLoader.SetEventSource(value);
                }
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public DesignerHost DesignerHost => this._designerHost;

        [Browsable(true), Description("获取或设置 DesignerLoader")]
        [DefaultValue(null)]
        public IDesignerLoader DesignerLoader
        {
            get { return this._designerLoader; }
            set
            {
                if (this._designerLoader != null)
                {
                    this._designerLoader.DesignerHost = null;
                }
                this._designerLoader = value;
            }
        }

        [Browsable(false)]
        [DefaultValue(false)]
        public bool IsDirty { get; private set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public string LayoutXML
        {
            get
            {
                var stringWriter = new StringWriter();
                var stream = new XmlTextWriter(stringWriter);
                using (IWriter writer = new XmlFormWriter(stream))
                {
                    this.StoreInternal(writer);
                }
                this.ClearDirty();
                return stringWriter.ToString();
            }
            set
            {
                var stream = new XmlTextReader(new StringReader(value));
                using (IReader reader = new XmlFormReader(stream))
                {
                    this.LoadInternal(reader);
                }
                this.ClearDirty();
            }
        }

        [Browsable(false)]
        [DefaultValue(0)]
        public int UndoCount => this._designerHost?.UndoCount ?? 0;

        [Browsable(false)]
        [DefaultValue(0)]
        public int RedoCount => this._designerHost?.RedoCount ?? 0;

        #endregion

        #region 服务属性

        [Browsable(false)]
        public ISelectionService SelectionService { get; }

        [Browsable(false)]
        public IComponentChangeService ComponentChangeService { get; }

        #endregion

        public Designer()
        {
            this._license = LicenseManager.Validate(typeof(Designer), this);
            this._designerHost = new DesignerHost();
            this._designerHost.Owner = this;
            this._designerHost.AddService(typeof(Designer), this);

            this.SelectionService = _designerHost.GetService<ISelectionService>();
            this.ComponentChangeService = _designerHost.GetService<IComponentChangeService>();
        }

        ~Designer()
        {
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._license != null)
                {
                    this._license.Dispose();
                    this._license = null;
                }
                this._designerHost.Dispose();
                this._designerHost = null;
            }
            base.Dispose(disposing);
        }

        #region Load

        private bool LoadInternal(IReader reader)
        {
            if (this._designerLoader == null)
            {
                return false;
            }

            this._designerLoader.Load(this._designerHost.DesignedForm, reader, this._designedComponents, false);
            this.ClearDirty();

            return true;

        }

        public void LoadFromFile(string fileName)
        {
            if (fileName.EndsWith(FILE_EXT_XML, true, null))
            {
                using (var xmlFormReader = new XmlFormReader(fileName))
                {
                    this.LoadInternal(xmlFormReader);
                }
            }
        }

        public void Load(ref XmlReader reader)
        {
            using (var xmlFormReader = new XmlFormReader(reader))
            {
                this.LoadInternal(xmlFormReader);
            }
        }

        #endregion

        #region Store

        public void Store(ref XmlWriter writer)
        {
            using (var xmlFormWriter = new XmlFormWriter(writer))
            {
                this.StoreInternal(xmlFormWriter);
            }
        }

        public void SaveToFile(string fileName)
        {
            if (fileName.EndsWith(FILE_EXT_XML, true, null))
            {
                using (var writer = new XmlFormWriter(fileName))
                {
                    this.StoreInternal(writer);
                }
            }
        }

        private bool StoreInternal(IWriter writer)
        {
            if (this._designerLoader == null)
            {
                return false;
            }

            //  非控件的组件集合
            var components = new List<IComponent>();
            // 添加 RootComponent
            components.Add((this.Active && this.DesignContainer != null
                ? this._designerHost.RootComponent
                : this._designerHost.DesignedForm)
            );
            // 添加加载时指定的组件
            if (this._designedComponents != null)
            {
                components.AddRange(this._designedComponents.Values);
            }
            // 添加设计时的其它组件
            foreach (IComponent component in this._designerHost.Components)
            {
                if (!(component is Control) && !(component is ToolStripItem) && (this._designedComponents?.Values.Contains(component) != true))
                {
                    components.Add(component);
                }
            }
            // Store
            this._designerLoader.Store(components.ToArray(), writer);
            this.ClearDirty();

            return true;
        }

        #endregion

        #region Commond

        /// <summary>
        /// 全选
        /// </summary>
        public void SelectAll()
        {
            var selectionService = this._designerHost.GetService<ISelectionService>();
            var control = this._designerHost.RootComponent as Control;
            selectionService.SetSelectedComponents(control.Controls, SelectionTypes.Replace);
        }

        /// <summary>
        /// 复制
        /// </summary>
        public void CopyControls()
        {
            if (this._designerLoader == null)
            {
                return;
            }

            var selectionService = this._designerHost.GetService<ISelectionService>();
            var selectedComponents = selectionService.GetSelectedComponents();
            if (selectedComponents.Count == 0)
            {
                return;
            }
            else if (selectedComponents.Count == 1)
            {
                foreach (IComponent item in selectedComponents)
                {
                    if (item == this._designerHost.RootComponent) return;
                }
            }

            var container = this._designerHost.GetService<IContainerService>();
            var components = container.Components;
            container.Clear();
            using (var stream = new MemoryStream())
            using (IWriter writer = new XmlFormWriter(stream))
            {
                var array = new Control[selectedComponents.Count];
                int num = 0;
                foreach (IComponent component in selectedComponents)
                {
                    array[num++] = (component as Control);
                }
                this._designerLoader.Store(array, writer);
                Clipboard.SetData(CLIPBOARD_FORMAT, Encoding.UTF8.GetString(stream.ToArray()));
            }
            foreach (var item in components)
            {
                container.Add(item.Value, item.Key);
            }
            this._pasteShift = this.GridSize;
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        public void PasteControls()
        {
            this._pastedControls.Clear();
            if (this._designerLoader != null)
            {
                object data = Clipboard.GetData(CLIPBOARD_FORMAT);
                if (data == null) return;

                var loadMode = this._designerLoader.LoadMode;
                this._designerLoader.LoadMode = LoadModes.Duplicate;
                var selectionService = this._designerHost.GetService<ISelectionService>();

                var rootControl = this._designerHost.RootComponent as Control;
                if (selectionService.SelectionCount == 1)
                {
                    var selectedComponents = selectionService.GetSelectedComponents();
                    var enumerator = selectedComponents.GetEnumerator();
                    enumerator.MoveNext();
                    if (enumerator.Current is Control control)
                    {
                        var designer = this._designerHost.GetDesigner(control);
                        if (typeof(ParentControlDesigner).IsAssignableFrom(designer.GetType()))
                        {
                            rootControl = control;
                        }
                    }
                }
                using (var ts = this._designerHost.CreateTransaction(TRANS_PRASE))
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data as string)))
                    using (var reader = new XmlFormReader(stream))
                    {
                        this._designerLoader.ComponentLoaded += this.PasteControlHandler;
                        this._designerLoader.Load((rootControl != null) ? rootControl : this._designerHost.DesignedForm, reader, null, true);
                        this._designerLoader.ComponentLoaded -= this.PasteControlHandler;
                    }
                    var root = this._designerHost.RootComponent as Control;
                    var componentChangeService = this._designerHost.GetService<IComponentChangeService>();
                    foreach (var control in this._pastedControls)
                    {
                        componentChangeService.OnComponentChanging(control, null);
                        if (control.Parent == root)
                        {
                            control.Location += this._pasteShift;
                        }
                        if (this._designerHost.Components[control.Text] != null)
                        {
                            control.Text = control.Name;
                        }
                        control.BringToFront();
                        componentChangeService.OnComponentChanged(control, null, null, null);
                    }
                    ts.Commit();
                    this._pasteShift += this.GridSize;
                }
                selectionService.SetSelectedComponents(this._pastedControls.ToArray(), SelectionTypes.Replace);
                this.SetDirty();
                this._designerLoader.LoadMode = loadMode;
            }
        }

        private void PasteControlHandler(object sender, ComponentEventArgs e)
        {
            if (this._designerLoader != null && e.Component is Control control)
            {
                this._pastedControls.Add(control);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        public void DeleteSelected()
        {
            ExecuteTransaction(component =>
            {
                this._designerHost.Remove(component);
                component.Dispose();
            });
        }

        /// <summary>
        /// 置于顶层
        /// </summary>
        public void BringToFront()
        {
            ExecuteTransaction(component =>
            {
                if (component is Control control) control.BringToFront();
            });
        }

        /// <summary>
        /// 置于底层
        /// </summary>
        public void SendToBack()
        {
            ExecuteTransaction(component =>
            {
                if (component is Control control) control.SendToBack();
            });
        }

        protected void ExecuteTransaction(Action<IComponent> action)
        {
            if (!this._active) return;

            var selectionService = this._designerHost.GetService<ISelectionService>();
            if (selectionService.SelectionCount == 0) return;

            var selectedComponents = selectionService.GetSelectedComponents();
            using (var ts = this._designerHost.CreateTransaction(TRANS_SENDTOBACK))
            {
                foreach (IComponent component in selectedComponents)
                {
                    if (component == this._designerHost.RootComponent) continue;

                    action.Invoke(component);
                }
                ts.Commit();
            }
        }

        /// <summary>
        /// 使用相同大小
        /// </summary>
        /// <param name="resize"></param>
        public void MakeSameSize(ResizeType resize)
        {
            if (!this._active) return;
            var selectionService = this._designerHost.GetService<ISelectionService>();
            if (selectionService.SelectionCount < 2) return;

            var componentChangeService = this._designerHost.GetService<IComponentChangeService>();
            var selectedComponents = selectionService.GetSelectedComponents();
            using (var ts = this._designerHost.CreateTransaction(TRANS_MAKESAMESIZE))
            {
                var primaryControl = (Control)selectionService.PrimarySelection;
                var primarySize = primaryControl.Size;
                foreach (Control control in selectedComponents)
                {
                    if (control == primaryControl) continue;

                    var size = control.Size;
                    if ((resize & ResizeType.SameWidth) != 0)
                    {
                        size.Width = primarySize.Width;
                    }
                    if ((resize & ResizeType.SameHeight) != 0)
                    {
                        size.Height = primarySize.Height;
                    }
                    var member = TypeDescriptor.GetProperties(control)[PROP_NAME_SIZE];
                    componentChangeService.OnComponentChanging(control, member);
                    object oldValue = control.Size;
                    control.Size = size;
                    componentChangeService.OnComponentChanged(control, member, oldValue, size);
                }
                ts.Commit();
            }
        }

        /// <summary>
        /// 对齐
        /// </summary>
        /// <param name="align"></param>
        public void Align(AlignType align)
        {
            if (!this._active) return;

            var selectionService = this._designerHost.GetService<ISelectionService>();
            if (selectionService.SelectionCount < 2) return;

            var componentChangeService = this._designerHost.GetService<IComponentChangeService>();
            using (var ts = this._designerHost.CreateTransaction(TRANS_ALIGN))
            {
                var selectedComponents = selectionService.GetSelectedComponents();
                var primaryControl = (Control)selectionService.PrimarySelection;
                var primaryLocation = primaryControl.Location;
                var size = primaryControl.Size;
                foreach (Control control in selectedComponents)
                {
                    if (control == primaryControl) continue;

                    var location = control.Location;
                    #region 根据对齐方式计算控件的位置

                    if ((align & AlignType.Left) != 0)
                    {
                        location.X = primaryLocation.X;
                    }
                    else if ((align & AlignType.Right) != 0)
                    {
                        location.X = primaryLocation.X + size.Width - control.Size.Width;
                    }
                    else if ((align & AlignType.Center) != 0)
                    {
                        location.X = primaryLocation.X + (size.Width - control.Size.Width) / 2;
                    }
                    if ((align & AlignType.Top) != 0)
                    {
                        location.Y = primaryLocation.Y;
                    }
                    else if ((align & AlignType.Bottom) != 0)
                    {
                        location.Y = primaryLocation.Y + size.Height - control.Size.Height;
                    }
                    else if ((align & AlignType.Middle) != 0)
                    {
                        location.Y = primaryLocation.Y + (size.Height - control.Size.Height) / 2;
                    }

                    #endregion

                    var member = TypeDescriptor.GetProperties(control)[PROP_NAME_LOCATION];
                    componentChangeService.OnComponentChanging(control, member);
                    object oldValue = control.Location;
                    control.Location = location;
                    componentChangeService.OnComponentChanged(control, member, oldValue, location);
                }
                ts.Commit();
            }
        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <returns></returns>
        public bool Undo()
        {
            return this._designerHost.Undo();
        }

        /// <summary>
        /// 重做
        /// </summary>
        /// <returns></returns>
        public bool Redo()
        {
            return this._designerHost.Redo();
        }

        /// <summary>
        /// 重置撤销
        /// </summary>
        public void ResetUndo()
        {
            this._designerHost.ResetUndo();
        }

        #endregion

        #region 动态设置 DefaultValueAttribute

        public void AddDefaultValue(IComponent component, string propName, object value)
        {
            this.AddDefault(component, propName, value);
        }
        public void AddDefaultValue(Type componentType, string propName, object value)
        {
            this.AddDefault(componentType, propName, value);
        }
        public void RemoveDefaultValues(IComponent component)
        {
            this.RemoveDefaults(component);
        }
        public void RemoveDefaultValues(Type componentType)
        {
            this.RemoveDefaults(componentType);
        }
        private void AddDefault(object component, string propName, object value)
        {
            if (this._changedDefaults == null)
            {
                this._changedDefaults = new Dictionary<object, Dictionary<string, object>>();
                if (this._designerHost != null)
                {
                    this._designerHost.FilterProperties += this.FilterProperties;
                }
            }

            var dictionary = this._changedDefaults[component];
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>();
                this._changedDefaults[component] = dictionary;
            }
            dictionary[propName] = value;
        }
        private void RemoveDefaults(object component)
        {
            if (this._changedDefaults != null)
            {
                this._changedDefaults.Remove(component);

                if (this._changedDefaults.Count == 0)
                {
                    this._changedDefaults = null;
                    if (this._designerHost != null)
                    {
                        this._designerHost.FilterProperties -= this.FilterProperties;
                    }
                }
            }
        }
        private void FilterProperties(object sender, FilterEventArgs e)
        {
            Dictionary<string, object> dictionary = null;
            var component = sender as IComponent;
            if (!this._changedDefaults.TryGetValue(component, out dictionary))
            {
                this._changedDefaults.TryGetValue(component.GetType(), out dictionary);
            }

            if (dictionary != null)
            {
                foreach (var item in dictionary)
                {
                    var propertyDescriptor = e.Data[item.Key] as PropertyDescriptor;
                    if (propertyDescriptor != null)
                    {
                        var defaultValueAttribute = new DefaultValueAttribute(item.Value);
                        var value = TypeDescriptor.CreateProperty(component.GetType(), propertyDescriptor, new Attribute[]
                        {
                            defaultValueAttribute
                        });
                        e.Data[item.Key] = value;
                    }
                }
            }

        }

        #endregion

        #region 组件变化事件 ComponentChangeService

        private void RegisterListners()
        {
            var componentChangeService = this._designerHost.GetService<IComponentChangeService>();
            componentChangeService.ComponentAdded += this.ComponentAddedOrRemoved;
            componentChangeService.ComponentRemoved += this.ComponentAddedOrRemoved;
            componentChangeService.ComponentChanged += this.ComponentChanged;
        }
        private void UnregisterListners()
        {
            var componentChangeService = this._designerHost.GetService<IComponentChangeService>();
            componentChangeService.ComponentAdded -= this.ComponentAddedOrRemoved;
            componentChangeService.ComponentRemoved -= this.ComponentAddedOrRemoved;
            componentChangeService.ComponentChanged -= this.ComponentChanged;
        }
        private void ComponentAddedOrRemoved(object sender, ComponentEventArgs e)
        {
            this.SetDirty();
        }
        private void ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            this.SetDirty();
        }

        #endregion

        /// <summary>
        /// 设置为已更新
        /// </summary>
        public void SetDirty()
        {
            if (!this.IsDirty)
            {
                this.IsDirty = true;
                this.DirtyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public void ClearDirty()
        {
            if (this.IsDirty)
            {
                this.IsDirty = false;
                this.DirtyChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Control CreateControl(Type controlType, Size controlSize, Point controlLocation)
        {
            if (this.DesignerHost?.RootComponent == null) return null;

            var component = this.DesignerHost.CreateComponent(controlType);
            if (component == null) return null;

            var designer = this.DesignerHost.GetDesigner(component);
            if (designer == null) return null;

            if (designer is IComponentInitializer initializer)
            {
                initializer.InitializeNewComponent(null);
            }

            if (component is Control control)
            {
                control.Size = controlSize;
                control.Location = controlLocation;
                control.Parent = this.DesignerHost.RootComponent as Control;
                return control;
            }
            return null;
        }

        protected void EnableEdit(bool enable)
        {
            if (enable)
            {
                #region 开始设计
                this._designerLoader?.UnbindEvents(this._designerHost.DesignedForm.FindForm());
                this._designerHost.StartDesign();
                this.RegisterListners();
                if (this._designedComponents != null)
                {
                    foreach (var item in this._designedComponents)
                    {
                        this._designerHost.Container.Add(item.Value);
                    }
                }
                var container = this._designerHost.GetService<IContainerService>() ;
                var array = new KeyValuePair<string, IComponent>[container.Components.Count];
                container.Components.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    var item = array[i];
                    if (item.Value.Site == null)
                    {
                        this._designerHost.Container.Add(item.Value, item.Key);
                    }
                }
                this._designerHost.EndLoad();
                this.ClearDirty();
                if (this._designerLoader != null)
                {
                    this._designerLoader.RefreshEventData();
                }
                #endregion
            }
            else
            {
                #region 结束设计

                this._designerHost.StopDesign();
                this.ClearDirty();
                this.UnregisterListners();
                this._designerLoader?.BindEvents(this._designerHost.DesignedForm.FindForm());

                #endregion
            }
        }

    }
}
