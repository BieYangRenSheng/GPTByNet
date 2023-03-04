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

// ������key ����appsetting
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
    //��ȡע���ĵ�·��  
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    //��ʾ����ע��
    c.IncludeXmlComments(xmlPath, true);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatGpt", Version = "v1" });

    //���Jwt��֤����,�������ͷ��Ϣ
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

    //���ýӿ�Auth��Ȩ��ť
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Value Bearer {token}",
        Name = "Authorization",//jwtĬ�ϵĲ�������
        In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
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
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); //�۵�Api
    c.DefaultModelsExpandDepth(-1); //ȥ��Model ��ʾ
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
