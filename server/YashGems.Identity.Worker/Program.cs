using YashGems.Identity.Worker;
using YashGems.Identity.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MessageSubscriber>();
builder.Services.AddSingleton<IEmailService, EmailService>();

var host = builder.Build();
host.Run();
