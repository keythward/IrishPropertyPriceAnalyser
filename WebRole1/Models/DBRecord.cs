// a database object for a document

using System;
using Microsoft.Azure.Documents;
using System.Collections.Generic;

namespace WebRole1.Models
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
