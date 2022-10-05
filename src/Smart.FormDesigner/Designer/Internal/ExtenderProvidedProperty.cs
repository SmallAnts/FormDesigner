using System.ComponentModel;
using System.Windows.Forms;

namespace Smart.FormDesigner.Internal
{
    internal class ExtenderProvidedProperty
    {
        public ExtenderProvidedProperty(string name, ExtenderProvidedPropertyAttribute attribute, Control source, Control target)
        {
            this.ProperyName = name;
            this.Attribute = attribute;
            this.Source = source;
            this.Target = target;
        }

        public string ProperyName { get; }
        public ExtenderProvidedPropertyAttribute Attribute { get; }
        public Control Source { get; }
        public Control Target { get; }

        public void Invoke()
        {
            object provider = Attribute.Provider;
            var method = provider.GetType().GetMethod($"Set{ProperyName}");
            if (method != null && this.Target != this.Source)
            {
                var array = new object[2];
                array[0] = this.Source;
                method.Invoke(provider, array);
            }
        }

    }
}
