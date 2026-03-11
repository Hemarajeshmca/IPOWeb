using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using System.Web.Helpers;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
//using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using Microsoft.Extensions.Configuration;


namespace SkybirdAdminWeb.Controllers
{
	public static class ExcelHelper
	{
        private const string noRecordsToDisplay = "No records to display";

        public static byte[] ExportToText(DataSet dataSet,String? fromDate = "", String? toDate = "", String? fileType = "Text",String? sFilepath="")
        {
            if (!Directory.Exists(sFilepath))
            {
                // If the folder does not exist, create it
                Directory.CreateDirectory(sFilepath);
            }
            string sFilepathtxt = sFilepath + "\\demo.txt";
            string sFilepathpdf = sFilepath + "\\demo.pdf";
            TextWriter txt = new StreamWriter(sFilepathtxt);
            try
            {
                byte[] byteResult = null;

                //if (dataSet == null) { return byteResult; }
                
                if (dataSet.Tables.Count > 0)
                {

                    // string tab = "\t";
                    string space = new string(' ', 8);

                    foreach (DataTable dt in dataSet.Tables)
                    {
                        if (dt.Rows.Count >= 1)
                        {

                            //DataTable totalDtCDSL = dt.Select("Depository = 'C' AND Type = 'DEMAT'").CopyToDataTable();
                            var rowsCDSL = dt.AsEnumerable().Where(x => x.Field<string>("Depository") != "N" && x.Field<string>("Type") == "DEMAT");
                            //DataTable totalReDtNSDL = dt.Select("Depository = 'N' AND Type = 'REMAT'").CopyToDataTable();
                            DataTable totalDtCDSL = rowsCDSL.Any() ? rowsCDSL.CopyToDataTable() : null;
                            if (totalDtCDSL != null)
                            {
                                string total = totalDtCDSL.Compute("Sum(Share)", "").ToString();

                                //This is for Demat CDSL
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    DEMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO CDSL  FROM  "+fromDate+"  TO  "+toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno Dist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                DataRow[] rsltCDSL = dt.Select("Depository = 'C' AND Type = 'DEMAT'");
                                if (rsltCDSL.Length >= 1)
                                {


                                    foreach (DataRow row in rsltCDSL)
                                    {
                                        txt.Write(row[2].ToString().PadRight(10, ' ') + space + row[3].ToString().PadRight(10, ' ') + space + row[4].ToString().PadRight(10, ' ') + space + row[5].ToString().PadRight(10, ' ') + space + row[6].ToString().PadRight(10, ' ') + space + row[7].ToString().PadRight(10, ' ') + space + row[8].ToString().PadRight(10, ' ') + space + row[9].ToString().PadRight(10, ' '));
                                        txt.Write('\n');
                                    }
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write("Total : " + total);
                                    txt.Write('\n');
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write('\n');
                                }
                            }
                            else
                            {
                                //total = "0";

                                //This is for Demat CDSL
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    DEMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO CDSL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno Dist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');

                                txt.Write("Nil");
                                txt.Write('\n');

                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write("Total : 0.00");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write('\n');

                                //End of Demat CDSL
                            }

                            //End of Demat CDSL

                            //This is for Remat CDSL

                            var rows = dt.AsEnumerable().Where(x => x.Field<string>("Depository") != "C" && x.Field<string>("Type") == "REMAT");
                            DataTable totalReDtCDSL = rows.Any() ? rows.CopyToDataTable() : null;
                            if (totalReDtCDSL != null)
                            {
                                string totalRe = totalReDtCDSL.Compute("Sum(Share)", "").ToString();
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    REMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO CDSL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno Dist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                DataRow[] rsltReCDSL = dt.Select("Depository = 'C' AND Type = 'REMAT'");
                                if (rsltReCDSL.Length >= 1)
                                {

                                    foreach (DataRow row in rsltReCDSL)
                                    {
                                        txt.Write(row[2].ToString().PadRight(10, ' ') + space + row[3].ToString().PadRight(10, ' ') + space + row[4].ToString().PadRight(10, ' ') + space + row[5].ToString().PadRight(10, ' ') + space + row[6].ToString().PadRight(10, ' ') + space + row[7].ToString().PadRight(10, ' ') + space + row[8].ToString().PadRight(10, ' ') + space + row[9].ToString().PadRight(10, ' '));
                                        txt.Write('\n');
                                    }
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write("Total : " + totalRe);
                                    txt.Write('\n');
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write('\n');
                                }
                            }
                            else
                            {
                                //total = "0";

                                //This is for Remat CDSL
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    REMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO CDSL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno Dist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');

                                txt.Write("Nil");
                                txt.Write('\n');

                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write("Total : 0.00");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write('\n');

                                //End of Demat RematCDSL
                            }

                            //End of Remat CDSL


                            //This is for Demat NSDL
                            //DataTable totalDtNSDL = dt.Select("Depository = 'N' AND Type = 'DEMAT'").CopyToDataTable();
                            var rows1 = dt.AsEnumerable().Where(x => x.Field<string>("Depository") != "N" && x.Field<string>("Type") == "DEMAT");
                            //DataTable totalReDtNSDL = dt.Select("Depository = 'N' AND Type = 'REMAT'").CopyToDataTable();
                            DataTable totalDtNSDL = rows.Any() ? rows1.CopyToDataTable() : null;
                            if (totalDtNSDL != null)
                            {
                                string totalNSDL = totalDtNSDL.Compute("Sum(Share)", "").ToString();
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    DEMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO NSDL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno\tDist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                DataRow[] rsltNSDL = dt.Select("Depository = 'N' AND Type = 'DEMAT'");
                                if (rsltNSDL.Length > 0)
                                {
                                    foreach (DataRow row in rsltNSDL)
                                    {
                                        //txt.Write(row[2] + "\t" + row[3] + "\t" + row[4] + "\t" + row[5] + "\t" + row[6] + "\t" + row[7] + "\t" + row[8] + "\t" + row[9]);
                                        txt.Write(row[2].ToString().PadRight(10, ' ') + space + row[3].ToString().PadRight(10, ' ') + space + row[4].ToString().PadRight(10, ' ') + space + row[5].ToString().PadRight(10, ' ') + space + row[6].ToString().PadRight(10, ' ') + space + row[7].ToString().PadRight(10, ' ') + space + row[8].ToString().PadRight(10, ' ') + space + row[9].ToString().PadRight(10, ' '));
                                        txt.Write('\n');
                                    }
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write("Total : " + totalNSDL);
                                    txt.Write('\n');
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write('\n');
                                }
                            }
                            else
                            {
                                //This is for Demat NSDL

                                // totalNSDL = "0";
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    DEMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO NSDL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno\tDist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');

                                //txt.Write(row[2] + "\t" + row[3] + "\t" + row[4] + "\t" + row[5] + "\t" + row[6] + "\t" + row[7] + "\t" + row[8] + "\t" + row[9]);
                                txt.Write("Nil");
                                txt.Write('\n');

                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write("Total : 0.00");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write('\n');
                                //End of Demat NSDL
                            }
                            //End of Demat NSDL


                            //This is for Remat NSDL
                            var rows1RNSDL = dt.AsEnumerable().Where(x => x.Field<string>("Depository") != "N" && x.Field<string>("Type") == "REMAT");
                            //DataTable totalReDtNSDL = dt.Select("Depository = 'N' AND Type = 'REMAT'").CopyToDataTable();
                            DataTable totalReDtNSDL = rows1RNSDL.Any() ? rows1RNSDL.CopyToDataTable() : null;
                            if (totalReDtNSDL != null)
                            {
                                string totalReNSDL = totalReDtNSDL.Compute("Sum(Share)", "").ToString();
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    REMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO NSDL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno\tDist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                DataRow[] rsltReNSDL = dt.Select("Depository = 'N' AND Type = 'DEMAT'");
                                if (rsltReNSDL.Length > 0)
                                {
                                    foreach (DataRow row in rsltReNSDL)
                                    {
                                        //txt.Write(row[2] + "\t" + row[3] + "\t" + row[4] + "\t" + row[5] + "\t" + row[6] + "\t" + row[7] + "\t" + row[8] + "\t" + row[9]);
                                        txt.Write(row[2].ToString().PadRight(10, ' ') + space + row[3].ToString().PadRight(10, ' ') + space + row[4].ToString().PadRight(10, ' ') + space + row[5].ToString().PadRight(10, ' ') + space + row[6].ToString().PadRight(10, ' ') + space + row[7].ToString().PadRight(10, ' ') + space + row[8].ToString().PadRight(10, ' ') + space + row[9].ToString().PadRight(10, ' '));
                                        txt.Write('\n');
                                    }
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write("Total : " + totalReNSDL);
                                    txt.Write('\n');
                                    txt.Write("====================================================================================================");
                                    txt.Write('\n');
                                    txt.Write('\n');
                                }
                            }
                            else
                            {
                                //This is for Remat NSDL

                                //totalNSDL = "0";
                                txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    REMAT    Date : " + DateTime.Now.ToShortDateString());
                                txt.Write('\n');
                                txt.Write("LIST OF CASES CONFIRMED TO NSDL  FROM  " + fromDate + "  TO  " + toDate);
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                //txt.Write("Folio No\tDrnno\tGnsainwdno\tDist From\tDist To\tCert no\tShares\tApproved_date");
                                txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');

                                //txt.Write(row[2] + "\t" + row[3] + "\t" + row[4] + "\t" + row[5] + "\t" + row[6] + "\t" + row[7] + "\t" + row[8] + "\t" + row[9]);
                                txt.Write("Nil");
                                txt.Write('\n');

                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write("Total : 0.00");
                                txt.Write('\n');
                                txt.Write("====================================================================================================");
                                txt.Write('\n');
                                txt.Write('\n');
                                //End of Remat NSDL
                            }
                            //End of Remat NSDL                        
                        }
                        else // Print the nil values
                        {
                            string total = "0";

                            //This is for Demat CDSL
                            txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    DEMAT    Date : " + DateTime.Now.ToShortDateString());
                            txt.Write('\n');
                            txt.Write("LIST OF CASES CONFIRMED TO CDSL  FROM  " + fromDate + "  TO  " + toDate);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            //txt.Write("Folio No\tDrnno\tGnsainwdno Dist From\tDist To\tCert no\tShares\tApproved_date");
                            txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');

                            txt.Write("Nil");
                            txt.Write('\n');

                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write("Total : " + total);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write('\n');

                            //End of Demat CDSL

                            //This is For Remat CDSL                        
                            txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    REMAT    Date : " + DateTime.Now.ToShortDateString());
                            txt.Write('\n');
                            txt.Write("LIST OF CASES CONFIRMED TO CDSL  FROM  " + fromDate + "  TO  " + toDate);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            //txt.Write("Folio No\tDrnno\tGnsainwdno Dist From\tDist To\tCert no\tShares\tApproved_date");
                            txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');

                            txt.Write("Nil");
                            txt.Write('\n');

                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write("Total : " + total);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write('\n');

                            //End of Remat CDSL

                            //This is for Demat NSDL


                            txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    DEMAT    Date : " + DateTime.Now.ToShortDateString());
                            txt.Write('\n');
                            txt.Write("LIST OF CASES CONFIRMED TO NSDL  FROM  " + fromDate + "  TO  " + toDate);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            //txt.Write("Folio No\tDrnno\tGnsainwdno\tDist From\tDist To\tCert no\tShares\tApproved_date");
                            txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');

                            //txt.Write(row[2] + "\t" + row[3] + "\t" + row[4] + "\t" + row[5] + "\t" + row[6] + "\t" + row[7] + "\t" + row[8] + "\t" + row[9]);
                            txt.Write("Nil");
                            txt.Write('\n');

                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write("Total : " + total);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write('\n');
                            //End of Demat NSDL

                            //This is for Remat NSDL


                            txt.Write("Unit : LOYAL TEXTILE MILLS LTD   -    REMAT    Date : " + DateTime.Now.ToShortDateString());
                            txt.Write('\n');
                            txt.Write("LIST OF CASES CONFIRMED TO NSDL  FROM   " + fromDate + "  TO  " + toDate);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            //txt.Write("Folio No\tDrnno\tGnsainwdno\tDist From\tDist To\tCert no\tShares\tApproved_date");
                            txt.Write("Folio No" + space.PadRight(10, ' ') + "Drnno" + space.PadRight(10, ' ') + "Gnsainwdno" + space.PadRight(10, ' ') + "Dist From" + space.PadRight(10, ' ') + "Dist To" + space.PadRight(10, ' ') + "Cert no" + space.PadRight(10, ' ') + "Shares" + space.PadRight(10, ' ') + "Approved_date");
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');

                            //txt.Write(row[2] + "\t" + row[3] + "\t" + row[4] + "\t" + row[5] + "\t" + row[6] + "\t" + row[7] + "\t" + row[8] + "\t" + row[9]);
                            txt.Write("Nil");
                            txt.Write('\n');

                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write("Total : " + total);
                            txt.Write('\n');
                            txt.Write("====================================================================================================");
                            txt.Write('\n');
                            txt.Write('\n');
                            //End of Remat NSDL
                        }
                        //End of Demat NSDL
                    }
                    txt.Close();
                }
                //Writing as PDF file
                StreamReader rdr = new StreamReader(sFilepathtxt);

                //Create a New instance on Document Class

                Document doc = new Document();

                BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);

                //Create a New instance of PDFWriter Class for Output File

                PdfWriter.GetInstance(doc, new FileStream(sFilepathpdf, FileMode.Create));

                //Open the Document

                doc.Open();

                //Add the content of Text File to PDF File

                doc.Add(new Paragraph(rdr.ReadToEnd(), font));

                //Close the Document

                doc.Close();
                rdr.Close();
                //End of Writing in PDF
               // var stream = new FileStream(@"C:\demo\demo.txt", FileMode.Open);
				//var stream = new FileStream(@"C:\demo\demo.pdf", FileMode.Open);
				if (fileType == "Text")
                {
					var stream = new FileStream(sFilepathtxt, FileMode.Open);
					stream.Flush();
					stream.Position = 0;

					byteResult = new byte[stream.Length];
					stream.Read(byteResult, 0, byteResult.Length);
					stream.Close();
				}
                else
                {
					var stream = new FileStream(sFilepathpdf, FileMode.Open);
					stream.Flush();
					stream.Position = 0;

					byteResult = new byte[stream.Length];
					stream.Read(byteResult, 0, byteResult.Length);
					stream.Close();
				}
                
                //return new FileStreamResult(stream, "application/pdf");
                /*stream.Flush();
                stream.Position = 0;

                byteResult = new byte[stream.Length];
                stream.Read(byteResult, 0, byteResult.Length);
                stream.Close();*/
                txt.Close();
                return byteResult;
                // return byteResult;
            }
            catch (Exception ex)
            {
                txt.Close();
                throw new Exception(ex.InnerException.Message, ex);
            }
        }

