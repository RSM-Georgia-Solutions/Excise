﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;

namespace Excise
{
    [FormAttribute("133", "ARInvoice.b1f")]
    class ARInvoice : SystemFormBase
    {
        public ARInvoice()
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
                var invObjectString = pVal.ObjectKey;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(invObjectString);
                string invDocEnttry = string.Empty;
                try
                {
                     invDocEnttry = xmlDoc.GetElementsByTagName("DocEntry").Item(0).InnerText;
                }
                catch (Exception e)
                {
                    Application.SBO_Application.SetStatusBarMessage("Invalid Document Number",
                        BoMessageTime.bmt_Short, true);
                }


                Documents invoiceDi = (Documents)DiManager.Company.GetBusinessObject(BoObjectTypes.oInvoices);
                invoiceDi.GetByKey(int.Parse(invDocEnttry, CultureInfo.InvariantCulture));

                Form invoice = Application.SBO_Application.Forms.ActiveForm;
                string currency = "GEL";
                try
                {
                    currency = ((ComboBox)invoice.Items.Item("63").Specific).Selected.Value;
                    if (currency != "GEL")
                    {
                        return;
                    }

                }
                catch (Exception e)
                {
                    //currency not shown
                }

                int series = int.Parse(((ComboBox)invoice.Items.Item("88").Specific).Selected.Value);
                string invNumber = ((EditText)invoice.Items.Item("8").Specific).Value;
                DateTime postingDate = DateTime.ParseExact(((EditText)invoice.Items.Item("10").Specific).Value, "yyyyMMdd", CultureInfo.InvariantCulture);
                int bplId = int.Parse(((ComboBox)invoice.Items.Item("2001").Specific).Selected.Value, CultureInfo.InvariantCulture);
                Matrix invoiceMatrix = (Matrix)invoice.Items.Item("38").Specific;


                for (int i = 1; i < invoiceMatrix.RowCount; i++)
                {
                    string itemCode = ((EditText)invoiceMatrix.Columns.Item("1").Cells.Item(i).Specific).Value;
                    string quantityString = ((EditText)invoiceMatrix.Columns.Item("11").Cells.Item(i).Specific).Value;

                    SAPbobsCOM.Items item = (SAPbobsCOM.Items)DiManager.Company.GetBusinessObject(BoObjectTypes.oItems);
                    item.GetByKey(itemCode);
                    string exciseString;
                    try
                    {
                        exciseString = item.UserFields.Fields.Item("U_Excise").Value.ToString();
                    }
                    catch (Exception e)
                    {
                        Application.SBO_Application.SetStatusBarMessage("U_Excise UDF დასამატებელია",
                            BoMessageTime.bmt_Short, true);
                        return;
                    }
                    double excise = double.Parse(exciseString, CultureInfo.InvariantCulture);
                    if (string.IsNullOrWhiteSpace(exciseString) || excise == 0)
                    {
                        continue;
                    }

                    double quantity = double.Parse(quantityString, CultureInfo.InvariantCulture);
                    double fullExcise = Math.Round(quantity * excise, 6);
                    if (invoiceDi.CancelStatus == CancelStatusEnum.csCancellation)
                    {
                        string result = DiManager.AddJournalEntryCredit(DiManager.Company, exciseAccount, glRevenueAccount, -fullExcise, series, invNumber + " "+ $"{itemCode}", glRevenueAccount, postingDate, bplId, currency);
                    }
                    else
                    {
                        string result = DiManager.AddJournalEntryCredit(DiManager.Company, exciseAccount, glRevenueAccount, fullExcise, series, invNumber + " " + $"{itemCode}", glRevenueAccount, postingDate, bplId, currency);
                    }
                }
            }
        }



        private void OnCustomInitialize()
        {

        }

        private Button Button1;
        private string glRevenueAccount;
        private string exciseAccount;

        private void Button1_PressedBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            Form invoice = Application.SBO_Application.Forms.ActiveForm;
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
            Recordset recSetAct =
                (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes
                    .BoRecordset);
            recSetAct.DoQuery(DiManager.QueryHanaTransalte($"SELECT * FROM [@RSM_EXCP]"));

            exciseAccount = recSetAct.Fields.Item("U_ExciseAcc").Value.ToString();



            if (string.IsNullOrWhiteSpace(exciseAccount))
            {
                Application.SBO_Application.SetStatusBarMessage("აქციზის ანგარიში არაა მითითებული (General Settings)",
                    BoMessageTime.bmt_Short, true);
                BubbleEvent = false;
                return;
            }
        }
    }
}
