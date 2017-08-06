using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace SQLiteTest
{
    class Program
    {
        private static void Main(string[] args)
        {
            var sc = new SQLiteConfig("test.db");

            var sid = sc.AddSection("Properties", false);
            if (sid >0)
            {
                var kid = (sc.AddKey("Properties", "int value"));
                if (kid >0) 
                {
                    if (sc.AddValue(kid, "10") > 0)
                    {
                        WriteLine("Value added ok");
                    }
                  else
                  {
                        WriteLine("Value not added");
                    }
                }
            }

        }
    }
}
