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
using System.Text.Json;

namespace TransitWorker.Position;

public class TTC : BackgroundService
{
    private readonly ILogger<TTC> _logger;

    public TTC(ILogger<TTC> logger)
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

    static void Parse(TTCMessage message, string databaseConnection)
    {
        var Entities = new List<Entity>();
        DateTime now = DateTime.UtcNow;

        foreach (var entity in message.vehicle)
        {
            Entity e = new Entity
            {
                Id = Guid.NewGuid(),
                VehicleId = entity.id,
                AgencyId = "TTC",
                VehicleLabel = (entity.dirTag == null ? string.Empty : entity.dirTag),
                Timestamp = 0L,
                RouteId = entity.routeTag,
                DirectionId = 0,
                TripId = (entity.dirTag == null ? string.Empty : entity.dirTag),
                Bearing = Convert.ToDouble(entity.heading),
                BearingValid = true,
                Latitude = Convert.ToDouble(entity.lat),
                Longitude = Convert.ToDouble(entity.lon),
                Created = now,
                Deleted = false,
            };
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
    TTCMessage message;
    try
    {
        HttpClient client = new HttpClient();

        _logger.LogInformation("Position.TTC running at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string ttc = "https://retro.umoiq.com/service/publicJSONFeed?command=vehicleLocations&a=ttc&t=0";

                try
                {
                    var x = client.GetStreamAsync(ttc);
                        if (x != null && x.Result != null)
                        {
                            message = JsonSerializer.Deserialize<TTCMessage>(x.Result);
                            Parse(message, databaseConnection);
                        }
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
