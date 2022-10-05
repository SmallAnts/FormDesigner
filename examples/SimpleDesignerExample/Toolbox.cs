using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using SimpleDesignerExample.Properties;
using Smart.FormDesigner;
using Smart.FormDesigner.Toolbox;

namespace SimpleDesignerExample
{
    internal class Toolbox : ToolStrip, IToolbox
    {
        private ToolboxService _toolboxService;

        public Toolbox()
        {
            this.DoubleBuffered = true;
            this.ItemClicked += Toolbox_ItemClicked;
            this._toolboxService = new ToolboxService(this);
            this.LoadItems();
        }

        private void LoadItems()
        {
            this.Items.Add(new ToolStripButton(Resources.arrow_cursor_16px));
            this._toolboxService.AddToolboxItem(CreateToolboxItem(typeof(Label)));
            this._toolboxService.AddToolboxItem(CreateToolboxItem(typeof(PictureBox)));
            this._toolboxService.AddToolboxItem(CreateToolboxItem(typeof(DataGridView)));
            this._toolboxService.AddToolboxItem(CreateToolboxItem(typeof(BindingSource)));
        }

        private void Toolbox_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag is ToolboxBaseItem baseItem && baseItem.Tag is ToolboxItem item)
            {
                this.SelectedItem = item;
                return;
            }
            this.SelectedItem = null;
        }
        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if (BeginDragAndDrop != null)
                {
                    if (((ToolStripButton)sender).Tag is ToolboxBaseItem item)
                    {
                        this.SelectedItem = item.Tag as ToolboxItem;
                        BeginDragAndDrop.Invoke(this, new ToolboxItemDragEventArgs(item));
                    }
                }
                return;
            }

        }
        private void Button_DoubleClick(object sender, EventArgs e)
        {
            var item = (ToolStripButton)sender;
            if (item.Tag is ToolboxBaseItem && DropControl != null)
                DropControl(this, EventArgs.Empty);
        }
        private ToolboxItem CreateToolboxItem(Type type)
        {
            try
            {
                if (!(TypeDescriptor.GetAttributes(type)[typeof(ToolboxItemAttribute)] is ToolboxItemAttribute attr))
                {
                    return new ToolboxItem(type);
                }

                var ctor = attr.ToolboxItemType.GetConstructor(new Type[] { });
                if (ctor == null)
                {
                    return new ToolboxItem(type);
                }
                var tbi = (ToolboxItem)ctor.Invoke(default);
                tbi.Initialize(type);
                return tbi;
            }
            catch (Exception)
            {
                //throw ex;
                return new ToolboxItem(type);
            }

        }

        [Browsable(false)]
        [DefaultValue(null)]
        public Designer Designer
        {
            set
            {
                _toolboxService.Designer = value;
            }
            get
            {
                return _toolboxService.Designer;
            }
        }

        ToolboxCategoryCollection IToolbox.Items
        {
            get
            {
                var tbItems = new ToolboxCategoryItem[1];
                var items = new List<ToolboxItem>();
                foreach (ToolStripItem item in this.Items)
                {
                    if (item.Tag is ToolboxBaseItem baseItem)
                    {
                        items.Add((ToolboxItem)baseItem.Tag);
                    }
                }
                tbItems[0] = new ToolboxCategoryItem("controls", new ToolboxItemCollection(items.ToArray()));
                return new ToolboxCategoryCollection(tbItems);
            }
        }

        [Browsable(false)]
        [DefaultValue(null)]
        public ToolboxItem SelectedItem { get; set; }

        [Browsable(false)]
        [DefaultValue(null)]
        public string SelectedCategory { get; set; }

        public event EventHandler BeginDragAndDrop;
        public event EventHandler DropControl;

        public void AddItem(ToolboxItem item, string category)
        {
            var button = new ToolStripButton() { Image = item.Bitmap, Tag = new ToolboxBaseItem(item.DisplayName, 0, item), DoubleClickEnabled = true };
            button.DoubleClick += Button_DoubleClick;
            button.MouseMove += Button_MouseMove;
            this.Items.Add(button);
        }

        public void RemoveItem(ToolboxItem item, string category)
        {
            this.Items.RemoveByKey(item.DisplayName);
        }

        public void AddCategory(string text) => throw new NotImplementedException();
    }
}
