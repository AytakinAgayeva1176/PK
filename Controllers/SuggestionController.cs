using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PK.Helpers;
using PK.Models;
using PK.ViewModels;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionController : ControllerBase
    {
        #region ctor
        private readonly TokenSettings tokenSettings;
        private readonly string uri = "http://172.16.10.132:3574/nc/ferrum_pk_5det/api/v1/suggest";
        public SuggestionController(TokenSettings tokenSettings)
        {
            this.tokenSettings = tokenSettings;
        }

        #endregion

        #region Create
        /// <summary>
        /// Create Suggestion
        /// </summary>
        /// <param name="model">Suggestion model </param>
        /// <returns></returns>
        [HttpPost("Create")]
        public async Task<IActionResult> Create(SuggestionVM model)
        {
            var uniq_id = Guid.NewGuid().ToString();
            Suggestion suggestion = new Suggestion()
            {
                user_id = model.user_id,
                session = model.session,
                uniq_id = uniq_id,
                reference_uri = model.reference_uri,
                point = model.point,
                content = model.content,
            };
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var data = JsonConvert.SerializeObject(suggestion);
            StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");
            var json = await client.PostAsync(uri, queryString);

            if (json.IsSuccessStatusCode)
            {

                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Suggestion>(EmpResponse);
                return Ok(new SystemMessaging(MesagesCode.Insert, "Suggestion insert succesfully", item));
            }

            return BadRequest();
        }

        #endregion



    }
}