        public static byte[] ExportToExcelDownload(DataSet dataSet, String? fromDate = "", String? toDate = "")
		{
			byte[] byteResult = null;
			if (dataSet == null) { return byteResult; }

			if (dataSet.Tables.Count > 0)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
					{
						// Add a WorkbookPart to the document.
						WorkbookPart workbookpart = AddWorkbookPart(spreadsheetDocument);
						AddSheet(spreadsheetDocument, out Sheets sheets, out uint currentSheetID);
						AddNewPartStyle(workbookpart);

						int rowIndexCount = 1;
                        //ExportToText(dataSet);
                        foreach (DataTable dt in dataSet.Tables)
						{
							// Add a WorksheetPart to the WorkbookPart.
							WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
							worksheetPart.Worksheet = new Worksheet();
							Columns columns = SetDefaultColumnWidth();
							worksheetPart.Worksheet.Append(columns);

							SheetData sheetData = new SheetData();
							worksheetPart.Worksheet.AppendChild(sheetData);

							// Append a new worksheet and associate it with the workbook.
							Sheet sheet = new Sheet()
							{
								Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
								SheetId = currentSheetID,
								Name = string.IsNullOrWhiteSpace(dt.TableName) ? "Sheet" + currentSheetID : dt.TableName
							};

							if (dt.Rows.Count == 0)
							{
								//if table rows count is 0, create Excel Sheet with default message
								CreateDefaultWithMessage(rowIndexCount, sheetData);
								int numberOfColumns = dt.Columns.Count;
								string[] excelColumnNames = new string[numberOfColumns];

								//Create Header
								Row SheetrowHeader = CreateHeader(rowIndexCount, dt, numberOfColumns, excelColumnNames);
								sheetData.Append(SheetrowHeader);
								++rowIndexCount;

								//Create Body
								rowIndexCount = CreateBody(rowIndexCount, dt, sheetData, excelColumnNames);
							}
							else
							{
								int numberOfColumns = dt.Columns.Count;
								string[] excelColumnNames = new string[numberOfColumns];

								//Create Header
								Row SheetrowHeader = CreateHeader(rowIndexCount, dt, numberOfColumns, excelColumnNames);
								sheetData.Append(SheetrowHeader);
								++rowIndexCount;

								//Create Body
								rowIndexCount = CreateBody(rowIndexCount, dt, sheetData, excelColumnNames);
							}

							sheets.Append(sheet);

							++currentSheetID;

							rowIndexCount = 1;
						}
						workbookpart.Workbook.Save();

						// Close the document.
						//spreadsheetDocument.Close();

					}

					stream.Flush();
					stream.Position = 0;

					byteResult = new byte[stream.Length];
					stream.Read(byteResult, 0, byteResult.Length);
				}
			}
			return byteResult;
		}

		//Customize column width
		private static Columns SetDefaultColumnWidth()
		{
			Columns columns = new Columns();
			//width of 1st Column
			columns.Append(new Column() { Min = 1, Max = 100, Width = 20, CustomWidth = true });
			////with of 2st Column
			//columns.Append(new Column() { Min = 2, Max = 2, Width = 50, CustomWidth = true });
			////set column width from 3rd to 400 columns
			//columns.Append(new Column() { Min = 3, Max = 400, Width = 10, CustomWidth = true });
			return columns;
		}

		private static void AddNewPartStyle(WorkbookPart workbookpart)
		{
			WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
			stylePart.Stylesheet = GenerateStylesheet();
			stylePart.Stylesheet.Save();
		}

		private static void AddSheet(SpreadsheetDocument spreadsheetDocument, out Sheets sheets, out uint currentSheetID)
		{
			sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
			currentSheetID = 1;
		}

		private static WorkbookPart AddWorkbookPart(SpreadsheetDocument spreadsheetDocument)
		{
			WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
			workbookpart.Workbook = new Workbook();
			return workbookpart;
		}

		private static void CreateDefaultWithMessage(int rowIndexCount, SheetData sheetData)
		{
			Row Sheetrow = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
			Cell cellHeader = new Cell() { CellReference = "A1", CellValue = new CellValue(noRecordsToDisplay), DataType = CellValues.String };
			cellHeader.StyleIndex = 1;

			Sheetrow.Append(cellHeader);
			sheetData.Append(Sheetrow);
		}

		private static int CreateBody(int rowIndexCount, DataTable dt, SheetData sheetData, string[] excelColumnNames)
		{
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				Row Sheetrow = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
				for (int j = 0; j < dt.Columns.Count; j++)
				{
					// insert value in cell with dataType (String, Int, decimal, datatime)
					Sheetrow.Append(GetCellWithDataType(excelColumnNames[j] + rowIndexCount, dt.Rows[i][j], dt.Columns[j].DataType));
				}
				sheetData.Append(Sheetrow);
				++rowIndexCount;
			}

			return rowIndexCount;
		}

		private static Row CreateHeader(int rowIndexCount, DataTable dt, int numberOfColumns, string[] excelColumnNames)
		{
			Row SheetrowHeader = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
			for (int n = 0; n < numberOfColumns; n++)
			{
				excelColumnNames[n] = GetExcelColumnName(n);

				Cell cellHeader = new Cell() { CellReference = excelColumnNames[n] + rowIndexCount, CellValue = new CellValue(dt.Columns[n].ColumnName), DataType = CellValues.String };
				cellHeader.StyleIndex = 2;				
				SheetrowHeader.Append(cellHeader);
			}

			return SheetrowHeader;
		}

		private static string GetExcelColumnName(int columnIndex)
		{
			if (columnIndex < 26)
			{
				return ((char)('A' + columnIndex)).ToString();
			}

			char firstChar = (char)('A' + (columnIndex / 26) - 1);
			char secondChar = (char)('A' + (columnIndex % 26));

			return string.Format(CultureInfo.CurrentCulture, "{0}{1}", firstChar, secondChar);
		}

		private static Stylesheet GenerateStylesheet()
		{
			Fonts fonts = GenerateFonts();
			Fills fills = GenerateFills();
			Borders borders = GenerateBorders();
			CellFormats cellFormats = GenerateCellFormats();
			Column column = GenerateColumnProperty();
			Stylesheet styleSheet = new Stylesheet(fonts, fills, borders, cellFormats, column);

			return styleSheet;
		}

		private static Column GenerateColumnProperty()
		{
			return new Column
			{
				Width = 100,
				CustomWidth = true
			};
		}

		private static CellFormats GenerateCellFormats()
		{
			CellFormats cellFormats = new CellFormats(
				// default - Cell StyleIndex = 0 
				new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }),

				// default2 - Cell StyleIndex = 1
				new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true },

				// header - Cell StyleIndex = 2
				new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 1, FillId = 0, BorderId = 1, ApplyFill = true },

				// DateTime DataType - Cell StyleIndex = 3
				new CellFormat(new Alignment() { Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 15, ApplyNumberFormat = true },

				// int,long,short DataType - Cell StyleIndex = 4
				new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 1 },

				// decimal DataType  - Cell StyleIndex = 5
				new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 2 }
				);
			return cellFormats;
		}

		private static Borders GenerateBorders()
		{
			Borders borders = new Borders(
				// index 0 default
				new Border(),

				// index 1 black border
				new Border(
					new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
					new DiagonalBorder())
				);
			return borders;
		}

		private static Fills GenerateFills()
		{
			Fills fills = new Fills(
				// Index 0
				new Fill(new PatternFill() { PatternType = PatternValues.None }),

				// Index 1
				new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }),

				// Index 2 - header
				new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "66666666" } }) { PatternType = PatternValues.Solid })
				);
			return fills;
		}

		private static Fonts GenerateFonts()
		{
			Fonts fonts = new Fonts(
				// Index 0 - default
				new DocumentFormat.OpenXml.Spreadsheet.Font(
					new FontSize() { Val = 10 },
					new FontName() { Val = "Arial Unicode" }
				),

				// Index 1 - header
				new DocumentFormat.OpenXml.Spreadsheet.Font(
					new FontSize() { Val = 10 },
					new Bold()//,

				//new Color() { Rgb = "FFFFFF" }

				));
			return fonts;
		}

		private static Cell GetCellWithDataType(string cellRef, object value, Type type)
		{
			if (type == typeof(DateTime))
			{
				Cell cell = new Cell()
				{
					DataType = new EnumValue<CellValues>(CellValues.Number),
					StyleIndex = 3
				};

				if (value != DBNull.Value)
				{
					System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
					DateTime valueDate = (DateTime)value;
					string valueString = valueDate.ToOADate().ToString(cultureinfo);
					CellValue cellValue = new CellValue(valueString);
					cell.Append(cellValue);
				}

				return cell;
			}
			if (type == typeof(long) || type == typeof(int) || type == typeof(short))
			{
				Cell cell = new Cell() { CellReference = cellRef, CellValue = new CellValue(value.ToString()), DataType = CellValues.Number };
				cell.StyleIndex = 4;
				return cell;
			}
			if (type == typeof(decimal))
			{
				Cell cell = new Cell() { CellReference = cellRef, CellValue = new CellValue(value.ToString()), DataType = CellValues.Number };
				cell.StyleIndex = 5;
				return cell;
			}
			else
			{
				Cell cell = new Cell() { CellReference = cellRef, CellValue = new CellValue(value.ToString()), DataType = CellValues.String };
				cell.StyleIndex = 1;
				return cell;
			}
		}
	}
}
