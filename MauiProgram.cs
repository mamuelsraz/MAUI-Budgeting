using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using MAUIBUDGET.Services;
using MAUIBUDGET.ViewModels;
using MauiIcons.Material;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MAUIBUDGET
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMaterialMauiIcons()
                .UseSkiaSharp()
                .UseLiveCharts()
                .UseLocalNotification(config =>
                {
                    config.AddCategory(new NotificationCategory(NotificationCategoryType.Service)
                    {
                        ActionList = new HashSet<NotificationAction>
                        {
                            new NotificationAction(200)
                            {
                                Title = "Category A",
                                Android =
                                {
                                    LaunchAppWhenTapped = false
                                },
                            },
                            new NotificationAction(300)
                            {
                                Title = "Category B",
                                Android =
                                {
                                    LaunchAppWhenTapped = false
                                },
                            },
                            new NotificationAction(400)
                            {
                                Title = "Other..",
                                Android =
                                {
                                    LaunchAppWhenTapped = true
                                },
                            }
                        },

                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
        builder.Services.AddSingleton<INotificationManager, MAUIBUDGET.Platforms.Android.AndroidNotificationManager>();
#elif WINDOWS
        builder.Services.AddSingleton<INotificationManager, MAUIBUDGET.Platforms.Windows.WindowsNotificationManager>();
#endif
            builder.Services.AddSingleton<ExpensesService>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<CategoryPageViewModel>();
            builder.Services.AddTransient<AddExpensePageViewModel>();
            builder.Services.AddTransient<AddCategoryPageViewModel>();
            return builder.Build();
        }
    }
}
