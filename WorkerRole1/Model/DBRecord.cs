// a database object for a document

using Microsoft.Azure.Documents;
using System.Collections.Generic;
using WorkerRole1.DatabaseConnections;

namespace WorkerRole1.Model
{
    public class DBRecord
    {
        public string id; // county
        public List<ListObject> records; // list of records for county
        // cast document to object
        public static explicit operator DBRecord(Document doc)
        {
            DBRecord rec = new DBRecord();
            rec.id = doc.GetPropertyValue<string>("id");
            rec.records = doc.GetPropertyValue<List<ListObject>>("records");
            return rec;
        }
    }
}
