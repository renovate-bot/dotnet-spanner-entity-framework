// Copyright 2021 Google Inc. All Rights Reserved.
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
using Google.Cloud.EntityFrameworkCore.Spanner.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Cloud Spanner supports multiple query hints. CommandInterceptors can be used to
/// add hints to queries that are generated by Entity Framework.
/// See https://docs.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors#example-command-interception-to-add-query-hints
/// for more information on using CommandInterceptors.
///
/// See https://cloud.google.com/spanner/docs/query-syntax#table-hints for more information
/// on possible table and statement hints for Cloud Spanner.
/// 
/// Run from the command line with `dotnet run QueryHintSample`
/// </summary>
public static class QueryHintSample
{
    // This sample uses a DbContext that adds a command interceptor to the context.
    // This command interceptor will add an index hint to any query that has been
    // tagged with a specific value.
    class TaggedSampleDbContext : SpannerSampleDbContext
    {
        private static readonly TaggedQueryCommandInterceptor s_interceptor
            = new TaggedQueryCommandInterceptor();

        public TaggedSampleDbContext(string connectionString) : base(connectionString)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(s_interceptor);
        }
    }
    
    public static async Task Run(string connectionString)
    {
        using var context = new TaggedSampleDbContext(connectionString);
        await Setup(context);

        var singersOrderedByFullName = context.Singers
            // This will add the following comment to the generated query:
            // `-- Use hint: force_index FullName`
            // This comment will be picked up by the interceptor and an index
            // hint will be added to the query that is executed.
            .TagWith("Use hint: force_index FullName")
            .OrderBy(s => s.FullName)
            .AsAsyncEnumerable();
        var first = true;
        await foreach (var singer in singersOrderedByFullName)
        {
            if (first)
            {
                Console.WriteLine("Singers ordered by full name:");
                first = false;
            }
            Console.WriteLine($"{singer.FullName}");
        }
    }
    
    class TaggedQueryCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            ManipulateCommand(command);

            return result;
        }

        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ManipulateCommand(command);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Adds a table hint to all queries that have been tagged with
        /// `Use hint: force_index FullName`
        /// </summary>
        /// <param name="command">The command to possibly add a hint to</param>
        private static void ManipulateCommand(DbCommand command)
        {
            if (command.CommandText.StartsWith("-- Use hint: force_index FullName", StringComparison.Ordinal))
            {
                command.CommandText = command.CommandText.Replace
                (
                    "FROM `Singers`", 
                    "FROM `Singers`@{FORCE_INDEX=Idx_Singers_FullName}"
                );
            }
            Console.WriteLine($"\nQuery being executed:\n{command.CommandText}\n");
        }
    }
    
    private static async Task Setup(SpannerSampleDbContext context)
    {
        await context.Singers.AddRangeAsync(
            new Singer
            {
                SingerId = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Henderson",
                BirthDate = new SpannerDate(1983, 10, 19),
            },
            new Singer
            {
                SingerId = Guid.NewGuid(),
                FirstName = "Peter",
                LastName = "Allison",
                BirthDate = new SpannerDate(2000, 5, 2),
            },
            new Singer
            {
                SingerId = Guid.NewGuid(),
                FirstName = "Mike",
                LastName = "Nicholson",
                BirthDate = new SpannerDate(1976, 8, 31),
            });
        await context.SaveChangesAsync();
    }
}
