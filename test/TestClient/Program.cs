using System;
using System.Threading.Tasks;
using Swisschain.Sirius.VaultApi.ApiClient;
using Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests;

namespace TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press enter:");
            Console.ReadLine();

            var client = new VaultApiClient("",
                "http://localhost:5701");

            var requests =  await client.TransferValidationRequests.GetAsync(new GetTransferValidationRequestsRequest());

            foreach (var request in requests.Response.Requests)
            {
                Print(request);
            }
        }

        static void Print(object obj)
        {
            var ser = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            
            Console.WriteLine(ser);
        }
    }
}
