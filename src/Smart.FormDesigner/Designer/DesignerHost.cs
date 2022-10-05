using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Smart.FormDesigner.Internal;
using Smart.FormDesigner.Services;

using static Smart.FormDesigner.Constants;

namespace Smart.FormDesigner
{
    /// <summary>
    /// 提供用于管理设计器事务和组件的接口
    /// </summary>
    public class DesignerHost :
        IContainer, //IDisposable
        IComponentChangeService,
        IDesignerEventService,
        //IDesignerLoaderHost,
        IDesignerHost, // IServiceContainer, IServiceProvider
        IExtenderListService,
        IExtenderProviderService,
        IUIService
    {
        private struct PanelControl
        {
            public Control Control;
            public int Row;
            public int Column;
        }

        private bool _active;
        private int _actionIndex;
        private List<MegaAction> _actions;
        private bool _doingAction;
        private bool _designStopped;
        private Control _designSurface;
        private Control _designedForm;
        private Point _designedFormLocation;
        private List<IExtenderProvider> _extenderProviders;
        private List<ExtenderProvidedProperty> exts = new List<ExtenderProvidedProperty>();
        private string memberName;
        private bool inRename = false;
        private Control _rootView;
        private Dictionary<IComponent, int> _selectedTab;
        private Dictionary<IComponent, DesignerSite> _sites;
        private ServiceContainer _serviceContainer;
        private List<DefaultDesignerTransaction> _transactions;

        public Control DesignedForm
        {
            get
            {
                return this._designedForm;
            }
            set
            {
                if (this._designedForm != null)
                {
                    var form = (this._designedForm is Form form1) ? form1 : this._designedForm.FindForm();
                    if (form != null)
                    {
                        form.Closed -= new EventHandler(this.DesginedFormClosed);
                    }
                }
                if (value != null)
                {
                    var form = (value is Form form1) ? form1 : value.FindForm();
                    if (form != null)
                    {
                        form.Closed += new EventHandler(this.DesginedFormClosed);
                    }
                    this._designedFormLocation = value.Location;
                }

                this._designedForm = value;
            }
        }

        public Control DesignContainer { get; set; }

        public string LogName { get; set; }

        public Designer Owner { get; set; }

        public int UndoCount => this._actionIndex + 1;
        public int RedoCount => this._actions.Count - this._actionIndex - 1;

        public SortedList<string, string> DesignedContainers { get; private set; }
        public Hashtable Parents { get; private set; }

        public DesignerHost()
        {
            this.DesignedContainers = new SortedList<string, string>();
            this.Parents = new Hashtable();
            this.Styles = new Hashtable();

            this._active = false;
            this._actions = new List<MegaAction>();
            this._designStopped = false;
            this._doingAction = false;
            this._extenderProviders = new List<IExtenderProvider>();
            this._selectedTab = new Dictionary<IComponent, int>();
            this._serviceContainer = new ServiceContainer();
            this._sites = new Dictionary<IComponent, DesignerSite>();
            this._transactions = new List<DefaultDesignerTransaction>();

            this._serviceContainer.AddService(typeof(IDesignerHost), this);
            this._serviceContainer.AddService(typeof(IContainer), this);
            this._serviceContainer.AddService(typeof(IComponentChangeService), this);
            this._serviceContainer.AddService(typeof(IContainerService), new ContainerService());

            var selectionService = new SelectionService(this);
            selectionService.SelectionChanged += this.SelectionComponentChanged;
            this._serviceContainer.AddService(typeof(ISelectionService), selectionService);
            this._serviceContainer.AddService(typeof(IDesignerEventService), this);
            this._serviceContainer.AddService(typeof(IExtenderProviderService), this);
            this._serviceContainer.AddService(typeof(IExtenderListService), this);

            var typeDescriptorFilterService = new TypeDescriptorFilterService(this);
            typeDescriptorFilterService.FilterAttribute += this.OnFilterAttributes;
            typeDescriptorFilterService.FilterEvnt += this.OnFilterEvnts;
            typeDescriptorFilterService.FilterProperty += this.OnFilterPropeties;
            this._serviceContainer.AddService(typeof(ITypeDescriptorFilterService), typeDescriptorFilterService);

            this._serviceContainer.AddService(typeof(INameCreationService), new NameCreationService());

            var menuCommandService = new DefaultMenuCommandService(this);
            menuCommandService.AddingVerb += this.LocalMenuAddingVerb;
            this._serviceContainer.AddService(typeof(IMenuCommandService), menuCommandService);

            this._serviceContainer.AddService(typeof(IUIService), this);
            this._serviceContainer.AddService(typeof(DesignerOptionService), new DefaultDesignerOptionService());

            this._serviceContainer.AddService(typeof(ITypeResolutionService), new TypeResolutionService());
            this._serviceContainer.AddService(typeof(ITypeDiscoveryService), new TypeDiscoveryService());

            this._serviceContainer.AddService(typeof(EventService), new EventService(this));
        }

        private void DesginedFormClosed(object sendr, EventArgs a)
        {
            if (this._active)
            {
                this.StopDesign();
            }
            this.DesignedForm = null;
        }

        private void SelectionComponentChanged(object sender, EventArgs e)
        {
            if (!this.Loading)
            {
                this.SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Redo()
        {
            if (this._actionIndex < this._actions.Count - 1)
            {
                this._actionIndex++;
                this._doingAction = true;
                this._actions[this._actionIndex].Redo();
                this._doingAction = false;
                return true;
            }
            return false;
        }

        public bool Undo()
        {
            if (this._actionIndex >= 0 && this._actionIndex < this._actions.Count)
            {
                this._doingAction = true;
                this._actions[this._actionIndex].Undo();
                this._doingAction = false;
                this._actionIndex--;
                return true;
            }
            return false;
        }

        public void ResetUndo()
        {
            this._actions.Clear();
            this._actionIndex = -1;
        }

        public void StartDesign()
        {
            if (this._designedForm == null) return;
            this.Loading = true;
            this._designStopped = false;
            this.Styles.Clear();
            this.Styles.Add(STYLE_KEY_HIGHLIGHTCOLOR, SystemColors.HighlightText);
            this.Styles.Add(STYLE_KEY_DIALOGFONT, Control.DefaultFont);
            if (this.DesignContainer == null)
            {
                this._designSurface = new Internal.DesignSurface();
                ((Internal.DesignSurface)this._designSurface).DesignedControl = this._designedForm;
                this._designSurface.Name = DEFAULT_NAME_DESIGNSURFACE;
                this._designSurface.Size = this._designedForm.ClientSize;
            }
            else
            {
                this._designSurface = this.CreateDesignSurface(this._designedForm.GetType());
                this._designSurface.Parent = this.DesignContainer;
            }
            this.Add(this._designSurface, this._designSurface.Name);
            if (this.DesignContainer != null)
            {
                this._designedForm.CopyPropertiesTo(this._designSurface);
            }
            var rootDesigner = (IRootDesigner)this.GetDesigner(this._designSurface);
            this._rootView = (Control)rootDesigner.GetView(ViewTechnology.Default);
            this._rootView.Dock = DockStyle.Fill;
            var array = this._designedForm.Controls.ToArray();
            this._designSurface.Location = new Point(8, 8);
            this._rootView.Size = ((this.DesignContainer == null)
                ? this._designedForm.ClientSize
                : this.DesignContainer.ClientSize);
            this.AddingControls(array, this._designSurface);
            if (this.DesignContainer == null)
            {
                this._designedForm.Controls.Add(this._rootView);
            }
            else
            {
                this.DesignContainer.Controls.Add(this._rootView);
            }
            this.Activate();
            this.Loading = false;
            this.ResetUndo();
            Application.AddMessageFilter(this.GetService<EventService>());
        }

        public void StopDesign()
        {
            if (this._designedForm != null)
            {
                this.Loading = true;

                var selectionService = this._serviceContainer.GetService<ISelectionService>();
                selectionService.SetSelectedComponents(null);

                var value = this.GetService<EventService>();
                Application.RemoveMessageFilter(value);

                this._actions.Clear();
                this._transactions.Clear();
                this._designStopped = true;
                var controls = this._designSurface.Controls.ToArray();
                this.RestoreControls(controls, this._designedForm);
                if (this._designSurface.GetType().IsAssignableFrom(this._designedForm.GetType()))
                {
                    var site = new DesignerSite(this, this._designedForm);
                    this._designedForm.Site = site;
                    this.AddExtProviders(this._designedForm, this._designSurface);
                    this._designedForm.Site = null;
                }
                this._selectedTab.Clear();
                if (this._sites.Count != 0)
                {
                    var components = this._sites.Keys.ToArray();
                    for (int i = 0; i < components.Length; i++)
                    {
                        var component = components[i];
                        this.Remove(component, true);
                    }
                }
                if (this.DesignContainer == null)
                {
                    this._designedForm.Controls.Remove(this._rootView);
                    ((Internal.DesignSurface)this._designSurface).DesignedControl = null;
                }
                else
                {
                    this.DesignContainer.Controls.Remove(this._rootView);
                    this._designSurface.CopyPropertiesTo(this._designedForm);
                }
                this._rootView.Dispose();
                this._rootView = null;
                this.DestroyComponent(this._designSurface);
                this._designSurface = null;
                this.Deactivate();
                this.UpdateExtProviders();
                this._designedForm.Location = this._designedFormLocation;
                this.Loading = false;
            }
        }

        public void EndLoad()
        {
            if (this.DesignContainer != null)
            {
                this._designedForm.Site = new DesignerSite(this, this._designedForm);
                this.exts.Clear();
                this.AddExtProviders(this._designSurface, this._designedForm);
                this.UpdateExtProviders();
                this._designedForm.Site = null;
            }
            this.LoadComplete?.Invoke(this, EventArgs.Empty);
        }

        public Control CreateDesignSurface(Type rootType)
        {
            var control = Activator.CreateInstance(rootType);
            if (control is Form form)
            {
                form.TopLevel = false;
            }
            return (Control)control;
        }

        private void AddTableLayout(TableLayoutPanel panel, Control parent)
        {
            var array = this.MovePanelControls(panel);
            this.AddingControl(panel, parent);
            panel.Parent = parent;
            for (int i = 0; i < array.Length; i++)
            {
                var panelControl = array[i];
                if (panelControl.Control is TableLayoutPanel tableLayoutPanel)
                {
                    this.AddTableLayout(tableLayoutPanel, null);
                }
                else
                {
                    this.AddingControl(panelControl.Control, null);
                }
                panel.Controls.Add(panelControl.Control, panelControl.Column, panelControl.Row);
            }
        }

        private void RestoreTableLayout(TableLayoutPanel panel, Control parent)
        {
            var array = this.MovePanelControls(panel);
            var temp = array;
            for (int i = 0; i < temp.Length; i++)
            {
                var panelControl = temp[i];
                if (panelControl.Control is TableLayoutPanel tableLayoutPanel)
                {
                    this.RestoreTableLayout(tableLayoutPanel, null);
                }
                else
                {
                    this.RestoreControl(panelControl.Control, null);
                }
            }
            this.RestoreControl(panel, parent);
            temp = array;
            for (int i = 0; i < temp.Length; i++)
            {
                var panelControl = temp[i];
                panel.Controls.Add(panelControl.Control, panelControl.Column, panelControl.Row);
            }
        }

        private PanelControl[] MovePanelControls(TableLayoutPanel panel)
        {
            var array = new PanelControl[panel.Controls.Count];
            int num = 0;
            foreach (Control control in panel.Controls)
            {
                var panelControl = default(PanelControl);
                panelControl.Control = control;
                panelControl.Row = panel.GetRow(control);
                panelControl.Column = panel.GetColumn(control);
                array[num++] = panelControl;
            }
            panel.Controls.Clear();
            return array;
        }

        private void AddingControl(Control control, Control parent)
        {
            if (control.Site == null)
            {
                control.Parent = parent;
                control.Parent = null;
                this.Add(control, control.Name);

                string text = control.Text;
                if (this.GetDesigner(control) is ComponentDesigner componentDesigner)
                {
                    if (this.CanInitializeExisting(control))
                    {
                        componentDesigner.InitializeExistingComponent(null);
                    }
                    else
                    {
                        componentDesigner.InitializeNewComponent(null);
                    }
                }
                control.Text = text;
                control.Parent = parent;
            }

            if (control.Controls.Count != 0 && this.GetDesigner(control) is ParentControlDesigner)
            {
                var array = control.Controls.ToArray();
                this.AddingControls(array, control);
            }
        }

        private void AddingControls(Array controls, Control parent)
        {
            foreach (Control control in controls)
            {
                if (control is TableLayoutPanel tableLayoutPanel)
                {
                    this.AddTableLayout(tableLayoutPanel, parent);
                }
                else
                {
                    this.AddingControl(control, parent);
                }
            }
        }

        private bool CanInitializeExisting(object control)
        {
            Type type = control.GetType();
            return typeof(Panel).IsAssignableFrom(type)
                || typeof(Button).IsAssignableFrom(type)
                || control is TabControl
                || control is DataGridView
                ;
        }

        private void AddExtProviders(Control target, Control src)
        {
            var srcProperties = TypeDescriptor.GetProperties(src);
            var destProperties = TypeDescriptor.GetProperties(target);
            foreach (PropertyDescriptor srcPropertyDescriptor in srcProperties)
            {
                var attributes = srcPropertyDescriptor.Attributes;
                if (attributes[typeof(ExtenderProvidedPropertyAttribute)] is ExtenderProvidedPropertyAttribute eppAttr && eppAttr.Provider != null)
                {
                    object srcValue = srcPropertyDescriptor.GetValue(src);
                    if (srcValue != null && destProperties[srcPropertyDescriptor.Name] != null)
                    {
                        var epp = new ExtenderProvidedProperty(srcPropertyDescriptor.Name, eppAttr, src, target);
                        epp.Invoke();
                        this.exts.Add(epp);
                    }
                }
            }
        }

        private void UpdateExtProviders()
        {
            foreach (var ext in this.exts)
            {
                ext.Invoke();
            }
            this.exts.Clear();
        }

        private void RestoreControl(Control control, Control parent)
        {
            control.Visible = control.IsVisiable();
            this.AddExtProviders(control, control);

            if (control.Controls.Count != 0 && this.GetDesigner(control) is ParentControlDesigner)
            {
                var controls = control.Controls.ToArray();
                this.RestoreControls(controls, control);
            }
            if (control.Site != null)
            {
                control.Name = control.Site.Name;
            }
            this.Remove(control);
            control.Parent = parent;
            if (!control.Enabled)
            {
                control.Enabled = false;
            }
            if (control is TabControl tabControl)
            {
                this._selectedTab.TryGetValue(tabControl, out int selectedIndex);
                var tabPages = tabControl.TabPages;
                if (selectedIndex >= tabPages.Count)
                {
                    selectedIndex = 0;
                }
                tabControl.SelectedTab = tabPages[selectedIndex];
            }
        }

        private void RestoreControls(Array controls, Control parent)
        {
            foreach (Control control in controls)
            {
                if (control != null)
                {
                    if (control is TableLayoutPanel)
                    {
                        this.RestoreTableLayout(control as TableLayoutPanel, parent);
                    }
                    else
                    {
                        this.RestoreControl(control, parent);
                    }
                }
            }
        }


        #region 事件

        public event EventHandler<FilterEventArgs> FilterAttributes;
        public event EventHandler<FilterEventArgs> FilterEvents;
        public event EventHandler<FilterEventArgs> FilterProperties;
        public event EventHandler<AddingVerbEventArgs> AddingVerb;

        private void OnFilterAttributes(object sender, FilterEventArgs e)
        {
            this.FilterAttributes?.Invoke(sender, e);
        }
        private void OnFilterEvnts(object sender, FilterEventArgs e)
        {
            this.FilterEvents?.Invoke(sender, e);
        }
        private void OnFilterPropeties(object sender, FilterEventArgs e)
        {
            this.FilterProperties?.Invoke(sender, e);
        }

        private void LocalMenuAddingVerb(object sender, AddingVerbEventArgs e)
        {
            this.AddingVerb?.Invoke(sender, e);
        }

        #endregion

        #region IDesignerHost 接口成员

        #region IDesignerHost 属性

        public IContainer Container => this;

        public bool Loading { get; set; }

        public bool InTransaction => this._transactions.Count > 0;

        public IComponent RootComponent => this._designSurface;

        public string RootComponentClassName => this.RootComponent.GetType().ToString();

        public string TransactionDescription
        {
            get
            {
                if (this.InTransaction)
                {
                    var designerTransaction = this._transactions[this._transactions.Count - 1];
                    return designerTransaction.Description;
                }

                return null;
            }
        }
        #endregion

        #region IDesignerHost 事件

        public event EventHandler Activated;
        public event EventHandler Deactivated;
        public event EventHandler LoadComplete;
        public event DesignerTransactionCloseEventHandler TransactionClosed;
        public event DesignerTransactionCloseEventHandler TransactionClosing;
        public event EventHandler TransactionOpened;
        public event EventHandler TransactionOpening;
        #endregion

        #region IDesignerHost 方法

        public void Activate()
        {
            this._active = true;
            this.DesignedForm.Focus();
        }

        private void Deactivate()
        {
            this._active = false;
        }

        public DesignerTransaction CreateTransaction()
        {
            return this.CreateTransaction(null);
        }
        public DesignerTransaction CreateTransaction(string desc)
        {
            if (!this.Loading && this._transactions.Count == 0 && this.TransactionOpening != null)
            {
                this.TransactionOpening(this, EventArgs.Empty);
            }
            DefaultDesignerTransaction designerTransaction;
            if (desc == null)
            {
                designerTransaction = new DefaultDesignerTransaction(this);
            }
            else
            {
                designerTransaction = new DefaultDesignerTransaction(this, desc);
            }
            this._transactions.Add(designerTransaction);
            if (!this.Loading && this._transactions.Count == 1)
            {
                this.TransactionOpened?.Invoke(this, EventArgs.Empty);
                if (!this._doingAction)
                {
                    while (this.RedoCount > 0)
                    {
                        this._actions.RemoveAt(this._actions.Count - 1);
                    }
                    var megaAction = new MegaAction(this);
                    this._actions.Add(megaAction);
                    this._actionIndex = this._actions.Count - 1;
                    megaAction.StartActions();
                }
            }
            return designerTransaction;
        }

        public void ClosingTransaction(DefaultDesignerTransaction transaction, bool commiting)
        {
            if (!this.Loading)
            {
                if (this._transactions.Count == 1)
                {
                    this.TransactionClosing?.Invoke(this, new DesignerTransactionCloseEventArgs(commiting, true));
                    this.TransactionClosed?.Invoke(this, new DesignerTransactionCloseEventArgs(commiting, true));
                    if (!this._doingAction)
                    {
                        var megaAction = this._actions[this._actions.Count - 1];
                        megaAction.StopActions();
                        if (!commiting)
                        {
                            this._doingAction = true;
                            megaAction.Undo();
                            this._doingAction = false;
                            this._actions.RemoveAt(this._actions.Count - 1);
                        }
                    }
                }
            }
            this._transactions.Remove(transaction);
        }
        public void TransactionCommiting(DefaultDesignerTransaction transaction)
        {
            this.ClosingTransaction(transaction, true);
        }
        public void TransactionCanceling(DefaultDesignerTransaction transaction)
        {
            this.ClosingTransaction(transaction, false);
        }


        public IComponent CreateComponent(Type componentType)
        {
            return this.CreateComponent(componentType, null);
        }
        public IComponent CreateComponent(Type componentType, string name)
        {
            var  component = (IComponent)Activator.CreateInstance(componentType);
            if (name == null)
            {
                if (this.GetService(typeof(INameCreationService)) is INameCreationService nameCreationService)
                {
                    name = nameCreationService.CreateName(this, componentType);
                }
                else
                {
                    name = componentType.Name;
                }
            }
            if (componentType == typeof(SplitContainer))
            {
                this.Add((component as SplitContainer).Panel1, null);
                this.Add((component as SplitContainer).Panel2, null);
            }
            this.Add(component, name);
            return component;
        }
        public void DestroyComponent(IComponent component)
        {
            if (component.Site != null && component.Site.Container == this)
            {
                this.Remove(component);
                component.Dispose();
            }
        }

        public IDesigner GetDesigner(IComponent component)
        {
            if (component?.Site == null || component.Site.Container != this)
            {
                return null;
            }
            return (component.Site as DesignerSite)?.Designer;
        }

        public Type GetType(string typeName)
        {
            if (this.GetService(typeof(ITypeResolutionService)) is ITypeResolutionService typeResolutionService)
            {
                return typeResolutionService.GetType(typeName);
            }
            else
            {
                return Type.GetType(typeName);
            }
        }

        #endregion

        #endregion

        #region IServiceProvider 接口成员

        public object GetService(Type serviceType)
        {
            return this._serviceContainer.GetService(serviceType);
        }

        #endregion

        #region IServiceContainer 接口成员 

        public void AddService(Type serviceType, object serviceInstance)
        {
            this._serviceContainer.AddService(serviceType, serviceInstance);
        }
        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            this._serviceContainer.AddService(serviceType, serviceInstance, promote);
        }
        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            this._serviceContainer.AddService(serviceType, callback);
        }
        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            this._serviceContainer.AddService(serviceType, callback);
        }
        public void RemoveService(Type serviceType)
        {
            this._serviceContainer.RemoveService(serviceType);
        }
        public void RemoveService(Type serviceType, bool promote)
        {
            this._serviceContainer.RemoveService(serviceType, promote);
        }

