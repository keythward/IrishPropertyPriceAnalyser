using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerRole1;

namespace WorkerRoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkerRole wr = new WorkerRole();
            wr.OnStart();
            wr.Run();
            Console.ReadKey();
        }
    }
}
