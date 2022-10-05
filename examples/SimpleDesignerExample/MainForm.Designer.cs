namespace SimpleDesignerExample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.propertyBox = new Smart.FormDesigner.PropertyboxControl();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolbox1 = new SimpleDesignerExample.Toolbox();
            this.toolStripFormat = new System.Windows.Forms.ToolStrip();
            this.tbNewForm = new System.Windows.Forms.ToolStripButton();
            this.tbOpenForm = new System.Windows.Forms.ToolStripButton();
            this.tbSaveForm = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tbPreview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbUndo = new System.Windows.Forms.ToolStripButton();
            this.tbRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tbAlignLeft = new System.Windows.Forms.ToolStripButton();
            this.tbAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.tbAlignRight = new System.Windows.Forms.ToolStripButton();
            this.tbAlignTop = new System.Windows.Forms.ToolStripButton();
            this.tbAlignMiddle = new System.Windows.Forms.ToolStripButton();
            this.tbAlignBottom = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tbSameWidth = new System.Windows.Forms.ToolStripButton();
            this.tbSameHeight = new System.Windows.Forms.ToolStripButton();
            this.tbSameBoth = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.LeftToolStripPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStripFormat.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitter1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(769, 403);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // toolStripContainer1.LeftToolStripPanel
            // 
            this.toolStripContainer1.LeftToolStripPanel.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripContainer1.LeftToolStripPanel.Controls.Add(this.toolbox1);
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(800, 428);
            this.toolStripContainer1.TabIndex = 9;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStripFormat);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyBox);
            this.splitContainer1.Size = new System.Drawing.Size(766, 403);
            this.splitContainer1.SplitterDistance = 533;
            this.splitContainer1.TabIndex = 13;
            // 
            // propertyBox
            // 
            this.propertyBox.Designer = null;
            this.propertyBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyBox.Location = new System.Drawing.Point(0, 0);
            this.propertyBox.Name = "propertyBox";
            this.propertyBox.ShowEventTab = false;
            this.propertyBox.Size = new System.Drawing.Size(229, 403);
            this.propertyBox.TabIndex = 2;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 403);
            this.splitter1.TabIndex = 12;
            this.splitter1.TabStop = false;
            // 
            // toolbox1
            // 
            this.toolbox1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolbox1.Location = new System.Drawing.Point(0, 3);
            this.toolbox1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.toolbox1.Name = "toolbox1";
            this.toolbox1.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.toolbox1.Size = new System.Drawing.Size(31, 126);
            this.toolbox1.TabIndex = 0;
            // 
            // toolStripFormat
            // 
            this.toolStripFormat.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripFormat.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbNewForm,
            this.tbOpenForm,
            this.tbSaveForm,
            this.toolStripSeparator3,
            this.tbPreview,
            this.toolStripSeparator2,
            this.tbUndo,
            this.tbRedo,
            this.toolStripSeparator1,
            this.tbDelete,
            this.toolStripSeparator5,
            this.tbAlignLeft,
            this.tbAlignCenter,
            this.tbAlignRight,
            this.tbAlignTop,
            this.tbAlignMiddle,
            this.tbAlignBottom,
            this.toolStripSeparator4,
            this.tbSameWidth,
            this.tbSameHeight,
            this.tbSameBoth});
            this.toolStripFormat.Location = new System.Drawing.Point(3, 0);
            this.toolStripFormat.Name = "toolStripFormat";
            this.toolStripFormat.Size = new System.Drawing.Size(387, 25);
            this.toolStripFormat.TabIndex = 9;
            this.toolStripFormat.Text = "toolStrip1";
            // 
            // tbNewForm
            // 
            this.tbNewForm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbNewForm.Image = global::SimpleDesignerExample.Properties.Resources.new_from_16x;
            this.tbNewForm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbNewForm.Name = "tbNewForm";
            this.tbNewForm.Size = new System.Drawing.Size(23, 22);
            this.tbNewForm.Text = "新建表单";
            this.tbNewForm.ToolTipText = "新建表单 (Ctrl + Ｎ)";
            this.tbNewForm.Click += new System.EventHandler(this.tbNewForm_Click);
            // 
            // tbOpenForm
            // 
            this.tbOpenForm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbOpenForm.Image = global::SimpleDesignerExample.Properties.Resources.open_file_16x;
            this.tbOpenForm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbOpenForm.Name = "tbOpenForm";
            this.tbOpenForm.Size = new System.Drawing.Size(23, 22);
            this.tbOpenForm.Text = "打开文件";
            this.tbOpenForm.ToolTipText = "打开文件 (Ctrl + O)";
            this.tbOpenForm.Click += new System.EventHandler(this.tbOpenForm_Click);
            // 
            // tbSaveForm
            // 
            this.tbSaveForm.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbSaveForm.Enabled = false;
            this.tbSaveForm.Image = global::SimpleDesignerExample.Properties.Resources.save_16x;
            this.tbSaveForm.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbSaveForm.Name = "tbSaveForm";
            this.tbSaveForm.Size = new System.Drawing.Size(23, 22);
            this.tbSaveForm.Text = "保存表单";
            this.tbSaveForm.ToolTipText = "保存表单 (Ctrl + S)";
            this.tbSaveForm.Click += new System.EventHandler(this.tbSaveForm_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tbPreview
            // 
            this.tbPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbPreview.Enabled = false;
            this.tbPreview.Image = global::SimpleDesignerExample.Properties.Resources.preview_16x;
            this.tbPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbPreview.Name = "tbPreview";
            this.tbPreview.Size = new System.Drawing.Size(23, 22);
            this.tbPreview.Text = "预览";
            this.tbPreview.ToolTipText = "预览 (F5)";
            this.tbPreview.Visible = false;
            this.tbPreview.Click += new System.EventHandler(this.tbPreview_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tbUndo
            // 
            this.tbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbUndo.Enabled = false;
            this.tbUndo.Image = global::SimpleDesignerExample.Properties.Resources.undo_16x;
            this.tbUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbUndo.Name = "tbUndo";
            this.tbUndo.Size = new System.Drawing.Size(23, 22);
            this.tbUndo.Text = "撤销（Ctrl+Z）";
            this.tbUndo.Click += new System.EventHandler(this.tbUndo_Click);
            // 
            // tbRedo
            // 
            this.tbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbRedo.Enabled = false;
            this.tbRedo.Image = global::SimpleDesignerExample.Properties.Resources.redo_16x;
            this.tbRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbRedo.Name = "tbRedo";
            this.tbRedo.Size = new System.Drawing.Size(23, 22);
            this.tbRedo.Text = "重做（Ctrl+Y）";
            this.tbRedo.Click += new System.EventHandler(this.tbRedo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tbDelete
            // 
            this.tbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbDelete.Enabled = false;
            this.tbDelete.Image = global::SimpleDesignerExample.Properties.Resources.delete_16x;
            this.tbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbDelete.Name = "tbDelete";
            this.tbDelete.Size = new System.Drawing.Size(23, 22);
            this.tbDelete.Text = "删除选中项";
            this.tbDelete.Click += new System.EventHandler(this.tbDelete_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tbAlignLeft
            // 
            this.tbAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbAlignLeft.Enabled = false;
            this.tbAlignLeft.Image = global::SimpleDesignerExample.Properties.Resources.align_left_16x;
            this.tbAlignLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbAlignLeft.Name = "tbAlignLeft";
            this.tbAlignLeft.Size = new System.Drawing.Size(23, 22);
            this.tbAlignLeft.Text = "左对齐";
            this.tbAlignLeft.Click += new System.EventHandler(this.tbAlignLeft_Click);
            // 
            // tbAlignCenter
            // 
            this.tbAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbAlignCenter.Enabled = false;
            this.tbAlignCenter.Image = global::SimpleDesignerExample.Properties.Resources.align_center_16x;
            this.tbAlignCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbAlignCenter.Name = "tbAlignCenter";
            this.tbAlignCenter.Size = new System.Drawing.Size(23, 22);
            this.tbAlignCenter.Text = "居中对齐";
            this.tbAlignCenter.Click += new System.EventHandler(this.tbAlignCenter_Click);
            // 
            // tbAlignRight
            // 
            this.tbAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbAlignRight.Enabled = false;
            this.tbAlignRight.Image = global::SimpleDesignerExample.Properties.Resources.align_right_16x;
            this.tbAlignRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbAlignRight.Name = "tbAlignRight";
            this.tbAlignRight.Size = new System.Drawing.Size(23, 22);
            this.tbAlignRight.Text = "右对齐";
            this.tbAlignRight.Click += new System.EventHandler(this.tbAlignRight_Click);
            // 
            // tbAlignTop
            // 
            this.tbAlignTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbAlignTop.Enabled = false;
            this.tbAlignTop.Image = global::SimpleDesignerExample.Properties.Resources.align_top_16x;
            this.tbAlignTop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbAlignTop.Name = "tbAlignTop";
            this.tbAlignTop.Size = new System.Drawing.Size(23, 22);
            this.tbAlignTop.Text = "顶端对齐";
            this.tbAlignTop.Click += new System.EventHandler(this.tbAlignTop_Click);
            // 
            // tbAlignMiddle
            // 
            this.tbAlignMiddle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbAlignMiddle.Enabled = false;
            this.tbAlignMiddle.Image = global::SimpleDesignerExample.Properties.Resources.align_middlle_16x;
            this.tbAlignMiddle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbAlignMiddle.Name = "tbAlignMiddle";
            this.tbAlignMiddle.Size = new System.Drawing.Size(23, 22);
            this.tbAlignMiddle.Text = "中间对齐";
            this.tbAlignMiddle.Click += new System.EventHandler(this.tbAlignMiddle_Click);
            // 
            // tbAlignBottom
            // 
            this.tbAlignBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbAlignBottom.Enabled = false;
            this.tbAlignBottom.Image = global::SimpleDesignerExample.Properties.Resources.align_bottom_16x;
            this.tbAlignBottom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbAlignBottom.Name = "tbAlignBottom";
            this.tbAlignBottom.Size = new System.Drawing.Size(23, 22);
            this.tbAlignBottom.Text = "底端对齐";
            this.tbAlignBottom.Click += new System.EventHandler(this.tbAlignBottom_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // tbSameWidth
            // 
            this.tbSameWidth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbSameWidth.Enabled = false;
            this.tbSameWidth.Image = global::SimpleDesignerExample.Properties.Resources.same_width_16x;
            this.tbSameWidth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbSameWidth.Name = "tbSameWidth";
            this.tbSameWidth.Size = new System.Drawing.Size(23, 22);
            this.tbSameWidth.Text = "使宽度相同";
            this.tbSameWidth.Click += new System.EventHandler(this.tbSameWidth_Click);
            // 
            // tbSameHeight
            // 
            this.tbSameHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbSameHeight.Enabled = false;
            this.tbSameHeight.Image = global::SimpleDesignerExample.Properties.Resources.same_height_16x;
            this.tbSameHeight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbSameHeight.Name = "tbSameHeight";
            this.tbSameHeight.Size = new System.Drawing.Size(23, 22);
            this.tbSameHeight.Text = "使高度相同";
            this.tbSameHeight.Click += new System.EventHandler(this.tbSameHeight_Click);
            // 
            // tbSameBoth
            // 
            this.tbSameBoth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbSameBoth.Enabled = false;
            this.tbSameBoth.Image = global::SimpleDesignerExample.Properties.Resources.same_size_16x;
            this.tbSameBoth.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbSameBoth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbSameBoth.Name = "tbSameBoth";
            this.tbSameBoth.Size = new System.Drawing.Size(23, 22);
            this.tbSameBoth.Text = "使大小相同";
            this.tbSameBoth.Click += new System.EventHandler(this.tbSameBoth_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainForm";
            this.Text = "表单设置器";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.LeftToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.LeftToolStripPanel.PerformLayout();
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStripFormat.ResumeLayout(false);
            this.toolStripFormat.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStripFormat;
        private System.Windows.Forms.ToolStripButton tbNewForm;
        private System.Windows.Forms.ToolStripButton tbOpenForm;
        private System.Windows.Forms.ToolStripButton tbSaveForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tbPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tbUndo;
        private System.Windows.Forms.ToolStripButton tbRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbDelete;
        private System.Windows.Forms.ToolStripButton tbAlignLeft;
        private System.Windows.Forms.ToolStripButton tbAlignCenter;
        private System.Windows.Forms.ToolStripButton tbAlignRight;
        private System.Windows.Forms.ToolStripButton tbAlignTop;
        private System.Windows.Forms.ToolStripButton tbAlignMiddle;
        private System.Windows.Forms.ToolStripButton tbAlignBottom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tbSameWidth;
        private System.Windows.Forms.ToolStripButton tbSameHeight;
        private System.Windows.Forms.ToolStripButton tbSameBoth;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Smart.FormDesigner.PropertyboxControl propertyBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private Toolbox toolbox1;
    }
}