        #endregion

        #region IContainer 接口成员

        public ComponentCollection Components
        {
            get
            {
                var components = this._sites.Select(s => s.Value.Component).ToArray();
                return new ComponentCollection(components);
            }
        }

        public void Add(IComponent component)
        {
            this.Add(component, null);
        }

        public void Add(IComponent component, string name)
        {
            if (component != null && !this._designStopped)
            {
                if (component.Site == null || component.Site.Container != this)
                {
                    if (component is Control control)
                    {
                        this.Parents[component] = control.Parent;
                    }
                    var designerTransaction = this.CreateTransaction(TRANS_ADD_COMPONENT);
                    using (designerTransaction)
                    {
                        var designerSite = new DesignerSite(this, component);
                        if (name == null || this.Components[name] != null)
                        {
                            var nameCreationService = this.GetService<INameCreationService>();
                            name = nameCreationService.CreateName(this, component.GetType());
                        }
                        if (component is Control control2 && control2.Name != name)
                        {
                            control2.Name = name;
                        }
                        designerSite.Name = name;
                        string text = component.GetType().ToString().ToUpper();

                        component.Site = designerSite;

                        if (component is TabControl tabControl)
                        {
                            var selectedTab = tabControl.SelectedTab;
                            var tabPages = tabControl.TabPages;
                            int index = 0;
                            foreach (TabPage tabPage in tabPages)
                            {
                                if (tabPage == selectedTab)
                                {
                                    this._selectedTab.TryGetValue(component, out index);
                                    break;
                                }
                                index++;
                            }
                        }
                        var designer = this.CreateComponentDesigner(component);

                        this.ComponentAdding?.Invoke(this, new ComponentEventArgs(component));
                        designerSite.Designer = designer;
                        this._sites.Add(component, designerSite);
                        designer.Initialize(component);
                        var extenderProviderService = this.GetService<IExtenderProviderService>();
                        if (designer is IExtenderProvider extenderProvider)
                        {
                            extenderProviderService.AddExtenderProvider(extenderProvider);
                        }
                        extenderProvider = (component as IExtenderProvider);
                        if (extenderProvider != null)
                        {
                            extenderProviderService.AddExtenderProvider(extenderProvider);
                        }
                        var container = this.GetService<IContainerService>();
                        if (!(component is Control))
                        {
                            container.Add(component, name);
                        }
                        this.ComponentAdded?.Invoke(this, new ComponentEventArgs(component));
                        designerTransaction.Commit();
                        this.CheckContainedComponents(component);
                    }
                }
            }
        }

