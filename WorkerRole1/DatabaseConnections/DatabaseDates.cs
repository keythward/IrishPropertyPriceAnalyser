﻿// connect to database, pull down dates document, update dates document if needed

using Microsoft.Azure.Documents; // documentdb
using Microsoft.Azure.Documents.Client; // documentdb
using Microsoft.Azure.Documents.Linq; // documentdb
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkerRole1.Model;

namespace WorkerRole1.DatabaseConnections
{
    public class DatabaseDates
    {
        // db connection strings
        private static string EndpointUri = "https://ppr.documents.azure.com:443/";
        private static string AuthorizationKey = "vsM9HJjmfmWRzyUZ3tIUrtxEk4zM1vqScU09vM47XybdVdSGn4Qll+8jHloQWBREg/cpk+8TruuZZT/aV11cPw==";
        private static string DatabaseId = "ppr_database";
        private static string CollectionId = "ppr_records";
        private static DocumentClient client = new DocumentClient(new Uri(EndpointUri), AuthorizationKey);

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
        public static async Task<Document> UpdateDocument(DocumentCollection coll, UpdateDates record)
        {
            return await client.UpsertDocumentAsync(coll.SelfLink, record);
        }

        // read the document, modify it, call update method on modified document
        public static async void ModifyDatesDocument(UpdateDates update)
        {
            Console.WriteLine("modifying: update dates document");
            Database database = GetDatabase(DatabaseId).Result;
            DocumentCollection collection = GetCollection(database, CollectionId).Result;
            await UpdateDocument(collection, update);
            Console.WriteLine("update dates document modified");
        }

        // read the dates from the update dates document and return them
        public static async Task<UpdateDates> ReadDatesDocument()
        {
            string doc_id = "update_dates";
            Database database = GetDatabase(DatabaseId).Result;
            DocumentCollection collection = GetCollection(database, CollectionId).Result;
            Document d= await client.ReadDocumentAsync("/dbs/"+DatabaseId+"/colls/"+CollectionId+"/docs/"+doc_id);
            UpdateDates docrecord = (UpdateDates)d;
            return docrecord;
        }

        // create the update dates document
        public static async void CreateDatesDocument()
        {
            var queryDone = false;
            UpdateDates update = new UpdateDates();
            update.id = "update_dates";
            update.lastUpdate = DateTime.Now;
            update.updatedTo = DateTime.Now;
            while (!queryDone)
            {
                try
                {
                    Console.WriteLine("creating document for the update dates document");
                    Database database = GetDatabase(DatabaseId).Result;
                    DocumentCollection collection = GetCollection(database, CollectionId).Result;
                    await client.CreateDocumentAsync(collection.SelfLink, update);
                    Console.WriteLine("update dates document created");
                    queryDone = true;
                }
                catch (DocumentClientException documentClientException)
                {
                    var statusCode = (int)documentClientException.StatusCode;
                    if (statusCode == 429 || statusCode == 503)
                        Thread.Sleep(documentClientException.RetryAfter);
                    else
                        throw;
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
                            throw;
                    }
                }
            }
        }
    }
}