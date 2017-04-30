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

        public string DisplayName { get; set; }
    }

    public static class PlanetProfileData
    {
        public static List<PlanetData> Data
        {
            get
            {
                List<PlanetData> data = new List<PlanetData>();

                data.Add(new PlanetData { Name = "sun", Distance = 0, Order = 0, Radius = 695700, Gravity = 274, DisplayName = "Sun" });
                data.Add(new PlanetData { Name = "mercury", Distance = 0.39, Order = 1, Radius = 2440, Gravity = 3.7, DisplayName = "Mercury" });
                data.Add(new PlanetData { Name = "venus", Distance = 0.72, Order = 2, Radius = 6052, Gravity = 8.87, DisplayName="Venus" });
                data.Add(new PlanetData { Name = "earth", Distance = 1.00, Order = 3, Radius = 6371, Gravity = 9.798, DisplayName="Earth" });
                data.Add(new PlanetData { Name = "mars", Distance = 1.52, Order = 4, Radius = 3390, Gravity = 3.71, DisplayName = "Mars" });
                data.Add(new PlanetData { Name = "jupiter", Distance = 5.20, Order = 5, Radius = 69911, Gravity = 24.92, DisplayName="Jupiter" });
                data.Add(new PlanetData { Name = "saturn", Distance = 9.58, Order = 6, Radius = 58232, Gravity = 10.44, DisplayName="Saturn" });
                data.Add(new PlanetData { Name = "uranus", Distance = 19.20, Order = 7, Radius = 25362, Gravity = 8.87, DisplayName="Uranus" });
                data.Add(new PlanetData { Name = "neptune", Distance = 30.05, Order = 8, Radius = 24622, Gravity = 11.15, DisplayName="Neptune" });
                data.Add(new PlanetData { Name = "pluto", Distance = 39.48, Order = 9, Radius = 1187, Gravity = 0.58, DisplayName="Pluto" });
                data.Add(new PlanetData { Name = "moon", Distance = 1.002562, Order = 10, Radius = 1737, Gravity = 1.62, DisplayName="Moon" });

                return data;
            }
        }
    }

}