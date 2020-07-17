using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;

namespace WhAnno.Utils.XmlSave
{
    /// <summary>
    /// 提供保存成Xml节点或从Xml节点载入填充实例的方法。
    /// </summary>
    public interface IXmlSavable
    {
        /// <summary>
        /// 限定名称。
        /// </summary>
        /// <remarks>
        /// <para>生成<see cref="XmlElement"/>时，是<see cref="XmlElement.Name"/>的赋值依据。</para>
        /// <para>填充<see cref="IXmlSavable"/>实例时，是匹配<see cref="XmlElement"/>的主键。</para>
        /// </remarks>
        string Name { get; set; }
        /// <summary>
        /// 将实例转换成Xml元素。
        /// </summary>
        /// <returns></returns>
        XmlElement ToXmlElement();
        /// <summary>
        /// 使用Xml元素填充实例。
        /// </summary>
        /// <returns>仅修改自身时，返回自身。需要重新构造时，返回新的实例。</returns>
        object FromXmlElement(XmlElement element);
    }

    /// <summary>
    /// <see cref="XmlElement"/>生成器。
    /// </summary>
    public static class XmlElementGenerator
    {
        /// <summary>
        /// 使用<see cref="XmlElementGenerator"/>任何生成方法前，应先将新建的<see cref="XmlDocument"/>绑定到此属性上。
        /// </summary>
        public static XmlDocument GlobalDoc { get; set; }

        private static XmlElement XmlElementRename(XmlElement element, string newName)
        {
            XmlElement result = GlobalDoc.CreateElement(newName);
            result.SetAttributeNodes(element.Attributes);
            result.AppendChildren(element.ChildNodes);
            return result;
        }

        /// <summary>
        /// 从某种类型实例创建<see cref="XmlElement"/>。
        /// </summary>
        /// <param name="name"><see cref="XmlElement"/>的名字。</param>
        /// <param name="instance">实例。</param>
        /// <remarks>
        /// <para>构造<see cref="XmlElement"/>规则：</para>
        /// <para>若实例为<see cref="IXmlSavable"/>，调用<see cref="IXmlSavable.ToXmlElement"/>，并强制<see cref="XmlElement.Name"/>为<paramref name="name"/>。</para>
        /// <para>若实例在当前程序集扩展了方法"ToXmlElement"，则调用该方法。并强制<see cref="XmlElement.Name"/>为<paramref name="name"/>。</para>
        /// <para>否则，对实例调用<see cref="FromString(string, object)"/>方法</para>
        /// </remarks>
        /// <returns></returns>
        private static XmlElement FromInstance(string name, object instance)
        {
            XmlElement result;
            if (instance is IXmlSavable savable)
                result = XmlElementRename(savable.ToXmlElement(), name);
            else
            {
                MethodInfo toXmlElement = instance?.GetType().GetExtensionMethod("ToXmlElement");
                if (toXmlElement != null)
                    result = XmlElementRename(toXmlElement.Invoke(null, new object[] { instance }) as XmlElement, name);
                else
                    result = FromString(name, instance);
            }
            return result;
        }

        /// <summary>
        /// 从某种类型实例转换字符串创建<see cref="XmlElement"/>。
        /// </summary>
        /// <param name="name"><see cref="XmlElement"/>的名字。</param>
        /// <param name="instance">实例。</param>
        /// <returns></returns>
        /// <remarks>调用<see cref="object.ToString"/>方法</remarks>
        public static XmlElement FromString(string name, object instance)
        {
            XmlElement result = GlobalDoc.CreateElement(name);
            if (instance != null)
            {
                result.SetAttribute("Type", instance.GetType().Name);
                result.InnerText = instance.ToString();
            }
            else
            {
                result.SetAttribute("Type", "Null");
            }
            return result;

        }

