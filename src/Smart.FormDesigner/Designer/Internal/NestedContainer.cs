using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;

namespace Smart.FormDesigner.Internal
{
    /// <summary>
    /// 使容器可以拥有一个所属组件。
    /// </summary>
    internal class NestedContainer : Disposable, INestedContainer, IContainer, IDisposable
    {
        private IContainer container;
        public IComponent Owner { get; private set; }

        private Dictionary<IComponent, string> components;
        public ComponentCollection Components
        {
            get
            {
                var array = this.components.Keys.ToArray();
                return new ComponentCollection(array);
            }
        }

        public NestedContainer(IComponent owner, IDesignerHost host)
        {
            this.Owner = owner;
            this.container = host.GetService<IContainer>();
            this.components = new Dictionary<IComponent, string>();
        }

        public void Add(IComponent component, string name)
        {
            this.components.Add(component, name);
            this.container.Add(component, name);
        }

        public void Add(IComponent component)
        {
            this.Add(component, null);
        }

        public void Remove(IComponent component)
        {
            this.components.Remove(component);
            this.container.Remove(component);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.container.Dispose();
            }
        }
    }
}
