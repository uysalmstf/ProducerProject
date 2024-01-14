using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Confluent.Kafka;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static async Task Main()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "PLAINTEXT://localhost:29092"
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();
        var topic = "TokenMintedNew";
        


        var web3 = new Web3("https://polygon-mumbai.infura.io/v3/91fd828a265b41eca8ed7e7c8c38075e");
        string dosyaAdi = "sss.txt";

        // Dosyanın tam yolu oluşturulur
        string dosyaYolu = Path.Combine(Directory.GetCurrentDirectory(), dosyaAdi);
        string abi = File.ReadAllText(dosyaAdi);

       // string abi = await System.IO.File.ReadAllTextAsync("");
        string contractAddress = "0x1d2D3D25FD0d70CCEa044068c70D5b91D9E4F935";
       // var contract = web3.Eth.GetContract(abi, contractAddress);

        var eventFilter = web3.Eth.GetEvent<TokenMintedEventDTO>(contractAddress);
        var filterAll = eventFilter.CreateFilterInput();
        var logs = await eventFilter.GetAllChangesAsync(filterAll);
        foreach (var log in logs)
        {
            //Console.WriteLine($"Minter: {log.Event.Minter}, Token ID:{ log.Event.TokenId}, Price Paid: { log.Event.PricePaid}");
            //Kafka preprocessing begins here
           // string[] messageArray = { log.Event.Minter.ToString(), log.Event.TokenId.ToString(), log.Event.PricePaid.ToString() };
            Dictionary<string, string> messageArr = new Dictionary<string, string>();

            messageArr["Minter"] = log.Event.Minter.ToString();
            messageArr["TokenId"] = log.Event.TokenId.ToString();
            messageArr["PricePaid"] = log.Event.PricePaid.ToString();

            string messageJson = JsonConvert.SerializeObject(messageArr);
            var message = new Message<Null, string> { Value = messageJson };
            producer.Produce(topic, message, deliveryReport => {
                Console.WriteLine(deliveryReport.Message.Value);
            });
            
        }
        producer.Flush();
    }
}

[Event("TokenMinted")]
public class TokenMintedEventDTO : IEventDTO
{
    [Parameter("address", "minter", 1, false)]
    public string Minter { get; set; }

    [Parameter("uint256", "tokenId", 2, false)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "pricePaid", 3, false)]
    public BigInteger PricePaid { get; set; }
}
