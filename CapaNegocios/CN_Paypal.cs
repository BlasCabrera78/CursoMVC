using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CapaNegocio
{
    // Contenedor genérico para envolver estado + respuesta de PayPal
    public class Response_Paypal<T>
    {
        public bool Status { get; set; }
        public T Response { get; set; }
        public string Error { get; set; }

        public static implicit operator Response_Paypal<T>(Response_Paypal<Response_Capture> v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator Response_Paypal<T>(Response_Paypal<Response_Checkout> v)
        {
            throw new NotImplementedException();
        }
    }

    // ========== MODELOS MÍNIMOS PARA CHECKOUT ==========
    // Adapta/expande según tu modelo real

    // Monto total
    public class ItemTotal
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }

    public class Breakdown
    {
        public ItemTotal item_total { get; set; }
    }

    public class Amount
    {
        public string currency_code { get; set; }
        public string value { get; set; }
        public Breakdown breakdown { get; set; }
    }

    public class UnitAmount
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }

    public class Item
    {
        public string name { get; set; }
        public UnitAmount unit_amount { get; set; }
        public string quantity { get; set; }
    }

    public class PurchaseUnit
    {
        public Amount amount { get; set; }
        public Item[] items { get; set; }
    }

    // Orden que envías a PayPal (request)
    public class Checkout_Order
    {
        public string intent { get; set; } = "CAPTURE";
        public PurchaseUnit[] purchase_units { get; set; }
        public ApplicationContext application_context { get; set; }


    }

    public class ApplicationContext
    {
        public string brand_name { get; set; }
        public string landing_page { get; set; }      // "NO_PREFERENCE"
        public string user_action { get; set; }       // "PAY_NOW"
        public string return_url { get; set; }
        public string cancel_url { get; set; }

        public static implicit operator ApplicationContext(CapaEntidades.Paypal.ApplicationContext v)
        {
            throw new NotImplementedException();
        }
    }

    // Respuesta de crear orden (simplificada)
    public class Response_Checkout
    {
        public string id { get; set; }          // order id
        public string status { get; set; }      // e.g., "CREATED"
        public LinkDescription[] links { get; set; }
    }

    public class LinkDescription
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }

    // Respuesta de captura
    public class Response_Capture
    {
        public string id { get; set; }
        public string status { get; set; }
        public PurchaseUnitCapture[] purchase_units { get; set; }
    }

    public class PurchaseUnitCapture
    {
        public PaymentCollection payments { get; set; }
    }

    public class PaymentCollection
    {
        public Capture[] captures { get; set; }
    }

    public class Capture
    {
        public string id { get; set; }
        public string status { get; set; }
        public Amount amount { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }

    // ========== SERVICIO CN_Paypal ==========
    public class CN_Paypal
    {
        private static readonly string urlpaypal = ConfigurationManager.AppSettings["UrlPaypal"];
        private static readonly string clientId = ConfigurationManager.AppSettings["ClienId"]; // ojo a la clave exacta en tu Web.config
        private static readonly string secret = ConfigurationManager.AppSettings["Secret"];

        // Crea la orden en PayPal
        public async Task<Response_Paypal<Response_Checkout>> CrearSolicitud(Checkout_Order orden)
        {
            var response_paypal = new Response_Paypal<Response_Checkout>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(urlpaypal);

                    var authToken = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                    var json = JsonConvert.SerializeObject(orden);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("/v2/checkout/orders", data);

                    response_paypal.Status = response.IsSuccessStatusCode;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonRespuesta = await response.Content.ReadAsStringAsync();
                        var checkout = JsonConvert.DeserializeObject<Response_Checkout>(jsonRespuesta);

                        response_paypal.Response = checkout;
                    }
                    else
                    {
                        response_paypal.Error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                response_paypal.Status = false;
                response_paypal.Error = ex.Message;
            }

            return response_paypal;
        }

        // Captura (aprueba) el pago de una orden
        // token = id de la orden (por ejemplo, el que recibes en return_url)
        public async Task<Response_Paypal<Response_Capture>> AprobarPago(string token)
        {
            var response_paypal = new Response_Paypal<Response_Capture>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(urlpaypal);

                    var authToken = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                    // Cuerpo vacío "{}" como en tus capturas
                    var data = new StringContent("{}", Encoding.UTF8, "application/json");

                    // POST /v2/checkout/orders/{token}/capture
                    HttpResponseMessage response = await client.PostAsync($"/v2/checkout/orders/{token}/capture", data);

                    response_paypal.Status = response.IsSuccessStatusCode;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonRespuesta = await response.Content.ReadAsStringAsync();
                        var capture = JsonConvert.DeserializeObject<Response_Capture>(jsonRespuesta);

                        response_paypal.Response = capture;
                    }
                    else
                    {
                        response_paypal.Error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                response_paypal.Status = false;
                response_paypal.Error = ex.Message;
            }

            return response_paypal;
        }

        public async Task<Response_Paypal<CapaEntidades.Paypal.Response_Checkout>> CrearSolicitud(CapaEntidades.Paypal.Checkout_Order oCheckOutOrder)
        {
            throw new NotImplementedException();
        }
    }
}