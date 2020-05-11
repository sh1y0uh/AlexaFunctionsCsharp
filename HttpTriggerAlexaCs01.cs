using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
// using System.Text.Json;
// using System.Text.Json.Serialization;
using System.Collections.Generic;
// Alexa.NETの名前空間を追加
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

namespace AlexaFunctions
{
    public static class HttpTriggerAlexaCs01
    {
        private static string IntroductionMessage = "アレクサ、誰かにあいさつしてと教えてね！";
        private static string name;
        private static string nameAnother;
        [FunctionName("HttpTriggerAlexaCs01")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            SkillRequest skillRequest = new SkillRequest();

            // HTTPリクエストのJson形式のボディ部をAlexaのSkillRequest型のオブジェクトにデシリアライズ
            // skillRequest = JsonSerializer.Deserialize<SkillRequest>(await req.ReadAsStringAsync());
            skillRequest = JsonConvert.DeserializeObject<SkillRequest>(await req.ReadAsStringAsync());
            log.LogInformation(await req.ReadAsStringAsync());


            // AlexaのSkillResponseオブジェクトのインスタンスを作成
            SkillResponse skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            Session session = new Session();

            switch (skillRequest.Request)
            {
                case LaunchRequest lr:
                    skillResponse = ResponseBuilder.Tell(IntroductionMessage);
                    break;
                case IntentRequest ir:
                    switch (ir.Intent.Name)
                    {
                        case "AMAZON.HelpIntent":
                            skillResponse = ResponseBuilder.Tell(IntroductionMessage);
                            break;
                        case "GreetingIntent":

                            name = ir.Intent.Slots["name"].Value;
                            nameAnother = ir.Intent.Slots["name"].Resolution.Authorities[0].Values[0].Value.Name;
                            skillResponse = ResponseBuilder.Ask($"{name}にあいさつしてよろしいですか？", new Reprompt($"あの～、{name}にあいさつしてよろしいですか？"));
                            break;
                        case "AMAZON.YesIntent":
                            skillResponse = ResponseBuilder.Tell($"こんにちは！{nameAnother}！");
                            break;
                        case "AMAZON.CancellIntent":
                        case "AMAZON.StopIntent":
                            skillResponse = ResponseBuilder.Tell("キャンセルしました。");
                            break;
                        default:
                            skillResponse = ResponseBuilder.Tell("すみません、わかりません。");
                            break;

                    }
                    break;
                default: skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech { Text = "すみません。わかりません", }; break;
            }
            return new OkObjectResult(skillResponse);
        }
    }
}