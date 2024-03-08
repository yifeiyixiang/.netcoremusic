using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.WebSockets; 
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace diary
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
            // ���Session����
            services.AddSession();
            services.AddHttpContextAccessor();
            // ���WebSocket֧�� 
            services.AddWebSockets(options =>
            {
                // �������������WebSocketѡ�� 
            });
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;//json
            });

            // ��ӱ��ػ�֧��
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // ������������
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("zh-CN"); // ����Ĭ�ϵ���������
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("zh-CN") }; // ֧�ֵ����������б�
                options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("zh-CN") }; // ֧�ֵ�UI���������б�
            });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(webSocket);
                        // ����HandleWebSocket��������WebSocket����
                        await HandleWebSocket(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
            // ʹ��Session�м��
            app.UseSession();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
        private static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        public async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
        {
            // ����µ�WebSocket���ӵ��ֵ���
            string socketId = Guid.NewGuid().ToString();
            _sockets.TryAdd(socketId, webSocket);

            var buffer = new byte[4096];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // ���տͻ��˷��͵���Ϣ
            while (!result.CloseStatus.HasValue)
            {
                // ������յ�����Ϣ
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                // ������Ϣ���������ӵĿͻ���
                foreach (var socket in _sockets)
                {
                    if (socket.Value.State == WebSocketState.Open)
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(message);
                        await socket.Value.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // �Ƴ��ѹرյ�WebSocket����
            _sockets.TryRemove(socketId, out _);
        }
        private async Task Echo(WebSocket webSocket)
        {
            // ����µ�WebSocket���ӵ��ֵ���
            string socketId = Guid.NewGuid().ToString();
            _sockets.TryAdd(socketId, webSocket);
            byte[] buffer = new byte[1024];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                // ������յ�����Ϣ
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                // ������Ϣ���ͻ���
                string response = message;
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);

                foreach (var socket in _sockets)
                {
                    if (socket.Value.State == WebSocketState.Open)
                    {
                        await socket.Value.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
