using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Smart.FormDesigner
{
    internal static class ControlExtensions
    {
        public static Control FindFirst(this Control control, string name)
        {
            if (control == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            var controls = control.Controls.Find(name, true);
            if (controls.Length > 0)
            {
                return controls[0];
            }
            return null;
        }

        public static bool HaveParentInList(this Control control, List<IComponent> parentsList)
        {
            if (control == null)
            {
                return false;
            }

            for (var parent = control.Parent; parent != null; parent = parent.Parent)
            {
                if (parentsList.Contains(parent))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsVisiable(this Control control)
        {
            bool visible = (bool)typeof(Control).InvokeMember(
                "GetState",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null,
                control,
                new object[] { 2 });
            return visible;
        }

        public static Control[] ToArray(this Control.ControlCollection controls)
        {
            var arrray = new Control[controls.Count];
            controls.CopyTo(arrray, 0);
            return arrray;
        }


        #region Tab 顺序

        public static Control First(this Control.ControlCollection controls)
        {
            if (controls.Count == 0)
            {
                return null;
            }
            int tabIndex = controls[0].TabIndex;
            int index = 0;
            for (int i = 1; i < controls.Count; i++)
            {
                if (controls[i].TabIndex < tabIndex)
                {
                    index = i;
                    tabIndex = controls[i].TabIndex;
                }
            }
            return controls[index];
        }

        public static Control Last(this Control.ControlCollection controls)
        {
            if (controls.Count == 0)
            {
                return null;
            }

            int tabIndex = controls[0].TabIndex;
            int index = 0;
            for (int i = 1; i < controls.Count; i++)
            {
                if (controls[i].TabIndex >= tabIndex)
                {
                    index = i;
                    tabIndex = controls[i].TabIndex;
                }
            }
            return controls[index];
        }

        public static Control Next(this Control.ControlCollection controls, Control current)
        {
            int currentIndex = controls.IndexOf(current);
            int index = -1;
            int currentTabIndex = current.TabIndex;
            int tabIndex = 0;
            bool flag = false;
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].TabIndex >= currentTabIndex)
                {
                    if (controls[i].TabIndex != currentTabIndex || i > currentIndex)
                    {
                        if (controls[i].TabIndex == currentTabIndex)
                        {
                            index = i;
                            break;
                        }
                        if (!flag)
                        {
                            flag = true;
                            tabIndex = controls[i].TabIndex;
                            index = i;
                        }
                        else if (controls[i].TabIndex < tabIndex)
                        {
                            tabIndex = controls[i].TabIndex;
                            index = i;
                        }
                    }
                }
            }
            return (index < 0) ? null : controls[index];
        }

        public static Control Previous(this Control.ControlCollection controls, Control current)
        {
            int currentIndex = controls.IndexOf(current);
            int index = -1;
            int currentTabIndex = current.TabIndex;
            int tabIndex = 0;
            bool flag = false;
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].TabIndex <= currentTabIndex)
                {
                    if (controls[i].TabIndex != currentTabIndex || i < currentIndex)
                    {
                        if (controls[i].TabIndex == currentTabIndex)
                        {
                            index = i;
                            break;
                        }
                        if (!flag)
                        {
                            flag = true;
                            tabIndex = controls[i].TabIndex;
                            index = i;
                        }
                        else if (controls[i].TabIndex > tabIndex)
                        {
                            tabIndex = controls[i].TabIndex;
                            index = i;
                        }
                    }
                }
            }
            return (index < 0) ? null : controls[index];
        }

        public static Control Next(this Control current)
        {
            return Next(current.Parent.Controls, current);
        }

        public static Control Previous(this Control current)
        {
            return Previous(current.Parent.Controls, current);
        }
        #endregion
    }
}
