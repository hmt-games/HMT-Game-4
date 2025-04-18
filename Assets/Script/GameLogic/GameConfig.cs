namespace GameConfig
{
    public struct ActionTickTimeCost
    {
        public const int Harvest = 1;
        public const int PluckPerStage = 1;
        public const int Till = 1;
        public const int SprayUp = 1;
        public const int Spray = 1;
        public const int Sample = 1;
        public const int PlantPerStage = 1;
    }

    public struct SprayConfig
    {
        public const float WaterAmount = 50.0f;
        public const float NutrientConcentration = 1.0f;
        public const float SprayAmountPerAction = 10.0f;
    }

    //TODO: probably want this to be a parameter of plant
    public struct HarvestConfig
    {
        public const int harvestAmountMin = 3;
        public const int harvestAmountMax = 5;
    }
}