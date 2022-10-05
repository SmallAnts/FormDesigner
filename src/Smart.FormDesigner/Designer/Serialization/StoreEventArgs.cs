using System.ComponentModel;
using System.ComponentModel.Design;

namespace Smart.FormDesigner.Serialization
{
    public class StoreEventArgs : ComponentEventArgs
    {
        public StoreEventArgs(IComponent component) : base(component)
        {
        }

        public bool Cancel { get; set; }
    }
}
