using System.Collections;
using System.ComponentModel.Design;

namespace Smart.FormDesigner.Internal
{
    internal class ChangeAction : DesignerAction
    {
        private Hashtable _oldValue;
        private Hashtable _newValue;

        public ChangeAction(IDesignerHost host, object control, Hashtable oldValues, Hashtable newValues, MegaAction owner)
            : base(host, control, owner)
        {
            this._oldValue = new Hashtable();
            this._newValue = new Hashtable();

            foreach (DictionaryEntry item in oldValues)
            {
                object newValue = newValues[item.Key];
                if (newValue != null)
                {
                    if (!newValue.Equals(item.Value))
                    {
                        this._oldValue.Add(item.Key, item.Value);
                        this._newValue.Add(item.Key, newValue);
                    }
                    newValues.Remove(item.Key);
                }
                else
                {
                    this._oldValue.Add(item.Key, item.Value);
                }
            }

            foreach (DictionaryEntry item in newValues)
            {
                this._newValue.Add(item.Key, item.Value);
            }

        }

        public override void Undo()
        {
            var selectionService = this.host.GetService<ISelectionService>();
            selectionService.SetSelectedComponents(null);
            base.SetProperties(this._oldValue);
        }

        public override void Redo()
        {
            var selectionService = this.host.GetService<ISelectionService>();
            selectionService.SetSelectedComponents(null);
            base.SetProperties(this._newValue);
        }
    }

}