        /// <summary>
        /// 从某种集合类实例创建<see cref="XmlElement"/>。
        /// </summary>
        /// <typeparam name="T">集合的元素类型。</typeparam>
        /// <param name="name"><see cref="XmlElement"/>的名字。</param>
        /// <param name="collection">集合实例。</param>
        /// <param name="itemToXmlElement">集合元素到<see cref="XmlElement"/>的转换，默认为<see cref="FromString(string, object)"/>，其中<see cref="string"/>恒为<paramref name="bingdingName"/>。</param>
        /// <param name="bingdingName">
        /// <para>每个集合元素构造的节点的限定名称绑定。</para>
        /// <para>默认<paramref name="itemToXmlElement"/>情况下，若<typeparamref name="T"/>实现<see cref="IXmlSavable"/>接口，需要将限定名称具体绑定到每项的<see cref="IXmlSavable.Name"/>，请将此项设置为null或空字符串。</para>
        /// <para>默认<paramref name="itemToXmlElement"/>情况下，若<typeparamref name="T"/>未实现<see cref="IXmlSavable"/>，此项为null或空字符串引发<see cref="ArgumentNullException"/>异常。</para>
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// 默认<paramref name="itemToXmlElement"/>情况下，<typeparamref name="T"/>未实现<see cref="IXmlSavable"/>，而<paramref name="bingdingName"/>为null或空字符串时引发。
        /// </exception>
        public static XmlElement FromCollection<T>(string name, ICollection<T> collection, Func<T, XmlElement> itemToXmlElement = null, string bingdingName = "Item")
        {
            if (itemToXmlElement == null)
            {
                if (bingdingName == null || bingdingName.Length == 0)
                {
                    if (typeof(IXmlSavable).IsAssignableFrom(typeof(T)))
                        itemToXmlElement = (item) => FromInstance((item as IXmlSavable).Name, item);
                    else
                        throw new ArgumentNullException($"类型{typeof(T).Name}未实现{typeof(IXmlSavable).Name}，限定名称绑定{nameof(bingdingName)}不可为空。");
                }
                else
                    itemToXmlElement = (item) => FromInstance(bingdingName, item);
            }

            XmlElement result = FromString(name, collection);

            foreach (T item in collection)
                result.AppendChild(itemToXmlElement(item));

            return result;
        }

        /// <summary>
        /// 从实例中给定属性创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="props">给定属性。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        /// <remarks>使用<see cref="FromInstance(string, object)"/>转换。</remarks>
        public static XmlElement[] FromPropsOf(object instance, IEnumerable<PropertyInfo> props)
        {
            List<XmlElement> result = new List<XmlElement>();
            foreach (PropertyInfo prop in props)
                result.Add(FromInstance(prop.Name, prop.GetValue(instance)));

            return result.ToArray();
        }

        /// <summary>
        /// 使用指定筛选方法从实例中筛选属性创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="PropertyInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        /// <remarks>使用<see cref="FromInstance(string, object)"/>转换。</remarks>
        public static XmlElement[] FromPropsOf(object instance, Func<PropertyInfo, bool> screen = null)
        {
            return FromPropsOf(instance,
                               from prop in instance.GetType().GetProperties() 
                               where screen == null || screen(prop) 
                               select prop);
        }

        /// <summary>
        /// 使用指定绑定约束和/或筛选方法从实例中筛选属性创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="binding">绑定约束。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="PropertyInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        /// <remarks>使用<see cref="FromInstance(string, object)"/>转换。</remarks>
        public static XmlElement[] FromPropsOf(object instance, BindingFlags binding, Func<PropertyInfo, bool> screen = null)
        {
            return FromPropsOf(instance,
                               from prop in instance.GetType().GetProperties(binding)
                               where screen == null || screen(prop)
                               select prop);
        }

        /// <summary>
        /// 从实例中给定字段创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="fields">给定字段。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        /// <remarks>使用<see cref="FromInstance(string, object)"/>转换。</remarks>
        public static XmlElement[] FromFieldsOf(object instance, IEnumerable<FieldInfo> fields)
        {
            List<XmlElement> result = new List<XmlElement>();
            foreach (FieldInfo prop in fields)
                result.Add(FromInstance(prop.Name, prop.GetValue(instance)));

            return result.ToArray();
        }

        /// <summary>
        /// 使用指定筛选方法从实例中筛选字段创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="FieldInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        /// <remarks>使用<see cref="FromInstance(string, object)"/>转换。</remarks>
        public static XmlElement[] FromFieldsOf(object instance, Func<FieldInfo, bool> screen = null)
        {
            return FromFieldsOf(instance,
                                from fields in instance.GetType().GetFields()
                                where screen == null || screen(fields)
                                select fields);
        }

