using Smart.FormDesigner;
using Smart.FormDesigner.Serialization;

namespace SimpleDesignerExample
{
    partial class CustomDesignerControl
    {
 
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.defaultDesignerLoader1 = new Smart.FormDesigner.Serialization.DefaultDesignerLoader();
            this.designer1 = new Smart.FormDesigner.Designer();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // designer1
            // 
            this.designer1.DesignedForm = this;
            this.designer1.DesignerLoader = this.defaultDesignerLoader1;
            this.designer1.GridSize = new System.Drawing.Size(8, 8);
            // 
            // PrintDesignerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "PrintDesignerControl";
            this.Size = new System.Drawing.Size(420, 267);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DefaultDesignerLoader defaultDesignerLoader1;
        private Designer designer1;
        private System.Windows.Forms.BindingSource bindingSource1;
    }
}