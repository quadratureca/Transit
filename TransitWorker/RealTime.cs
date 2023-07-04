using Microsoft.Data.SqlClient;
using ProtoBuf;
using System.Data;
using System.Net;
using System.Reflection;
using TransitRealtime;
using TransitWorker.Models;

namespace TransitWorker;

public class RealTime : BackgroundService
{
    private readonly ILogger<RealTime> _logger;

    public RealTime(ILogger<RealTime> logger)
    {
        _logger = logger;
    }


    static void GetCity(FeedMessage message, IParser parser, string databaseConnection)
    {
        IParser cityParser = parser as IParser;
        if (cityParser != null)
        {
            cityParser.Parse(message, databaseConnection);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string databaseConnection = @"Data Source=quadrature.ca;Initial Catalog=Transit;Persist Security Info=True;User ID=sa;Password=M155155auga?;TrustServerCertificate=true";
        FeedMessage message;
        try
        {
            HttpClient client = new HttpClient();

            _logger.LogInformation("RealTime running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string mississauga = "https://www.miapp.ca/GTFS_RT/Vehicle/VehiclePositions.pb";
                    string yrt = "https://rtu.yrt.ca/gtfsrealtime/VehiclePositions";
                    string goTransit = "http://api.openmetrolinx.com/OpenDataAPI/api/V1/Gtfs.proto/Feed/VehiclePosition?key=30023110";
                    string barrie = "http://www.myridebarrie.ca/gtfs/GTFS_TripUpdates.pb";
                    string hamilton = "https://opendata.hamilton.ca/GTFS-RT/GTFS_VehiclePositions.pb";

                    try
                    {
                        var x = client.GetStreamAsync(goTransit);
                        message = Serializer.Deserialize<FeedMessage>(x.Result);
                        GetCity(message, new GOTransit(), databaseConnection);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, new object[0]);
                    }

                    try
                    {
                        var x = client.GetStreamAsync(mississauga);
                        message = Serializer.Deserialize<FeedMessage>(x.Result);
                        GetCity(message, new Mississauga(), databaseConnection);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, new object[0]);
                    }

                    try
                    {
                        var x = client.GetStreamAsync(yrt);
                        message = Serializer.Deserialize<FeedMessage>(x.Result);
                        GetCity(message, new YRT(), databaseConnection);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, new object[0]);
                    }

                    try
                    {
                        using (TransitContext db = new TransitContext())
                        {
                            db.DbConnectionString(databaseConnection);

                            var procs = db.GetProcedures();
                            procs.DeleteAgedRecordsAsync().Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, new object[0]);
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
