using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Layer0SybilFinder
{
    public class EtherscanResponseDto
    {
        public string status { get; set; }
        public string message { get; set; }
        public EtherscanTransaction?[] result { get; set; }
    }
    public class EtherscanTransaction
    {
        public string hash { get; set; }
        public string from { get; set; }
        public string to { get; set; }
    }
}
