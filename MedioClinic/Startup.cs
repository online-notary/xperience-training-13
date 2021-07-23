using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Autofac;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.Web.Mvc;

using Core.Configuration;
using XperienceAdapter.Localization;
using MedioClinic.Configuration;
using MedioClinic.Extensions;
using MedioClinic.Models;

namespace MedioClinic
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public IConfigurationSection? Options { get; }

        public string? DefaultCulture => SettingsKeyInfoProvider.GetValue($"{Options?.GetSection("SiteCodeName")}.CMSDefaultCultureCode");

        public AutoFacConfig AutoFacConfig => new AutoFacConfig();

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            Environment = webHostEnvironment;
            Configuration = configuration;
            Options = configuration.GetSection(nameof(XperienceOptions));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Enable desired Kentico Xperience features
            var kenticoServiceCollection = services.AddKentico(features =>
            {
                // features.UsePageBuilder();
                // features.UseActivityTracking();
                // features.UseABTesting();
                // features.UseWebAnalytics();
                // features.UseEmailTracking();
                // features.UseCampaignLogger();
                // features.UseScheduler();
                features.UsePageRouting(new PageRoutingOptions { CultureCodeRouteValuesKey = "culture" });
            });

            if (Environment.IsDevelopment())
            {
                // By default, Xperience sends cookies using SameSite=Lax. If the administration and live site applications
                // are hosted on separate domains, this ensures cookies are set with SameSite=None and Secure. The configuration
                // only applies when communicating with the Xperience administration via preview links. Both applications also need 
                // to use a secure connection (HTTPS) to ensure cookies are not rejected by the client.
                kenticoServiceCollection.SetAdminCookiesSameSiteNone();

                // By default, Xperience requires a secure connection (HTTPS) if administration and live site applications
                // are hosted on separate domains. This configuration simplifies the initial setup of the development
                // or evaluation environment without a the need for secure connection. The system ignores authentication
                // cookies and this information is taken from the URL.
                kenticoServiceCollection.DisableVirtualContextSecurityForLocalhost();
            }

            // services.AddAuthentication();
            // services.AddAuthorization();

            services.AddLocalization();

            services.AddControllersWithViews()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        var assemblyName = typeof(SharedResource).GetTypeInfo().Assembly.GetName().Name;

                        return factory.Create("SharedResource", assemblyName);
                    };
                });

            /* Conventional routing: Begin */
            //services.Configure<RouteOptions>(options => options.AppendTrailingSlash = true);
            /* Conventional routing: End */

            services.Configure<XperienceOptions>(Options);
            var xperienceOptions = Options.Get<XperienceOptions>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/html";

                        await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
                        await context.Response.WriteAsync("An error happened.<br><br>\r\n");

                        var exceptionHandlerPathFeature =
                            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

                        if (exceptionHandlerPathFeature?.Error is System.IO.FileNotFoundException)
                        {
                            await context.Response.WriteAsync("A file error happened.<br><br>\r\n");
                        }

                        await context.Response.WriteAsync("<a href=\"/\">Home</a><br>\r\n");
                        await context.Response.WriteAsync("</body></html>\r\n");
                        await context.Response.WriteAsync(new string(' ', 512)); // IE padding
                    });
                });

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseLocalizedStatusCodePagesWithReExecute("/{0}/error/{1}/");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseKentico();

            /* Conventional routing: Begin */
            //app.UseRouting();
            /* Conventional routing: End */

            app.UseRequestCulture();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Kentico().MapRoutes();

                endpoints.MapControllerRoute(
                    name: "error",
                    pattern: "{culture}/error/{code}",
                    defaults: new { controller = "Error", action = "Index" }
                    );

                /* Conventional routing: Begin */
                //MapCultureSpecificRoutes(endpoints, optionsAccessor);
                /* Conventional routing: End */

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("The site has not been configured yet.");
                });
            });
        }

        /// <summary>
        /// Registers a handler in case Xperience is not initialized yet.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        private void RegisterInitializationHandler(ContainerBuilder builder) =>
            CMS.Base.ApplicationEvents.Initialized.Execute += (sender, eventArgs) => AutoFacConfig.ConfigureContainer(builder);

        public void ConfigureContainer(ContainerBuilder builder)
        {
            try
            {
                AutoFacConfig.ConfigureContainer(builder);
            }
            catch
            {
                RegisterInitializationHandler(builder);
            }
        }

        /* Conventional routing: Begin */

        /// <summary>
        /// Registers culture-specific routes using the conventional ASP.NET routing.
        /// </summary>
        /// <param name="builder">Route builder.</param>
        /// <param name="optionsAccessor">Options accessor.</param>
        //private void MapCultureSpecificRoutes(IEndpointRouteBuilder builder, IOptions<XperienceOptions> optionsAccessor)
        //{
        //    if (AppCore.Initialized)
        //    {
        //        var currentSiteName = optionsAccessor.Value.SiteCodeName;
        //        string? spanishCulture = default;

        //        if (!string.IsNullOrEmpty(currentSiteName))
        //        {
        //            var cultures = CultureSiteInfoProvider.GetSiteCultures(currentSiteName);
        //            spanishCulture = cultures.FirstOrDefault(culture => culture.CultureCode.Equals("es-ES")).CultureCode;
        //        }

        //        if (!string.IsNullOrEmpty(DefaultCulture) && !string.IsNullOrEmpty(spanishCulture))
        //        {
        //            var routeOptions = new List<RouteBuilderOptions>
        //            {
        //                new RouteBuilderOptions("home", new { controller = "Home", action = "Index" }, new[]
        //                {
        //                    (DefaultCulture, "home"),
        //                    (spanishCulture, "inicio"),
        //                }),

        //                new RouteBuilderOptions("doctor-listing", new { controller = "Doctors", action = "Index" }, new[]
        //                {
        //                    (DefaultCulture, "doctors"),
        //                    (spanishCulture, "medicos"),
        //                }),

        //                new RouteBuilderOptions("doctor-detail", new { controller = "Doctors", action = "Detail" }, new[]
        //                {
        //                    (DefaultCulture, "doctors/{urlSlug?}"),
        //                    (spanishCulture, "medicos/{urlSlug?}"),
        //                }),

        //                new RouteBuilderOptions("contact", new { controller = "Contact", action = "Index" }, new[]
        //                {
        //                    (DefaultCulture, "contact-us"),
        //                    (spanishCulture, "contacta-con-nosotros"),
        //                }),
        //            };

        //            foreach (var options in routeOptions)
        //            {
        //                foreach (var culture in options.CulturePatterns)
        //                {
        //                    mapRouteCultureVariantsImplementation(culture?.Culture!, options?.RouteName!, culture?.RoutePattern!, options?.RouteDefaults!);
        //                }
        //            }

        //            void mapRouteCultureVariantsImplementation(
        //                string culture,
        //                string routeName,
        //                string routePattern,
        //                object routeDefaults)
        //            {
        //                var stringParameters = new string[] { culture, routeName, routePattern };

        //                if (stringParameters.All(parameter => !string.IsNullOrEmpty(parameter)) && routeDefaults != null)
        //                {
        //                    builder.MapControllerRoute(
        //                    name: $"{routeName}_{culture}",
        //                    pattern: AddCulturePrefix(culture, routePattern!),
        //                    defaults: routeDefaults,
        //                    constraints: new { culture = new Kentico.Web.Mvc.Internal.SiteCultureConstraint() }
        //                    );
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Decorates route patterns with a well-known culture prefix.
        ///// </summary>
        ///// <param name="culture">Culture prefix.</param>
        ///// <param name="pattern">Route pattern.</param>
        ///// <returns></returns>
        //private static string AddCulturePrefix(string culture, string pattern) =>
        //    $"{{culture={culture.ToLowerInvariant()}}}/{pattern}";

        /* Conventional routing: End */
    }
}
