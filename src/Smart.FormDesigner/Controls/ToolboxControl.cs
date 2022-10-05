using Smart.FormDesigner.Properties;
using Smart.FormDesigner.Toolbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Smart.FormDesigner
{
    /// <summary>
    /// 工具箱
    /// </summary>
    public class ToolboxControl : UserControl, IToolbox
    {
        private ToolboxService _toolboxService;
        private ToolboxListControl _listbox;
        private ImageList _images;
        private Dictionary<string, List<ToolboxItemWithImage>> _toolboxItems;

        protected enum PictureIndex
        {
            Plus = 0,
            Minus = 1,
            Arrow = 2
        }

        public string DefaultCategoryText { get; set; } = "常规";
        public string PointerItemText { get; set; } = "指针";

        public ToolboxControl()
        {
            InitializeComponent();

            _images.Images.Add(Resources.normal_16x, Color.Transparent);
            _images.Images.Add(Resources.launch_16x, Color.Transparent);
            _images.Images.Add(Resources.arrow_cursor_16px, Color.Transparent);

            _toolboxItems = new Dictionary<string, List<ToolboxItemWithImage>>();

            _listbox.Click += OnItemClick;
            _listbox.DoubleClick += OnItemDoubleClick;
            _listbox.ItemDrag += OnItemDrag;

            _toolboxService = new ToolboxService(this);
        }

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

        #region IToolbox 接口成员

        public event EventHandler BeginDragAndDrop;
        public event EventHandler<ToolboxItemUsedArgs> ToolboxItemUsed;
        public event EventHandler DropControl;

        public ToolboxCategoryCollection Items
        {
            get
            {
                var tbItems = new ToolboxCategoryItem[_toolboxItems.Count];

                int i = 0;
                foreach (var de in _toolboxItems)
                {
                    var categoryItems = de.Value;
                    var ic = new System.Drawing.Design.ToolboxItem[categoryItems.Count];

                    int idx = 0;
                    foreach (ToolboxItemWithImage tiwi in categoryItems)
                        ic[idx++] = tiwi.Item;

                    tbItems[i++] = new ToolboxCategoryItem((string)de.Key, new System.Drawing.Design.ToolboxItemCollection(ic));
                }

                return new ToolboxCategoryCollection(tbItems);
            }
        }

        [Browsable(false)]
        public string SelectedCategory
        {
            get
            {
                int index = _listbox.SelectedIndex;
                if (index < 0)
                    return null;

                ToolboxBaseItem item;
                do
                    item = (ToolboxBaseItem)_listbox.Items[index--];
                while (item.IsGroup == false && index >= 0);

                return (item.IsGroup) ? item.Text : null;
            }
            set
            {
                int index = FindCategoryItem(value);

                if (index < 0)
                    return;

                ToolboxBaseItem item = (ToolboxBaseItem)_listbox.Items[index];
                if ((ToolboxCategoryState)item.Tag == ToolboxCategoryState.Collapsed)
                    this.ExpandCategory(index);

                _listbox.SelectedIndex = index + 1;
            }
        }

        [Browsable(false)]
        public System.Drawing.Design.ToolboxItem SelectedItem
        {
            get
            {
                var item = _listbox.SelectedItem;
                return item?.Tag as System.Drawing.Design.ToolboxItem;
            }
            set
            {
                if (value == null)
                {
                    this._listbox.SelectedIndex = -1;
                    this.ToolboxItemUsed?.Invoke(this, new ToolboxItemUsedArgs(this.SelectedItem));
                }
                else
                {
                    foreach (ToolboxBaseItem item in _listbox.Items)
                    {
                        if (item.Tag == value)
                        {
                            _listbox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public void AddCategory(string text)
        {
            var group = new ToolboxBaseItem(text, (int)PictureIndex.Minus, isGroup: true);
            var pointer = new ToolboxPointerItem(PointerItemText);

            _listbox.Items.AddRange(new ToolboxBaseItem[] { group, pointer });
            _toolboxItems[text] = new List<ToolboxItemWithImage>();
        }

        public void AddItem(System.Drawing.Design.ToolboxItem item, string category)
        {
            if (category == null)
                category = DefaultCategoryText;

            if (_toolboxItems.ContainsKey(category) == false)
                AddCategory(category);

            var categoryItems = _toolboxItems[category];
            int imageIndex = _images.Images.Add(item.Bitmap, item.Bitmap.GetPixel(0, 0));

            categoryItems.Add(new ToolboxItemWithImage(item, imageIndex));

            int categoryIndex = FindCategoryItem(category);
            var newItem = new ToolboxBaseItem(item.DisplayName, imageIndex, item);
            _listbox.Items.Insert(categoryIndex + 1 + categoryItems.Count, newItem);
        }

        public void RemoveItem(System.Drawing.Design.ToolboxItem item, string category)
        {
            if (category == null)
                category = DefaultCategoryText;

            if (_toolboxItems.ContainsKey(category) == false)
                return;

            var categoryItems = _toolboxItems[category];
            int index = 0;
            foreach (ToolboxItemWithImage tbi in categoryItems)
            {
                if (tbi.Item == item)
                {
                    categoryItems.RemoveAt(index);

                    int categoryIndex = FindCategoryItem(category);
                    _listbox.Items.RemoveAt(categoryIndex + 2 + index);

                    break;
                }
                index++;
            }
        }

        #endregion


        private int FindCategoryItem(string category)
        {
            int i = 0;
            foreach (ToolboxBaseItem item in _listbox.Items)
            {
                if (item.IsGroup && item.Text == category)
                    return i;
                i++;
            }
            return -1;
        }
        private void CollapseCategory(int categoryItem)
        {
            var item = (ToolboxBaseItem)_listbox.Items[categoryItem];

            if (item.IsGroup == false || (ToolboxCategoryState)(item.Tag) == ToolboxCategoryState.Collapsed)
                return;

            item.Tag = ToolboxCategoryState.Collapsed;

            _listbox.BeginUpdate();

            item.ImageIndex = (int)PictureIndex.Plus;
            _listbox.Invalidate(_listbox.GetItemRectangle(categoryItem));

            var categoryItems = _toolboxItems[item.Text];

            categoryItem++;
            _listbox.Items.RemoveAt(categoryItem); // remove Pointer
            int i = 0;
            while (i < categoryItems.Count)
            {
                _listbox.Items.RemoveAt(categoryItem);
                i++;
            }

            _listbox.EndUpdate();
        }
        private void ExpandCategory(int categoryItem)
        {
            var item = (ToolboxBaseItem)_listbox.Items[categoryItem];

            if (item.IsGroup == false || (ToolboxCategoryState)(item.Tag) == ToolboxCategoryState.Expanded)
                return;

            item.Tag = ToolboxCategoryState.Expanded;

            _listbox.BeginUpdate();

            item.ImageIndex = (int)PictureIndex.Minus;
            _listbox.Invalidate(_listbox.GetItemRectangle(categoryItem));

            var categoryItems = _toolboxItems[item.Text];

            _listbox.Items.Insert(++categoryItem, new ToolboxPointerItem(PointerItemText));

            int i = 0;
            while (i < categoryItems.Count)
            {
                var tbItem = (ToolboxItemWithImage)categoryItems[i];
                var newItem = new ToolboxBaseItem(tbItem.Item.DisplayName, tbItem.ImageIndex, tbItem.Item);
                _listbox.Items.Insert(++categoryItem, newItem);

                i++;
            }

            _listbox.EndUpdate();
        }

        private void OnItemDrag(object sender, ToolboxItemDragEventArgs arg)
        {
            if (arg.Item.IsGroup == true)
                return;

            if (arg.Item.Tag is System.Drawing.Design.ToolboxItem && BeginDragAndDrop != null)
                BeginDragAndDrop(this, null);
        }
        private void OnItemDoubleClick(object sender, EventArgs e)
        {
            var tb = _listbox.SelectedItem;

            if (tb.IsGroup == true)
                return;

            if (tb.Tag is System.Drawing.Design.ToolboxItem && DropControl != null)
                DropControl(this, EventArgs.Empty);
        }
        private void OnItemClick(object sender, EventArgs e)
        {
            int selected = _listbox.SelectedIndex;
            if (selected < 0)
                return;

            var selectedItem = (ToolboxBaseItem)_listbox.Items[selected];
            if (selectedItem.IsGroup)
            {
                if ((ToolboxCategoryState)selectedItem.Tag == ToolboxCategoryState.Expanded)
                    CollapseCategory(selected);
                else
                    ExpandCategory(selected);
            }
        }

        internal System.Drawing.Design.ToolboxItem CreateToolboxItem(Type type)
        {
            try
            {
                if (!(TypeDescriptor.GetAttributes(type)[typeof(ToolboxItemAttribute)] is ToolboxItemAttribute tia))
                    return new System.Drawing.Design.ToolboxItem(type);

                var ci = tia.ToolboxItemType.GetConstructor(new Type[] { });
                if (ci == null)
                    return new System.Drawing.Design.ToolboxItem(type);

                var tbi = (System.Drawing.Design.ToolboxItem)ci.Invoke(new object[] { });
                tbi.Initialize(type);
                return tbi;
            }
            catch (Exception)
            {
                //throw ex;
                return new System.Drawing.Design.ToolboxItem(type);
            }

        }

        public void AddToolboxItem(Type componentType, string category)
        {
            _toolboxService.AddToolboxItem(CreateToolboxItem(componentType), category);
        }
        public void AddToolboxItems(string category, IEnumerable<Type> componentTypes)
        {
            foreach (var componentType in componentTypes)
            {
                _toolboxService.AddToolboxItem(CreateToolboxItem(componentType), category);
            }
        }


  

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._listbox = new Smart.FormDesigner.Toolbox.ToolboxListControl();
            this._images = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // _listbox
            // 
            this._listbox.BackColor = System.Drawing.SystemColors.Control;
            this._listbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._listbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listbox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this._listbox.GroupBackColor = System.Drawing.SystemColors.Window;
            this._listbox.Images = this._images;
            this._listbox.ItemHeight = 24;
            this._listbox.ItemHoverBackColor = System.Drawing.SystemColors.ControlLight;
            this._listbox.Location = new System.Drawing.Point(0, 0);
            this._listbox.Name = "_listbox";
            this._listbox.SelectedItemBackColor = System.Drawing.SystemColors.ActiveCaption;
            this._listbox.SelectedItemBorderColor = System.Drawing.SystemColors.WindowFrame;
            this._listbox.SelectedItemHoverBackColor = System.Drawing.SystemColors.InactiveCaption;
            this._listbox.Size = new System.Drawing.Size(149, 256);
            this._listbox.TabIndex = 0;
            // 
            // _images
            // 
            this._images.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this._images.ImageSize = new System.Drawing.Size(16, 16);
            this._images.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ToolboxControl
            // 
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.Controls.Add(this._listbox);
            this.Name = "ToolboxControl";
            this.Size = new System.Drawing.Size(149, 256);
            this.ResumeLayout(false);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_toolboxService != null)
                    _toolboxService.Dispose();
            }
            base.Dispose(disposing);
        }

        private IContainer components;

    }

    internal class ToolboxItemWithImage
    {
        public System.Drawing.Design.ToolboxItem Item { get; }

        public int ImageIndex { get; }

        public ToolboxItemWithImage(System.Drawing.Design.ToolboxItem item, int image)
        {
            this.Item = item;
            this.ImageIndex = image;
        }
    }

}

