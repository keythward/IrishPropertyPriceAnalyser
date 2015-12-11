using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using WorkerRole1.Model;
using WorkerRole1.DatabaseConnections;
using HtmlAgilityPack; // html agility pack
using WorkerRole1.Records;
using WorkerRole1.Data;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public static DateTime dateTimeLastUpdated;
        public static DateTime dateTimeUpdatedTo;

        // list of counties
        public enum County
        {
            Kerry, Cork, Limerick, Tipperary, Waterford, Kilkenny, Wexford, Laois, Carlow, Kildare, Wicklow,
            Offaly, Dublin, Meath, Westmeath, Louth, Monaghan, Cavan, Longford, Donegal, Leitrim, Sligo, Roscommon, Mayo, Galway, Clare
        };

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            Console.WriteLine("running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");
            Console.WriteLine("started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");
            Console.WriteLine("stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // check if new documents need to be created
            // documents created on first of every month for dublin and 1st of january and july for rest of country
            if (DateTime.Now.Day == 1)
            {
                string year = DateTime.Now.Year.ToString()+"_";
                List<AlteredRecord> list = new List<AlteredRecord>();
                if (DateTime.Now.Month == 1)
                {
                    for (County co = County.Kerry; co <= County.Clare; co++)
                    {
                        if (co == County.Dublin)
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), list);
                            db.CreateDocument(year+"1");
                        }
                        else
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), list);
                            db.CreateDocument(year + "A");
                        }
                    }

                }
                else if (DateTime.Now.Month == 2)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "2");
                }
                else if (DateTime.Now.Month == 3)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "3");
                }
                else if (DateTime.Now.Month == 4)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "4");
                }
                else if (DateTime.Now.Month == 5)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "5");
                }
                else if (DateTime.Now.Month == 6)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "6");
                }
                else if(DateTime.Now.Month == 7)
                {
                    for (County co = County.Kerry; co <= County.Clare; co++)
                    {
                        if (co == County.Dublin)
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), list);
                            db.CreateDocument(year + "7");
                        }
                        else
                        {
                            DatabaseConnect db = new DatabaseConnect(co.ToString(), list);
                            db.CreateDocument(year + "B");
                        }
                    }
                }
                else if (DateTime.Now.Month == 8)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "8");
                }
                else if (DateTime.Now.Month == 9)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "9");
                }
                else if (DateTime.Now.Month == 10)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "10");
                }
                else if (DateTime.Now.Month == 11)
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "11");
                }
                else 
                {
                    DatabaseConnect db = new DatabaseConnect("Dublin", list);
                    db.CreateDocument(year + "12");
                }
            }
            // get dates from database document and check them against the websites dates to see if update available
            UpdateDates update = DatabaseDates.ReadDatesDocument().Result;
            dateTimeLastUpdated = update.lastUpdate;
            dateTimeUpdatedTo = update.updatedTo;
            if (UpdateAvailable(update.lastUpdate))
            {
                // download file
                DownloadFile dlf = new DownloadFile(dateTimeUpdatedTo.Year.ToString());
                string success = dlf.Download(); // returns null if no download or filename if success
                if (success != null)
                {
                    Console.WriteLine("PPR file downloaded sucessfully");
                    List<Record> recordList = dlf.ConvertFile(success); // returns list of record objects for sorting
                    if(recordList!=null || recordList.Count > 0) // list is null if has thrown exception or could be empty
                    {
                        CleanUpRecords cur = new CleanUpRecords();
                        List<AlteredRecord> alteredRecords= cur.CleanRecords(recordList); // clean records and return an altered record list
                        AddToDatabase atb = new AddToDatabase(); 
                        atb.AddList(alteredRecords,dateTimeUpdatedTo); // send list of to be broken down into counties and added to database documents
                    }
                }
            }
            // close the program
            OnStop();
        }

        // check if an update is available
        public bool UpdateAvailable(DateTime lastTimeProjectUpdated)
        {
            Console.WriteLine("checking for website update on: " + DateTime.Now.ToString());
            bool updateAvailable = false;
            string url = "https://www.propertypriceregister.ie/website/npsra/pprweb.nsf/PPRDownloads?OpenForm";
            try
            {
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);
                var node = doc.DocumentNode.SelectSingleNode("//span[@id='LastUpdated']");
                if (node != null)
                {
                    var innerText = node.InnerText;
                    DateTime lastTimeWebsiteUpdated = Convert.ToDateTime(innerText);
                    if (!DateTime.Equals(lastTimeWebsiteUpdated, lastTimeProjectUpdated))
                    {
                        updateAvailable = true;
                        Console.WriteLine("website update found");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: node for updated website date is empty");
                }
            }
            catch (WebException ex) // exception thrown if url not found
            {
                Console.WriteLine("URL not found exception: " + ex.Message);
            }
            catch(FormatException ex) // format is not datetime
            {
                Console.WriteLine("ERROR: node format does not match a DateTime format: " + ex.Message);
            }
            return updateAvailable;
        }
    }
}
