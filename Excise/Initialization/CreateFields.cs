using SAPbobsCOM;

namespace Excise.Initialization
{
    class CreateFields : IRunnable
    {
        public void Run(DiManager diManager)
        {
            if (!DiManager.Company.InTransaction)
            {
                DiManager.Company.StartTransaction();
            }
            diManager.AddField("RSM_EXCP", "ExciseAcc", "აქციზის ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_EXCP", "ExciseAccReturn", "აქციზის ანგარიში უკან დაბრუნება", BoFieldTypes.db_Alpha,
                20, false);

            if (DiManager.Company.InTransaction)
            {
                DiManager.Company.EndTransaction(BoWfTransOpt.wf_Commit);
            }
        }
    }
}
