using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Smart.FormDesigner.Internal
{
    internal abstract class DesignerAction
    {
        protected string name;
        protected IDesignerHost host;
        protected MegaAction owner;

        public DesignerAction(IDesignerHost host, object control, MegaAction owner)
        {
            this.host = host;
            this.name = this.ComponentName(control as Component);
            this.owner = owner;
        }

        public abstract void Undo();

        public abstract void Redo();

        public virtual void Dispose()
        {
            this.host = null;
        }

        protected string ComponentName(Component control)
        {
            return control?.Site?.Name ?? string.Empty;
        }

        protected void SetProperties(Hashtable props)
        {
            var container = this.host.GetService<IContainer>();
            if (container.Components[this.name] is IComponent component)
            {
                this.owner.LoadProperties(component, props, (DesignerHost)this.host);
            }
        }

    }

}
