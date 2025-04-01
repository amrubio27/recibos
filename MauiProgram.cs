using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using recibos.core.data.db;
using recibos.core.data.services.location;
using recibos.features.Receipts.Data;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.UseCases;
using recibos.features.Receipts.Presentation.Models;
using recibos.features.Receipts.Presentation.Pages;
using recibos.features.Receipts.Presentation.ViewModels;

namespace recibos;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // Añadir el Toolkit
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register database first to ensure it's available for dependency injection
        builder.Services.AddSingleton<ReceiptDatabase>();

        // Register services
        builder.Services.AddSingleton<IReceiptRepository, ReceiptRepository>();
        builder.Services.AddSingleton<ILocationService, LocationService>();

        // Registrar el mapper de presentación aquí
        builder.Services.AddSingleton<IReceiptPresentationMapper, ReceiptPresentationMapper>();

        // Registrar Casos de Uso
        builder.Services.AddTransient<GetReceiptsUseCase>();
        builder.Services.AddTransient<GetReceiptDetailsUseCase>();
        builder.Services.AddTransient<AddReceiptUseCase>();
        builder.Services.AddTransient<UpdateReceiptUseCase>();
        builder.Services.AddTransient<DeleteReceiptUseCase>();

        // Register ViewModels
        builder.Services.AddSingleton<ReceiptsViewModel>();
        builder.Services.AddTransient<ReceiptDetailViewModel>();
        builder.Services.AddTransient<NewReceiptViewModel>();

        // Register Pages
        builder.Services.AddSingleton<ReceiptsPage>();
        builder.Services.AddTransient<ReceiptDetailPage>();
        builder.Services.AddTransient<NewReceiptPage>();

        // Register AppShell
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}