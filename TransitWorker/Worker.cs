using Microsoft.Data.SqlClient;
using ProtoBuf;
using System.Data;
using System.Net;
using System.Reflection;
using TransitRealtime;
using TransitWorker.Models;

namespace TransitWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }


    static void GetCity(FeedMessage message, IParser parser)
    {
        IParser cityParser = parser as IParser;
        if (cityParser != null)
        {
            cityParser.Parse(message);
        }

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //string positions = "https://www.miapp.ca/GTFS_RT/Vehicle/VehiclePositions.pb";
        //string positions = "https://rtu.york.ca/gtfsrealtime/VehiclePositions";

        try
        {
            HttpClient client = new HttpClient();

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string mississauga = "https://www.miapp.ca/GTFS_RT/Vehicle/VehiclePositions.pb";
                    //string york = "https://rtu.york.ca/gtfsrealtime/VehiclePositions";
                    string goTransit = "http://api.openmetrolinx.com/OpenDataAPI/api/V1/Gtfs.proto/Feed/VehiclePosition?key=XXXXXXXX";

                    var x = client.GetStreamAsync(goTransit);
                    FeedMessage message = Serializer.Deserialize<FeedMessage>(x.Result);
                    GetCity(message, new GOTransit());

                    x = client.GetStreamAsync(mississauga);
                    message = Serializer.Deserialize<FeedMessage>(x.Result);
                    GetCity(message, new Mississauga());

                    //x = client.GetStreamAsync(york);
                    //message = Serializer.Deserialize<FeedMessage>(x.Result);
                    //GetCity(message, new York());


                    using (TransitContext db = new TransitContext())
                    {
                        var procs = db.GetProcedures();
                        procs.DeleteAgedRecordsAsync().Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex.Message, new object[0]);
                }
                finally
                {
                    await Task.Delay(15000, stoppingToken);
                }
            } // end while
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex.Message, new object[0]);
        }
    }
}
