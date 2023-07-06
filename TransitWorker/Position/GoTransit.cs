using Microsoft.Data.SqlClient;
using ProtoBuf;
using System;
using System.Data;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using TransitRealtime;
using TransitWorker.Models;
using System.Collections.Generic;

namespace TransitWorker.Position;

public class GoTransit : BackgroundService
{
    private readonly ILogger<GoTransit> _logger;

    public GoTransit(ILogger<GoTransit> logger)
    {
        _logger = logger;
    }


    //static void GetCity(FeedMessage message, IParser parser, string databaseConnection)
    //{
    //    IParser cityParser = parser as IParser;
    //    if (cityParser != null)
    //    {
    //        cityParser.Parse(message, databaseConnection);
    //    }
    //}

    static void Parse(FeedMessage message, string databaseConnection)
    {
        var Entities = new List<Entity>();
        DateTime now = DateTime.UtcNow;

        foreach (var entity in message.Entities)
        {
            Entity e = new Entity();
            e.Id = Guid.NewGuid();
            e.AgencyId = "GO";
            e.VehicleId = entity.Vehicle.Vehicle.Id;
            e.VehicleLabel = entity.Vehicle.Vehicle.Label;
            e.Timestamp = (long)entity.Vehicle.Timestamp;
            e.RouteId = entity.Vehicle.Trip.RouteId;
            e.DirectionId = entity.Vehicle.Trip.DirectionId;
            e.TripId = entity.Vehicle.Trip.TripId;
            e.Bearing = entity.Vehicle.Position.Bearing;
            e.BearingValid = false;
            e.Latitude = entity.Vehicle.Position.Latitude;
            e.Longitude = entity.Vehicle.Position.Longitude;
            e.Created = now;
            e.Deleted = false;
            Entities.Add(e);
        }

        var copy = new SqlBulkCopy(databaseConnection);

        copy.DestinationTableName = "dbo.Entity";
        copy.ColumnMappings.Add(nameof(Entity.Id), "Id");
        copy.ColumnMappings.Add(nameof(Entity.AgencyId), "AgencyId");
        copy.ColumnMappings.Add(nameof(Entity.VehicleId), "VehicleId");
        copy.ColumnMappings.Add(nameof(Entity.VehicleLabel), "VehicleLabel");
        copy.ColumnMappings.Add(nameof(Entity.Timestamp), "Timestamp");
        copy.ColumnMappings.Add(nameof(Entity.RouteId), "RouteId");
        copy.ColumnMappings.Add(nameof(Entity.DirectionId), "DirectionId");
        copy.ColumnMappings.Add(nameof(Entity.TripId), "TripId");
        copy.ColumnMappings.Add(nameof(Entity.Bearing), "Bearing");
        copy.ColumnMappings.Add(nameof(Entity.BearingValid), "BearingValid");
        copy.ColumnMappings.Add(nameof(Entity.Latitude), "Latitude");
        copy.ColumnMappings.Add(nameof(Entity.Longitude), "Longitude");
        copy.ColumnMappings.Add(nameof(Entity.Created), "Created");
        copy.ColumnMappings.Add(nameof(Entity.Deleted), "Deleted");

        copy.WriteToServer(Entities.ToArray<Entity>().ToDataTable());
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string databaseConnection = @"Data Source=quadrature.ca;Initial Catalog=Transit;Persist Security Info=True;User ID=sa;Password=M155155auga?;TrustServerCertificate=true";
        FeedMessage message;
        try
        {
            HttpClient client = new HttpClient();

            _logger.LogInformation("Position.GoTransit running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //string mississauga = "https://www.miapp.ca/GTFS_RT/Vehicle/VehiclePositions.pb";
                    //string yrt = "https://rtu.yrt.ca/gtfsrealtime/VehiclePositions";
                    string goTransit = "http://api.openmetrolinx.com/OpenDataAPI/api/V1/Gtfs.proto/Feed/VehiclePosition?key=30023110";
                    //string barrie = "http://www.myridebarrie.ca/gtfs/GTFS_TripUpdates.pb";
                    //string hamilton = "https://opendata.hamilton.ca/GTFS-RT/GTFS_VehiclePositions.pb";

                    //try
                    //{
                    //    var x = client.GetStreamAsync(goTransit);
                    //    message = Serializer.Deserialize<FeedMessage>(x.Result);
                    //    GetCity(message, new GOTransit(), databaseConnection);
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.LogError(ex.Message, new object[0]);
                    //}

                    try
                    {
                        var x = client.GetStreamAsync(goTransit);
                        message = Serializer.Deserialize<FeedMessage>(x.Result);
                        Parse(message, databaseConnection);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, new object[0]);
                    }

                    //try
                    //{
                    //    var x = client.GetStreamAsync(yrt);
                    //    message = Serializer.Deserialize<FeedMessage>(x.Result);
                    //    GetCity(message, new YRT(), databaseConnection);
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.LogError(ex.Message, new object[0]);
                    //}

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
