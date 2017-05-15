using Microsoft.ApplicationInsights;
using NasaBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Serialization;

namespace NasaBot.Controllers
{
    public class MessageController : ApiController
    {
        #region Properties
        private string ProcessFileName = "Process.xml";
        private string CurrentIntent = string.Empty;
        #endregion

        private TelemetryClient _client;

        public TelemetryClient client
        {
            get
            {
                return _client = _client ?? new TelemetryClient();
            }
        }

        [ResponseType(typeof(QueryResponse))]
        public async Task<IHttpActionResult> Post([FromBody] QueryRequest request)
        {
            try
            {
                var value = JsonConvert.SerializeObject(request);

                client.TrackTrace("The post value is " + value);

                QueryResponse response = null;

                var messageText = request.Query;

                if (!string.IsNullOrEmpty(messageText))
                {
                    response = GetAnswer(request);
                }
                if (response != null)
                {
                    return CreatedAtRoute("DefaultApi", null, response);
                }
                return CreatedAtRoute("DefaultApi", null, new QueryResponse { Response = "Sorry, I haven't learned that yet", SessionId = string.Empty });
            }
            catch (Exception ex)
            {
                return CreatedAtRoute("DefaultApi", null, new QueryResponse { Response = "Sorry, I haven't learned that yet", SessionId = string.Empty });
            }
        }

        private QueryResponse GetAnswer(QueryRequest request)
        {
            if (!string.IsNullOrEmpty(request.Query))
            {
                var response = QueryApiAi(request);
                if(response==null)
                {
                    response = QueryQnA(request);
                }
                return response;
            }
            else
            {
                return null;
            }
        }

