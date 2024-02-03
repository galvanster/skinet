
using Core.Entities.Identity;
using Core.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// add services to the container
           builder.Services.AddAutoMapper(typeof(MappingProfiles));
           builder.Services.AddControllers();
           builder.Services.AddApplicationServices(builder.Configuration);
           builder.Services.AddIdentityServices(builder.Configuration);
           builder.Services.AddDbContext<StoreContext>(x =>
             x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
           builder.Services.AddDbContext<AppIdentityDbContext>(x =>
             x.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection")));

            // builder.Services.AddApplicationServices();
             builder.Services.AddSwaggerDocumentation();
             builder.Services.AddCors(opt =>
             {
                 opt.AddPolicy("CorsPolicy", policy =>
                 {
                     policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
                 });
             });


// configure the http request pipeline         
            var app = builder.Build();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwaggerDocumentation();
            app.MapControllers();
         
            var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<StoreContext>();
            var identityContext = services.GetRequiredService<AppIdentityDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var loggerFactory =  services.GetRequiredService<ILoggerFactory>();
            try
            {

                await context.Database.MigrateAsync();
                await identityContext.Database.MigrateAsync();
                await StoreContextSeed.SeedAsync(context, loggerFactory);
                await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred during migration");
            }

            await app.RunAsync();
 