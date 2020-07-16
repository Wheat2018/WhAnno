using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WhAnno.Anno.Base;
using WhAnno.Utils.Expend;
using WhAnno.Utils.XmlSave;

namespace WhAnno.Utils
{
    public static class XmlExpand
    {
        public static XmlElement KeyValueToXmlElement<T>(KeyValuePair<string, T> pair)
        {
            XmlElement res = XmlElementGenerator.FromString("x", pair);
            res.AppendChildren(XmlElementGenerator.FromNamePropsOf(pair, "Key"));
            res.AppendChildren(XmlElementGenerator.FromNamePropsOf(pair, "Value"));
            return res;
        }
        public static object KeyValueFromXmlElement<T>(KeyValuePair<string, T> _, XmlElement element)
        {
            return new KeyValuePair<string, T>(element.GetElement("Key").ToNewInstance<string>(),
                                               element.GetElement("Value").ToNewInstance<T>());
        }

        public static XmlElement ToXmlElement(this KeyValuePair<string, Pen> pair) => KeyValueToXmlElement(pair);
        public static object FromXmlElement(this KeyValuePair<string, Pen> _, XmlElement element)
        {
            //Pen没有默认构造，必须手动构造再赋值。
            Pen pen = Pens.Black;
            pen = element.GetElement("Value").ToInstance(pen) as Pen;
            return new KeyValuePair<string, Pen>(element.GetElement("Key").ToNewInstance<string>(), pen);
        }

        public static XmlElement ToXmlElement(this KeyValuePair<string, string> pair) => KeyValueToXmlElement(pair);
        public static object FromXmlElement(this KeyValuePair<string, string> pair, XmlElement element) => KeyValueFromXmlElement(pair, element);


        public static XmlElement ToXmlElement(this Pen pen)
        {
            XmlElement res = XmlElementGenerator.FromString("x", pen);
            res.AppendChildren(XmlElementGenerator.FromNamePropsOf(pen, "Width", "Color"));
            return res;
        }
        public static object FromXmlElement(this Pen _, XmlElement element)
        {
            return new Pen(element.GetElement("Color").ToNewInstance<Color>(),
                           element.GetElement("Width").ToNewInstance<float>());
        }

        public static XmlElement ToXmlElement(this Color color)
        {
            XmlElement res = XmlElementGenerator.FromString("x", color);
            res.AppendChildren(XmlElementGenerator.FromNamePropsOf(color, "A", "R", "G", "B"));
            return res;
        }
        public static object FromXmlElement(this Color _, XmlElement element)
        {
            return Color.FromArgb(element.GetElement("A").ToNewInstance<int>(),
                                  element.GetElement("R").ToNewInstance<int>(),
                                  element.GetElement("G").ToNewInstance<int>(),
                                  element.GetElement("B").ToNewInstance<int>());
        }

    }

    /// <summary>
    /// 管理设置。
    /// </summary>
    public class Setting : IXmlSavable
    {

        public static Setting Global { get; } = new Setting("Global");

        public class Category : IXmlSavable
        {
            public string Name { get; set; }

            public BrushBase Brush { get; set; } = new Anno.Brush.Rectangle();

            XmlElement IXmlSavable.ToXmlElement()
            {
                XmlElement res = XmlElementGenerator.FromString(Name, this);
                res.AppendChildren(XmlElementGenerator.FromNamePropsOf(this, "BrushPen", "BrushType"));
                return res;
            }

            object IXmlSavable.FromXmlElement(XmlElement element)
            {
                XmlElementConverter.ToNameFieldsOf(this, element.GetChildElements(), "BrushPen", "BrushType");
                return this;
            }

        }
        /// <summary>
        /// 类别的集合。
        /// </summary>
        /// <remarks>不重复地记录类别字符串，每种类别可能包含某些属性值（表现为字典），使用类别字符串以检索到这些属性值。</remarks>
        public class CategorySet : HashSet<string>, IXmlSavable
        {
            /// <summary>
            /// 类别的附加属性。
            /// </summary>
            /// <typeparam name="T">属性类型。</typeparam>
            /// <remarks>若<typeparamref name="T"/>并非实现了<see cref="IConvertible"/>的类型，需要覆盖<see cref="CategoryProperty{T}.PropertyToXmlElement"/>和<see cref="CategoryProperty{T}.PropertyFromXmlElement"/>方法，否则生成Xml或从Xml载入时会发生不可预料的错误。</remarks>
            public class CategoryProperty<T> : Dictionary<string, T>, IXmlSavable
            {
                public T DefaultValue { get; set; } = default;

                public CategoryProperty(CategorySet owner, string id)
                {
                    owner.ItemAdded += (sender, item) => Add(item, DefaultValue);
                    ((IXmlSavable)this).Name = id;
                }

                string IXmlSavable.Name { get; set; }

                XmlElement IXmlSavable.ToXmlElement()
                {
                    return XmlElementGenerator.FromCollection(((IXmlSavable)this).Name, this, bingdingName: typeof(T).Name);
                }

                object IXmlSavable.FromXmlElement(XmlElement element)
                {
                    XmlElementConverter.ToCollection(this, element, bindingName: typeof(T).Name);
                    return this;
                }

            }

            public CategoryProperty<Pen> Pens { get; private set; }

            public CategoryProperty<string> BrushTypes { get; private set; }

            public CategorySet(string id)
            {
                Pens = new CategoryProperty<Pen>(this, "Pens")
                {
                    DefaultValue = new Pen(Color.Black, 2),
                };
                BrushTypes = new CategoryProperty<string>(this, "BrushTypes")
                {
                    DefaultValue = "Rectangle"
                };
                ((IXmlSavable)this).Name = id;
            }

            public event EventHandler<string> ItemAdded;
            public event EventHandler<string> ItemRemoved;

            public new void Add(string item)
            {
                if (base.Add(item)) ItemAdded?.Invoke(this, item);
            }
            public new void Remove(string item)
            {
                if (base.Remove(item)) ItemRemoved?.Invoke(this, item);
            }

            string IXmlSavable.Name { get; set; }

            XmlElement IXmlSavable.ToXmlElement()
            {
                XmlElement result = XmlElementGenerator.FromCollection(((IXmlSavable)this).Name, this, bingdingName: "Category");
                result.AppendChildren(XmlElementGenerator.FromSavablePropsOf(this));
                return result;
            }

            object IXmlSavable.FromXmlElement(XmlElement element)
            {
                XmlElementConverter.ToCollection(this, element, bindingName: "Category");
                XmlElementConverter.ToSavablePropsOf(this, element.GetChildElements());
                return this;
            }
        }

        /// <summary>
        /// 当前标注任务中所有可能出现的类别名。
        /// </summary>
        public CategorySet Categories { get; } = new CategorySet("Categories");

        public Setting(string id)
        {
            ((IXmlSavable)this).Name = id;
        }

        public void Save(string filename) => ((IXmlSavable)this).Save(filename);

        public void Load(string filename) => ((IXmlSavable)this).Load(filename);

        string IXmlSavable.Name { get; set; }

        XmlElement IXmlSavable.ToXmlElement()
        {
            XmlElement result = XmlElementGenerator.FromString(((IXmlSavable)this).Name, this);
            result.AppendChildren(XmlElementGenerator.FromSavablePropsOf(this));
            return result;
        }

        object IXmlSavable.FromXmlElement(XmlElement element)
        {
            XmlElementConverter.ToSavablePropsOf(this, element.GetChildElements());
            return this;
        }
    }
}
