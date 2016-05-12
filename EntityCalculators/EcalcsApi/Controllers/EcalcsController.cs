using EcalcsApi.Models;
using EcalcsApi.Services;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace EcalcsApi.Controllers
{
    public class EcalcsController : ApiController
    {
        private const string LOG_TAG = "EcalcsController";
        private const string EcalcsEntitiesUrl = "{0}/api/entities/all";
        private const string EcalcsEntityViewUrl = "{0}/api/entities/{1}/{2}";

        private ISettingService _settingService;

        public EcalcsController()
        {
            _settingService = ServiceFactory.GetSettingService();
        }

        // GET api/ecalcs/entities
        [SwaggerOperation("GetEntities")]
        [SwaggerResponse(HttpStatusCode.OK, "Entities", typeof(List<Entity>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/ecalcs/entities", Name = "Entities")]
        public async Task<IHttpActionResult> GetEntities()
        {
            var error = "";

            try
            {
                var url = EcalcsEntitiesUrl;
                url = string.Format(url, _settingService.GetEcalsBaseUrl());
                return Ok(await GetAllEntities(url));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
        }

        // GET api/ecalcs/0/0
        [SwaggerOperation("GetEntityViewByTypeNKey")]
        [SwaggerResponse(HttpStatusCode.OK, "Ecalcs Data", typeof(EcalcsView))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "An exception occured", typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("api/ecalcs/{type}/{businesskey}", Name = "EntityView")]
        public async Task<IHttpActionResult> GetEntityViewByTypeNKey(int type, int businesskey)
        {
            var error = "";

            try
            {
                var url = EcalcsEntityViewUrl;
                url = string.Format(url, _settingService.GetEcalsBaseUrl(), type, businesskey);
                return Ok(new EcalcsView(await GetHierarchyViewByUrl(url)));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
        }

        // PRIVATE
        private async Task<List<Entity>> GetAllEntities(string url)
        {
            DateTime now = DateTime.Now;
            HttpClient client = null;
            List<Entity> entities= new List<Entity>();

            try
            {
                client = new HttpClient();

                //var base64stuff = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userId, token)));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64stuff);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(url);

                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("GetAllEntities returned: " + response.StatusCode);
                }

                entities = JsonConvert.DeserializeObject<List<Entity>>(responseString,
                                                      new JsonSerializerSettings()
                                                      {
                                                          NullValueHandling = NullValueHandling.Ignore
                                                      });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }

            return entities;
        }

        private async Task<HierarchyView> GetHierarchyViewByUrl(string url)
        {
            DateTime now = DateTime.Now;
            HttpClient client = null;
            HierarchyView view = new HierarchyView();

            try
            {
                client = new HttpClient();

                //var base64stuff = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userId, token)));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64stuff);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(url);

                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("GetHierarchyViewByUrl returned: " + response.StatusCode);
                }

                view = JsonConvert.DeserializeObject<HierarchyView>(responseString,
                                                      new JsonSerializerSettings()
                                                      {
                                                          NullValueHandling = NullValueHandling.Ignore
                                                      });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }

            return view;
        }
    }
}
