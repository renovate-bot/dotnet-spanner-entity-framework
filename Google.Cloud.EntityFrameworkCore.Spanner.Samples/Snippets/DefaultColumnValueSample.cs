﻿// Copyright 2024 Google Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Cloud.EntityFrameworkCore.Spanner.Samples.SampleModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Sample for using a DEFAULT column value.
/// 
/// Run from the command line with `dotnet run DefaultColumnValue`
/// </summary>
public static class DefaultColumnValueSample
{
    public static async Task Run(string connectionString)
    {
        using var context = new SpannerSampleDbContext(connectionString);
        // Get the current timestamp from the server.
        using var cmd = context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT CURRENT_TIMESTAMP";
        var timestamp = (DateTime) (await cmd.ExecuteScalarAsync())!;
        Console.WriteLine($"The current server time is #{timestamp:yyyy-MM-ddTHH:mm:ss.fffffffZ}");
        
        // Insert a new Singer, Album, and Track.
        var singer = new Singer
        {
            FirstName = "Alice",
            LastName = "Jameson",
            Albums = new List<Album>
            {
                new() {Title = "Rainforest", Tracks = new List<Track>
                {
                    new() {TrackId = 1, Title = "Butterflies"}
                }}
            }
        };
        await context.Singers.AddAsync(singer);
        await context.SaveChangesAsync();
        Console.WriteLine("Inserting data");
        
        // The RecordedAt property of the Track is set to the current server time.
        Console.WriteLine($"Track was recorded at #{singer.Albums.First().Tracks.First().RecordedAt:yyyy-MM-ddTHH:mm:ss.fffffffZ}");
    }
}
