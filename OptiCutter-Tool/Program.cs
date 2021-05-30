using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using OptiCutter_Tool.Services.OptiCutter;

namespace OptiCutter_Tool
{
    class Program
    {
        public static string fileOutputPath = "path/to/write/to";

        static async Task<int> Main(string[] args)
        {
            var builder = new HostBuilder()
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHttpClient(OptiCutterService.HttpClientFactoryName, (client) =>
                  {
                      client.BaseAddress = new Uri("https://www.opticutter.com");
                      client.Timeout = new TimeSpan(0, 0, 10);
                  }).ConfigurePrimaryHttpMessageHandler(() =>
                  {
                      return new HttpClientHandler()
                      {
                          AllowAutoRedirect = false,
                          UseCookies = false
                      };
                  });
                  services.AddTransient<IOptiCutterService, OptiCutterService>();
                  services.AddTransient<MainApplication>();
              }).UseConsoleLifetime();

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var myService = services.GetRequiredService<MainApplication>();
                    var result = await myService.Run();

                    Console.WriteLine(result);
                }
                catch(HttpRequestException ex)
                {
                    Console.WriteLine("Http Error occurred");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

            return 0;
        }
    }

    public class MainApplication
    {
        IOptiCutterService _optiCutterService;
        public MainApplication(IOptiCutterService optiCutterService)
        {
            _optiCutterService = optiCutterService;
        }

        public async Task<string> Run()
        {
            var deskScenarios = new Dictionary<string, OptiCutterLinearCutCalculatorRequest>
            {
                { "20-inches-wide", new OptiCutterLinearCutCalculatorRequest
                    {
                        Kerf = 0,
                        Stock = new List<OptiCutterLinearCutCalculatorBoard>
                        {
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 96 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 120 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 144, },
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 192 }
                        },
                        Requirements = new List<OptiCutterLinearCutCalculatorBoard>
                        {
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 4, Length = 62,  },
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 2, Length = 21.5 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 2, Length = 23 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 2, Length = 17 }
                        }
                    }
                },

                { "25-inches-wide", new OptiCutterLinearCutCalculatorRequest
                    {
                        Kerf = 0,
                        Stock = new List<OptiCutterLinearCutCalculatorBoard>
                        {
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 96 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 120 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity= 50, Length = 144, },
                        },
                        Requirements = new List<OptiCutterLinearCutCalculatorBoard>
                        {
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 5, Length = 62,  },
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 2, Length = 26.5 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 2, Length = 23 },
                            new OptiCutterLinearCutCalculatorBoard { Quantity = 2, Length = 22 }
                        }
                    }
                }


            };

            foreach((var scenarioName, var request) in deskScenarios)
            {
                var boardsResponse = await _optiCutterService.GetCalculatedBoardsUrl(request);
                var boardsPdf = await _optiCutterService.GetCalculatedBoardsPdf(boardsResponse.SessionCookie);
                try
                {
                    File.WriteAllBytes($"{Program.fileOutputPath}/{scenarioName}.pdf", boardsPdf);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
            return "I am finshed!";
        }
    }
}
