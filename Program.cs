using DotNetEnv;
using log4net.Config;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
XmlConfigurator.Configure(new FileInfo("log4net.config"));

// Use Startup class
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services, builder);

var app = builder.Build();
startup.Configure(app);

startup.ConfigureHangfireJobs(app);

app.Run();