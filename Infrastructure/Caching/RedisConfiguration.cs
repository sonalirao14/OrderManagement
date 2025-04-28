using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public class RedisConfiguration
    {

        public string Host { get; set; } = "localhost:6379";
        public int DefaultDatabase { get; set; } = 1;
        public int ConnectTimeout { get; set; } = 5000;
        public bool AbortOnConnectFail { get; set; } = false;
    }
}
