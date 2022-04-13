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
    public class ShortcutController : ControllerBase
    {
        #region ctor
        private readonly TokenSettings tokenSettings;
        private readonly string uri = "http://172.16.10.132:3574/nc/ferrum_pk_5det/api/v1/shortcut";
        public ShortcutController(TokenSettings tokenSettings)
        {
            this.tokenSettings = tokenSettings;
        }

        #endregion

        #region Create
        /// <summary>
        /// Create Shortcut
        /// </summary>
        /// <param name="model">Shortcut model </param>
        /// <returns></returns>
        [HttpPost("Create")]
        public async Task<IActionResult> Create(ShortcutVM model)
        {
            var uniq_id = Guid.NewGuid().ToString();
            Shortcut shortcut = new Shortcut()
            {
                link = model.link,
                uniq_id = uniq_id,
                user_id = model.user_id,
                selected = model.selected
            };
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var data = JsonConvert.SerializeObject(shortcut);
            StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");
            var json = await client.PostAsync(uri, queryString);

            if (json.IsSuccessStatusCode)
            {
               
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Shortcut>(EmpResponse);
                return Ok(new SystemMessaging(MesagesCode.Insert, "Shortcut insert succesfully",item));
            }

            return BadRequest();
        }


        #endregion


        #region GetByUnique_Id / FindOne
        /// <summary>
        /// Get Shortcut By UserId
        /// </summary>
        /// <param name="user_id"> User id of Shortcut </param>
        /// <returns></returns>
        [HttpPost("GetByUniqueId/{user_id}")]
        public async Task<IActionResult> GetByUniqueId(string user_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri + "/findOne?where=(user_id,like," + user_id + ")");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Shortcut>(EmpResponse);
                if (item == null)
                {
                    var result = new SystemMessaging(MesagesCode.NotFound, "Shortcut doesn't exist");
                    return Ok(result);
                }
                return Ok(item);

            }
            return BadRequest();
        }

        #endregion



        #region Update
        /// <summary>
        /// Update Shortcut
        /// </summary>
        /// <param name="model">Shortcut model</param>
        /// <returns></returns>
        [HttpPut("Update")]
        public async Task<IActionResult> Update(ShortcutVM model)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var Shortcut = await client.GetAsync(uri + "/findOne?where=(user_id,like," + model.user_id + ")");
            if (Shortcut.IsSuccessStatusCode)
            {
                var EmpResponse = Shortcut.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Shortcut>(EmpResponse);
                if (item != null)
                {
                    item.selected=model.selected;
                    item.link=model.link;   
                    var data = JsonConvert.SerializeObject(item);
                    StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");

                    var json = await client.PutAsync(uri +"/"+ item.id , queryString);

                    if (json.IsSuccessStatusCode) return Ok(new SystemMessaging(MesagesCode.Update, "Shortcut updated succesfully!", item));
                    else  return BadRequest(new SystemMessaging(MesagesCode.Exception, "Shortcut couldn't update!",item));
                }
                else return BadRequest(new SystemMessaging(MesagesCode.NotFound, "Shortcut doesn't exist", model));
            }

            return BadRequest();
        }


        #endregion

    }
}
