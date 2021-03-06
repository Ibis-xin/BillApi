using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text;
using BillApi.Helper;
using BillApi.Models;
using BillApi.MongoCollection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Serilog;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace BillApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.

        string MyAllowSpecificOrigins = "*";
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder
                                        .AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                                  });
            });

            services.AddSingleton<IRecord, Record>();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
            services.AddSingleton<BasicAuth>();
            services.AddSingleton<JwtHelpers>();
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
                    options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
                        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                        // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                        // 一般我們都會驗證 Issuer
                        ValidateIssuer = true,
                        ValidIssuer = Configuration.GetValue<string>("JwtSettings:Issuer"),

                        // 通常不太需要驗證 Audience
                        ValidateAudience = false,
                        //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

                        // 一般我們都會驗證 Token 的有效期間
                        ValidateLifetime = true,

                        // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                        ValidateIssuerSigningKey = false,

                        // "1234567890123456" 應該從 IConfiguration 取得
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JwtSettings:SignKey")))
                    };
                });
            services.AddControllers()
             .AddJsonOptions(opt =>
             {
                 opt.JsonSerializerOptions.IgnoreNullValues = true;
                 var enumConverter = new JsonStringEnumConverter();
                 opt.JsonSerializerOptions.Converters.Add(enumConverter);
             })
             .ConfigureApiBehaviorOptions(options =>
             {
                 options.InvalidModelStateResponseFactory = context =>
                 {
                     var fields = (
                         from key in context.ModelState.Keys.ToList()
                         where context.ModelState[key].ValidationState == ModelValidationState.Invalid
                         select $"'{key}'")
                         .ToList();

                     string title = $"Input data validation error, Field {string.Join(", ", fields)} shouldn't be empty";
                     return new BadRequestObjectResult(new MessageTemplate { BadRequestError = title });
                 };
             });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BillApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BillApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);
            
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireCors(MyAllowSpecificOrigins);
            });
        }
    }
}