        public void Remove(IComponent component)
        {
            this.Remove(component, true);
        }
        private void Remove(IComponent component, bool fireEvents)
        {
            if (component.Site != null && component.Site.Container == this)
            {
                var designerTransaction = this.CreateTransaction(TRANS_REMOVE_COMPONENT);
                using (designerTransaction)
                {
                    if (fireEvents && component != this._rootView)
                    {
                        this.ComponentRemoving?.Invoke(this, new ComponentEventArgs(component));
                    }
                    if (!this._designStopped)
                    {
                        var container = this.GetService<IContainerService>();
                        container.Remove(component.Site.Name);
                    }
                    var extenderProviderService = this.GetService<IExtenderProviderService>();
                    if (component is IExtenderProvider extenderProvider)
                    {
                        extenderProviderService.RemoveExtenderProvider(extenderProvider);
                    }
                    this._sites.Remove(component);
                    var designer = ((DesignerSite)component.Site).Designer;
                    if (designer is IExtenderProvider extenderProvider2)
                    {
                        extenderProviderService.RemoveExtenderProvider(extenderProvider2);
                    }

                    if (fireEvents && component != this._rootView)
                    {
                        var e = new ComponentEventArgs(component);
                        this.ComponentRemoved?.Invoke(this, e);
                        if (this.GetService(typeof(ISelectionService)) is SelectionService selectionService)
                        {
                            selectionService.OnComponentRemoved(this, e);
                        }
                    }
                    component.Site = null;
                    try
                    {
                        designer?.Dispose();
                    }
                    catch (Exception)
                    {
                        //TODO: designer.Dispose Exception
                    }
                    designerTransaction.Commit();
                }
            }
        }

