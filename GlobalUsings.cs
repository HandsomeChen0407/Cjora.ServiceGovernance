global using System.Text;
global using System.Collections.Concurrent;

global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;

global using Consul;

global using Cjora.ServiceGovernance.Options;
global using Cjora.ServiceGovernance.Models;
global using Cjora.ServiceGovernance.Abstractions;
global using Cjora.ServiceGovernance.Service;
global using Cjora.ServiceGovernance.Handlers;