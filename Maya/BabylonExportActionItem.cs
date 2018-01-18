using Maya2Babylon.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Maya2Babylon
{
    class BabylonExportActionItem
    {
        private ExporterForm form;

        public void Close()
        {
            if (form == null)
            {
                return;
            }
            form.Dispose();
            form = null;
        }
    }
}
