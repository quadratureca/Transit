﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using TransitAPI.Models;

namespace TransitAPI.Models
{
    public partial interface ITransitContextProcedures
    {
        Task<List<Agency>> GetAgenciesAsync(OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<Entity>> GetRelevantEntitiesAsync(double? North, double? East, double? South, double? West, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
    }
}
