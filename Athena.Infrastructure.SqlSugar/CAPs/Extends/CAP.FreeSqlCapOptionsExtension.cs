// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Athena.Infrastructure.SqlSugar.CAPs.Extends;
using DotNetCore.CAP.Persistence;

// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP;

internal class SqlSugarCapOptionsExtension : ICapOptionsExtension
{
    public void AddServices(IServiceCollection services)
    {
        services.AddSingleton(new CapStorageMarkerService("SqlSugar"));
        services.AddSingleton<IDataStorage, SqlSugarDataStorage>();
        services.AddSingleton<IStorageInitializer, SqlSugarStorageInitializer>();
    }
}