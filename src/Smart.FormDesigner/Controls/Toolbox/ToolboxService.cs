using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using static Smart.FormDesigner.Constants;

namespace Smart.FormDesigner.Toolbox
{
    /// <summary>
    /// 提供在设计模式下管理和查询工具箱的方法和属性。
    /// </summary>
    public class ToolboxService : AbstractService, IToolboxService
    {
        private IToolbox _toolbox;
        private List<Designer> _designers;
        private Designer _defaultDesigner;
        private Dictionary<string, ToolboxItemCreatorCallback> _creators;

        public Designer Designer
        {
            get
            {
                return this._defaultDesigner;
            }
            set
            {
                if (value == null)
                {
                    this.RemoveDesigner(this._defaultDesigner);
                }
                else if (!this._designers.Exists(d => d == value))
                {
                    this.AddDesigner(value);
                }
                this._defaultDesigner = value;
            }
        }

        public ToolboxService(IToolbox toolbox)
        {
            this._toolbox = toolbox;
            this._toolbox.BeginDragAndDrop += this.OnDragAndDrop;
            this._toolbox.DropControl += this.OnDropControl;
            this._creators = new Dictionary<string, ToolboxItemCreatorCallback>();
            this._designers = new List<Designer>();
        }

        #region IToolboxService  接口属性成员

        public CategoryNameCollection CategoryNames
        {
            get
            {
                var items = this._toolbox.Items;
                if (items.Count <= 0) return null;

                var names = items.Select(c => c.Name).ToArray();
                return new CategoryNameCollection(names);
            }
        }
        public string SelectedCategory
        {
            get
            {
                return this._toolbox.SelectedCategory;
            }
            set
            {
                this._toolbox.SelectedCategory = value;
            }
        }

        #endregion

        #region IToolboxService  接口方法成员

        public void AddCreator(ToolboxItemCreatorCallback creator, string format)
        {
            this.AddCreator(creator, format, null);
        }

        public void AddCreator(ToolboxItemCreatorCallback creator, string format, IDesignerHost host)
        {
            this._creators[format] = creator;
        }

        public void AddLinkedToolboxItem(ToolboxItem toolboxItem, IDesignerHost host)
        {
        }

        public void AddLinkedToolboxItem(ToolboxItem toolboxItem, string category, IDesignerHost host)
        {
        }

        public void AddToolboxItem(ToolboxItem toolboxItem)
        {
            this._toolbox.AddItem(toolboxItem, null);
        }

        public void AddToolboxItem(ToolboxItem toolboxItem, string category)
        {
            this._toolbox.AddItem(toolboxItem, category);
        }

        public ToolboxItem DeserializeToolboxItem(object serializedObject)
        {
            return this.DeserializeToolboxItem(serializedObject, null);
        }

        public ToolboxItem DeserializeToolboxItem(object serializedObject, IDesignerHost host)
        {
            var dataObject = serializedObject as IDataObject;
            var typeFromHandle = typeof(ToolboxItem);
            string[] formats = dataObject.GetFormats();
            for (int i = 0; i < formats.Length; i++)
            {
                string key = formats[i];
                if (this._creators.TryGetValue(key, out ToolboxItemCreatorCallback toolboxItemCreatorCallback))
                {
                    ToolboxItem toolboxItem = null;
                    try
                    {
                        toolboxItem = toolboxItemCreatorCallback(serializedObject, key);
                    }
                    catch
                    {
                    }
                    if (toolboxItem != null)
                    {
                        return toolboxItem;
                    }
                }
                else
                {
                    object data = dataObject.GetData(key);
                    if (typeFromHandle.IsAssignableFrom(data.GetType()))
                    {
                        return data as ToolboxItem;
                    }
                }
            }
            return null;
        }

        public ToolboxItem GetSelectedToolboxItem()
        {
            return this.GetSelectedToolboxItem(null);
        }
        public ToolboxItem GetSelectedToolboxItem(IDesignerHost host)
        {
            return this._toolbox.SelectedItem;
        }

        public ToolboxItemCollection GetToolboxItems(string category, IDesignerHost host)
        {
            var toolboxCategoryItem = this._toolbox.Items.FirstOrDefault(i => i.Name == category);
            if (toolboxCategoryItem != null)
            {
                return new ToolboxItemCollection(toolboxCategoryItem.Items);
            }
            return null;
        }

        public ToolboxItemCollection GetToolboxItems(string category)
        {
            return this.GetToolboxItems(category, null);
        }

        public ToolboxItemCollection GetToolboxItems(IDesignerHost host)
        {
            var list = new List<ToolboxItem>();
            foreach (var category in this._toolbox.Items)
            {
                foreach (ToolboxItem item in category.Items)
                {
                    list.Add(item);
                }
            }
            return new ToolboxItemCollection(list.ToArray());
        }

        public ToolboxItemCollection GetToolboxItems()
        {
            IDesignerHost host = null;
            return this.GetToolboxItems(host);
        }

