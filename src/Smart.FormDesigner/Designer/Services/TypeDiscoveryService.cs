using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Smart.FormDesigner.Services
{
    /// <summary>
    /// 提供程序集或类型检索服务
    /// </summary>
    public class TypeDiscoveryService : AbstractService, ITypeDiscoveryService
    {
        private List<Assembly> assemblies;

        public TypeDiscoveryService()
        {
            this.assemblies = new List<Assembly>();
            this.assemblies.AddRange(new Assembly[] {
                typeof(Size).Assembly,
                typeof(Control).Assembly,
                typeof(DataSet).Assembly,
                typeof(XmlElement).Assembly
            });
        }

        // 发现设计时可用的类型
        #region ITypeDiscoveryService 接口成员

        /// <inheritdoc />
        public ICollection GetTypes(Type baseType, bool excludeGlobalTypes)
        {
            var list = new List<Type>();
            if (baseType == null)
            {
                baseType = typeof(object);
            }
            foreach (var assembly in this.assemblies)
            {
                if (!excludeGlobalTypes || !assembly.GlobalAssemblyCache)
                {
                    list.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(baseType)));
                }
            }
            return list;
        }

        #endregion

    }
}
