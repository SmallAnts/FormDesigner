using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

namespace Smart.FormDesigner
{
    public class AddingVerbEventArgs : EventArgs
    {
        public AddingVerbEventArgs() { }

        public AddingVerbEventArgs(IComponent component, DesignerVerb verb)
        {
            this.Component = component;
            this.Verb = verb;
        }
        public IComponent Component { get; set; }
        public DesignerVerb Verb { get; set; }

        public bool Cancel { get; set; }
    }
}
