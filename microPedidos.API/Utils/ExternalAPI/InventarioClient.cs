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

                response.EnsureSuccessStatusCode(); 

                return response;
            }
            catch (HttpRequestException httpEx)
            {
                // Manejo de errores HTTP
                Console.WriteLine($"Error en la petición HTTP: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
                throw;
            }
        }

    }
}
