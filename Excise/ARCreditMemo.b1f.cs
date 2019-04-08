using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;
using SAPbobsCOM;
using System.Globalization;

namespace Excise
{
    [FormAttribute("179", "ARCreditMemo.b1f")]
    class ARCreditMemo : SystemFormBase
    {
        public ARCreditMemo()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Button1 = ((SAPbouiCOM.Button)(this.GetItem("1").Specific));
            this.Button1.PressedBefore += new SAPbouiCOM._IButtonEvents_PressedBeforeEventHandler(this.Button1_PressedBefore);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.DataAddAfter += new DataAddAfterHandler(this.Form_DataAddAfter);

        }

        private void Form_DataAddAfter(ref SAPbouiCOM.BusinessObjectInfo pVal)
        {
            if (pVal.ActionSuccess)
            {
                Form invoice = SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm;
                int series = int.Parse(((ComboBox)invoice.Items.Item("88").Specific).Selected.Value);
                string invNumber = ((EditText)invoice.Items.Item("8").Specific).Value;
                DateTime postingDate = DateTime.ParseExact(((EditText)invoice.Items.Item("10").Specific).Value, "yyyyMMdd", CultureInfo.CurrentCulture);
                int bplId = int.Parse(((ComboBox)invoice.Items.Item("2001").Specific).Selected.Value);
                Matrix invoiceMatrix = (Matrix)invoice.Items.Item("38").Specific;
                string currency = "GEL";
                double rate = 1;
                try
                {
                    currency = ((ComboBox)invoice.Items.Item("63").Specific).Selected.Value;
                    rate = double.Parse(((EditText)invoice.Items.Item("64").Specific).Value);
                }
                catch (Exception e)
                {
                    //currency not shown
                }

                for (int i = 1; i < invoiceMatrix.RowCount; i++)
                {
                    string itemCode = ((EditText)invoiceMatrix.Columns.Item("1").Cells.Item(i).Specific).Value;
                    string quantityString = ((EditText)invoiceMatrix.Columns.Item("11").Cells.Item(i).Specific).Value;

                    string grossTotalString;
                    try
                    {
                        grossTotalString = ((EditText)invoiceMatrix.Columns.Item("288").Cells.Item(i).Specific).Value;
                    }
                    catch (Exception e)
                    {
                        Application.SBO_Application.SetStatusBarMessage("Gross Total (LC) გამოაჩინეთ - (Form Settings)",
                            BoMessageTime.bmt_Short, true);
                        return;
                    }
                    SAPbobsCOM.Items item = (SAPbobsCOM.Items)DiManager.Company.GetBusinessObject(BoObjectTypes.oItems);

                    item.GetByKey(itemCode);
                    string exciseString = item.UserFields.Fields.Item("U_Excise").Value.ToString();
                    if (string.IsNullOrWhiteSpace(exciseString))
                    {
                        continue;
                    }

                    try
                    {
                        double grossTotal = double.Parse(grossTotalString.Split(' ')[0]);
                    }
                    catch (Exception e)
                    {
                    }

                    double quantity = double.Parse(quantityString);
                    double excise = double.Parse(exciseString);
                    double fullExcise = Math.Round(quantity * excise * rate);
                    string result = DiManager.AddJournalEntryCredit(DiManager.Company, "3130", glRevenueAccount, -fullExcise, series, invNumber, glRevenueAccount, postingDate, bplId, currency);
                }
            }
        }

        private void OnCustomInitialize()
        {

        }

        private SAPbouiCOM.Button Button1;
        private string glRevenueAccount;
        private void Button1_PressedBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            Form invoice = SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm;
            Matrix invoiceMatrix = (Matrix)invoice.Items.Item("38").Specific;
            try
            {
                glRevenueAccount = ((EditText)invoiceMatrix.Columns.Item("29").Cells.Item(1).Specific).Value;
            }
            catch (Exception e)
            {
                Application.SBO_Application.SetStatusBarMessage("გამოაჩინეთ G/L Account Form Settings -იდან",
                    BoMessageTime.bmt_Short, true);
            }
        }
    }
}
