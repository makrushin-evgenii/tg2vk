using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework;
using TelegramAggregator.Controls.AuthControl;
using TelegramAggregator.Controls.CalendarControl;
using TelegramAggregator.Controls.DialogsControl;
using TelegramAggregator.Controls.MessagesControl;
using TelegramAggregator.Controls.MessagesControl.Handlers;
using TelegramAggregator.Controls.MessagesControl.Handlers.Messages;
using TelegramAggregator.Controls.MessagesControl.Services.NotificationsService;
using TelegramAggregator.Model.Repositories;

namespace TelegramAggregator
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        private IConfiguration _configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTelegramBot<AggregatorBot>(_configuration.GetSection("AggregatorBot"))
                .AddAuthHandlers()
//                .AddCalendarHandlers()
                .AddDialogsHandlers()
                .AddUpdateHandler<SendMessagesHandler>()
                .AddUpdateHandler<ForwardMessagesHandler>()
                .AddUpdateHandler<ReplyMessagesHandler>()
                .AddUpdateHandler<EditMessagesHandler>()
                .AddUpdateHandler<DeleteMessagesHandler>()
                .AddUpdateHandler<LikesHandler>()
                .Configure();

            services.AddCalendarControlServices();

            // Add bot configuration
            services.AddSingleton(_configuration
                .GetSection("AggregatorBot")
                .Get<AggregatorBotConfiguration>());

            services.AddScoped<IBotUserRepository, BotUserRepository>();
            services.AddScoped<INotificationsService, NotificationsService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            ILogger logger = loggerFactory.CreateLogger<Startup>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseTelegramBotLongPolling<AggregatorBot>();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                    appBuilder.Run(context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        return Task.CompletedTask;
                    })
                );

                logger.LogInformation($"Setting webhook for {nameof(AggregatorBot)}...");
                app.UseTelegramBotWebhook<AggregatorBot>();
                logger.LogInformation("Webhook is set for bot " + nameof(AggregatorBot));
            }

            app.Run(async context => { await context.Response.WriteAsync("Hello World!"); });
        }
    }
}