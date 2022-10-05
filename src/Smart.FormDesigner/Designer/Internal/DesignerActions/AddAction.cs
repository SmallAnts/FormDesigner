using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Smart.FormDesigner.Internal
{
    internal class AddAction : DesignerAction
    {
        private Type _type = null;
        private Control _parent = null;

        protected string parentName = string.Empty;
        protected Hashtable properties;

        public AddAction(IDesignerHost host, object component, MegaAction owner)
            : base(host, component, owner)
        {
            this._type = component.GetType();
            if (component is Control control)
            {
                this._parent = control.Parent;
                this.parentName = base.ComponentName(this._parent);
            }
        }

        public override void Undo()
        {
            var container = this.host.GetService<IContainer>();
            IComponent component = container.Components[this.name];
            if (component != null)
            {
                if (this._parent == null && component is Control control)
                {
                    this._parent = control.Parent;
                    this.parentName = base.ComponentName(this._parent);
                }
                this.properties = this.owner.StoreProperties(component, null, null);
                var selectionService = this.host.GetService<ISelectionService>();
                container.Remove(component);
                component.Dispose();
                selectionService.SetSelectedComponents(null);
            }
        }

        public override void Redo()
        {
            var selectionService = this.host.GetService<ISelectionService>();
            selectionService.SetSelectedComponents(null);
            IComponent obj = this.host.CreateComponent(this._type, this.name);
            if (obj is Control control)
            {
                var container = this.host.GetService<IContainer>();
                var parentControl = !this.parentName.IsNullOrEmpty() ? (container.Components[this.parentName] as Control) : null;
                if (parentControl != null && this._parent != parentControl)
                {
                    this._parent = parentControl;
                }
                control.Parent = this._parent;
                control.BringToFront();
            }
            base.SetProperties(this.properties);
        }

    }

}
