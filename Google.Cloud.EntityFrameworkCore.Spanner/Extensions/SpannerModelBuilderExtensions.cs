// Copyright 2025, Google Inc. All rights reserved.
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

using Google.Cloud.EntityFrameworkCore.Spanner.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Google.Cloud.EntityFrameworkCore.Spanner.Extensions;

public static class SpannerModelBuilderExtensions
{
    /// <summary>
    /// Use AUTO_INCREMENT instead of GENERATED BY DEFAULT AS IDENTITY for primary keys for this model.
    /// </summary>
    public static ModelBuilder UseAutoIncrement(this ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Model;
        model.SetIdentityOptions(new SpannerIdentityOptionsData{GenerationStrategy = GenerationStrategy.AutoIncrement});
        return modelBuilder;
    }
    
    /// <summary>
    /// Disable the use of IDENTITY columns for primary keys for this model.
    /// </summary>
    public static ModelBuilder DisableIdentityColumns(this ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Model;
        model.SetIdentityOptions(new SpannerIdentityOptionsData{GenerationStrategy = GenerationStrategy.Disabled});
        return modelBuilder;
    }

}