        public bool IsSupported(object serializedObject, ICollection filterAttributes)
        {
            return true;
        }

        public bool IsSupported(object serializedObject, IDesignerHost host)
        {
            return true;
        }

        public bool IsToolboxItem(object serializedObject, IDesignerHost host)
        {
            return this.DeserializeToolboxItem(serializedObject, host) != null;
        }

        public bool IsToolboxItem(object serializedObject)
        {
            return this.IsToolboxItem(serializedObject, null);
        }

        public void Refresh()
        {
            this._toolbox.Refresh();
        }

        public void RemoveCreator(string format, IDesignerHost host)
        {
            this._creators.Remove(format);
        }

        public void RemoveCreator(string format)
        {
            this.RemoveCreator(format, null);
        }

        public void RemoveToolboxItem(ToolboxItem toolboxItem, string category)
        {
            this._toolbox.RemoveItem(toolboxItem, category);
        }

        public void RemoveToolboxItem(ToolboxItem toolboxItem)
        {
            this._toolbox.RemoveItem(toolboxItem, null);
        }

        public void SelectedToolboxItemUsed()
        {
            this._toolbox.SelectedItem = null;
        }

        public object SerializeToolboxItem(ToolboxItem toolboxItem)
        {
            return (toolboxItem == null) ? null : new DataObject(toolboxItem);
        }

        public bool SetCursor()
        {
            var currentCursor = this._toolbox.SelectedItem == null ? Cursors.Arrow : Cursors.Cross;
            if (currentCursor == Cursors.Arrow)
            {
                foreach (var designer in this._designers)
                {
                    if (designer.DesignContainer != null || designer.DesignedForm == null)
                    {
                        return false;
                    }
                    designer.DesignedForm.Cursor = Cursors.Arrow;
                }
                return false;
            }
            else
            {
                foreach (var designer in this._designers)
                {
                    if (designer.DesignContainer != null)
                    {
                        designer.DesignContainer.Cursor = currentCursor;
                    }
                    else
                    {
                        designer.DesignedForm.Cursor = currentCursor;
                    }
                }
                return true;
            }
        }

        public void SetSelectedToolboxItem(ToolboxItem toolboxItem)
        {
            this._toolbox.SelectedItem = toolboxItem;
        }


        #endregion

        private  void AddDesigner(Designer designer)
        {
            if (designer != null)
            {
                designer.DesignerHost.AddService(typeof(IToolboxService), this);
                this._designers.Add(designer);
                if (this._designers.Count == 1)
                {
                    this._defaultDesigner = designer;
                }
            }
        }

        private void RemoveDesigner(Designer designer)
        {
            if (this._designers.Exists(d => d == designer))
            {
                designer.DesignerHost.RemoveService(typeof(IToolboxService));
                this._designers.Remove(designer);
                if (this._defaultDesigner == designer)
                {
                    if (this._designers.Count > 0)
                    {
                        this._defaultDesigner = this._designers[0];
                    }
                    else
                    {
                        this._defaultDesigner = null;
                    }
                }
            }
        }

        private void OnDragAndDrop(object sender, EventArgs e)
        {
            var selectedItem = this._toolbox.SelectedItem;
            if (selectedItem != null && this._toolbox is Control control)
            {
                var data = (DataObject)this.SerializeToolboxItem(selectedItem);
                control.DoDragDrop(data, DragDropEffects.Copy);
            }
        }

        private void OnDropControl(object sender, EventArgs e)
        {
            var selectedItem = this._toolbox.SelectedItem;
            var designer = this.Designer;
            if (selectedItem != null && designer != null)
            {
                using (var ts = designer.DesignerHost.CreateTransaction(TRANS_CREATE_COMPONENT))
                {
                    var component = selectedItem.CreateComponents(designer.DesignerHost)[0];
                    if (component is Control control)
                    {
                        try
                        {
                            var designerHost = designer.DesignerHost;
                            var rootControl = (Control)designerHost.RootComponent;
                            var componentChangeService = designerHost.GetService<IComponentChangeService>();
                            var componentDesigner = (ComponentDesigner)designerHost.GetDesigner(component);
                            componentDesigner.InitializeNewComponent(new Hashtable());
                            componentChangeService.OnComponentChanging(component, null);

                            control.SuspendLayout();
                            control.Parent = rootControl;
                            control.Location = new Point((rootControl.Width - control.Width) / 2, (rootControl.Height - control.Height) / 2);
                            //control.Text = control.Name;
                            control.ResumeLayout();

                            componentChangeService.OnComponentChanged(component, null, null, null);
                            var selectionService = designerHost.GetService<ISelectionService>();
                            var components = new Control[] { control };
                            selectionService.SetSelectedComponents(components, SelectionTypes.Replace);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"OnDropControl Error: {ex.Message}");
                        }
                    }
                    ts.Commit();
                }
                this._toolbox.SelectedItem = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this._toolbox.BeginDragAndDrop -= this.OnDragAndDrop;
                this._toolbox.DropControl -= this.OnDropControl;
            }
        }

    }
}