        private QueryResponse QueryApiAi(QueryRequest request)
        {
            try
            {
                string uri = $"https://api.api.ai/v1/query?v=20150910&query={request.Query}&lang=en&sessionId={request.SessionId}";
                using (var aiCLient = new HttpClient())
                {
                    aiCLient.DefaultRequestHeaders.Add("Authorization", "Bearer 74f1e3958c9c4156ac1c05c751b56d61");

                    client.TrackTrace("The nlu post value is " + uri);

                    var response = aiCLient.GetAsync(new Uri(uri));
                    var dataString = response.Result.Content.ReadAsStringAsync();

                    dynamic apiResponse = JsonConvert.DeserializeObject(dataString.Result);
                    if (apiResponse != null && apiResponse.result != null)
                    {
                        var processEntities = GetEntities();

                        var listedIntents = GetListedIntents();

                        CurrentIntent = apiResponse.result.metadata?.intentName?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(CurrentIntent) && !CurrentIntent.Equals("Default Fallback Intent", StringComparison.InvariantCultureIgnoreCase))
                        {

                            if (listedIntents.Any(x => x.Name == CurrentIntent))
                            {
                                GenerateProcessSession(request.SessionId);
                            }

                            var nluEntities = new Dictionary<string, object>();

                            if (apiResponse.result.parameters != null)
                            {
                                foreach (var processEntity in processEntities)
                                {
                                    if (IsPropertyExist(apiResponse.result.parameters, processEntity.Entity))
                                    {
                                        ProcessNextStep(request.SessionId, processEntity.Entity, apiResponse.result.parameters[processEntity.Entity]);
                                        nluEntities.Add(processEntity.Entity, apiResponse.result.parameters[processEntity.Entity]);
                                    }
                                }
                            }

                            var queryResponse = ContinueSequentialFlow(request);

                            return !string.IsNullOrEmpty(queryResponse?.Response ?? string.Empty) ? queryResponse : null;
                        }
                        else
                        {
                            string smallTalk = apiResponse.result?.action.ToString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(smallTalk) && (smallTalk.StartsWith("smalltalk") || smallTalk.StartsWith("input.unknown")))
                            {
                                return new QueryResponse { Response = apiResponse.result?.fulfillment?.speech.ToString() ?? string.Empty, Intent = string.Empty, SessionId = request.SessionId };
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private QueryResponse QueryQnA(QueryRequest request)
        {
            try
            {
                string uri = $"https://westus.api.cognitive.microsoft.com/qnamaker/v1.0/knowledgebases/a6b25d23-f807-4257-8d4e-1b31e4c93bce/generateAnswer";
                using (var aiCLient = new HttpClient())
                {
                    aiCLient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "65a01e9af16a4c78a0a736a10efb920f");
                    QnaRequest qna = new QnaRequest { question = request.Query };
                    var input = JsonConvert.SerializeObject(qna);
                    var response = aiCLient.PostAsync(uri, new StringContent(input.ToString(), Encoding.UTF8, "application/json"));
                    var dataString = response.Result.Content.ReadAsStringAsync();

                    QnaResponse data = JsonConvert.DeserializeObject<QnaResponse>(dataString.Result);
                    if (!string.IsNullOrEmpty(data?.answer ?? string.Empty))
                    {
                        return new QueryResponse { Response = data.answer, Intent = string.Empty, SessionId = request.SessionId };
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private QueryResponse ContinueSequentialFlow(QueryRequest request)
        {
            var processObject = (Process)WebCache.Get(request.SessionId);
            QueryResponse response = new QueryResponse() { SessionId = request.SessionId };
            if (processObject == null)
            {
                return null;
            }

            if (processObject != null)
            {
                
                foreach (var question in processObject.Questions)
                {
                    if (question.Intent == CurrentIntent)
                    {
                        ContinueSequentialFlowStep(ref response, question, processObject.GlobalVariables);
                    }
                }
            }

            return response;

        }

        private void ContinueSequentialFlowStep(ref QueryResponse response, Question question, IEnumerable<Variable> globalVariables)
        {
            foreach (var step in question.Steps)
            {
                if (string.IsNullOrEmpty(step.Value) && !step.IsOptional)
                {
                    if (!string.IsNullOrEmpty(step.CustomBinding))
                    {
                        List<string> stepValues = new List<string>();
                        foreach (var stepInput in step.StepInputs.OrderBy(x => x.InputFieldIndex))
                        {
                            if (stepInput.IsGlobal)
                            {
                                if (globalVariables?.Any() ?? false)
                                {
                                    stepValues.Add(globalVariables.ElementAt(stepInput.InputFieldIndex).Value);
                                }
                            }
                            else
                            {
                                stepValues.Add(question.Steps.ElementAt(stepInput.InputFieldIndex).Value);
                            }
                        }

                        GenerateCustomSelection(ref response, step.CustomBinding, stepValues, step.CustomMapping, step.Question, step.StepButtons, step.ReturnAttachment);
                        question.ClearValues = true;
                        break;
                    }
                    else
                    {
                        response.Response = step.Question;
                        question.ClearValues = false;
                        break;
                    }
                }
            }
        }

        private void GenerateCustomSelection(ref QueryResponse queryResponse, string url, IEnumerable<string> inputs, string mappingFile, string stepInfo, IEnumerable<StepButton> stepButtons, bool isAttachment)
        {
            var processObject = (Process)WebCache.Get(queryResponse.SessionId);

            if (processObject != null)
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var clientUrl = string.Format(url, inputs.ToArray());

                var response = httpClient.GetAsync(clientUrl);
                var dataString = response.Result.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject(dataString.Result);

                //Read the mapping file
                XmlSerializer serializer = new XmlSerializer(typeof(Mapping));
                StreamReader reader = new StreamReader(HostingEnvironment.MapPath(string.Format("~/Content/{0}", mappingFile)));
                var mapping = (Mapping)serializer.Deserialize(reader);
                reader.Close();

                dynamic obj = apiResponse;
                string heirarcy = mapping.heirarcy;
                if (!string.IsNullOrEmpty(heirarcy))
                {
                    var pathSplit = heirarcy.Split('.');
                    obj = FinalObject(pathSplit, obj);
                }

                if (obj != null)
                {
                    queryResponse.Response = GetValueFromObject(apiResponse, mapping.responsetext);
                }
                else
                {
                    queryResponse.Response = "Nothing found";
                }
            }
        }

        #region Private methods

        private IEnumerable<ProcessEntity> GetEntities()
        {
            if (!string.IsNullOrEmpty(ProcessFileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Process));

                StreamReader reader = new StreamReader(HostingEnvironment.IsHosted ? HostingEnvironment.MapPath(string.Format("~/Content/{0}", ProcessFileName)) : string.Format("{0}\\Content\\{1}", System.AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\", string.Empty), ProcessFileName));
                var process = (Process)serializer.Deserialize(reader);
                reader.Close();

                return process.Entities;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<Intent> GetListedIntents()
        {
            if (!string.IsNullOrEmpty(ProcessFileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Process));

                StreamReader reader = new StreamReader(HostingEnvironment.IsHosted ? HostingEnvironment.MapPath(string.Format("~/Content/{0}", ProcessFileName)) : string.Format("{0}\\Content\\{1}", System.AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\", string.Empty), ProcessFileName));
                var process = (Process)serializer.Deserialize(reader);
                reader.Close();

                return process.ListedIntents;
            }
            else
            {
                return null;
            }
        }

        private void GenerateProcessSession(string session)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Process));

            StreamReader reader = new StreamReader(HostingEnvironment.MapPath(string.Format("~/Content/{0}", ProcessFileName)));
            var process = (Process)serializer.Deserialize(reader);
            reader.Close();
            process.SenderId = session;

            if (WebCache.Get(session) != null)
            {
                process = (Process)WebCache.Get(session);
                process.Questions.Where(x=> x.ClearValues).ToList().ForEach(x => x.Steps.ForEach(y => y.Value = string.Empty));
            }
            else
            {
                WebCache.Set(session, process, 5);
            }
        }

        private bool IsPropertyExist(dynamic settings, string name)
        {
            return settings[name] != null;
        }

        private void ProcessNextStep(string sender, string entity, dynamic witResult)
        {
            if (WebCache.Get(sender) != null)
            {
                var process = (Process)WebCache.Get(sender);

                foreach (var processItem in process.Questions)
                {
                    if (processItem.Intent == CurrentIntent)
                    {
                        ProcessStepValues(sender, entity, witResult, processItem.Steps);
                    }
                }
            }
        }

        private void ProcessStepValues(string sender, string entity, dynamic witResult, List<Step> steps)
        {
            foreach (var item in steps)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    if (string.Equals(item.FieldName, entity, StringComparison.InvariantCultureIgnoreCase))
                    {
                        switch (item.FieldType)
                        {
                            case "date":
                                CheckGenericDatesCovered(item, witResult, sender);
                                break;
                            case "number":
                                CheckGenericNumbersCovered(item, witResult.entity.ToString(), sender);
                                break;
                            case "JArray":
                                string values = string.Join(",", witResult);
                                CheckGenericObjectCovered(item, values, sender);
                                break;
                            case "String":
                                CheckGenericObjectCovered(item, witResult.ToString(), sender);
                                break;
                            case "unit-weight":
                                CheckGenericWeightCovered(item, witResult, sender);
                                break;
                            default:
                                CheckGenericObjectCovered(item, witResult.ToString(), sender);
                                break;
                        }
                        break;
                    }
                }
            }
        }

        private void CheckGenericNumbersCovered(Step step, string value, string sender)
        {
            if (!string.IsNullOrEmpty(value))
            {
                step.Value = value;
                FillGlobalValue(sender, step.FieldName, step.Value);
            }
        }

        private void CheckGenericObjectCovered(Step step, string value, string sender)
        {
            if (!string.IsNullOrEmpty(value))
            {
                step.Value = value;
                FillGlobalValue(sender, step.FieldName, step.Value);
            }
        }

        private void CheckGenericWeightCovered(Step step, dynamic value, string sender)
        {
            if (value != null)
            {
                if (value.HasValues == null)
                {
                    step.Value = value.amount + " " + value.unit;
                    FillGlobalValue(sender, step.FieldName, step.Value);
                }
            }
        }

        private void CheckGenericDatesCovered(Step step, dynamic mydates, string sender)
        {
            if (mydates.interpretation != null)
            {
                step.Value = mydates.interpretation.ToString("MM/dd/yyyy");
                FillGlobalValue(sender, step.FieldName, step.Value);
            }
        }

        private void FillGlobalValue(string sender, string fieldName, string value)
        {
            var process = (Process)WebCache.Get(sender);
            if (process != null)
            {
                var globalValue = process.GlobalVariables.FirstOrDefault(x => x.Name == fieldName);
                if (globalValue != null)
                {
                    globalValue.Value = value;
                }
            }
        }

        private string GetValueFromObject(dynamic obj, string property)
        {
            if (property.Contains("."))
            {
                dynamic innerobj = FinalObject(property.Split('.'), obj);
                return innerobj != null ? Convert.ToString(innerobj) : string.Empty;
            }
            else
            {
                return Convert.ToString(obj[property]);
            }
        }

        private dynamic FinalObject(string[] pathSplit, dynamic obj)
        {
            for (int i = 0; i < pathSplit.Length; i++)
            {
                obj = obj[pathSplit[i]];

                if (obj == null)
                    break;

                if (obj.GetType().Name == "JArray" && !(i + 1 == pathSplit.Length))
                {
                    obj = obj[0];
                }
            }

            return obj;
        }

        #endregion
    }
}
