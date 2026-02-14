using Microsoft.AspNetCore.Http;
using System.Net;

namespace CarRental.Api.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Obtiene la dirección IP real del cliente, considerando proxies y load balancers
        /// </summary>
        public static string GetClientIpAddress(this HttpContext context)
        {
            if (context == null)
                return "Unknown";

            // 1. Intentar obtener IP de headers de proxy/load balancer
            var ipAddress = GetIpFromHeaders(context);

            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "Unknown")
                return ipAddress;

            // 2. Si no hay headers, usar la conexión remota directa
            ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown";

            // 3. Convertir ::1 (IPv6 localhost) a 127.0.0.1 (IPv4 localhost) para consistencia
            if (ipAddress == "::1")
                return "127.0.0.1";

            // 4. Si es IPv6, intentar mapear a IPv4
            if (IPAddress.TryParse(ipAddress, out var ip))
            {
                // Si es IPv6 mapeado a IPv4, extraer la parte IPv4
                if (ip.IsIPv4MappedToIPv6)
                {
                    ipAddress = ip.MapToIPv4().ToString();
                }
            }

            return ipAddress;
        }

        /// <summary>
        /// Intenta obtener la IP real de los headers HTTP cuando hay proxies/load balancers
        /// </summary>
        private static string GetIpFromHeaders(HttpContext context)
        {
            // Lista de headers en orden de prioridad
            var headerKeys = new[]
            {
                "X-Forwarded-For",      // Header estándar de proxies
                "X-Real-IP",            // Header común en Nginx
                "CF-Connecting-IP",     // Cloudflare
                "True-Client-IP",       // Akamai y Cloudflare Enterprise
                "X-Client-IP",          // Alternativa común
                "X-Cluster-Client-IP",  // Rackspace y Riverbed
                "Forwarded"             // RFC 7239 (más nuevo)
            };

            foreach (var key in headerKeys)
            {
                if (context.Request.Headers.TryGetValue(key, out var values))
                {
                    var ipString = values.FirstOrDefault();

                    if (string.IsNullOrEmpty(ipString))
                        continue;

                    // X-Forwarded-For puede contener múltiples IPs separadas por coma
                    // Formato: "client, proxy1, proxy2"
                    // La primera IP es la del cliente real
                    var ips = ipString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var ip in ips)
                    {
                        var trimmedIp = ip.Trim();

                        // Validar que sea una IP válida
                        if (IPAddress.TryParse(trimmedIp, out var parsedIp))
                        {
                            // Ignorar IPs privadas/locales si estamos buscando la IP pública
                            // Comentar estas líneas si quieres capturar también IPs privadas
                            if (IsPrivateIpAddress(parsedIp))
                                continue;

                            return trimmedIp;
                        }
                    }
                }
            }

            return "Unknown";
        }

        /// <summary>
        /// Determina si una IP es privada/local
        /// </summary>
        private static bool IsPrivateIpAddress(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return ipAddress.IsIPv6LinkLocal ||
                       ipAddress.IsIPv6SiteLocal ||
                       ipAddress.ToString() == "::1";
            }

            var bytes = ipAddress.GetAddressBytes();

            return bytes[0] switch
            {
                10 => true,                                          // 10.0.0.0/8
                127 => true,                                         // 127.0.0.0/8 (localhost)
                172 => bytes[1] >= 16 && bytes[1] <= 31,            // 172.16.0.0/12
                192 => bytes[1] == 168,                              // 192.168.0.0/16
                169 => bytes[1] == 254,                              // 169.254.0.0/16 (link-local)
                _ => false
            };
        }

        /// <summary>
        /// Obtiene el User Agent del cliente
        /// </summary>
        public static string GetUserAgent(this HttpContext context)
        {
            if (context == null)
                return "Unknown";

            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Limitar longitud para evitar problemas en base de datos
            if (userAgent.Length > 500)
                userAgent = userAgent.Substring(0, 500);

            return string.IsNullOrEmpty(userAgent) ? "Unknown" : userAgent;
        }
    }
}