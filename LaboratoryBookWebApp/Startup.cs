using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LaboratoryBookWebApp
{
    public class Startup
    {       
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Login/Authentication/";

                    });             

            services.AddMvc().AddXmlSerializerFormatters(); 
        }
       
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts();

            app.UseHttpsRedirection();           

            app.UseStaticFiles();

            app.UseAuthentication();                

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Login}/{action=Authentication}/");
            });
        }
    }
}