        /// <summary>
        /// 使用指定绑定约束和/或筛选方法从实例中筛选字段创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="binding">绑定约束。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="FieldInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        /// <remarks>使用<see cref="FromInstance(string, object)"/>转换。</remarks>
        public static XmlElement[] FromFieldsOf(object instance, BindingFlags binding, Func<FieldInfo, bool> screen = null)
        {
            return FromFieldsOf(instance,
                                from fields in instance.GetType().GetFields(binding)
                                where screen == null || screen(fields)
                                select fields);
        }

        /// <summary>
        /// 从实例中的所有实现<see cref="IXmlSavable"/>接口的非静态属性创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <returns><see cref="XmlElement"/>数组。</returns>
        public static XmlElement[] FromSavablePropsOf(object instance)
        {
            return FromPropsOf(instance,
                               BindingFlags.Instance | BindingFlags.Public,
                               (prop) => typeof(IXmlSavable).IsAssignableFrom(prop.PropertyType));
        }

        /// <summary>
        /// 从实例中的指定名称属性创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="names">名称数组。</param>
        /// <returns></returns>
        public static XmlElement[] FromNamePropsOf(object instance, params string[] names)
        {
            return FromPropsOf(instance, screen: (prop) =>
            {
                foreach (string name in names)
                    if (name == prop.Name) return true;
                return false;
            });
        }

        /// <summary>
        /// 从实例中的指定名称字段创建<see cref="XmlElement"/>数组。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="names">名称数组。</param>
        /// <returns></returns>
        public static XmlElement[] FromNameFieldsOf(object instance, params string[] names)
        {
            return FromFieldsOf(instance, screen: (field) =>
            {
                foreach (string name in names)
                    if (name == field.Name) return true;
                return false;
            });
        }

        /// <summary>
        /// 将<see cref="IXmlSavable"/>实例生成XML文档保存到指定文件中。如果存在指定文件，方法将覆盖它。
        /// </summary>
        /// <param name="savable">实例。</param>
        /// <param name="filename">文件名。</param>
        public static void Save(this IXmlSavable savable, string filename)
        {
            GlobalDoc = new XmlDocument();
            GlobalDoc.AppendChild(savable.ToXmlElement());
            GlobalDoc.Save(filename);
            GlobalDoc = null;
        }
    }

    /// <summary>
    /// 根据<see cref="XmlElement"/>为实例赋值。
    /// </summary>
    public static class XmlElementConverter
    {
        /// <summary>
        /// 按照<see cref="XmlElement"/>生成某种类型实例。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="element"><see cref="XmlElement"/>实例。</param>
        /// <returns>转换生成的目标类型实例。</returns>
        /// <remarks>
        /// <para>生成实例并使用<see cref="ToInstance(XmlElement, object)"/>填充实例。</para>
        /// 若类型实现接口<see cref="IConvertible"/>，使用<see cref="Convert.ChangeType(object, Type)"/>生成实例，否则使用反射调用默认构造器生成实例，若无默认构造，引发异常。
        /// </remarks>
        /// <exception cref="MissingMethodException"><paramref name="type"/>类型未实现<see cref="IConvertible"/>，又不支持默认构造。</exception>
        public static T ToNewInstance<T>(this XmlElement element) => (T)ToNewInstance(element, typeof(T));

