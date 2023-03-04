using ChatGptByNet.Filters;
using Microsoft.AspNetCore.Mvc;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System.Security.Claims;

namespace ChatGptByNet.Controllers
{
    [Route("api/[controller][action]")]
    [ApiController]
    [UserAuthorize]
    public class ChatGptController : ControllerBase
    {
        private readonly ILogger<ChatGptController> _logger;
        private readonly IOpenAIService _openAIService;
        private readonly ChatGptDbContext _chatGptDbContext;

        public ChatGptController(ILogger<ChatGptController> logger, IOpenAIService openAIService, ChatGptDbContext chatGptDbContext)
        {
            _openAIService = openAIService;
            _logger = logger;
            _chatGptDbContext = chatGptDbContext;   
        }

        /// <summary>
        /// 普通GPT3.0
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet(Name ="GetText")]
        public async Task<IActionResult> Get(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length > 1000)
                return BadRequest("字符为空或者超出限制");

            if(HttpContext.User == null || HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name) == null)
            {
                return BadRequest("用户信息错误");
            }

            try
            {
                var email = HttpContext.User.Claims.First(m => m.Type == ClaimTypes.Name).Value;
                var user = _chatGptDbContext.UserModels.FirstOrDefault(m => m.Email == email);

                if (user == null)
                {
                    return BadRequest("用户信息错误");
                }

                if (user.Count == 0)
                {
                    return BadRequest("用户达到最大请求限制");
                }

                var completionResult = await _openAIService.Completions.CreateCompletion(new CompletionCreateRequest()
                {
                    Prompt = text,
                    Model =  OpenAI.GPT3.ObjectModels.Models.TextDavinciV3,
                    MaxTokens = 2048
                });

                if (completionResult.Successful)
                {
                    _chatGptDbContext.LogModels.Add(new Models.LogModel
                    {
                        Request = text,
                        Response = completionResult.Choices.FirstOrDefault()?.Text,
                        InsertTime = DateTime.Now,
                        UserName = user.Email
                    });
                    user.Count -= 1;
                    _chatGptDbContext.UserModels.Update(user);
                    await _chatGptDbContext.SaveChangesAsync();
                    return Ok(completionResult.Choices.FirstOrDefault()?.Text);
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }
                    _logger.LogError($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                throw;
            }
            return Ok();
        }

        /// <summary>
        /// Chat Gpt 3.5
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetByChatGpt")]
        public async Task<IActionResult> GetByChatGpt(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length > 1000)
                return BadRequest("字符为空或者超出限制");

            if (HttpContext.User == null || HttpContext.User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name) == null)
            {
                return BadRequest("用户信息错误");
            }

            try
            {
                var user = _chatGptDbContext.UserModels.FirstOrDefault(m => m.Email == HttpContext.User.Claims.First(m => m.Type == ClaimTypes.Name).Value);

                if (user == null)
                {
                    return BadRequest("用户信息错误");
                }

                if (user.Count == 0)
                {
                    return BadRequest("用户达到最大请求限制");
                }

                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are a helpful assistant."),
                    ChatMessage.FromUser("Who won the world series in 2020?"),
                    ChatMessage.FromAssistance("The Los Angeles Dodgers won the World Series in 2020."),
                    ChatMessage.FromUser("Where was it played?")
                },
                    Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo,
                    MaxTokens = 1000//optional
                });

                if (completionResult.Successful)
                {
                    _chatGptDbContext.LogModels.Add(new Models.LogModel
                    {
                        Request = text,
                        Response = completionResult.Choices.First().Message.Content,
                        InsertTime = DateTime.Now,
                        UserName = user.Email
                    });
                    user.Count -= 1;
                    _chatGptDbContext.UserModels.Update(user);
                    await _chatGptDbContext.SaveChangesAsync();
                    return Ok(completionResult.Choices.First().Message.Content);
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }
                    _logger.LogError($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                throw;
            }
            return Ok();
        }
    }
}
