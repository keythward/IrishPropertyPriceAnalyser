// connection to documentDB, contains method to read a document and cast it to a record and return it

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System;
using System.Linq;
using WebRole1.Models;

namespace WebRole1.DatabaseConn
{
    public static class DatabaseConnect2
    {
        private static Database ReadDatabase()
        {
            var db = Client.CreateDatabaseQuery()
                            .Where(d => d.Id == DatabaseId)
                            .AsEnumerable()
                            .FirstOrDefault();
            return db;
        }

        private static DocumentCollection ReadCollection(string databaseLink)
        {
            var col = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == CollectionId)
                              .AsEnumerable()
                              .FirstOrDefault();
            return col;
        }

        private static string databaseId;
        private static String DatabaseId
        {
            get
            {
                if (string.IsNullOrEmpty(databaseId))
                {
                    databaseId = "db";
                }

                return databaseId;
            }
        }

        private static string collectionId;
        private static String CollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionId))
                {
                    collectionId = "records";
                }

                return collectionId;
            }
        }

        private static Database database;
        private static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = ReadDatabase();
                }

                return database;
            }
        }

        private static DocumentCollection collection;
        private static DocumentCollection Collection
        {
            get
            {
                if (collection == null)
                {
                    collection = ReadCollection(Database.SelfLink);
                }

                return collection;
            }
        }

        private static DocumentClient client;
        private static DocumentClient Client
        {
            get
            {
                if (client == null)
                {
                    // local
                    string endpoint = ConfigurationManager.ConnectionStrings["DatabaseEndpointUriWeb"].ConnectionString;
                    string authKey = ConfigurationManager.ConnectionStrings["DatabaseConnectionStringReadOnly"].ConnectionString;
                    // azure
                    //string authKey= RoleEnvironment.GetConfigurationSettingValue("DatabaseConnectionStringReadOnly");
                    //string endpoint= RoleEnvironment.GetConfigurationSettingValue("DatabaseEndpointUriWeb");
                    Uri endpointUri = new Uri(endpoint);
                    client = new DocumentClient(endpointUri, authKey);
                }

                return client;
            }
        }

        // get a document from DB
        private static Document GetDocument(string id)
        {
            return Client.CreateDocumentQuery(Collection.DocumentsLink)
                .Where(d => d.Id == id)
                .AsEnumerable()
                .FirstOrDefault();
        }

        // cast the document to a record and return it
        public static DBRecord ReadDocument(string id)
        {
            Document doc = GetDocument(id);
            DBRecord docrecord = (DBRecord)doc;
            return docrecord;
        }
    }
}
