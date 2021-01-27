using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalis
{
    public class Settings
    {
        public DatabaseSettings Database { get; set; }

        public class DatabaseSettings
        {
            public string[] Urls { get; set; }

            public string DatabaseName { get; set; }

            public string CertPath { get; set; }

            public string CertPass { get; set; }
        }
    }
}
