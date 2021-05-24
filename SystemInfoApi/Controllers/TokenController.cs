using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemInfoApi.Models;
using SystemInfoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace SystemInfoApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class TokenController : Controller
    {
        private readonly IConfiguration config;
        public TokenController(IConfiguration configuration_)
        {
            config = configuration_;
        }
        
        /// <summary>
        /// Get the Authorization Token.
        /// </summary>
        /// <param name="LoginInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Token([FromBody] LoginInfo Login)
        {
            try
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                Log.Information($"Remote Ip Address: {remoteIp}, UserAgent: {userAgent}");
                
                var settingUserName = config.GetSection("Login").GetSection("userName").Value;
                var settingPassword = config.GetSection("Login").GetSection("password").Value;
                
                if (Login.User == settingUserName && Login.Password == settingPassword)
                {
                    var jwt = new JwtService(config);
                    var token = jwt.GenerateSecurityToken(
                        Login.User,
                        Login.Password);
                    return await Task.Run(() => Ok(token));
                }
                else
                {
                    return await Task.Run(() => Unauthorized($"Username or Password mismatch."));
                }
                

            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}\n{ex}");
                return await Task.Run(() => Forbid(string.Empty));
            }
            
            
        }
    }
}