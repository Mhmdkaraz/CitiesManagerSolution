using CitiesManager.Core.Entities;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => {
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));
}).AddXmlSerializerFormatters();

builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddApiVersioning(config => {
    //Reads version number from request url at "apiVersion" constraint
    config.ApiVersionReader = new UrlSegmentApiVersionReader();

    //Reads version number from request string called "api-version"
    //config.ApiVersionReader = new QueryStringApiVersionReader();//("custom")

    //Reads version number from request header called "api-version" eg. api-version:1.0
    //config.ApiVersionReader = new HeaderApiVersionReader("api-version"); 

    config.DefaultApiVersion = new ApiVersion(1,0);
    config.AssumeDefaultVersionWhenUnspecified = true;
});


//CORS: localhost 4200
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
        .WithHeaders("Authorization", "origin", "accept", "content-type")
        .WithMethods("GET","POST","PUT","DELETE");
    });
    //Custom policy
    options.AddPolicy("4100Client",policyBuilder => {
        policyBuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins2").Get<string[]>())
        .WithHeaders("Authorization", "origin", "accept")
        .WithMethods("GET");
    });
});



builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
//Swagger
//generates description for all endpoints
builder.Services.AddEndpointsApiExplorer();//It enables swagger to read metadata (HTTP method, URL, attributes etc.) of endpoints (Web API action methods)
//generates OpenAPI specification
builder.Services.AddSwaggerGen(options => {
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "1.0" });
    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "2.0" });
});//It configure swagger to generate documentation for API's endpoint

builder.Services.AddVersionedApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

//Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
})
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
              .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwagger();//creates endpoint for swagger.json(contains the information related to all the action methods of the controllers)
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
});//creates swaggger UI for testing all Web API endpoints / action methods

app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
