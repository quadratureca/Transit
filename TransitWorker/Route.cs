using Microsoft.Data.SqlClient;
using ProtoBuf;
using System.Data;
using System.Net;
using System.Reflection;
using TransitRealtime;
using TransitWorker.Models;

namespace TransitWorker;

public class Route : BackgroundService
{
    private readonly ILogger<Route> _logger;

    public Route(ILogger<Route> logger)
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
        _logger.LogInformation("Route running at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message, new object[0]);
            }
            finally
            {
                await Task.Delay(86400000, stoppingToken);
            }
        } // end while

    }
}
