using Rocket.API;

namespace VehicleSQL
{
    public class VehicleSQLConfiguration : IRocketPluginConfiguration
    {
        public string TableName;

        public void LoadDefaults()
        {
            TableName = "Vehicles";
        }
    }
}