        /// <summary>
        /// 按照<see cref="XmlElement"/>生成某种类型实例。
        /// </summary>
        /// <param name="element"><see cref="XmlElement"/>实例。</param>
        /// <param name="type">目标类型。</param>
        /// <returns>转换生成的目标类型实例。</returns>
        /// <remarks>
        /// <para>生成实例并使用<see cref="ToInstance(XmlElement, object)"/>填充实例。</para>
        /// 若类型实现接口<see cref="IConvertible"/>，使用<see cref="Convert.ChangeType(object, Type)"/>生成实例，否则使用反射调用默认构造器生成实例，若无默认构造，引发异常。
        /// </remarks>
        /// <exception cref="MissingMethodException"><paramref name="type"/>类型未实现<see cref="IConvertible"/>，又不支持默认构造。</exception>
        public static object ToNewInstance(this XmlElement element, Type type)
        {
            //生成
            object result;
            if (typeof(IConvertible).IsAssignableFrom(type) && !type.IsEnum)
                result = Convert.ChangeType(element.GetText(), type);
            else
            {
                try
                {
                    result = Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    throw new MissingMethodException(
                        $"生成新实例失败，类型{type.Name}不支持默认构造。考虑先手动申请实例，再调用XmlElement填充方法。", ex);
                }
            }
            //填充
            result = ToInstance(element, result);
            return result;
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>填充某种类型实例。
        /// </summary>
        /// <param name="element"><see cref="XmlElement"/>实例。</param>
        /// <param name="instance">实例。</param>
        /// <returns>
        /// 所给实例或生成的实例。
        /// <para>实例生成规则：</para>
        /// <para>若实例为<see cref="IXmlSavable"/>，调用<see cref="IXmlSavable.FromXmlElement(XmlElement)"/>修改内容。</para>
        /// <para>若实例在当前程序集扩展了方法"FromXmlElement(XmlElement)"，则调用该方法修改内容。</para>
        /// <para>否则，对实例调用<see cref="Convert.ChangeType(object, Type)"/>方法，将<paramref name="element"/>的文本内容转换成实例对象。该情况下，若实例未实现<see cref="IConvertible"/>接口，引发异常。</para>
        /// </returns>
        /// <remarks>若<paramref name="instance"/>实际为类类型，则函数修改实例内容。若实际为值类型，函数无法修改其内容，应当将函数返回值重新赋予原实例。</remarks>
        /// <exception cref="Exception">
        /// 实例类型未实现<see cref="IXmlSavable"/>或<see cref="IConvertible"/>，也没有扩展FromXmlElement(XmlElement)方法时引发。
        /// </exception>
        public static object ToInstance(this XmlElement element, object instance)
        {
            if (instance == null) return null;

