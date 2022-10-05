using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Smart.FormDesigner.Serialization
{
    public interface IDesignerLoader
    {
        event ComponentEventHandler ComponentLoaded;

        LoadModes LoadMode { get; set; }

        IDesignerHost DesignerHost { get; set; }
        void Load(Control parent, IReader reader, Dictionary<string, IComponent> components, bool ignoreParent);
        void Store(IComponent[] parents, IWriter writer);
        void SetEventSource(object eventSource);
        void BindEvents(object eventSource);
        void UnbindEvents(object eventSource);
        void RefreshEventData();
    }
}
