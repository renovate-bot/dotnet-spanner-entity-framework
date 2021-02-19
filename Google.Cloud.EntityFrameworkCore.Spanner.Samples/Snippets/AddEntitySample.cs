﻿// Copyright 2021 Google Inc. All Rights Reserved.
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
using System;
using System.Threading.Tasks;

/// <summary>
/// Simple sample showing how to insert a new entity with an ID generated by the client.
/// 
/// Run from the command line with `dotnet run AddEntitySample`
/// </summary>
public static class AddEntitySample
{
    public static async Task Run(string connectionString)
    {
        using var context = new SpannerSampleDbContext(connectionString);

        // Create a new Singer, add it to the context and save the changes.
        context.Singers.Add(new Singer
        {
            // Cloud Spanner does not support server side generation of Guid values,
            // so it must always be generated by the client.
            SingerId = Guid.NewGuid(),
            FirstName = "Jamie",
            LastName = "Yngvason"
        });
        var count = await context.SaveChangesAsync();

        // SaveChangesAsync returns the total number of rows that was inserted/updated/deleted.
        Console.WriteLine($"Added {count} singer.");
    }
}