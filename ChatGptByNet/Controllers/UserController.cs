using ChatGptByNet.Helper;
using ChatGptByNet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatGptByNet.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ChatGptDbContext _chatGptDbContext;

        public UserController(ILogger<UserController> logger,ChatGptDbContext chatGptDbContext)
        {
            _logger = logger;
            _chatGptDbContext = chatGptDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Register([Required] UserModel userModel)
        {
            if (userModel == null )
                return BadRequest("用户信息不能为空");

            userModel.PassWord = MD5Helper.CreateMD5(userModel.PassWord);

            _chatGptDbContext.UserModels.Add(userModel);
            try
            {
                if(await _chatGptDbContext.SaveChangesAsync()>0)
                    return Ok("注册成功");
            }
            catch (Exception e)
            {
                _logger.LogError($"注册失败{e.Message}");
                return BadRequest(e.Message);
            }
            return BadRequest("注册失败");
        }

        /// <summary>
        /// 登录后更新最后更新时间来保证token时效性
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        public  IActionResult Login(UserModel userModel)
        {
            if (userModel == null)
                return BadRequest("用户信息不能为空");

            userModel.PassWord = MD5Helper.CreateMD5(userModel.PassWord);
            var user = _chatGptDbContext.UserModels.Where(x => x.Email == userModel.Email ).FirstOrDefault();

            if(user == null)
                return BadRequest("用户名或者密码错误");

            if (user.Count == 0)
                return BadRequest("用户请求次数达到限制");

            if(user.Status == 1)
            {
                var token = EncryptAndDecryptHelper.EncryptString(user.Email, "abddccaaxxmmnnqqoowwdd");
                return Ok(token);
            }
            else
            {
                return BadRequest("用户状态错误");
            }
        }
    }
}
