using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM;
using Application = SAPbouiCOM.Framework.Application;

namespace Excise.Initialization
{
    class CreateTables : IRunnable
    {
        public void Run(DiManager diManager)
        {
            if (!DiManager.Company.InTransaction)
            {
                DiManager.Company.StartTransaction();
            }
            diManager.CreateTable("RSM_EXCP", SAPbobsCOM.BoUTBTableType.bott_NoObjectAutoIncrement);
            if (DiManager.Company.InTransaction)
            {
                DiManager.Company.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }
    }
}
