using NasaBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace NasaBot.Controllers
{
    public class CalculatorController : ApiController
    {
        [HttpGet]
        public object CalculateDistance(string planets)
        {
            JObject obj = new JObject();

            if (!string.IsNullOrEmpty(planets))
            {
                string[] planetData = planets.Split(',');
                if (planetData?.Any() ?? false)
                {
                    List<PlanetData> calculatedPlanets = new List<PlanetData>();
                    foreach (var item in planetData)
                    {
                        var p = PlanetProfileData.Data.FirstOrDefault(x => x.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase));
                        if (p !=null)
                        {
                            calculatedPlanets.Add(p);
                        }
                    }
                    if(calculatedPlanets?.Any() ?? false)
                    {
                        if(calculatedPlanets.Count == 1)
                        {
                            calculatedPlanets.Add(PlanetProfileData.Data.FirstOrDefault(x => x.Order == 3));
                        }
                        calculatedPlanets = calculatedPlanets.OrderBy(x => x.Order).ToList();
                        var planet1 = calculatedPlanets.FirstOrDefault().Distance;
                        var planet2 = calculatedPlanets.LastOrDefault().Distance;

                        var result = (planet2 - planet1) * 150;
                        obj.Add("result", $"The average distance between {calculatedPlanets.FirstOrDefault().Name} and {calculatedPlanets.LastOrDefault().Name} is {result} million kilometers. which is {result/1.6} million miles.");
                        return obj;
                    }
                }
            }

            obj.Add("result", $"Sorry, I am unable to find it.");
            return obj;
        }

        [HttpGet]
        public object CalculateDiameter(string diameterplanets)
        {
            JObject obj = new JObject();

            if (!string.IsNullOrEmpty(diameterplanets))
            {
                string[] planetData = diameterplanets.Split(',');
                if (planetData?.Any() ?? false)
                {
                    List<PlanetData> calculatedPlanets = new List<PlanetData>();
                    foreach (var item in planetData)
                    {
                        var p = PlanetProfileData.Data.FirstOrDefault(x => x.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase));
                        if (p != null)
                        {
                            calculatedPlanets.Add(p);
                        }
                    }
                    if (calculatedPlanets?.Any() ?? false)
                    {
                        if (calculatedPlanets.Count == 1)
                        {
                            calculatedPlanets.Add(PlanetProfileData.Data.FirstOrDefault(x => x.Order == 3));
                        }
                        calculatedPlanets = calculatedPlanets.OrderByDescending(x => x.Radius).ToList();

                        StringBuilder sb = new StringBuilder();
                        if (calculatedPlanets.Count() > 2)
                        {
                            sb.Append($"From top to bottom the order is {string.Join(",", calculatedPlanets)}.");
                        }

                        var first = calculatedPlanets.FirstOrDefault();
                        foreach (var item in calculatedPlanets)
                        {
                            if(calculatedPlanets.IndexOf(item)==0)
                            {
                                continue;
                            }
                            var res = first.Radius / item.Radius;
                            sb.Append($"{first.Name} is bigger than {res} times when compared to {item.Name}.");
                        }

                        obj.Add("result", sb.ToString());
                        return obj;
                    }
                }
            }

            obj.Add("result", $"Sorry, I am unable to find it.");
            return obj;
        }

        [HttpGet]
        public object CalculateWeight(string weight, string gravityplanets)
        {
            JObject obj = new JObject();

            if (!string.IsNullOrEmpty(gravityplanets) && !string.IsNullOrEmpty(weight))
            {
                string[] planetData = gravityplanets.Split(',');
                string[] scale = weight.Split(' ');
                if (planetData?.Any() ?? false)
                {
                    List<PlanetData> calculatedPlanets = new List<PlanetData>();
                    foreach (var item in planetData)
                    {
                        var p = PlanetProfileData.Data.FirstOrDefault(x => x.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase));
                        if (p != null)
                        {
                            calculatedPlanets.Add(p);
                        }
                    }
                    if (calculatedPlanets?.Any() ?? false)
                    {
                        
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in calculatedPlanets)
                        {
                            var res = (Convert.ToDouble(scale[0]) / 9.81) * item.Gravity;
                            sb.Append($"Your weight on {item.Name} is {Math.Round(res,2)} {scale[1]}.");
                        }
                        
                        obj.Add("result", sb.ToString());
                        return obj;
                    }
                }
            }

            obj.Add("result", $"Sorry, I am unable to find it.");
            return obj;
        }

        [HttpGet]
        public object LocateIss()
        {
            JObject obj = new JObject();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var sateliteResponse = client.GetAsync("https://api.wheretheiss.at/v1/satellites");
                    var sateliteDataString = sateliteResponse.Result.Content.ReadAsStringAsync();
                    dynamic sateliteData = JsonConvert.DeserializeObject(sateliteDataString.Result);
                    if (sateliteData != null)
                    {
                        var issPositionResponse = client.GetAsync($"https://api.wheretheiss.at/v1/satellites/{sateliteData[0].id}");
                        var issDataString = issPositionResponse.Result.Content.ReadAsStringAsync();
                        dynamic issData = JsonConvert.DeserializeObject(issDataString.Result);
                        if (issData != null)
                        {
                            var mapPositionResponse = client.GetAsync($"https://api.wheretheiss.at/v1/coordinates/{issData.latitude},{issData.longitude}");
                            var mapDataString = mapPositionResponse.Result.Content.ReadAsStringAsync();
                            dynamic mapData = JsonConvert.DeserializeObject(mapDataString.Result);
                            if (mapData != null && mapData.country_code != null)
                            {
                                obj.Add("result", $"Currently ISS located on country {mapData.country_code} and timezone is {mapData.timezone_id}.");
                            }
                            else
                            {
                                obj.Add("result", $"I can't see it from here, so it must be flying over an ocean.");
                            }
                            return obj;
                        }
                    }
                }

                obj.Add("result", $"Sorry, I can't see it from here.");
                return obj;
            }
            catch (Exception)
            {
                obj.Add("result", $"Sorry, I can't see it from here.");
                return obj;
            }
        }
    }
}
