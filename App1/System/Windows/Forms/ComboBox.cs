using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App1.System.Windows.Forms
{
    public class ComboBox : ListControl
    {
        public ComboBoxStyle DropDownStyle = ComboBoxStyle.DropDownList;

        public override void PerformLayout()
        {
            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
