// See https://aka.ms/new-console-template for more information

using Layer0SybilFinder;
using System.Diagnostics.Tracing;
using System.Text.Json;

var etherscanApiKey = "ADDYOUROWNKEY";
var optimismApiKey = "ADDYOUROWNKEY";
var baseApiKey = "ADDYOUROWNKEY";
var arbitrumApiKey = "ADDYOUROWNKEY";
var bnbApiKey = "ADDYOUROWNKEY";
var polygonApiKey = "ADDYOUROWNKEY";
var lineaApiKey = "ADDYOUROWNKEY";
var scrollApiKey = "ADDYOUROWNKEY";

List<string> wallets = new List<string>();

using (var reader = new StreamReader("snapshot1_transactions.csv"))
{
    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine();
        var values = line.Split(",");

        wallets.Add(values[4]);
    }
    wallets = wallets.Distinct().ToList();
    wallets.RemoveAt(0);

    Task etherTask = SybilScan(wallets, "sybilethereum.csv", "etherscanjsons.txt", etherscanApiKey, "Ethereum", "https://api.etherscan.io/api");
    Task optimismTask = SybilScan(wallets, "sybiloptimism.csv", "optimismjsons.txt", optimismApiKey, "Optimism", "https://api-optimistic.etherscan.io/api");
    Task baseTask = SybilScan(wallets, "sybilbase.csv", "basejsons.txt", baseApiKey, "Base", "https://api.basescan.org/api");
    Task arbitrumTask = SybilScan(wallets, "sybilarbitrum.csv", "arbitrumjsons.txt", arbitrumApiKey, "Arbitrum", "https://api.arbiscan.io/api");
    Task bnbTask = SybilScan(wallets, "sybilbnb.csv", "bnbjsons.txt", bnbApiKey, "BNB", "https://api.bscscan.com/api");
    Task polygonTask = SybilScan(wallets, "sybilpolygon.csv", "polygonjsons.txt", polygonApiKey, "Polygon", "https://api.polygonscan.com/api");
    Task lineaTask = SybilScan(wallets, "sybillinea.csv", "lineajsons.txt", lineaApiKey, "Linea", "https://api.lineascan.build/api");
    Task scrollTask = SybilScan(wallets, "sybilscroll.csv", "scrolljsons.txt", scrollApiKey, "Scroll", "https://api.scrollscan.com/api");

    await Task.WhenAll(etherTask, optimismTask, baseTask, arbitrumTask, bnbTask, polygonTask, lineaTask, scrollTask);
}

async Task SybilScan(List<string> wallets, string sybilfile, string jsonsfile, string apiKey, string network, string apiUrl)
{
    using HttpClient client = new();

    using (StreamWriter writetext = new StreamWriter(sybilfile))
    {
        using (StreamWriter writejson = new StreamWriter(jsonsfile))
        {
            writetext.WriteLine("Wallet1,Wallet2,Network,TransactionHash");
            foreach (var wallet in wallets)
            {
                List<string> sybilAddresses = new List<string>();
                try
                {
                    var json = await client.GetStringAsync($"{apiUrl}?module=account&action=txlist&address={wallet}&startblock=0&endblock=99999999&apikey={apiKey}");
                    writejson.WriteLine(wallet + " " + json);
                    var deserializedResponse = JsonSerializer.Deserialize<EtherscanResponseDto>(json);
                    if (deserializedResponse.result != null)
                    {
                        foreach (var response in deserializedResponse?.result)
                        {
                            if (response.to != wallet && response.to != "" && wallets.Any(w => w == response.to))
                            {
                                writetext.WriteLine($"{wallet},{response.to},{network},{response.hash}");
                                writetext.Flush();
                            }
                            if (response.from != wallet && response.from != "" && wallets.Any(w => w == response.from))
                            {
                                writetext.WriteLine($"{response.from},{wallet},{network},{response.hash}");
                                writetext.Flush();
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }
    }
}