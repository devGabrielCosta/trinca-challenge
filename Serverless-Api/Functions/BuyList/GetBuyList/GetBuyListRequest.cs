using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serverless_Api.Functions.BuyList.GetBuyList
{
    public class GetBuyListRequest
    {   
        public string Id { get; set; }
        public Dictionary<string, int> BuyList {  get; set; }
    }
}
