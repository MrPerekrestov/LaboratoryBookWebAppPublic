using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


namespace LaboratoryBookWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            //.UseKestrel()
            //.ConfigureKestrel((context, options) =>
            //{                
            //    options.Listen(IPAddress.Loopback, 5001, listenOptions =>
            //    {
            //        listenOptions.UseHttps();
            //    });
            //})
            .UseStartup<Startup>();
    }
}
