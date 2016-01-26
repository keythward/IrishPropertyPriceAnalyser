// model class for the search page

using System.Collections.Generic;
using System.Threading.Tasks;
using WebRole1.DatabaseConn;

namespace WebRole1.Models
{
    public class Search
    {
        public string County { get; set; }
        public string Year { get; set; }

        public List<ListObject> Found
        {
            get
            {
                List<ListObject> list = new List<ListObject>();
                DBRecord test = DatabaseConnect2.ReadDocument("Kerry2010_B");                
                list = test.records;
                return list;
            }
        }










    }
}
