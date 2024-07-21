// Copyright 2020-2022 Mykhailo Shevchuk & Contributors
//
// Licensed under the MIT license;
// you may not use this file except in compliance with the License.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See LICENSE file in the project root for full license information.

namespace Athena.Serilog.Sinks.ZincSearch.Utils;

internal static class ZincSearchRoutesBuilder
{
    private const string LogEntriesEndpoint = "/api/_bulkv2";

    public static string BuildLogsEntriesRoute(string host) => $"{host.TrimEnd('/')}{LogEntriesEndpoint}";
}