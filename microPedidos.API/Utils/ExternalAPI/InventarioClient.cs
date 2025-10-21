using microPedidos.API.Model.Request;
using microPedidos.API.Model.Response;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace microPedidos.API.Utils.ExternalAPI
{
    public static class InventarioClient
    {
        private static readonly HttpClient _httpClient;

        // Bloque estático para inicializar HttpClient
        static InventarioClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(Variables.INVENTARIOAPI.Url)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<HttpResponseMessage> PutAsync(string endpoint, List<ActualizarStockProducto> request)
        {
            try
            {
                var content = JsonContent.Create(request);
                var response = await _httpClient.PutAsync(endpoint, content);

                // Leer el contenido ANTES de lanzar EnsureSuccessStatusCode
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error en PUT {endpoint}");
                    Console.WriteLine($"Status: {(int)response.StatusCode} - {response.ReasonPhrase}");
                    Console.WriteLine($"🧾 Detalle del error: {responseBody}");
                }
                else
                {
                    Console.WriteLine($"✅ PUT {endpoint} OK - {(int)response.StatusCode}");
                }

                // Esto lanza una excepción si no fue exitoso (mantiene tu flujo original)
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"⚠️ Error HTTP en {endpoint}: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error inesperado en {endpoint}: {ex.Message}");
                throw;
            }
        }


    }
}
