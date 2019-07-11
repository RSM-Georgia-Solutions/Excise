using Excise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excise.Initialization
{
    class Init
    {
        public void Run(DiManager diaManger)
        {
            IEnumerable<IRunnable> obects = new List<IRunnable>(){
                new  CreateTables(),
                new  CreateFields()};
            foreach (IRunnable item in obects)
            {
                item.Run(diaManger);
            }
        }
        public void Run(SAPbobsCOM.Company comp)
        {
            DiManager diManager = new DiManager();
            Run(diManager);
        }
    }
}
