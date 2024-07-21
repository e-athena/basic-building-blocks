// Copyright 2020-2022 Mykhailo Shevchuk & Contributors
//
// Licensed under the MIT license;
// you may not use this file except in compliance with the License.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See LICENSE file in the project root for full license information.

namespace Athena.Serilog.Sinks.ZincSearch;

/// <summary>
/// Credentials used for Grafana Loki authorization
/// </summary>
public class ZincSearchCredentials
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = null!;

    internal bool IsEmpty => string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password);
}