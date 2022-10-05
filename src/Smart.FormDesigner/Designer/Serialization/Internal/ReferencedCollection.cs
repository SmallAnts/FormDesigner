using System.Collections.ObjectModel;

namespace Smart.FormDesigner.Serialization
{
    internal class ReferencedCollection : Collection<ReferencedItem>
    {
        internal ReferencedCollection()
        {
        }

        public void Add(string key, ComponentProperty property)
        {
            foreach (ReferencedItem referencedItem in this)
            {
                if (referencedItem.Key == key)
                {
                    referencedItem.Properties.Add(property);
                    return;
                }
            }
            this.Add(new ReferencedItem()
            {
                Key = key,
                Properties = { property }
            });
        }
    }
}
