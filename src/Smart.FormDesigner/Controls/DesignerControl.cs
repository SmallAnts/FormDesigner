using System;
using System.Windows.Forms;
using Smart.FormDesigner.Serialization;

namespace Smart.FormDesigner
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DesignerControl : UserControl
    {
        public Control DesignedForm
        {
            get { return this.Designer.DesignedForm; }
            private set { this.Designer.DesignedForm = value; }
        }

        public IDesignerLoader DesignerLoader { get => this.Designer?.DesignerLoader; }

        public Designer Designer { get => this.designer; }

        #region 构造函数

        public DesignerControl()
        {
            InitializeComponent();
        }

        public DesignerControl(Control root) : this()
        {
            this.DesignedForm = root;
        }

        public DesignerControl(Control root, string layoutXml) : this()
        {
            this.DesignedForm = root;
            this.Designer.LayoutXML = layoutXml;
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;

            this.Designer.DesignContainer = this;
            this.Dock = DockStyle.Fill;
            if (this.DesignedForm != null)
            {
                this.Designer.Active = true;
            }
        }

        #region 设计器自动生成的代码

        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.defaultDesignerLoader = new Smart.FormDesigner.Serialization.DefaultDesignerLoader();
            this.designer = new Smart.FormDesigner.Designer();
            this.SuspendLayout();
            // 
            // designer
            // 
            this.designer.DesignerLoader = this.defaultDesignerLoader;
            this.designer.GridSize = new System.Drawing.Size(8, 8);
            // 
            // DesignerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "DesignerControl";
            this.Size = new System.Drawing.Size(339, 374);
            this.ResumeLayout(false);

        }

        private DefaultDesignerLoader defaultDesignerLoader;
        private Designer designer;

        #endregion
    }
}
