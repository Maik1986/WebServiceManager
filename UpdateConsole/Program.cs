using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServiceManager.Logic.Update;

namespace UpdateConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Update update = new Update();
            update.update(null, null);
        }
    }
}
