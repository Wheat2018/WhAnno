using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WhAnno.Utils.Expend;

namespace WhAnno.Utils
{
    public interface IXmlSavable
    {
        string ID { get; set; }
        /// <summary>
        /// 将实例转换成Xml元素。
        /// </summary>
        /// <returns></returns>
        XmlElement ToXmlElement();
        /// <summary>
        /// 使用Xml元素填充实例。
        /// </summary>
        /// <param name="str"></param>
        void FromXmlElement(XmlElement element);
    }

    /// <summary>
    /// 管理设置。
    /// </summary>
    public class Setting : IXmlSavable
    {
        public static Setting Global { get; } = new Setting("Global");

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
            public class CategoryProperty<T> : Dictionary<string, T>, IXmlSavable
            {
                public T DefaultValue { get; set; } = default;

                /// <summary>
                /// 类型<see cref="T"/>到<see cref="XmlElement"/>的转换。
                /// </summary>
                public Func<T, XmlElement> PropertyToXmlElement = new Func<T, XmlElement>((item) => XmlElementGenerator.FromInstance("Value", item));
                /// <summary>
                /// <see cref="XmlElement"/>到类型<see cref="T"/>的转换。
                /// </summary>
                public Func<XmlElement, T> PropertyFromXmlElement = new Func<XmlElement, T>((element) => XmlElementConverter.ToInstance<T>(element));

                public CategoryProperty(CategorySet owner, string id)
                {
                    owner.ItemAdded += (sender, item) => Add(item, DefaultValue);
                    ((IXmlSavable)this).ID = id;
                }

                string IXmlSavable.ID { get; set; }

                XmlElement IXmlSavable.ToXmlElement()
                {
                    return XmlElementGenerator.FromCollection(((IXmlSavable)this).ID, this, (pair) =>
                    {
                        XmlElement res = XmlElementGenerator.FromInstance("Item", pair);
                        res.AppendChild(XmlElementGenerator.FromInstance("Key", pair.Key));
                        res.AppendChild(PropertyToXmlElement(pair.Value));
                        return res;
                    });
                }

                void IXmlSavable.FromXmlElement(XmlElement element)
                {
                    XmlElementConverter.ToCollection(this, element, (_element) =>
                    {
                        KeyValuePair<string, T> pair = new KeyValuePair<string, T>(
                            XmlElementConverter.ToInstance<string>(_element.SelectSingleNode("Key") as XmlElement),
                            PropertyFromXmlElement(_element.SelectSingleNode("Value") as XmlElement));
                        return pair;
                    });
                }
            }

            public CategoryProperty<Pen> Pens { get; private set; }

            public CategorySet(string id)
            {
                Pens = new CategoryProperty<Pen>(this, "Pens")
                {
                    DefaultValue = new Pen(Color.Black, 2),
                    PropertyToXmlElement = (item) =>
                    {
                        XmlElement result = XmlElementGenerator.FromInstance("Value", item);
                        result.InnerText = item.Color.ToArgb() + " " + item.Width;
                        return result;
                    },
                    PropertyFromXmlElement = (element) =>
                    {
                        string[] texts = element.GetText().Split(" ".ToCharArray());
                        return new Pen(Color.FromArgb(Convert.ToInt32(texts[0])), Convert.ToInt32(texts[1]));
                    }
                };
                ((IXmlSavable)this).ID = id;
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

            string IXmlSavable.ID { get; set; }

            XmlElement IXmlSavable.ToXmlElement()
            {
                XmlElement result = XmlElementGenerator.FromCollection(((IXmlSavable)this).ID, this);
                result.AppendChildren(XmlElementGenerator.FromSavablePropsOfInstance(this));
                return result;
            }

            void IXmlSavable.FromXmlElement(XmlElement element)
            {
                XmlElementConverter.ToCollection(this, element);

                XmlElementConverter.ToSavablePropsOfInstance(this, element.GetChildElements());
            }
        }

        /// <summary>
        /// 当前标注任务中所有可能出现的类别名。
        /// </summary>
        public CategorySet Categories { get; } = new CategorySet("Categories");

        public Setting(string id)
        {
            ((IXmlSavable)this).ID = id;
        }

        public void Save(string filename)
        {
            XmlDocument doc = new XmlDocument();
            XmlElementGenerator.GlobalDoc = doc;
            doc.AppendChild(((IXmlSavable)this).ToXmlElement());
            doc.Save(filename);
            XmlElementGenerator.GlobalDoc = null;
        }

        public void Load(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlElement target = null;

            if (doc.DocumentElement.Name == ((IXmlSavable)this).ID) 
                target = doc.DocumentElement;
            else 
                target = doc.DocumentElement.SelectSingleNode(((IXmlSavable)this).ID) as XmlElement;
            
            if (target != null) ((IXmlSavable)this).FromXmlElement(target);
        }

        string IXmlSavable.ID { get; set; }

        XmlElement IXmlSavable.ToXmlElement()
        {
            XmlElement result = XmlElementGenerator.FromInstance(((IXmlSavable)this).ID, this);
            result.AppendChildren(XmlElementGenerator.FromSavablePropsOfInstance(this));
            return result;
        }

        void IXmlSavable.FromXmlElement(XmlElement element)
        {
            XmlElementConverter.ToSavablePropsOfInstance(this, element.GetChildElements());
        }
    }
}
