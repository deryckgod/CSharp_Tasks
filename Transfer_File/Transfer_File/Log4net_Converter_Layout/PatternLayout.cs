using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transfer_File.Log4net_Converter_Layout
{
    public class PatternLayout : log4net.Layout.PatternLayout
    {
        public PatternLayout()
        {
            this.AddConverter("Property", typeof(PatternConvert));
        }
    }
}
