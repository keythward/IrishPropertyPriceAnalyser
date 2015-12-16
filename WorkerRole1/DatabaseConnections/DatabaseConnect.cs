// database connection, creates and updates

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents; // documentdb
using Microsoft.Azure.Documents.Client; // documentdb
using Microsoft.Azure.Documents.Linq; // documentdb
using System.Threading;
using WorkerRole1.Records;
using WorkerRole1.Model;
using System.Configuration;

namespace WorkerRole1.DatabaseConnections
{
    public class DatabaseConnect
    {
        // db connection strings
        private static string connStr = ConfigurationManager.ConnectionStrings["DatabaseConnectionString"].ConnectionString;
        //private static string connStr = RoleEnvironment.GetConfigurationSettingValue("DatabaseConnectionString");
        private static string EndpointUri = "https://ppr.documents.azure.com:443/";
        private static string DatabaseId = "ppr_database";
        private static string CollectionId = "ppr_records";
        private static DocumentClient client = new DocumentClient(new Uri(EndpointUri), connStr);
        // fields
        public DBRecord document;

        internal DatabaseConnect(string county, List<AlteredRecord> list)
        {
            List<ListObject> List = new List<ListObject>();
            foreach (AlteredRecord ar in list)
            {
                ListObject lo = new ListObject();
                lo.Address = ar.Address;
                lo.Description = ar.Description;
                lo.NotFullMP = ar.NotFullMP;
                lo.PostCode = ar.PostCode;
                lo.Price = ar.Price;
                lo.SoldOn = ar.SoldOn;
                List.Add(lo);
            }
            document = new DBRecord();
            document.id = county;
            document.records = List;
        }

        // create or return a database connection
        public static async Task<Database> GetDatabase(string databaseName)
        {
            if (client.CreateDatabaseQuery().Where(db => db.Id == databaseName).AsEnumerable().Any())
            {
                return client.CreateDatabaseQuery().Where(db => db.Id == databaseName).AsEnumerable().FirstOrDefault();
            }
            return await client.CreateDatabaseAsync(new Database { Id = databaseName });
        }

        // create or return a collection on a database
        public static async Task<DocumentCollection> GetCollection(Database database, string collName)
        {
            if (client.CreateDocumentCollectionQuery(database.SelfLink).Where(coll => coll.Id == collName).ToArray().Any())
            {
                return client.CreateDocumentCollectionQuery(database.SelfLink).Where(coll => coll.Id == collName).ToArray().FirstOrDefault();
            }
            return await client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collName });
        }

        // update a modified document
        public async Task<Document> UpdateDocument(DocumentCollection coll, DBRecord record)
        {
            var updateDone = false;
            Document doc = null;
            while (!updateDone)
            {
                try
                {
                    Console.WriteLine("updating document for: " + record.id + " on: " + DateTime.Now.ToString());
                    doc=await client.UpsertDocumentAsync(coll.SelfLink, record);
                    updateDone = true;
                }
                catch (DocumentClientException documentClientException)
                {
                    var statusCode = (int)documentClientException.StatusCode;
                    if (statusCode == 429 || statusCode == 503)
                    {
                        Thread.Sleep(documentClientException.RetryAfter);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: updating document: " + document.id);
                        updateDone = true;
                    }
                }
                catch (AggregateException aggregateException)
                {
                    if (aggregateException.InnerException.GetType() == typeof(DocumentClientException))
                    {

                        var docExcep = aggregateException.InnerException as DocumentClientException;
                        var statusCode = (int)docExcep.StatusCode;
                        if (statusCode == 429 || statusCode == 503)
                            Thread.Sleep(docExcep.RetryAfter);
                        else
                        {
                            Console.WriteLine("ERROR: updating document: " + document.id);
                            updateDone = true;
                        }
                    }
                }
            }
            return doc;
        }

        // create a document
        public async void CreateDocument(string year)
        {
            var queryDone = false;
            document.id = document.id + year;
            while (!queryDone)
            {
                try
                {
                    Console.WriteLine("creating document for: " + document.id +" on: "+DateTime.Now.ToString());
                    Database database = GetDatabase(DatabaseId).Result;
                    DocumentCollection collection = GetCollection(database, CollectionId).Result;
                    await client.CreateDocumentAsync(collection.SelfLink, document);
                    Console.WriteLine("document was created: " + document.id);
                    queryDone = true;
                }
                catch (DocumentClientException documentClientException)
                {
                    var statusCode = (int)documentClientException.StatusCode;
                    if (statusCode == 429 || statusCode == 503)
                        Thread.Sleep(documentClientException.RetryAfter);
                    else
                        Console.WriteLine("ERROR: creating document: " + document.id);
                }
                catch (AggregateException aggregateException)
                {
                    if (aggregateException.InnerException.GetType() == typeof(DocumentClientException))
                    {

                        var docExcep = aggregateException.InnerException as DocumentClientException;
                        var statusCode = (int)docExcep.StatusCode;
                        if (statusCode == 429 || statusCode == 503)
                            Thread.Sleep(docExcep.RetryAfter);
                        else
                            Console.WriteLine("ERROR: creating document: " + document.id);
                    }
                }
            }
        }

        // read a document, modify it, call update method on modified document-all counties but dublin
        // with all other counties we add to the list on the document
        public async void ModifyDocumentBoggers(string year,char group)
        {
            Database database = GetDatabase(DatabaseId).Result;
            DocumentCollection collection = GetCollection(database, CollectionId).Result;
            string added = year + "_" + group;
            document.id = document.id + added;
            Document doc = await UpdateDocument(collection, document);
            if (doc != null)
            {
                Console.WriteLine("document: " + document.id + " updated");
            }
        }

        // read a document, modify it, call update method on modified document-dublin only
        // with dublin we replace whole document
        public async void ModifyDocumentDublin(string year,int month)
        {
            Database database = GetDatabase(DatabaseId).Result;
            DocumentCollection collection = GetCollection(database, CollectionId).Result;
            string added = year + "_" + month;
            document.id = document.id + added;
            Document doc=await UpdateDocument(collection, document);
            if (doc != null)
            {
                Console.WriteLine("document: " + document.id + " updated");
            }
        }
    }

    
    // object to populate list
    public class ListObject
    {
        public DateTime SoldOn { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public double Price { get; set; }
        public char NotFullMP { get; set; }
        public char Description { get; set; }
    }
}
