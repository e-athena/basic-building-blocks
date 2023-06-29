// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Athena.Infrastructure.FreeSql.CAPs.Extends;
using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP;

internal class FreeSqlCapOptionsExtension : ICapOptionsExtension
{
    public void AddServices(IServiceCollection services)
    {
        services.TryAddSingleton(new CapStorageMarkerService());
        services.TryAddSingleton<IDataStorage, FreeSqlDataStorage>();
        services.TryAddSingleton<IStorageInitializer, FreeSqlStorageInitializer>();
    }
}