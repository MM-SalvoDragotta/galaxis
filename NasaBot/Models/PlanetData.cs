using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NasaBot.Models
{
    public class PlanetData
    {
        public string Name { get; set; }

        public double Distance { get; set; }

        public double Radius { get; set; }

        public int Order { get; set; }

        public double Gravity { get; set; }
    }

    public static class PlanetProfileData
    {
        public static List<PlanetData> Data
        {
            get
            {
                List<PlanetData> data = new List<PlanetData>();

                data.Add(new PlanetData { Name = "sun", Distance = 0, Order = 0, Radius = 695700, Gravity = 274 });
                data.Add(new PlanetData { Name = "mercury", Distance = 0.39, Order = 1, Radius = 2440, Gravity = 3.7 });
                data.Add(new PlanetData { Name = "venus", Distance = 0.72, Order = 2, Radius = 6052, Gravity = 8.87 });
                data.Add(new PlanetData { Name = "earth", Distance = 1.00, Order = 3, Radius = 6371, Gravity = 9.798 });
                data.Add(new PlanetData { Name = "mars", Distance = 1.52, Order = 4, Radius = 3390, Gravity = 3.71 });
                data.Add(new PlanetData { Name = "jupiter", Distance = 5.20, Order = 5, Radius = 69911, Gravity = 24.92 });
                data.Add(new PlanetData { Name = "saturn", Distance = 9.58, Order = 6, Radius = 58232, Gravity = 10.44 });
                data.Add(new PlanetData { Name = "uranus", Distance = 19.20, Order = 7, Radius = 25362, Gravity = 8.87 });
                data.Add(new PlanetData { Name = "neptune", Distance = 30.05, Order = 8, Radius = 24622, Gravity = 11.15 });
                data.Add(new PlanetData { Name = "pluto", Distance = 39.48, Order = 9, Radius = 1187, Gravity = 0.58 });
                data.Add(new PlanetData { Name = "moon", Distance = 1.002562, Order = 10, Radius = 1737, Gravity = 1.62 });

                return data;
            }
        }
    }

}