        private IDesigner CreateComponentDesigner(IComponent component)
        {
            IDesigner result;
            if (this._sites.Count == 0)
            {
                result = new RootDesigner();
            }
            else
            {
                var typeResolutionService = this.GetService<ITypeResolutionService>();
                typeResolutionService.ReferenceAssembly(component.GetType().Assembly.GetName());
                var designer = TypeDescriptor.CreateDesigner(component, typeof(IDesigner));
                if (designer != null)
                {
                    result = designer;
                }
                else
                {
                    var designerAttribute = this.FindDesignerAttribute(component);
                    if (designerAttribute == null)
                    {
                        result = null;
                    }
                    else
                    {
                        result = this.InstantinateDesigner(designerAttribute);
                    }
                }
            }
            return result;
        }

        private DesignerAttribute FindDesignerAttribute(IComponent component)
        {
            var attributes = TypeDescriptor.GetAttributes(component);
            foreach (Attribute attribute in attributes)
            {
                if (attribute.GetType() == typeof(DesignerAttribute) && attribute.TypeId.ToString().IndexOf("IDesigner") >= 0)
                {
                    return (DesignerAttribute)attribute;
                }
            }
            return null;
        }

        private IDesigner InstantinateDesigner(DesignerAttribute da)
        {
            string text = ",";
            string[] array = da.DesignerTypeName.Split(text.ToCharArray());
            IDesigner result;
            if (array.Length <= 1)
            {
                result = null;
            }
            else
            {
                var assembly = Assembly.Load(array[1].Trim());
                IDesigner designer = null;
                if (assembly != null)
                {
                    designer = (IDesigner)assembly.CreateInstance(array[0].Trim());
                }
                result = designer;
            }
            return result;
        }

