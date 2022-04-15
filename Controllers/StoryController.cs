using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using PK.Helpers;
using PK.Helpers.Enums;
using PK.Models;
using PK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        #region ctor
        private readonly TokenSettings tokenSettings;
        private readonly FtpSettings ftpSettings;
        private readonly string uri = "http://172.16.10.132:3574/nc/ferrum_pk_5det/api/v1/story_section";
        public StoryController(TokenSettings tokenSettings, FtpSettings ftpSettings)
        {
            this.tokenSettings = tokenSettings;
            this.ftpSettings = ftpSettings;
        }

        #endregion

        #region Create
        /// <summary>
        /// Create Story
        /// </summary>
        /// <param name="model">Story model </param>
        /// <returns></returns>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm]StoryVM model)
        {
            var uniq_id = Guid.NewGuid().ToString();
            var content = new Random().Next(100000, 999999) + Path.GetExtension(model.content_source.FileName);
            var thumb = new Random().Next(100000, 999999) + Path.GetExtension(model.thumb_image.FileName);
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(model.content_source.FileName, out contentType);
            Story story = new Story()
            {
                content_source = "https://cdn.ferrumcapital.az/story/" + content,
                link = model.link,
                link_text = model.link_text,
                uniq_id = uniq_id,
                name = model.name,
                thumb_image = "https://cdn.ferrumcapital.az/story/" + thumb,
                type= contentType,
                status="active"
            };
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var data = JsonConvert.SerializeObject(story);
            StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");
            var json = await client.PostAsync(uri, queryString);

            if (json.IsSuccessStatusCode)
            {
                #region FTP
                FtpWebRequest requestContent =
               (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink  + "story/" + content);
                requestContent.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                requestContent.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream ftpStream = requestContent.GetRequestStream())
                {
                    model.content_source.CopyTo(ftpStream);
                }


                FtpWebRequest requestThumb =
              (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink + "story/" + thumb);
                requestThumb.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                requestThumb.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream ftpStream = requestThumb.GetRequestStream())
                {
                    model.thumb_image.CopyTo(ftpStream);
                }
                #endregion

                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Story>(EmpResponse);
                return Ok(new SystemMessaging(MesagesCode.Insert, "Story insert succesfully", item));
            }

            return BadRequest();
        }

        #endregion




        #region Activate
        /// <summary>
        /// Activate Story
        /// </summary>
        /// <param name="uniq_id">uniq_id of story</param>
        /// <returns></returns>
        [HttpPut("Activate")]
        public async Task<IActionResult> Activate(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var Story = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (Story.IsSuccessStatusCode)
            {
                var EmpResponse = Story.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Story>(EmpResponse);
                if (item != null)
                {
                    item.status = "active";
                    var data = JsonConvert.SerializeObject(item);
                    StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");

                    var json = await client.PutAsync(uri + "/" + item.id, queryString);

                    if (json.IsSuccessStatusCode) return Ok(new SystemMessaging(MesagesCode.Update, "Story activated succesfully!", item));
                    else return BadRequest(new SystemMessaging(MesagesCode.Exception, "Story couldn't activated!", item));
                }
                else return BadRequest(new SystemMessaging(MesagesCode.NotFound, "Story doesn't exist"));
            }

            return BadRequest();
        }


        #endregion


        #region Deactivate
        /// <summary>
        /// Deactivate Story
        /// </summary>
        /// <param name="uniq_id">uniq_id of story</param>
        /// <returns></returns>
        [HttpPut("Deactivate")]
        public async Task<IActionResult> Deactivate(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var Story = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (Story.IsSuccessStatusCode)
            {
                var EmpResponse = Story.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Story>(EmpResponse);
                if (item != null)
                {
                    item.status = "deactive";
                    var data = JsonConvert.SerializeObject(item);
                    StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");

                    var json = await client.PutAsync(uri + "/" + item.id, queryString);

                    if (json.IsSuccessStatusCode) return Ok(new SystemMessaging(MesagesCode.Update, "Story deactivated succesfully!", item));
                    else return BadRequest(new SystemMessaging(MesagesCode.Exception, "Story couldn't deactivated!", item));
                }
                else return BadRequest(new SystemMessaging(MesagesCode.NotFound, "Story doesn't exist"));
            }

            return BadRequest();
        }


        #endregion


        #region GetAllActive

        /// <summary>
        /// GetAllActive
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllActive")]
        public async Task<IActionResult> GetAllActive()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri + "?where=(status,eq,active)");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var items = JsonConvert.DeserializeObject<List<Story>>(EmpResponse);

                return Ok(items);

            }
            return BadRequest();
        }

        #endregion


        #region GetAll

        /// <summary>
        /// GetAll
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri);
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var items = JsonConvert.DeserializeObject<List<Story>>(EmpResponse);

                return Ok(items);

            }
            return BadRequest();
        }

        #endregion

        #region Delete
        /// <summary>
        /// Delete Story By UniqId
        /// </summary>
        /// <param name="uniq_id"> Unique id of Story </param>
        /// <returns></returns>
        [HttpDelete("Delete/{uniq_id}")]
        public async Task<IActionResult> Delete(string uniq_id)
        {
            SystemMessaging result;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var story = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (story.IsSuccessStatusCode)
            {
                var EmpResponse = story.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Story>(EmpResponse);
                if (item != null)
                {
                    var json = await client.DeleteAsync(uri + "/" + item.id);

                    if (json.IsSuccessStatusCode)
                    {

                        EmpResponse = json.Content.ReadAsStringAsync().Result;
                        if (EmpResponse == "1")
                        {
                            #region FTP
                            string contentPath = item.content_source;
                            int pos1 = contentPath.LastIndexOf("/") + 1;
                            FtpWebRequest request1 = (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink + "story/" + contentPath.Substring(pos1, contentPath.Length - pos1));
                            request1.Method = WebRequestMethods.Ftp.DeleteFile;
                            request1.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                            request1.GetResponse();

                            string thumbPath = item.thumb_image;
                            int pos2 = thumbPath.LastIndexOf("/") + 1;
                            FtpWebRequest request2 = (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink + "story/" + thumbPath.Substring(pos2, thumbPath.Length - pos2));
                            request2.Method = WebRequestMethods.Ftp.DeleteFile;
                            request2.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                            request2.GetResponse();
                            #endregion

                           
                            result = new SystemMessaging(MesagesCode.Delete, "Story deleted succesfully!");
                            return Ok(result);
                        }
                        else
                        {
                            result = new SystemMessaging(MesagesCode.Delete, "Story couldn't deleted!");
                            return BadRequest(result);
                        }

                    }
                }
                else
                {
                    result = new SystemMessaging(MesagesCode.Delete, "Story doesn't exist");
                    return BadRequest(result);
                }

            }

            return BadRequest();
        }


        #endregion
    }
}
