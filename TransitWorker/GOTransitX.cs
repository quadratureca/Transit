using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitRealtime;
using TransitWorker.Models;

namespace TransitWorker
{
    class GOTransitX : IParser
    {
        public void Parse(FeedMessage message, string databaseConnection)
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

            copy.WriteToServer(Extensions.ToDataTable(Entities.ToArray<Entity>()));
        }
    }
}
