// object class for document updateDates

using Microsoft.Azure.Documents;
using System;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace WorkerRole1.Model
{
    public class UpdateDates
    {
        public string id;
        public DateTime lastUpdate;
        public DateTime updatedTo;
        // cast document to object
        public static explicit operator UpdateDates(Document doc)
        {
            UpdateDates rec = new UpdateDates();
            rec.id = doc.GetPropertyValue<string>("id");
            rec.lastUpdate = doc.GetPropertyValue<DateTime>("lastUpdate");
            rec.updatedTo = doc.GetPropertyValue<DateTime>("updatedTo");
            return rec;
        }
    }
}
