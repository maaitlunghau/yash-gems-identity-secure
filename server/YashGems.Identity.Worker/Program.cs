using YashGems.Identity.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MessageSubscriber>();

var host = builder.Build();
host.Run();
