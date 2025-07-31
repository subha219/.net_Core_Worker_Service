using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;

namespace DemoWindowService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string FilePath = "Output.txt";

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
       
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        var httpClient = _httpClientFactory.CreateClient();
                        var response = await httpClient.GetAsync("https://random.dog/woof.json");
                       
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                               
                                responseModel result = JsonSerializer.Deserialize<responseModel>(content, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                                CreateXmlFile(result);
                            }
                        }
                        else
                        {
                            AppendToFile($"Warning: {response.StatusCode}");
                            AppendToFile($"Information: HTTP request failed with status code: {{StatusCode}} {response.StatusCode}");
                        }
                    }
                    await Task.Delay(1, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                AppendToFile($"Error: {ex.Message}");
            }
        }

        private void WriteToFile(string content)
        {
            File.WriteAllText(FilePath, content + Environment.NewLine);
        }

        private void AppendToFile(string content)
        {
            File.AppendAllText(FilePath, content + Environment.NewLine);
        }

        private void CreateXmlFile(responseModel responseModel)
        {
            try
            {
                string filename = responseModel.fileSizeBytes + ".xml";
                var doc = new XDocument(
                    new XElement("Dogs",
                        new XElement("Dog",
                            new XElement("FileSize", responseModel.fileSizeBytes),
                            new XElement("ImageUrl", responseModel.url)
                        )
                    )
                );

                string filePath = Path.Combine(AppContext.BaseDirectory+ "//APIResponse", filename);
                doc.Save(filePath);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
