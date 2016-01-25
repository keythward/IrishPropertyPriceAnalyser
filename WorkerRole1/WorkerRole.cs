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
        public static DateTime dateTimeLastUpdated; // date website last updated
        public static DateTime dateTimeUpdatedTo; // date documents updated too

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
                Trace.TraceInformation("Worker role running on first of month, creating new documents");
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
                // the main tasks of an update
                MainTasks(dateTimeUpdatedTo.Year.ToString());
            }
            // close the program
            OnStop();
        }

        // check if an update is available
        public bool UpdateAvailable(DateTime lastTimeProjectUpdated)
        {
            Console.WriteLine("checking for website update on: " + DateTime.Now.ToString());
            Trace.TraceInformation("checking for website update on: " + DateTime.Now.ToString());
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
                        Trace.TraceInformation("website update found");
                        dateTimeLastUpdated = lastTimeWebsiteUpdated; // change date of last website update
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: node for updated website date is empty");
                    Trace.TraceInformation("worker role ERROR: node for updated website date is empty");
                }
            }
            catch (WebException ex) // exception thrown if url not found
            {
                Console.WriteLine("URL not found exception: " + ex.Message);
                Trace.TraceInformation("worker role: URL not found exception: " + ex.Message);
            }
            catch(FormatException ex) // format is not datetime
            {
                Console.WriteLine("ERROR: node format does not match a DateTime format: " + ex.Message);
                Trace.TraceInformation("worker role ERROR: node format does not match a DateTime format: " + ex.Message);
            }
            return updateAvailable;
        }

        // the main tasks of an update
        public static void MainTasks(string year)
        {
            // download file
            DownloadFile dlf = new DownloadFile(year);
            string success = dlf.Download(); // returns null if no download or filename if success
            if (success != null)
            {
                Console.WriteLine("PPR file downloaded sucessfuly");
                Trace.TraceInformation("worker role: PPR file downloaded sucessfuly");
                List<Record> recordList = dlf.ConvertFile(success); // returns list of record objects for sorting
                if (recordList != null || recordList.Count > 0) // list is null if has thrown exception or could be empty
                {
                    CleanUpRecords cur = new CleanUpRecords();
                    List<AlteredRecord> alteredRecords = cur.CleanRecords(recordList); // clean records and return an altered record list
                    AddToDatabase atb = new AddToDatabase();
                    if (dateTimeUpdatedTo.Year.ToString().Equals(year)) // if for this year
                    {
                        atb.AddList(alteredRecords, dateTimeUpdatedTo); // send list of to be broken down into counties and added to database documents
                        AlterUpdateDates(); // change the dates in the update document to the new dates
                    }
                    else // if for last year
                    {
                        atb.AddListLastYear(alteredRecords,year); // send list of to be broken down into counties and added to database documents
                    }
                }
            }
        }

        // change the dates document
        public static void AlterUpdateDates()
        {
            UpdateDates update = new UpdateDates();
            update.lastUpdate = dateTimeLastUpdated;
            update.updatedTo = dateTimeUpdatedTo;
            update.id = "update_dates";
            if (update.updatedTo.Month == 12 && update.updatedTo.Day == 31) // if 31st december push to 1st january
            {
                update.updatedTo = update.updatedTo.AddDays(1);
            }
            DatabaseDates.ModifyDatesDocument(update);
        }
    }
}
