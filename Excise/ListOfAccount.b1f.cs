using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;

namespace Excise
{
    [FormAttribute("Excise.ListOfAccount", "ListOfAccount.b1f")]
    class ListOfAccount : UserFormBase
    {
        private readonly ExciseParams _exciseParams;
        private readonly string _exciseAccName;

        public ListOfAccount(ExciseParams exciseParams, string exciseAccName)
        {
            _exciseParams = exciseParams;
            _exciseAccName = exciseAccName;
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Grid0 = ((SAPbouiCOM.Grid)(this.GetItem("Item_0").Specific));
            this.Grid0.DoubleClickAfter += new SAPbouiCOM._IGridEvents_DoubleClickAfterEventHandler(this.Grid0_DoubleClickAfter);
            this.Grid0.ClickAfter += new SAPbouiCOM._IGridEvents_ClickAfterEventHandler(this.Grid0_ClickAfter);
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_1").Specific));
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("Item_2").Specific));
            this.EditText0.KeyDownAfter += new SAPbouiCOM._IEditTextEvents_KeyDownAfterEventHandler(this.EditText0_KeyDownAfter);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {

        }

        private SAPbouiCOM.Grid Grid0;

        private void OnCustomInitialize()
        {
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte($"SELECT AcctCode, AcctName FROM OACT WHERE Postable = 'Y'"));
        }

        private void Grid0_ClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Grid0.Rows.SelectedRows.Clear();
            Grid0.Rows.SelectedRows.Add(pVal.Row);
        }

        private void Grid0_DoubleClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            string acc = Grid0.DataTable.GetValue("AcctCode", Grid0.GetDataTableRowIndex(pVal.Row)).ToString();
            if (_exciseAccName == "ExciseAcc")
            {
                _exciseParams.ExciseAcc = acc;
                _exciseParams.FillCflAcc();
            }
            else
            {
                _exciseParams.ExciseAccReturn = acc;
                _exciseParams.FillCflAccReturn();
            }



            SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm.Close();
        }

        private StaticText StaticText0;
        private EditText EditText0;



        private void EditText0_KeyDownAfter(object sboObject, SBOItemEventArg pVal)
        {
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte($"SELECT AcctCode, AcctName FROM OACT WHERE Postable = 'Y' AND (AcctCode Like N'%{EditText0.Value}%' OR AcctName Like N'%{EditText0.Value}%')"));
        }
    }
}
