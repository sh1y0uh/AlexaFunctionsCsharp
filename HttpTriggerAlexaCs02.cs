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
    public static class HttpTriggerAlexaCs02
    {
        private static string IntroductionMessage = "アレクサ、アニマルブックスのおすすめ商品を教えてと声をかけてください。";
        [FunctionName("HttpTriggerAlexaCs02")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            SkillRequest skillRequest = new SkillRequest();

            // HTTPリクエストのJson形式のボディ部をAlexaのSkillRequest型のオブジェクトにデシリアライズ
            // skillRequest = JsonSerializer.Deserialize<SkillRequest>(await req.ReadAsStringAsync());
            skillRequest = JsonConvert.DeserializeObject<SkillRequest>(await req.ReadAsStringAsync());

            // AlexaのSkillResponseオブジェクトのインスタンスを作成
            SkillResponse skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            switch (skillRequest.Request)
            {
                // LaunchRequest：スキル呼び出しリクエストの場合
                case LaunchRequest lr:
                    skillResponse = ResponseBuilder.Tell(IntroductionMessage);
                    break;
                case IntentRequest ir:
                    switch (ir.Intent.Name)
                    {
                        case "AMAZON.HelpIntent":
                            skillResponse = ResponseBuilder.Tell(IntroductionMessage);
                            break;
                        case "RecommendIntent":
                            skillResponse = ResponseBuilder.Ask($"{ir.Intent.Slots["item"].Resolution.Authorities[0].Values[0].Value.Id}のおすすめを{ir.Intent.Slots["amount"].Value}つでよろしいでしょうか？", new Reprompt($"あの～、{ir.Intent.Slots["item"].Value}のおすすめでよろしいでしょうか？"));
                            break;
                        case "AMAZON.YesIntent":
                            skillResponse = ResponseBuilder.Tell("おすすめの商品はアレクサです。");
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