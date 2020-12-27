using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESTest.control;
using ESTestWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SLSM.Web.DTSWeb;

namespace ESTestWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddMemoryCache();

            services.AddSingleton<ILoginServices, LoginHelper>();
            services.AddSingleton<IESProvider, ESProvider>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
            {
                option.Cookie.Name = "MyApplicationTokenCookie";//���ô洢�û���¼��Ϣ���û�Token��Ϣ����Cookie����
                option.Cookie.HttpOnly = true;//���ô洢�û���¼��Ϣ���û�Token��Ϣ����Cookie���޷�ͨ���ͻ���������ű�(��JavaScript��)���ʵ�
                option.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;//���ô洢�û���¼��Ϣ���û�Token��Ϣ����Cookie��ֻ��ͨ��HTTPSЭ�鴫�ݣ������HTTPЭ�飬Cookie���ᱻ���͡�ע�⣬option.Cookie.SecurePolicy���Ե�Ĭ��ֵ��Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
            });

            services.AddSingleton<IConfiguration>(Configuration);

            DbHelperMySQL.DefaultConnectionString = Configuration.GetSection("ConnectionString")["DefaultConnection"];
            //DbHelperMySQL.DefaultConnectionString = conn.GetConnectionString("DefaultConnection");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddLog4Net();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            //�����û������֤
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
