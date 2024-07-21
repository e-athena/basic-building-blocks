// Copyright 2020-2022 Mykhailo Shevchuk & Contributors
//
// Licensed under the MIT license;
// you may not use this file except in compliance with the License.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Athena.Serilog.Sinks.ZincSearch.Models;

internal class ZincSearchBatch
{
    public ZincSearchBatch(string index)
    {
        Index = index;
    }

    [JsonPropertyName("index")] public string Index { get; }
    [JsonPropertyName("records")] public IList<object> Records { get; } = new List<object>();

    [JsonIgnore] public bool IsNotEmpty => Records.Count > 0;

    public string Serialize() => JsonSerializer.Serialize(this);
}