using Microsoft.AspNetCore.Mvc;

namespace HourConvertor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class HourConverter(IHttpClientFactory httpClientFactory) : ControllerBase
    {
        [HttpGet("time-difference")]
        public async Task<IActionResult> GetTimeDifference([FromQuery] string targetZone)
        {
            var baseZone = "America/Costa_Rica";

            if (string.IsNullOrWhiteSpace(targetZone))
            {
                return BadRequest(new { Error = "Debes especificar una zona válida." });
            }

            // 1. Limpiamos espacios vacíos accidentales que suelen llegar desde el HTML
            targetZone = targetZone.Trim();

            try
            {
                var httpClient = httpClientFactory.CreateClient();

                // 2. Codificamos las variables para que el carácter '/' viaje seguro por internet
                var safeBaseZone = Uri.EscapeDataString(baseZone);
                var safeTargetZone = Uri.EscapeDataString(targetZone);

                // Petición 1: Zona Base
                var baseResponse = await httpClient.GetFromJsonAsync<TimeApiResponse>($"https://timeapi.io/api/Time/current/zone?timeZone={safeBaseZone}");

                // Petición 2: Zona Destino
                var targetResponse = await httpClient.GetFromJsonAsync<TimeApiResponse>($"https://timeapi.io/api/Time/current/zone?timeZone={safeTargetZone}");

                if (baseResponse == null || targetResponse == null)
                {
                    return StatusCode(500, new { Error = "Error al obtener datos de la API externa." });
                }

                var baseDate = DateTime.Parse(baseResponse.DateTime);
                var targetDate = DateTime.Parse(targetResponse.DateTime);

                var difference = targetDate - baseDate;
                var hoursDifference = Math.Round(difference.TotalHours, 1);

                string message = hoursDifference > 0
                    ? $"Están {hoursDifference} hora(s) adelantados."
                    : hoursDifference < 0
                        ? $"Están {Math.Abs(hoursDifference)} hora(s) atrasados."
                        : "Están en la misma hora exacta.";

                return Ok(new
                {
                    BaseZone = baseZone,
                    BaseTime = baseResponse.Time,
                    TargetZone = targetZone,
                    TargetTime = targetResponse.Time,
                    DifferenceInHours = hoursDifference,
                    Message = message
                });
            }
            catch (HttpRequestException)
            {
                // 3. Este bloque nos salvará la vida: si la API externa devuelve 400,
                // esto te imprimirá en pantalla exactamente qué texto corrupto llegó al backend.
                return StatusCode(400, new { Error = $"La API externa no reconoció la zona: '{targetZone}'. Revisa lo que envía el frontend." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Ocurrió un error interno: {ex.Message}" });
            }
        }
    }

    internal class TimeApiResponse
    {
        public string Time { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
    }
}
