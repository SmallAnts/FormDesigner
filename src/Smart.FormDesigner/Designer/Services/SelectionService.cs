using Smart.FormDesigner.Internal;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Smart.FormDesigner.Services
{
    public class SelectionService : AbstractService, ISelectionService
    {
        private IDesignerHost host = null;
        private IComponent removedComponent = null;
        private ArrayList selectedComponents = null;

        public event EventHandler SelectionChanging;
        public event EventHandler SelectionChanged;

        public object PrimarySelection
        {
            get
            {
                if (this.selectedComponents.Count > 0)
                {
                    return this.selectedComponents[0];
                }
                return null;
            }
        }

        public int SelectionCount
        {
            get
            {
                return this.selectedComponents.Count;
            }
        }

        public SelectionService(IDesignerHost host)
        {
            this.host = host;
            this.selectedComponents = new ArrayList();
            if (host.GetService(typeof(IComponentChangeService)) is IComponentChangeService componentChangeService)
            {
                componentChangeService.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
            }
        }

        public ICollection GetSelectedComponents()
        {
            return this.selectedComponents.ToArray();
        }

        public bool GetComponentSelected(object component)
        {
            return this.selectedComponents.Contains(component);
        }

        public void SetSelectedComponents(ICollection components, SelectionTypes selectionType)
        {
            bool ctrlFlag = false;
            bool shiftFlag = false;
            if (this.removedComponent != null && components != null && components.Count == 1)
            {
                var enumerator = components.GetEnumerator();
                enumerator.MoveNext();
                if (enumerator.Current == this.removedComponent)
                {
                    return;
                }
            }
            if (components == null)
            {
                object[] array = new object[1];
                components = array;
            }
            var designerHost = this.host as DesignerHost;
            var arrayList = new ArrayList(this.selectedComponents);
            if ((selectionType & SelectionTypes.Primary) == SelectionTypes.Primary)
            {
                ctrlFlag = ((Control.ModifierKeys & Keys.Control) == Keys.Control);
                shiftFlag = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
            }
            if (selectionType == SelectionTypes.Replace)
            {
                this.selectedComponents.Clear();
                foreach (object current in components)
                {
                    if (current != this.removedComponent)
                    {
                        if (current != null && !this.selectedComponents.Contains(current) && (designerHost == null || designerHost.DesignContainer != null || current != this.host.RootComponent))
                        {
                            this.selectedComponents.Add(current);
                        }
                    }
                }
            }
            else
            {
                if (!ctrlFlag && !shiftFlag && components.Count == 1)
                {
                    foreach (object current in components)
                    {
                        if (!this.selectedComponents.Contains(current))
                        {
                            this.selectedComponents.Clear();
                        }
                    }
                }
                foreach (object current in components)
                {
                    if (current != this.removedComponent)
                    {
                        if (current != null && (designerHost == null || designerHost.DesignContainer != null || current != this.host.RootComponent))
                        {
                            if (ctrlFlag || shiftFlag)
                            {
                                if (this.selectedComponents.Contains(current))
                                {
                                    this.selectedComponents.Remove(current);
                                }
                                else
                                {
                                    this.selectedComponents.Insert(0, current);
                                }
                            }
                            else if (!this.selectedComponents.Contains(current))
                            {
                                this.selectedComponents.Add(current);
                            }
                            else
                            {
                                this.selectedComponents.Remove(current);
                                this.selectedComponents.Insert(0, current);
                            }
                        }
                    }
                }
            }
            bool noChangeFlag = true;
            if (arrayList.Count != this.selectedComponents.Count)
            {
                noChangeFlag = false;
            }
            else
            {
                for (int i = 0; i < arrayList.Count; i++)
                {
                    object obj = arrayList[i];
                    object selectedObj = this.selectedComponents[i];
                    if (!obj.Equals(selectedObj))
                    {
                        noChangeFlag = false;
                        break;
                    }
                }
            }
            if (!noChangeFlag)
            {
                try
                {
                    this.SelectionChanging?.Invoke(this, EventArgs.Empty);
                    this.SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception)
                {
                }
            }
        }

        public void SetSelectedComponents(ICollection components)
        {
            this.SetSelectedComponents(components, SelectionTypes.Replace);
        }

        protected void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            this.removedComponent = e.Component;
        }

        public void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            this.removedComponent = null;
            if (this.selectedComponents.Contains(e.Component))
            {
                this.SelectionChanging?.Invoke(this, e);
                this.selectedComponents.Remove(e.Component);
                this.SelectionChanged?.Invoke(this, e);
            }
        }

    }

}
