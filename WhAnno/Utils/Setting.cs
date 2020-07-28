using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WhAnno.Anno.Base;
using WhAnno.Utils.Expand;
using WhAnno.Utils.XmlSave;

namespace WhAnno.Utils
{
    public static class XmlExpand
    {
        public static XmlElement ToXmlElement(this BrushBase brush)
        {
            XmlElement res = XmlElementGenerator.FromString("Wheat", brush);
            res.AppendChildren(XmlElementGenerator.FromNameFieldsOf(brush, "pen", "font"));
            return res;
        }
        public static object FromXmlElement(this BrushBase brush, XmlElement element)
        {
            XmlElementConverter.ToNameFieldsOf(brush, element.GetChildElements(), "pen", "font");
            return brush;
        }

        public static XmlElement ToXmlElement(this Pen pen)
        {
            XmlElement res = XmlElementGenerator.FromString("Wheat", pen);
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
            XmlElement res = XmlElementGenerator.FromString("Wheat", color);
            res.AppendChildren(XmlElementGenerator.FromNamePropsOf(color, "A", "R", "G", "B"));
            if (color.IsNamedColor)
                res.AppendChildren(XmlElementGenerator.FromNamePropsOf(color, "Name"));
            return res;
        }
        public static object FromXmlElement(this Color _, XmlElement element)
        {
            //Named Color
            XmlElement nameColor = element.GetElement("Name");
            if (nameColor != null) return Color.FromName(nameColor.ToNewInstance<string>());
            //Other Color
            return Color.FromArgb(element.GetElement("A").ToNewInstance<int>(),
                                  element.GetElement("R").ToNewInstance<int>(),
                                  element.GetElement("G").ToNewInstance<int>(),
                                  element.GetElement("B").ToNewInstance<int>());
        }

        public static XmlElement ToXmlElement(this Font font)
        {
            XmlElement res = XmlElementGenerator.FromString("Wheat", font);
            res.AppendChild(XmlElementGenerator.FromString("fontFamily", font.FontFamily.Name));
            res.AppendChildren(XmlElementGenerator.FromNamePropsOf(font, "Size", "Style"));
            return res;
        }

        public static object FromXmlElement(this Font _, XmlElement element)
        {
            return new Font(element.GetElement("fontFamily").ToNewInstance<string>(),
                            element.GetElement("Size").ToNewInstance<float>(),
                            element.GetElement("Style").ToNewInstance<FontStyle>());
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
            public string Name => ((IXmlSavable)this).Name;

            public BrushBase Brush { get; set; } = new Anno.Brush.Rectangle();

            string IXmlSavable.Name { get; set; }

            public Category(string name)
            {
                if (name == null || name.Length == 0)
                    throw new ArgumentNullException("类别名不可为空");
                ((IXmlSavable)this).Name = name;
            }

            XmlElement IXmlSavable.ToXmlElement()
            {
                XmlElement res = XmlElementGenerator.FromString(((IXmlSavable)this).Name, this);
                res.AppendChildren(XmlElementGenerator.FromNamePropsOf(this, "Brush"));
                return res;
            }

            object IXmlSavable.FromXmlElement(XmlElement element)
            {
                ((IXmlSavable)this).Name = element.Name;
                XmlElementConverter.ToNamePropsOf(this, element.GetChildElements(), "Brush");
                return this;
            }

            public override bool Equals(object obj) => Name == (obj as Category)?.Name;

            public override int GetHashCode() => Name.GetHashCode();
        }

        /// <summary>
        /// 类别的集合。
        /// </summary>
        /// <remarks>不重复地记录类别字符串，每种类别可能包含某些属性值（表现为字典），使用类别字符串以检索到这些属性值。</remarks>
        public class CategorySet : HashSet<Category>, IXmlSavable
        {

            public CategorySet(string name)
            {
                ((IXmlSavable)this).Name = name;
            }

            public bool Add(string categoryName) => Add(new Category(categoryName));

            string IXmlSavable.Name { get; set; }

            public Category this[string categoryName]
            {
                get
                {
                    foreach (Category item in this)
                        if (item.Name == categoryName) return item;
                    return null;
                }
            }

            public bool Contain(string categoryName) => this[categoryName] != null;

            public bool Remove(string categoryName) => Remove(this[categoryName]);

            public void AutoColor()
            {
                Color[] colors = ColorList.Linspace(Count);
                int i = 0;
                foreach (Category category in this)
                    category.Brush.pen.Color = colors[i++];
            }

            XmlElement IXmlSavable.ToXmlElement()
            {
                return XmlElementGenerator.FromCollection(((IXmlSavable)this).Name, this, bingdingName: null);
            }

            object IXmlSavable.FromXmlElement(XmlElement element)
            {
                XmlElementConverter.ToCollection(this, element, (ele) =>
                {
                    Category category = new Category("Wheat");
                    (category as IXmlSavable).FromXmlElement(ele);
                    return category;
                }, bindingName: null);
                return this;
            }

        }

        /// <summary>
        /// 当前标注任务中所有可能出现的类别名。
        /// </summary>
        public CategorySet Categories { get; } = new CategorySet("Categories");

        internal Setting(string id)
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