        private void CheckContainedComponents(IComponent control)
        {
            Type type = control.GetType();
            PropertyInfo propertyInfo = null;
            foreach (string text in this.DesignedContainers.Keys)
            {
                if (type.FullName.IndexOf(text) >= 0)
                {
                    propertyInfo = type.GetProperty(this.DesignedContainers[text]);
                    break;
                }
            }
            if (propertyInfo == null)
            {
                if (control is DataSet dataset)
                {
                    this.AddDataSetComponents(dataset);
                }
            }
            else
            {
                object value = propertyInfo.GetValue(control, null);
                if (value != null)
                {
                    if (value is IList list)
                    {
                        var enumerator = list.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current is IComponent component)
                            {
                                this.Add(component, null);
                                this.AddChildControls(component);
                            }
                        }
                    }
                    else if (value is IComponent component)
                    {
                        this.Add(component, null);
                    }
                }
            }
        }

        private void AddDataSetComponents(DataSet ds)
        {
            foreach (DataTable dataTable in ds.Tables)
            {
                this.Add(dataTable, dataTable.TableName);
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    this.Add(dataColumn, dataColumn.ColumnName);
                }
            }
        }

        private void AddChildControls(IComponent comp)
        {
            if (comp is Control control && control.Controls.Count > 0)
            {
                var designer = this.GetDesigner(comp);
                if (typeof(ParentControlDesigner).IsAssignableFrom(designer.GetType()))
                {
                    foreach (Control component in control.Controls)
                    {
                        this.Add(component, null);
                    }
                }
            }
        }

        #endregion

        #region IComponentChangeService 接口成员 

        public event ComponentEventHandler ComponentAdded;
        public event ComponentEventHandler ComponentAdding;
        public event ComponentChangingEventHandler ComponentChanging;
        public event ComponentChangedEventHandler ComponentChanged;
        public event ComponentEventHandler ComponentRemoved;
        public event ComponentEventHandler ComponentRemoving;
        public event ComponentRenameEventHandler ComponentRename;

        public void OnComponentChanging(object component, MemberDescriptor member)
        {
            if (!this.Loading)
            {
                this.memberName = member?.Name;
                this.ComponentChanging?.Invoke(this, new ComponentChangingEventArgs(component, member));
            }
        }

        public void OnComponentChanged(object component, MemberDescriptor member, object oldValue, object newValue)
        {
            if (!this.Loading)
            {
                if (this.ComponentChanged != null)
                {
                    var value = (this.memberName != null && member != null && this.memberName == member.Name) ? null : oldValue;
                    var e = new ComponentChangedEventArgs(component, member, value, newValue);
                    try
                    {
                        this.ComponentChanged(this, e);
                    }
                    catch (Exception)
                    {
                        //TODO:ToolStrip ComponentChanged Error
                    }
                }
                if (member != null && member.Name == PROP_NAME_NAME)
                {
                    if (this.ComponentRename != null && !this.inRename)
                    {
                        this.inRename = true;
                        this.ComponentRename(this, new ComponentRenameEventArgs(component, (string)oldValue, (string)newValue));
                        this.inRename = false;
                    }
                }
            }
        }

        #endregion

        #region IDesignerEventService 接口成员
        // TODO: 实现 IDesignerEventService
        public IDesignerHost ActiveDesigner => this;

        public DesignerCollection Designers => new DesignerCollection(new IDesignerHost[] { this });

        public event ActiveDesignerEventHandler ActiveDesignerChanged;
        public event DesignerEventHandler DesignerDisposed;
        public event DesignerEventHandler DesignerCreated;
        public event EventHandler SelectionChanged;

        #endregion

        #region IExtenderProviderService 接口成员

        public void AddExtenderProvider(IExtenderProvider provider)
        {
            if (!this._extenderProviders.Contains(provider))
            {
                this._extenderProviders.Add(provider);
            }
        }

        public void RemoveExtenderProvider(IExtenderProvider provider)
        {
            if (this._extenderProviders.Contains(provider))
            {
                this._extenderProviders.Remove(provider);
            }
        }

        #endregion

        #region IExtenderListService 接口成员

        public IExtenderProvider[] GetExtenderProviders()
        {
            return this._extenderProviders.ToArray();
        }

        #endregion

        #region IUIService 接口成员

        public IDictionary Styles { get; private set; }

        public bool CanShowComponentEditor(object component)
        {
            return false;
        }

        public IWin32Window GetDialogOwnerWindow()
        {
            return this.RootComponent as IWin32Window;
        }

        public void SetUIDirty()
        {
            this.Owner.SetDirty();
        }

        public bool ShowComponentEditor(object component, IWin32Window parent)
        {
            return false;
        }

        public DialogResult ShowDialog(Form form)
        {
            try
            {
                return form.ShowDialog(this.GetDialogOwnerWindow());
            }
            catch
            {
                return DialogResult.Cancel;
            }
        }

        public void ShowError(Exception ex, string message)
        {
            MessageBox.Show(this.GetDialogOwnerWindow(), ex.Message, message);
        }
        void IUIService.ShowError(Exception ex)
        {
            MessageBox.Show(this.GetDialogOwnerWindow(), ex.Message);
        }
        void IUIService.ShowError(string message)
        {
            MessageBox.Show(this.GetDialogOwnerWindow(), message);
        }

        void IUIService.ShowMessage(string message, string caption)
        {
            MessageBox.Show(this.GetDialogOwnerWindow(), message, caption);
        }

        void IUIService.ShowMessage(string message)
        {
            MessageBox.Show(this.GetDialogOwnerWindow(), message);
        }

        public DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(this.GetDialogOwnerWindow(), message, caption, buttons);
        }

        public bool ShowToolWindow(Guid toolWindow)
        {
            return false;
        }

        #endregion

        #region IDisposable 接口成员

        public void Dispose()
        {
            this._designedForm = null;
            var array = this._sites.Values.ToArray();
            foreach (DesignerSite designerSite in array)
            {
                try
                {
                    designerSite.Designer.Dispose();
                    designerSite.Component.Site = null;
                }
                catch (Exception)
                {
                    //TODO: Designer.Dispose() exception
                }
            }
            this._sites.Clear();
            this._serviceContainer.RemoveService(typeof(IDesignerHost));
            this._serviceContainer.RemoveService(typeof(IContainer));
            this._serviceContainer.RemoveService(typeof(IComponentChangeService));
            this._serviceContainer.RemoveService(typeof(ISelectionService));
            this._serviceContainer.RemoveService(typeof(IDesignerEventService));
            this._serviceContainer.RemoveService(typeof(IExtenderProviderService));
            this._serviceContainer.RemoveService(typeof(IExtenderListService));
            this._serviceContainer.RemoveService(typeof(ITypeDescriptorFilterService));
            this._serviceContainer.RemoveService(typeof(INameCreationService));
            this._serviceContainer.RemoveService(typeof(IMenuCommandService));
            this._serviceContainer.RemoveService(typeof(IUIService));
            this._serviceContainer.RemoveService(typeof(ITypeResolutionService));
            this._serviceContainer.RemoveService(typeof(IDesignerOptionService));
            this._serviceContainer.RemoveService(typeof(DesignerOptionService));
            this._serviceContainer.RemoveService(typeof(EventService));
        }

        #endregion

    }
}