            if (instance is IXmlSavable savable)
            {
                savable.Name = element.Name;
                instance = savable.FromXmlElement(element);
            }
            else
            {
                MethodInfo fromXmlElement = instance?.GetType().GetExtensionMethod("FromXmlElement");
                if (fromXmlElement != null)
                    instance = fromXmlElement.Invoke(null, new object[] { instance, element });
                else if (instance is IConvertible && !(instance is Enum))
                    instance = Convert.ChangeType(element.GetText(), instance.GetType());
                else
                    throw new Exception($"实例具有类型{instance.GetType().Name}。该类型" +
                        $"1.未实现{typeof(IXmlSavable).Name}，无法从XmlElement填充实例。" +
                        $"2.未扩展FromXmlElement(XmlElement)->object方法，无法从XmlElement填充实例。" +
                        $"3.未实现IConvertible，无法从XmlElement文本内容直接填充实例；" +
                        $"考虑用以上三种方法之一解决该异常。");
            }
            return instance;
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>填充集合类型实例。
        /// </summary>
        /// <typeparam name="T">集合的元素类型，若<paramref name="itemFromXmlElement"/>为null，则该类型必须实现接口<see cref="IConvertible"/>，否则引发<see cref="ArgumentException"/>异常。</typeparam>
        /// <param name="collection">目标集合实例。</param>
        /// <param name="element"><see cref="XmlElement"/>实例。</param>
        /// <param name="itemFromXmlElement"><see cref="XmlElement"/>到集合元素的转换，方法需要构造实例。若该参数为null，则集合元素类型<typeparamref name="T"/>必须实现接口<see cref="IConvertible"/>或具有默认构造函数，此时调用<see cref="ToNewInstance{T}(XmlElement)"/>实现转换，否则引发<see cref="MissingMethodException"/>异常。</param>
        /// <param name="bindingName">每个集合元素构造的节点的限定名称绑定，仅在符合绑定时为集合添加构造元素，若设为null则无条件绑定。不区分大小写。</param>
        /// <exception cref="MissingMethodException"><paramref name="itemFromXmlElement"/>为null，而<typeparamref name="T"/>又未实现接口<see cref="IConvertible"/>且无默认构造函数时引发。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/>为null时引发。</exception>
        public static void ToCollection<T>(ICollection<T> collection, XmlElement element, Func<XmlElement, T> itemFromXmlElement = null, string bindingName = "Item")
        {
            if (collection is null)
                throw new ArgumentNullException("参数collection必须有所指。");

            if (itemFromXmlElement is null) itemFromXmlElement = (_element) =>
            {
                try
                {
                    return ToNewInstance<T>(_element);
                }
                catch (MissingMethodException ex)
                {
                    throw new MissingMethodException(
                        $"{nameof(ToCollection)}填充集合需要构造类型{typeof(T).Name}的实例。" +
                        $"而类型{typeof(T).Name}即没有实现IConvertible，又不支持默认构造。" +
                        $"考虑提供参数{nameof(itemFromXmlElement)}。", ex);
                }
            };

            collection.Clear();
            foreach (XmlElement ele in element.GetChildElements())
            {
                if (bindingName == null || ele.Name.ToLower() == bindingName.ToLower())
                    collection.Add(itemFromXmlElement(ele));
            }
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的给定属性，以属性名为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        /// <param name="props">给定属性。</param>
        /// <remarks>
        /// <para>注意：</para>
        /// <para>所匹配属性在实例中应当不为null，如果为null，使用<see cref="ToNewInstance(XmlElement, Type)"/>生成实例，需悉该方法对类型的要求。</para>
        /// <para>使用<see cref="ToInstance(XmlElement, object)"/>填充属性内容。</para>
        /// </remarks>
        /// <exception cref="MissingMethodException">
        /// 匹配属性在实例中为null，而属性有以下情况之一：
        /// 1.类型未实现IConvertible且无默认构造。
        /// 2.属性没有设置方法。
        /// </exception>
        public static void ToPropsOf(object instance, IEnumerable<XmlElement> elements, IEnumerable<PropertyInfo> props)
        {
            foreach (PropertyInfo prop in props)
            {
                foreach (XmlElement element in elements)
                {
                    if (element.Name == prop.Name)
                    {
                        object propObject = prop.GetValue(instance);
                        //属性未赋值，必须申请实例。
                        if (propObject == null)
                        {
                            try
                            {
                                propObject = ToNewInstance(element, prop.PropertyType);
                            }
                            catch (MissingMethodException ex)
                            {
                                throw new MissingMethodException(
                                    $"属性{prop.Name}({prop.PropertyType.Name})未赋值，" +
                                    $"且类型{prop.PropertyType.Name}未实现IConvertible且无默认构造。" +
                                    $"考虑先为属性申请实例，再调用XmlElement填充方法。", ex);
                            }
                            if (!prop.CanWrite)
                                throw new MissingMethodException($"属性{prop.Name}({prop.PropertyType.Name})未赋值且不可赋值。");
                        }
                        else
                            propObject = ToInstance(element, propObject);
                        if (prop.CanWrite) prop.SetValue(instance, propObject);
                    }
                }
            }
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的所有通过筛选方法的属性，以属性名为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="PropertyInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <remarks>
        /// <para>注意：</para>
        /// <para>所匹配属性在实例中应当不为null，如果为null，使用<see cref="ToNewInstance(XmlElement, Type)"/>生成实例，需悉该方法对类型的要求。</para>
        /// <para>使用<see cref="ToInstance(XmlElement, object)"/>填充属性内容。</para>
        /// </remarks>
        /// <exception cref="MissingMethodException">
        /// 匹配属性在实例中为null，而属性有以下情况之一：
        /// 1.类型未实现IConvertible且无默认构造。
        /// 2.属性没有设置方法。
        /// </exception>
        public static void ToPropsOf(object instance, IEnumerable<XmlElement> elements, Func<PropertyInfo, bool> screen = null)
        {
            ToPropsOf(instance, 
                      elements,
                      from prop in instance.GetType().GetProperties()
                      where screen == null || screen(prop)
                      select prop);
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的所有通过绑定约束和/或筛选方法的属性，以属性名为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        /// <param name="binding">绑定约束。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="PropertyInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <remarks>
        /// <para>注意：</para>
        /// <para>所匹配属性在实例中应当不为null，如果为null，使用<see cref="ToNewInstance(XmlElement, Type)"/>生成实例，需悉该方法对类型的要求。</para>
        /// <para>使用<see cref="ToInstance(XmlElement, object)"/>填充属性内容。</para>
        /// </remarks>
        /// <exception cref="MissingMethodException">
        /// 匹配属性在实例中为null，而属性有以下情况之一：
        /// 1.类型未实现IConvertible且无默认构造。
        /// 2.属性没有设置方法。
        /// </exception>
        public static void ToPropsOf(object instance, IEnumerable<XmlElement> elements, BindingFlags binding = (BindingFlags)0xffff, Func<PropertyInfo, bool> screen = null)
        {
            ToPropsOf(instance,
                      elements,
                      from prop in instance.GetType().GetProperties(binding)
                      where screen == null || screen(prop)
                      select prop);
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的给定属性，以属性名为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        /// <param name="fields">给定字段。</param>
        /// <remarks>
        /// <para>注意：</para>
        /// <para>所匹配字段在实例中应当不为null，如果为null，使用<see cref="ToNewInstance(XmlElement, Type)"/>生成实例，需悉该方法对类型的要求。</para>
        /// <para>使用<see cref="ToInstance(XmlElement, object)"/>填充字段内容。</para>
        /// </remarks>
        public static void ToFieldsOf(object instance, IEnumerable<XmlElement> elements, IEnumerable<FieldInfo> fields)
        {
            foreach (FieldInfo field in fields)
            {
                foreach (XmlElement element in elements)
                {
                    if (element.Name == field.Name)
                    {
                        object fieldObject = field.GetValue(instance);
                        //字段未赋值，必须申请实例。
                        if (fieldObject == null)
                        {
                            try
                            {
                                fieldObject = ToNewInstance(element, field.FieldType);
                            }
                            catch (MissingMethodException ex)
                            {
                                throw new MissingMethodException(
                                    $"字段{field.Name}({field.FieldType.Name})未赋值，" +
                                    $"且类型{field.FieldType.Name}未实现IConvertible且无默认构造。" +
                                    $"考虑先为属性申请实例，再调用XmlElement填充方法。", ex);
                            }
                        }
                        else
                            fieldObject = ToInstance(element, fieldObject);
                        field.SetValue(instance, fieldObject);
                    }
                }
            }
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的所有通过筛选方法的字段，以属性名为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="FieldInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <remarks>
        /// <para>注意：</para>
        /// <para>所匹配字段在实例中应当不为null，如果为null，使用<see cref="ToNewInstance(XmlElement, Type)"/>生成实例，需悉该方法对类型的要求。</para>
        /// <para>使用<see cref="ToInstance(XmlElement, object)"/>填充字段内容。</para>
        /// </remarks>
        public static void ToFieldsOf(object instance, IEnumerable<XmlElement> elements, Func<FieldInfo, bool> screen = null)
        {
            ToFieldsOf(instance,
                       elements,
                       from field in instance.GetType().GetFields()
                       where screen == null || screen(field)
                       select field);
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的所有通过绑定约束和/或筛选方法的字段，以属性名为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        /// <param name="binding">绑定约束。</param>
        /// <param name="screen">筛选方法，对传入的<see cref="FieldInfo"/>进行判断，返回是否匹配的布尔值。</param>
        /// <remarks>
        /// <para>注意：</para>
        /// <para>所匹配字段在实例中应当不为null，如果为null，使用<see cref="ToNewInstance(XmlElement, Type)"/>生成实例，需悉该方法对类型的要求。</para>
        /// <para>使用<see cref="ToInstance(XmlElement, object)"/>填充字段内容。</para>
        /// </remarks>
        public static void ToFieldsOf(object instance, IEnumerable<XmlElement> elements, BindingFlags binding = (BindingFlags)0xffff, Func<FieldInfo, bool> screen = null)
        {
            ToFieldsOf(instance,
                       elements,
                       from field in instance.GetType().GetFields(binding)
                       where screen == null || screen(field)
                       select field);
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的所有实现<see cref="IXmlSavable"/>接口的属性，以属性中<see cref="IXmlSavable.Name"/>为赋值主键匹配<see cref="XmlElement.Name"/>，区分大小写。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="elements"><see cref="XmlElement"/>数组。</param>
        public static void ToSavablePropsOf(object instance, IEnumerable<XmlElement> elements)
        {
            ToPropsOf(instance,
                      elements,
                      BindingFlags.Instance | BindingFlags.Public,
                      (prop) => typeof(IXmlSavable).IsAssignableFrom(prop.PropertyType));

        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的指定名称属性。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="names">名称数组。</param>
        /// <returns></returns>
        public static void ToNamePropsOf(object instance, IEnumerable<XmlElement> elements, params string[] names)
        {
            ToPropsOf(instance, elements, screen: (prop) =>
            {
                foreach (string name in names)
                    if (name == prop.Name) return true;
                return false;
            });
        }

        /// <summary>
        /// 按照<see cref="XmlElement"/>尝试填充实例中的指定名称字段。
        /// </summary>
        /// <param name="instance">实例。</param>
        /// <param name="names">名称数组。</param>
        /// <returns></returns>
        public static void ToNameFieldsOf(object instance, IEnumerable<XmlElement> elements, params string[] names)
        {
            ToFieldsOf(instance, elements, screen: (field) =>
            {
                foreach (string name in names)
                    if (name == field.Name) return true;
                return false;
            });
        }

        /// <summary>
        /// 从指定的URL加载XML文件，并赋值到<see cref="IXmlSavable"/>实例中。
        /// </summary>
        /// <param name="savable">实例。</param>
        /// <param name="filename">文件URL。</param>
        public static void Load(this IXmlSavable savable, string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlElement target;

            if (doc.DocumentElement.Name == savable.Name)
                target = doc.DocumentElement;
            else
                target = doc.DocumentElement.GetElement(savable.Name) as XmlElement;

            if (target != null) savable.FromXmlElement(target);
        }
    }

    public static class XmlNodeMethods
    {
        /// <summary>
        /// 将指定的节点数组添加到该节点的末尾。
        /// </summary>
        /// <param name="node">该节点。</param>
        /// <param name="children">要添加的节点数组。</param>
        /// <returns>添加的节点数组</returns>
        public static T AppendChildren<T>(this XmlNode node, T children) where T : IEnumerable
        {
            var e = children.GetEnumerator();
            bool next = e.MoveNext();
            XmlNode child = e.Current as XmlNode;
            while (next)
            {
                next = e.MoveNext();
                //由于AppenChild可能改变children的链表结构（如果是链表），所以应先取得下一节点，再调用AppenChild。
                node.AppendChild(child);
                if (next) child = e.Current as XmlNode;
            }
            return children;
        }

        public static T SetAttributeNodes<T>(this XmlElement node, T attributes) where T : IEnumerable
        {
            foreach (XmlAttribute attribute in attributes) node.SetAttribute(attribute.Name, attribute.Value);
            return attributes;
        }

        /// <summary>
        /// 获取正则项匹配的所有子节点。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="regex">正则表达式。</param>
        /// <returns></returns>
        public static IEnumerable<XmlElement> GetChildElements(this XmlElement node, Regex regex = null)
        {
            return from XmlNode child in node.ChildNodes
                   where child is XmlElement
                   where regex == null || regex.IsMatch(child.Name)
                   select child as XmlElement;
        }

        /// <summary>
        /// 获取节点下面的文本节点<see cref="XmlText"/>中的文本。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetText(this XmlElement node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                if (item is XmlText)
                {
                    return item.Value;
                }
            }
            return node.InnerText;
        }

        /// <summary>
        /// 从当前节点及其子节点中搜索指定名称节点。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <param name="name">节点限定名称。</param>
        /// <returns>找到的第一个限定名称等于<paramref name="name"/>的节点，若没有找到，则为null。</returns>
        public static XmlElement GetElement(this XmlElement element, string name, Search.Method method = Search.Method.BFS)
        {
            Func<XmlElement, IEnumerable<XmlElement>> explorer = (node) =>
            {
                return from XmlNode child in node.ChildNodes
                       where child is XmlElement
                       select child as XmlElement;
            };
            Func<XmlElement, bool> goal = (node) => node.Name == name;

            IEnumerable<XmlElement> resEnum = null;
            switch (method)
            {
                case Search.Method.BFS:
                    resEnum = Search.BFS(element, explorer, goal);
                    break;
                case Search.Method.DFS:
                    resEnum = Search.DFS(element, explorer, goal);
                    break;
            }
            return resEnum?.FirstOrDefault();
        }
    }

    /// <summary>
    /// 扩展枚举类型的<see cref="IXmlSavable"/>方法。
    /// </summary>
    public static class XmlEnumExpand
    {
        public static XmlElement ToXmlElement(this Enum e) => XmlElementGenerator.FromString("Wheat", e);
        public static object FromXmlElement(this Enum e, XmlElement element) => Enum.Parse(e.GetType(),
                                                                                           element.ToNewInstance<string>());
    }

    public static class TypeMethods
    {
        /// <summary>
        /// 在当前执行的代码的程序集中搜索该类型的指定名称扩展方法。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">方法名称。</param>
        /// <param name="assignableMethod">指示当该类型下找不到对应扩展方法时，是否继续寻找Assignable类型方法（见<see cref="Type.IsAssignableFrom(Type)"/>）。</param>
        /// <returns></returns>
        public static MethodInfo GetExtensionMethod(this Type type, string name, bool assignableMethod = true)
        {
            var query = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where !t.IsGenericType && !t.IsNested
                        from method in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.Name == name
                              && method.IsDefined(typeof(ExtensionAttribute), false)
                              && method.GetParameters()[0].ParameterType == type
                        select method;
            MethodInfo res = query.FirstOrDefault();
            if (res == null && assignableMethod)
            {
                query = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where !t.IsGenericType && !t.IsNested
                        from method in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.Name == name
                              && method.IsDefined(typeof(ExtensionAttribute), false)
                              && method.GetParameters()[0].ParameterType.IsAssignableFrom(type)
                        select method;
                res = query.FirstOrDefault();
            }
            return res;
        }
    }

    public static class Search
    {
        /// <summary>
        /// 指示使用何种搜索算法。
        /// </summary>
        public enum Method
        {
            /// <summary>
            /// 广度优先搜索。
            /// </summary>
            BFS = 0,
            /// <summary>
            /// 深度优先搜索
            /// </summary>
            DFS = 1,
        }

        /// <summary>
        /// 广度优先搜索。
        /// </summary>
        /// <typeparam name="T">节点类型。</typeparam>
        /// <param name="root">根节点。</param>
        /// <param name="explorer">节点展开方法。</param>
        /// <param name="goal">结束判定方法。</param>
        /// <returns>搜索结果形成的迭代器。</returns>
        public static IEnumerable<T> BFS<T>(T root, Func<T, IEnumerable<T>> explorer, Func<T, bool> goal)
        {
            Queue<T> frontier = new Queue<T>();

            //根节点入队
            frontier.Enqueue(root);

            while (frontier.Count > 0)
            {
                //取出节点
                T current = frontier.Dequeue();
                //目标判定
                if (goal(current)) yield return current;
                //节点展开
                foreach (T explored in explorer(current))
                    frontier.Enqueue(explored);
            }
        }

        /// <summary>
        /// 深度优先搜索。
        /// </summary>
        /// <typeparam name="T">节点类型。</typeparam>
        /// <param name="root">根节点。</param>
        /// <param name="explorer">节点展开方法。</param>
        /// <param name="goal">结束判定方法。</param>
        /// <returns>搜索结果形成的迭代器。</returns>
        public static IEnumerable<T> DFS<T>(T root, Func<T, IEnumerable<T>> explorer, Func<T, bool> goal)
        {
            Stack<T> frontier = new Stack<T>();

            //根节点入栈
            frontier.Push(root);

            while (frontier.Count > 0)
            {
                //取出节点
                T current = frontier.Pop();
                //目标判定
                if (goal(current)) yield return current;
                //节点展开
                foreach (T explored in explorer(current))
                    frontier.Push(explored);
            }
        }

    }
}
