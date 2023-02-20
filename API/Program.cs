
var builder = WebApplication.CreateBuilder(args);

// add services to the container
           builder.Services.AddAutoMapper(typeof(MappingProfiles));
           builder.Services.AddControllers();
           builder.Services.AddDbContext<StoreContext>(x =>
             x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

             builder.Services.AddApplicationServices();
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
            app.UseAuthorization();
            app.UseSwaggerDocumentation();
            app.MapControllers();
         
            var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory =  services.GetRequiredService<ILoggerFactory>();
            try
            {
                var context = services.GetRequiredService<StoreContext>();
                await context.Database.MigrateAsync();
                await StoreContextSeed.SeedAsync(context, loggerFactory);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred during migration");
            }

            await app.RunAsync();
 