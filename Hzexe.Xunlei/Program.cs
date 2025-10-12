using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Hzexe.Xunlei.Hubs;
using Hzexe.Xunlei;
using Hzexe.Xunlei_Library;
using Hzexe.Xunlei_Library.Prot;
using System.Linq;
using FreeSql;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 添加控制器服务
builder.Services.AddControllers();

// 添加SignalR服务
builder.Services.AddSignalR();

// 添加HttpClientFactory支持
builder.Services.AddHttpClient();

// 注册FreeSQL服务，使用单例模式
builder.Services.AddSingleton<IFreeSql>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = FreeSqlContext.CalculateConnectionString(configuration);
    
    return new FreeSql.FreeSqlBuilder()
        .UseConnectionString(DataType.Sqlite, connectionString)
        .UseAutoSyncStructure(false) // 不自动同步结构，因为只读模式
        .UseNoneCommandParameter(true)
        .Build();
});

// 注册Api服务，改为单例生命周期
builder.Services.AddSingleton<Api>();

// 注册Xunlei服务，改为单例生命周期
builder.Services.AddSingleton<Xunlei>();

// 注册后台任务更新服务
builder.Services.AddHostedService<TaskUpdateService>();

var app = builder.Build();

// 配置HTTP请求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 在开发环境中禁用HTTPS重定向
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// 配置默认路由，指向静态首页
app.MapFallbackToFile("index.html");

app.UseRouting();

// 配置SignalR Hub
app.MapHub<TaskUpdateHub>("/taskUpdateHub");

// 配置控制器路由
app.MapControllers();

app.MapRazorPages();

app.Run();
