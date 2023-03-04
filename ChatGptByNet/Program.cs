using ChatGptByNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenAI.GPT3.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContextPool<ChatGptDbContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(
            builder.Configuration.GetConnectionString("MySql"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), null)));

// 可配置key 放入appsetting
var apiKey = builder.Configuration.GetSection("OpenAIServiceOptions").Value;

if (!string.IsNullOrEmpty(apiKey))
{
    builder.Services.AddOpenAIService(settings => { settings.ApiKey = apiKey; });
}
else
{
    builder.Services.AddOpenAIService();
}

builder.Services.AddSwaggerGen(c =>
{
    //获取注释文档路径  
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    //显示方法注释
    c.IncludeXmlComments(xmlPath, true);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatGpt", Version = "v1" });

    //添加Jwt验证设置,添加请求头信息
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                new List<string>()
            }
        });

    //放置接口Auth授权按钮
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Value Bearer {token}",
        Name = "Authorization",//jwt默认的参数名称
        In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
        Type = SecuritySchemeType.ApiKey
    });
});

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("CorsPolicy", opt => opt
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("X-Pagination"));
});

var app = builder.Build();
app.UseCors("CorsPolicy");
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatGpt v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); //折叠Api
    c.DefaultModelsExpandDepth(-1); //去除Model 显示
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
