using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLib.WorkItems
{
    public class PrintJob
    {
        public PrintJob(string text) => Text = text;

        public string Text { get; }
    }
}
