using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhAnno.Anno.Base;

namespace WhAnno.AnnoList
{
    class AnnoLink : Label
    {
        public AnnotationBase Annotation { get; private set; }

        public AnnoLink(AnnotationBase annotation)
        {
            Annotation = annotation;
            Text = Annotation.ToString();
            AutoSize = false;
            AutoEllipsis = true;
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }
    }
}
