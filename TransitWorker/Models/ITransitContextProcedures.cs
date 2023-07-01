﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using TransitWorker.Models;

namespace TransitWorker.Models
{
    public partial interface ITransitContextProcedures
    {
        Task<int> DeleteAgedRecordsAsync(OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<GetRelevantEntitiesResult>> GetRelevantEntitiesAsync(double? North, double? East, double? South, double? West, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
    }
}
