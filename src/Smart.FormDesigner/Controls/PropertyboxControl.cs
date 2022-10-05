using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Smart.FormDesigner.Internal;

namespace Smart.FormDesigner
{
    /// <summary>
    /// 设计时属性控件
    /// </summary>
    public class PropertyboxControl : UserControl
    {
        private PropertyGrid propertyGrid;
        private ComboBox comboBox;

        public bool ShowEventTab { get; set; }

        private Designer _designer;
        public Designer Designer
        {
            get { return _designer; }
            set
            {
                if (this._designer != value)
                {
                    this._designer = value;
                    if (ShowEventTab)
                    {
                        this.propertyGrid.PropertyTabs.AddTabType(typeof(EventsTab));
                    }
                }
            }
        }

        public PropertyboxControl()
        {
            this.InitializeComponent();
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectionService = this._designer.SelectionService;
            if (this.comboBox.SelectedItem != null)
            {
                selectionService.SetSelectedComponents(new object[] {
                    this.comboBox.SelectedItem
                });
            }
        }

        protected virtual void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= this.comboBox.Items.Count)
            {
                return;
            }

            var g = e.Graphics;
            var stringColor = SystemBrushes.ControlText;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                {
                    g.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                    stringColor = SystemBrushes.HighlightText;
                }
                else
                {
                    g.FillRectangle(SystemBrushes.Window, e.Bounds);
                }
            }
            else
            {
                g.FillRectangle(SystemBrushes.Window, e.Bounds);
            }

            object item = this.comboBox.Items[e.Index];
            int xPos = e.Bounds.X;

            if (item is IComponent component)
            {
                string name = string.Empty;
                if (component.Site != null)
                {
                    name = component.Site.Name;
                }
                else if (item is Control control)
                {
                    name = control.Name;
                }
                if (string.IsNullOrEmpty(name))
                {
                    name = item.GetType().Name;
                }
                using (var font = new Font(this.comboBox.Font, FontStyle.Bold))
                {
                    g.DrawString(name, font, stringColor, xPos, e.Bounds.Y);
                    xPos += (int)g.MeasureString($"{name}-", font).Width;
                }
            }

            string typeString = item.GetType().ToString();
            g.DrawString(typeString, this.comboBox.Font, stringColor, xPos, e.Bounds.Y);
        }

        /// <summary>
        /// 设置下拉列表数据源
        /// </summary>
        /// <param name="components"></param>
        public void SetComponents(ComponentCollection components)
        {
            this.comboBox.Items.Clear();
            if (components != null)
            {
                foreach (object obj in components)
                {
                    this.comboBox.Items.Add(obj);
                }
            }
            this.comboBox.SelectedItem = this.propertyGrid.SelectedObject;
        }

        /// <summary>
        /// 设置当前选中控件
        /// </summary>
        /// <param name="selectedObjects"></param>
        public void SetSelectedObjects(params object[] selectedObjects)
        {
            if (selectedObjects?.Length == 0)
            {
                this.propertyGrid.SelectedObject = null;
            }
            else if (selectedObjects.Length == 1)
            {
                var selectedObject = selectedObjects[0];
                //this.PreFilterProperties(selectedObject);
                this.propertyGrid.SelectedObject = selectedObject;
                this.comboBox.SelectedItem = selectedObject;
            }
            else
            {
                this.propertyGrid.SelectedObjects = selectedObjects;
                this.comboBox.SelectedItem = null;
            }
        }

        #region 设计器自动生成的代码

        private void InitializeComponent()
        {
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // comboBox
            // 
            this.comboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Location = new System.Drawing.Point(0, 0);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(288, 20);
            this.comboBox.Sorted = true;
            this.comboBox.TabIndex = 0;
            this.comboBox.DrawItem += this.ComboBox_DrawItem;
            this.comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 20);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(288, 413);
            this.propertyGrid.TabIndex = 1;
            // 
            // PropertyboxControl
            // 
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.comboBox);
            this.Name = "PropertyboxControl";
            this.Size = new System.Drawing.Size(288, 433);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
