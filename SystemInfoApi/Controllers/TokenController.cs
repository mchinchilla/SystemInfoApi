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
        private readonly IConfiguration configuration;
        public TokenController(IConfiguration configuration_)
        {
            configuration = configuration_;
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

                /*string reqSQL = $"select * from devices where mac_address = '{deviceInfo.mac_address.ToUpper()}' and dynamic_key='{deviceInfo.dynamic_key}' and device_uuid = '{deviceInfo.device_uuid}' and serial_number = '{deviceInfo.serial_number}'";

                using (var connection = new SqlConnection(sql_Conn))
                {
                    deviceInfo = ((List<DeviceInfo>)await connection.QueryAsync<DeviceInfo>(reqSQL)).SingleOrDefault();
                    if (deviceInfo != null)
                    {
                        var jwt = new JwtService(configuration);
                        var token = jwt.GenerateSecurityToken(
                            user,
                            password);
                        return await Task.Run(() => token);
                    }
                    else
                    {
                        return await Task.Run(() => string.Empty);
                    }
                }*/
                
                var jwt = new JwtService(configuration);
                var token = jwt.GenerateSecurityToken(
                    Login.User,
                    Login.Password);
                return await Task.Run(() => Ok(token));
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}\n{ex}");
                return await Task.Run(() => Forbid(string.Empty));
            }
            
            
        }
    }
}