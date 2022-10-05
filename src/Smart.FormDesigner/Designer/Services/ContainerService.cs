using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Smart.FormDesigner.Services
{
    public interface IContainerService
    {
        IDictionary<string, IComponent> Components { get; }
        void Add(IComponent component, string name);
        IComponent Get(string name);
        void Remove(string name);
        void Clear();

    }
    public class ContainerService : IContainerService
    {
        private Dictionary<string, IComponent> _components = new Dictionary<string, IComponent>();
        public IDictionary<string, IComponent> Components { get { return this._components; } }

        public IComponent Get(string name)
        {
            this._components.TryGetValue(name, out IComponent component);
            return component;
        }

        public void Add(IComponent component, string name)
        {
            if (this._components.FirstOrDefault(c => c.Value == component).Value != null) return;

            this._components[name] = component;
        }

        public void Remove(string name)
        {
            if (!this._components.ContainsKey(name)) return;

            this._components.Remove(name);
        }

        public void Clear()
        {
            this._components.Clear();
        }
    }
}
