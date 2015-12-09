// object of cleaned up and sorted record object, this object goes into database

using System;

namespace WorkerRole1.Records
{
    public class AlteredRecord
    {
        public string Address { get; set; }
        public DateTime SoldOn { get; set; }
        public string PostCode { get; set; }
        public string County { get; set; }
        public double Price { get; set; }
        public char NotFullMP { get; set; }
        public char Description { get; set; }
    }
}
