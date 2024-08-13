﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Pot.Features.Calculators.Extensions;
using Pot.Maui.Domain.Extensions;
using Pot.Maui.Features.Login.Extensions;
using Pot.Maui.Features.ShowSummary.Extensions;

namespace Pot.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts
                        .AddFont("OpenSans-Regular.ttf", "OpenSansRegular")
                        .AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold")
                        .AddFont("materialdesignicons-webfont.ttf", "MaterialDesign");
                });

            builder.Services
                .AddDomain()
                .AddLogin()
                .AddCalculators()
                .AddSummary();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
