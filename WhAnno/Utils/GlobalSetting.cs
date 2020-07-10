using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhAnno.Utils
{
    /// <summary>
    /// 管理全局的设置，该类仅记录数据参考，不新增任何类模板。
    /// </summary>
    public static class GlobalSetting
    {
        /// <summary>
        /// 当前标注任务中所有可能出现的类别名，与其对应的GUI钢笔的键值字典。
        /// </summary>
        public static Dictionary<string, Pen> Categories { get; } = new Dictionary<string, Pen>();

        /// <summary>
        /// 注册类别。
        /// </summary>
        /// <returns>若注册成功返回true，若类别已存在返回false。</returns>
        public static bool Categories_Regist(string category, Pen pen = null)
        {
            if (Categories.ContainsKey(category)) return false;
            if (pen == null) pen = new Pen(Color.Black, 2);

            Categories.Add(category, pen);
            return true;
        }

        /// <summary>
        /// 重设类别对应颜色。
        /// </summary>
        /// <param name="width">钢笔宽度。</param>
        public static void Categories_SetColors(float width = 2)
        {
            Color[] colors = ColorList.Linspace(Categories.Count);
            int i = 0;
            foreach (string key in Categories.Keys)
            {
                Categories[key] = new Pen(colors[i++], width);
            }
        }

    }
}
