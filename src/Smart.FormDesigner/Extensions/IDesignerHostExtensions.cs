using System;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Smart.FormDesigner
{
    public static class DesignerHostExtensions
    {
        public static void SelectNextControl(this IDesignerHost designerHost, bool next)
        {
            if (designerHost.RootComponent is Control rootControl)
            {
                if (designerHost.GetService(typeof(ISelectionService)) is ISelectionService selectionService)
                {
                    var control = selectionService.PrimarySelection as Control;
                    if (control == null)
                    {
                        if (next)
                        {
                            control = rootControl.Controls.First();
                        }
                        else
                        {
                            control = rootControl;
                            while (control.Controls.Count != 0)
                            {
                                control = control.Controls.Last();
                                if (control is ContainerControl)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag = false;
                        var parent = control.Parent;
                        if (parent == null || (control == rootControl && control.Controls.Count == 0))
                        {
                            return;
                        }
                        if (!next)
                        {
                            if (control == rootControl)
                            {
                                control = control.Controls.Last();
                            }
                            else if (control.TabIndex == 0 && parent != rootControl)
                            {
                                control = parent;
                                parent = parent.Parent;
                            }
                            else
                            {
                                control = control.Previous();
                                while (control != null && control.Controls.Count != 0 && !(control is ContainerControl) && !(control is DataGridView))
                                {
                                    control = control.Controls.Last();
                                }
                            }
                        }
                        else if (control.Controls.Count != 0 && ((!(control is ContainerControl) && !(control is DataGridView)) || control == rootControl))
                        {
                            control = control.Controls.First();
                        }
                        else
                        {
                            while (control == parent.Controls.Last() && parent != rootControl)
                            {
                                control = parent;
                                parent = parent.Parent;
                                flag = true;
                            }
                            if (control.Controls.Count == 0 || control is ContainerControl || control is DataGridView || flag)
                            {
                                control = control.Next();
                            }
                        }
                    }
                    if (control == null)
                    {
                        control = rootControl;
                    }
                    var components = new Control[]
                    {
                        control
                    };
                    selectionService.SetSelectedComponents(components, SelectionTypes.Replace);
                }
            }
        }

        public static void Layout(this IDesignerHost designerHost, string transDesc, Action<Control> setAction)
        {
            if (designerHost.GetService(typeof(ISelectionService)) is ISelectionService selectionService)
            {
                var selectedComponents = selectionService.GetSelectedComponents();
                if (selectedComponents.Count > 0)
                {
                    var rootControl = (Control)designerHost.RootComponent;
                    if (rootControl != null)
                    {
                        rootControl.SuspendLayout();
                    }
                    using (var ts = designerHost.CreateTransaction(transDesc))
                    {
                        var primaryControl = selectionService.PrimarySelection as Control;
                        if (primaryControl != null)
                        {
                            var componentChangeService = designerHost.GetService<IComponentChangeService>();
                            foreach (object current in selectedComponents)
                            {
                                var control = current as Control;
                                if (control != null && control.Parent == primaryControl.Parent)
                                {
                                    componentChangeService.OnComponentChanging(control, null);
                                    setAction(control);
                                    componentChangeService.OnComponentChanged(control, null, null, null);
                                }
                            }
                        }
                        ts.Commit();
                    }
                    if (rootControl != null)
                    {
                        rootControl.ResumeLayout();
                    }
                }
            }
        }

    }
}
