using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;

namespace Excise
{
    [FormAttribute("Excise.ExciseParams", "ExciseParams.b1f")]
    class ExciseParams : UserFormBase
    {
        public ExciseParams()
        {
        }
        public string ExciseAcc { get; set; }
        public string ExciseAccReturn { get; set; }

        private Form _paramsForm;

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_0").Specific));
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("Item_1").Specific));
            this.EditText0.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText0_ChooseFromListBefore);
            this.StaticText1 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_2").Specific));
            this.EditText1 = ((SAPbouiCOM.EditText)(this.GetItem("Item_3").Specific));
            this.EditText1.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText1_ChooseFromListBefore);
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("Item_4").Specific));
            this.Button0.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button0_PressedAfter);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.VisibleAfter += new VisibleAfterHandler(this.Form_VisibleAfter);

        }

        private SAPbouiCOM.StaticText StaticText0;

        private void OnCustomInitialize()
        {
           
        }

        private SAPbouiCOM.EditText EditText0;
        private SAPbouiCOM.StaticText StaticText1;
        private SAPbouiCOM.EditText EditText1;

        public void FillCflAcc()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_1").ValueEx = ExciseAcc;
        }

        public void FillCflAccReturn()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_2").ValueEx = ExciseAccReturn;
        }

        private void EditText0_ChooseFromListBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            ListOfAccount list = new ListOfAccount(this, "ExciseAcc");
            list.Show();
        }

        private void EditText1_ChooseFromListBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            ListOfAccount list = new ListOfAccount(this, "ExciseAccReturn");
            list.Show();
        }

        private void Form_VisibleAfter(SAPbouiCOM.SBOItemEventArg pVal)
        {
            if (SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm.Title == "პარამეტრები")
            {
                _paramsForm = SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm;
                Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_EXCP]"));
                if (!recSet.EoF)
                {
                    ExciseAcc = recSet.Fields.Item("U_ExciseAcc").Value.ToString();
                    ExciseAccReturn = recSet.Fields.Item("U_ExciseAccReturn").Value.ToString();
                    _paramsForm.DataSources.UserDataSources.Item("UD_1").ValueEx = ExciseAcc;
                    _paramsForm.DataSources.UserDataSources.Item("UD_2").ValueEx = ExciseAccReturn;
                }
            }
        }

        private Button Button0;

        private void Button0_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (string.IsNullOrWhiteSpace(_paramsForm.DataSources.UserDataSources.Item("UD_1").ValueEx) || string.IsNullOrWhiteSpace(_paramsForm.DataSources.UserDataSources.Item("UD_2").ValueEx))
            {
                Application.SBO_Application.SetStatusBarMessage("შეავსეთ ყველა ველი",
                    BoMessageTime.bmt_Short, true);
                return;
            }
            Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_EXCP]"));
            if (recSet.EoF)
            {
                recSet.DoQuery(DiManager.QueryHanaTransalte($"INSERT INTO [@RSM_EXCP] (U_ExciseAcc, U_ExciseAccReturn) VALUES (N'{ExciseAcc}',N'{ExciseAccReturn}')"));
            }
            else
            {
                recSet.DoQuery(DiManager.QueryHanaTransalte($"UPDATE [@RSM_EXCP] SET U_ExciseAcc = N'{ExciseAcc}', U_ExciseAccReturn = N'{ExciseAccReturn}'"));
            }
        }
    }
}
