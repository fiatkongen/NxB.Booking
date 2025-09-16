using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Infrastructure
{
    public abstract class BaseModifiedHandler
    {
        protected readonly IServiceProvider _serviceProvider;

        protected BaseModifiedHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected ISettingsRepository GetSettingsRepository()
        {
            var settingsRepository = _serviceProvider.GetService<ISettingsRepository>();
            Debug.Assert(settingsRepository != null, nameof(settingsRepository) + " != null");
            return settingsRepository;
        }
    }
}
