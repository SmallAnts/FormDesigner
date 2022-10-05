using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Smart.FormDesigner.Services
{
    public class DefaultMenuCommandService : AbstractService, IMenuCommandService
    {
        private IDesignerHost host;
        private IDictionary commands;
        private IDictionary globalVerbs;
        private IDictionary menuItemVerb;
        private ContextMenuStrip menu;

        internal event EventHandler<AddingVerbEventArgs> AddingVerb;
        public DesignerVerbCollection Verbs
        {
            get
            {
                var designerVerbCollection = new DesignerVerbCollection();
                var selectionService = this.host.GetService<ISelectionService>();
                var selectedComponents = selectionService.GetSelectedComponents();
                foreach (IComponent component in selectedComponents)
                {
                    var designer = this.host.GetDesigner(component);
                    if (designer?.Verbs != null && AddingVerb != null)
                    {
                        foreach (DesignerVerb verb in designer.Verbs)
                        {
                            var args = new AddingVerbEventArgs(component, verb);
                            AddingVerb(this, args);
                            if (!args.Cancel)
                            {
                                designerVerbCollection.Add(verb);
                            }
                        }
                    }
                }
                foreach (DesignerVerb value in this.globalVerbs.Values)
                {
                    designerVerbCollection.Add(value);
                }
                return designerVerbCollection;
            }
        }

        public DefaultMenuCommandService(IDesignerHost host)
        {
            this.host = host;
            this.commands = new Hashtable();
            this.globalVerbs = new Hashtable();
            this.menuItemVerb = new Hashtable();
            this.menu = new ContextMenuStrip();
        }

        public void AddCommand(MenuCommand command)
        {
            if (command == null)
            {
                throw new NullReferenceException("command");
            }
            if (this.FindCommand(command.CommandID) != null)
            {
                throw new InvalidOperationException("添加的命令已经存在");
            }

            this.commands.Add(command.CommandID, command);
        }

        public void RemoveCommand(MenuCommand command)
        {
            if (command != null)
            {
                this.commands.Remove(command.CommandID);
            }
        }

        public MenuCommand FindCommand(CommandID commandID)
        {
            return (MenuCommand)this.commands[commandID];
        }

        public bool GlobalInvoke(CommandID commandID)
        {
            if (this.globalVerbs[commandID] is DesignerVerb designerVerb)
            {
                designerVerb.Invoke();
                return true;
            }
            else
            {
                if (this.FindCommand(commandID) is MenuCommand menuCommand)
                {
                    menuCommand.Invoke();
                    return true;
                }
            }
            return false;
        }

        public void AddVerb(DesignerVerb verb)
        {
            if (verb == null)
            {
                throw new NullReferenceException("verb");
            }
            this.globalVerbs.Add(verb.CommandID, verb);
        }

        public void RemoveVerb(DesignerVerb verb)
        {
            if (verb == null)
            {
                throw new NullReferenceException("verb");
            }
            this.globalVerbs.Remove(verb.CommandID);
        }

        public void ShowContextMenu(CommandID menuID, int x, int y)
        {
            var verbs = this.Verbs;
            int num = verbs.Count - this.globalVerbs.Values.Count;
            int num2 = 0;
            this.menu.Items.Clear();
            this.menuItemVerb.Clear();
            foreach (DesignerVerb designerVerb in verbs)
            {
                if (designerVerb.Visible)
                {
                    if (num > 0 && num2 == num)
                    {
                        this.menu.Items.Add(new ToolStripMenuItem("-"));
                    }
                    var menuItem = new ToolStripMenuItem(designerVerb.Text);
                    menuItem.Click += new EventHandler(this.MenuItemClickHandler);
                    this.menuItemVerb.Add(menuItem, designerVerb);
                    menuItem.Enabled = designerVerb.Enabled;
                    menuItem.Checked = designerVerb.Checked;
                    this.menu.Items.Add(menuItem);
                    num2++;
                }
            }
            var selectionService = this.host.GetService<ISelectionService>();
            if (!(selectionService.PrimarySelection is Control control))
            {
                control = (Control)this.host.RootComponent;
            }
            var point = control.PointToScreen(new Point(0, 0));
            this.menu.Show(control, new Point(x - point.X, y - point.Y));
        }

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                if (this.menuItemVerb[menuItem] is DesignerVerb designerVerb)
                {
                    designerVerb.Invoke();
                }
            }
        }

    }
}
