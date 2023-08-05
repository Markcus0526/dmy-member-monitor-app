using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Printing;
using System.Threading;
using INMC.INMMessage;
using System.Management;
using System.Diagnostics;
using System.IO;

namespace AgentEngine
{
    public class PrintTracker
    {
        object printobj = new object();

        ArrayList curPrintJobs = new ArrayList();
        ArrayList printHistory = new ArrayList();

        // List for Saving File History
        private int nCurSId = 0;
        private const String strHisFileName = "INMPrintHis_";

        #region CONSTRUCTOR
        public PrintTracker()
        {
            nCurSId = 0;

            RefreshPrintJobs();

            Process CurProcess = Process.GetCurrentProcess();
            nCurSId = CurProcess.SessionId;

            RestorePrintHistoryFromFile();
        }
        #endregion

        #region PUBLIC_MEMBERS
        public void RefreshPrinterState()
        {
            Monitor.Enter(printobj);

            ArrayList prevJobList = new ArrayList();
            foreach (M5PrintItem item in curPrintJobs)
            {
                M5PrintItem newitem = new M5PrintItem(item);
                prevJobList.Add(newitem);
            }

            RefreshPrintJobs();

            foreach (M5PrintItem item in curPrintJobs)
            {
                int nCount = prevJobList.Count;

                for (int i = 0; i < nCount; i++)
                {
                    M5PrintItem prevItem = (M5PrintItem)prevJobList[i];
                    if (prevItem.Printer == item.Printer &&
                        prevItem.PrintFile == item.PrintFile)
                    {
                        prevItem.PrintFile = "";
                    }
                }
            }

            foreach (M5PrintItem item in prevJobList)
            {
                if (item.PrintFile != "")
                {
                    if (IsItemInArrayList(item, printHistory) == false)
                    {
                        M5PrintItem newitem = new M5PrintItem(item);
                        printHistory.Add(newitem);
                        AddPrintHistoryToFile(newitem);
                    }
                }
            }

            Monitor.Exit(printobj);
        }

        public void GetPrinterHistory(ArrayList printHis)
        {
            Monitor.Enter(printobj);
            printHis.Clear();

            foreach (M5PrintItem item in printHistory)
            {
                M5PrintItem newitem = new M5PrintItem(item);
                printHis.Add(newitem);
            }

            ClearPrintHistory();
            Monitor.Exit(printobj);
        }
        #endregion

        #region PRIVATE_MEMBERS
        private void RefreshPrintJobs()
        {
            curPrintJobs.Clear();

            string searchQuery = "SELECT * FROM Win32_PrintJob";

            /*searchQuery can also be mentioned with where Attribute,
                but this is not working in Windows 2000 / ME / 98 machines 
                and throws Invalid query error*/
            ManagementObjectSearcher searchPrintJobs =
                      new ManagementObjectSearcher(searchQuery);
            ManagementObjectCollection prntJobCollection = searchPrintJobs.Get();
            foreach (ManagementObject prntJob in prntJobCollection)
            {
                System.String jobName = prntJob.Properties["Name"].Value.ToString();

                //Job name would be of the format [Printer name], [Job ID]
                char[] splitArr = new char[1];
                splitArr[0] = Convert.ToChar(",");
                string prnterName = jobName.Split(splitArr)[0];
                string documentName = prntJob.Properties["Document"].Value.ToString();

                M5PrintItem newitem = new M5PrintItem();
                newitem.Printer = prnterName;
                newitem.PrintFile = documentName;
                newitem.PrintPath = "";
                newitem.PrintTime = DateTime.Now;

                curPrintJobs.Add(newitem);
            }
        }

        public bool IsItemInArrayList(M5PrintItem item, ArrayList itemList)
        {
            bool bExist = false;

            foreach (M5PrintItem listitem in itemList)
            {
                if (listitem.Printer == item.Printer &&
                    listitem.PrintFile == item.PrintFile &&
                    listitem.PrintPath == item.PrintPath &&
                    listitem.PrintTime == item.PrintTime)
                {
                    bExist = true;
                    break;
                }
            }

            return bExist;
        }

        public void RestorePrintHistoryFromFile()
        {
            String strDelimiter = "|";
            printHistory.Clear();

            try
            {
                StreamReader writeStream = File.OpenText(System.Environment.SystemDirectory + "\\" + strHisFileName + nCurSId);

                while (writeStream.EndOfStream == false)
                {
                    M5PrintItem printItem = new M5PrintItem();
                    String strLine = writeStream.ReadLine();

                    int nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Printer Name
                    printItem.Printer = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get PrintTime
                    printItem.PrintTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Print FileName
                    printItem.PrintFile = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                    {
                        // Get Print Path
                        printItem.PrintPath = strLine;
                    }
                    else
                    {
                        // Get Print Path
                        printItem.PrintPath = strLine.Substring(0, nKeyPos);
                    }

                    printHistory.Add(printItem);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void AddPrintHistoryToFile(M5PrintItem printItem)
        {
            try
            {
                StreamWriter writeStream = File.AppendText(System.Environment.SystemDirectory + "\\" + strHisFileName + nCurSId);

                String strLine = "";
                strLine = printItem.Printer + "|" + printItem.PrintTime.ToString() + "|" + printItem.PrintFile + "|" + printItem.PrintPath + "\r\n";
                writeStream.Write(strLine);
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void ClearPrintHistory()
        {
            printHistory.Clear();

            try
            {
                StreamWriter writeStream = File.CreateText(System.Environment.SystemDirectory + "\\" + strHisFileName + nCurSId);
                writeStream.Write("");
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }
        #endregion
    }
}
