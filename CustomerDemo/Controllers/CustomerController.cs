using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : Controller
    {
        private static Dictionary<long, CustomerModel> customDic = new Dictionary<long, CustomerModel>();

        [HttpGet("leaderboard")]
        public async Task<IActionResult> LeaderBoard([FromQuery] int start, [FromQuery] int end)
        {
            return await Task.Run(() =>
            {
                List<CustomerModel> result = new List<CustomerModel>();
                int index = 1;
                customDic.Values.ToList().OrderByDescending(a => a.Score).ThenBy(a => a.CustomerID).ToList().ForEach(item =>
                {
                    item.Rank = index;

                    if (index >= start && index <= end)
                    {
                        result.Add(item);
                    }

                    if (index > end)
                    {
                        return;
                    }

                    index++;
                });

                return RenderView(result);
            });
        }

        [HttpGet("leaderboard/{customerid:long}")]
        public async Task<IActionResult> GetCustomersByID(long customerId, [FromQuery] int high, [FromQuery] int low)
        {
            return await Task.Run(() =>
            {
                List<CustomerModel> result = new List<CustomerModel>();
                bool isFind = false;
                int index = 1;
                Dictionary<int, CustomerModel> keyValuePairs = new Dictionary<int, CustomerModel>();

                if (customDic.ContainsKey(customerId))
                {
                    customDic.Values.ToList().OrderByDescending(a => a.Score).ThenBy(a => a.CustomerID).ToList().ForEach(item =>
                    {
                        item.Rank = index;
                        if (!isFind)
                        {
                            if (item.CustomerID != customerId)
                            {
                                if (keyValuePairs.Keys.Contains(index % high))
                                {
                                    keyValuePairs[index % high] = item;
                                }
                                else
                                {
                                    keyValuePairs.Add(index % high, item);
                                }
                            }
                            else
                            {
                                isFind = true;
                                foreach (KeyValuePair<int, CustomerModel> keyValuePair in keyValuePairs)
                                {
                                    result.Add(keyValuePair.Value);
                                }
                                result.Add(item);
                            }
                        }
                        else
                        {
                            if (low > 0)
                            {
                                low--;
                                result.Add(item);
                            }
                            else
                            {
                                return;
                            }
                        }

                        index++;
                    });
                }

                return RenderView(result);
            });
        }

        [HttpPost("{customerId:long}/score/{score:decimal}")]
        public async Task<IActionResult> SetCustomer(long customerId, decimal score)
        {
            return await Task.Run(() =>
            {
                if (customerId > 0)
                {
                    CustomerModel temp;
                    if (!customDic.Keys.Contains(customerId))
                    {
                        temp = new CustomerModel();
                        temp.CustomerID = customerId;
                        temp.Score = score;
                        customDic.Add(customerId, temp);
                    }
                    else
                    {
                        temp = customDic[customerId];
                        temp.Score += score;
                    }
                }

                List<CustomerModel> tempList = customDic.Values.ToList();
                int index = 1;
                tempList.ForEach(item =>
                {
                    item.Rank = index;
                    index++;
                });

                return RenderView(tempList);
            });
        }

        private IActionResult RenderView(List<CustomerModel> result)
        {
            var viewModel = result.OrderBy(a => a.Rank).ToList();
            return View("Index", viewModel);
        }
    }
}
