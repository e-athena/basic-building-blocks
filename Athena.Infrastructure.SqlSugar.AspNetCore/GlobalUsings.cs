// Global using directives

global using System.ComponentModel.DataAnnotations.Schema;
global using System.Reflection;
global using Athena.Infrastructure.Domain;
global using Athena.Infrastructure.Event.DomainEvents;
global using Athena.Infrastructure.Event.IntegrationEvents;
global using Athena.Infrastructure.Helpers;
global using Athena.Infrastructure.SqlSugar;
global using Athena.Infrastructure.SqlSugar.AspNetCore.Middlewares;
global using Athena.Infrastructure.SqlSugar.EventContexts;
global using Athena.Infrastructure.SqlSugar.Helpers;
global using Athena.Infrastructure.Tenants;
global using DotNetCore.CAP;
global using MediatR;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Configuration;
global using SqlSugar;
global using StackExchange.Redis;