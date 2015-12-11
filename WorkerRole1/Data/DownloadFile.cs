// download the data file from the PPR website
// convert the file (csv format) to a list of record objects for sorting

using FileHelpers; // file helpers library
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using WorkerRole1.Records;

namespace WorkerRole1.Data
{
    public class DownloadFile
    {
        public string url = "";
        public DownloadFile(string year)
        {
            url = "https://www.propertypriceregister.ie/website/npsra/ppr/npsra-ppr.nsf/downloads/ppr-"+year+".csv/$file/ppr-"+year+".csv";
        }

        // download data file from ppr website
        public string Download()
        {
            string bad = null;
            try
            {
                string tempFilePathWithFileName = Path.GetTempFileName();
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, tempFilePathWithFileName);
                }
                return tempFilePathWithFileName;
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: downloading PPR data file: "+e.Message);
                return bad;
            }
            catch (WebException we)
            {
                Console.WriteLine("ERROR: downloading PPR data file: "+we.Message);
                return bad;
            }
        }

        // covert the downloaded csv file to a list of record objects
        public List<Record> ConvertFile(string filename)
        {
            List<Record> recordList = new List<Record>();
            try
            {
                var engine = new FileHelperEngine<Record>();
                // switch error mode on - skip any records that cause error and save them
                engine.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;
                var records = engine.ReadFile(filename);
                // if any errors send them to console/log
                string tempFile = Path.GetTempFileName(); // create temp file
                if (engine.ErrorManager.HasErrors)
                {
                    engine.ErrorManager.SaveErrors(tempFile);
                }
                    
                ErrorInfo[] errors = ErrorManager.LoadErrors(tempFile);
                foreach (var err in errors)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error on Line number: {0}", err.LineNumber);
                    Console.WriteLine("Record causing the problem: {0}", err.RecordString);
                    Console.WriteLine("Complete exception information: {0}", err.ExceptionInfo.ToString());
                }
                File.Delete(tempFile); // delete temp file
                // add record objects to list
                foreach (var record in records)
                {
                    recordList.Add(record);
                }
                return recordList;
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: converting PPR data file: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: converting PPR data file: " + e.Message);
                return null;
            }

        }

    }
}
