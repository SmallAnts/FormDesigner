using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Smart.FormDesigner;

namespace SimpleDesignerExample
{
    public partial class MainForm : Form
    {
        private DesignerControl designerControl;
        private Designer designer => designerControl?.Designer;

        public MainForm()
        {
            InitializeComponent();
        }



        private void Designed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                this.designer.DeleteSelected();
            else if (e.Control == true && e.KeyCode == Keys.A)
                this.designer.SelectAll();
            else if (e.Control == true && e.KeyCode == Keys.C)
                this.designer.CopyControls();
            else if (e.Control == true && e.KeyCode == Keys.V)
                this.designer.PasteControls();
            else if (e.Control == true && e.KeyCode == Keys.Z)
                this.designer.Undo();
            else if (e.Control == true && e.KeyCode == Keys.Y)
                this.designer.Redo();
        }


        private void SelectionChanged(object sender, EventArgs e)
        {
            var selectionService = (ISelectionService)sender;
            int selectionCount = selectionService.SelectionCount;

            // 更新按钮状态
            EnableAlignResize(selectionCount > 1);
            if (selectionCount >= 1)
            {
                //this.tbDeleteSelection.Enabled = true;
                //this.tbCopy.Enabled = true;
                this.tbDelete.Enabled = true;
            }
            else
            {
                //this.tbDeleteSelection.Enabled = false;
                //this.tbCopy.Enabled = false;
                this.tbDelete.Enabled = false;
            }

            // /更新属性框
            this.propertyBox.Designer = this.designerControl.Designer;
            if (selectionCount == 0)
            {
                this.propertyBox.SetSelectedObjects(this.designerControl.DesignedForm);
            }
            else
            {
                var selected = new object[selectionCount];
                selectionService.GetSelectedComponents().CopyTo(selected, 0);
                this.propertyBox.SetSelectedObjects(selected);
            }
        }
        private void ComponentAdded(object sender, ComponentEventArgs e)
        {
            this.propertyBox.SetComponents(this.designer.DesignerHost.Container.Components);

            EnableUndoRedo();
        }
        private void ComponentRemoved(object sender, ComponentEventArgs e)
        {
            this.propertyBox.SetComponents(this.designer.DesignerHost.Container.Components);

            EnableUndoRedo();
        }
        private void ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            EnableUndoRedo();
        }

        private void EnableAlignResize(bool enable)
        {
            this.tbAlignBottom.Enabled = enable;
            this.tbAlignMiddle.Enabled = enable;
            this.tbAlignTop.Enabled = enable;
            this.tbAlignCenter.Enabled = enable;
            this.tbAlignRight.Enabled = enable;
            this.tbAlignLeft.Enabled = enable;

            this.tbAlignBottom.Enabled = enable;
            this.tbAlignMiddle.Enabled = enable;
            this.tbAlignTop.Enabled = enable;
            this.tbAlignCenter.Enabled = enable;
            this.tbAlignLeft.Enabled = enable;
            this.tbAlignRight.Enabled = enable;

            this.tbSameBoth.Enabled = enable;
            this.tbSameWidth.Enabled = enable;
            this.tbSameHeight.Enabled = enable;

            this.tbSameBoth.Enabled = enable;
            this.tbSameWidth.Enabled = enable;
            this.tbSameHeight.Enabled = enable;
        }

        private void EnableUndoRedo()
        {
            tbUndo.Enabled = (this.designer?.UndoCount > 0);
            tbRedo.Enabled = (this.designer?.RedoCount > 0);

            tbUndo.Enabled = (this.designer?.UndoCount > 0);
            tbRedo.Enabled = (this.designer?.RedoCount > 0);
        }

        private void tbNewForm_Click(object sender, EventArgs e)
        {
            if (designerControl != null)
            {
                designerControl.Dispose();
            }

            designerControl = new DesignerControl(new CustomDesignerControl());
            designerControl.Dock = DockStyle.Fill;
            designerControl.BackColor = Color.WhiteSmoke;
            designerControl.Designer.SelectionService.SelectionChanged += SelectionChanged;
            designerControl.Designer.ComponentChangeService.ComponentAdded += ComponentAdded;
            designerControl.Designer.ComponentChangeService.ComponentRemoved += ComponentRemoved;
            designerControl.Designer.ComponentChangeService.ComponentChanged += ComponentChanged;
            this.splitContainer1.Panel1.Controls.Add(designerControl);
            this.toolbox1.Designer = designerControl.Designer;
            this.designer.KeyDown += Designed_KeyDown;
            tbSaveForm.Enabled = true;

        }

        private void tbOpenForm_Click(object sender, EventArgs e)
        {
            var openFileName = new OpenFileDialog();

            openFileName.Filter = "XML text format (*.xml)|*.xml";
            openFileName.FilterIndex = 1;
            openFileName.RestoreDirectory = true;

            if (openFileName.ShowDialog() == DialogResult.OK)
            {
                tbNewForm_Click(sender, e);

                if (openFileName.FilterIndex == 1)
                {
                    var txtReader = new StreamReader(openFileName.FileName);
                    string layoutString = txtReader.ReadToEnd();
                    txtReader.Close();

                    this.designer.LayoutXML = layoutString;
                }
                else
                {
                    this.designer.LoadFromFile(openFileName.FileName);
                }
                tbSaveForm.Enabled = true;
            }
        }

        private void tbSaveForm_Click(object sender, EventArgs e)
        {
            var saveFileName = new SaveFileDialog();
            saveFileName.Filter = "XML Form (*.xml)|*.xml";
            saveFileName.FilterIndex = 1;
            saveFileName.RestoreDirectory = true;

            if (saveFileName.ShowDialog() == DialogResult.OK)
            {
                string test = this.designer.LayoutXML;

                TextWriter txtWriter = new StreamWriter(saveFileName.FileName);
                txtWriter.Write(test);
                txtWriter.Close();
            }
        }

        private void tbPreview_Click(object sender, EventArgs e)
        {

        }

        private void tbUndo_Click(object sender, EventArgs e)
        {
            this.designer.Undo();
            tbUndo.Enabled = (this.designer.UndoCount != 0);
            tbRedo.Enabled = (this.designer.RedoCount != 0);
        }

        private void tbRedo_Click(object sender, EventArgs e)
        {
            this.designer.Redo();
            tbUndo.Enabled = (this.designer.UndoCount != 0);
            tbRedo.Enabled = (this.designer.RedoCount != 0);
        }

        private void tbDelete_Click(object sender, EventArgs e)
        {
            this.designer.DeleteSelected();
        }

        private void tbAlignLeft_Click(object sender, EventArgs e)
        {
            this.designer.Align(AlignType.Left);
        }

        private void tbAlignCenter_Click(object sender, EventArgs e)
        {
            this.designer.Align(AlignType.Center);
        }

        private void tbAlignRight_Click(object sender, EventArgs e)
        {
            this.designer.Align(AlignType.Right);
        }

        private void tbAlignTop_Click(object sender, EventArgs e)
        {
            this.designer.Align(AlignType.Top);
        }

        private void tbAlignMiddle_Click(object sender, EventArgs e)
        {
            this.designer.Align(AlignType.Middle);
        }

        private void tbAlignBottom_Click(object sender, EventArgs e)
        {
            this.designer.Align(AlignType.Bottom);
        }

        private void tbSameWidth_Click(object sender, EventArgs e)
        {
            this.designer.MakeSameSize(ResizeType.SameWidth);
        }

        private void tbSameHeight_Click(object sender, EventArgs e)
        {
            this.designer.MakeSameSize(ResizeType.SameHeight);
        }

        private void tbSameBoth_Click(object sender, EventArgs e)
        {
            this.designer.MakeSameSize(ResizeType.SameHeight | ResizeType.SameWidth);
        }
    }
}
