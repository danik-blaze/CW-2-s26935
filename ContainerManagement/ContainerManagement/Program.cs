namespace ContainerManagement
{
    interface IHazardNotifier
    {
        void NotifyDanger(string message);
    }

    abstract class Container
    {
        private static int counter = 1;
        public string Id { get; }
        public double LoadMass { get; protected set; }
        public double Weight { get; }
        public double Height { get; }
        public double Depth { get; }

        protected Container(string type, double weight, double height, double depth)
        {
            Id = $"KON-{type}-{counter++}";
            Weight = weight;
            Height = height;
            Depth = depth;
            LoadMass = 0;
        }

        public void Load(double mass)
        {
            if (LoadMass + mass > Weight)
                throw new OverfillException($"Cannot load {Id}, exceeds max weight!");
            LoadMass += mass;
        }

        public virtual void Unload() => LoadMass = 0;

        public override string ToString() => $"{Id}: Load {LoadMass}/{Weight} kg";
    }

    class LiquidContainer : Container, IHazardNotifier
    {
        public bool IsHazardous { get; }

        public LiquidContainer(double weight, bool isHazardous) : base("L", weight, 250, 300)
        {
            IsHazardous = isHazardous;
        }

        public new void Load(double mass)
        {
            double limit = IsHazardous ? Weight * 0.5 : Weight * 0.9;
            if (LoadMass + mass > limit)
                NotifyDanger($"Dangerous operation on {Id}! Load limit exceeded.");
            else
                base.Load(mass);
        }

        public void NotifyDanger(string message) => Console.WriteLine("ALERT: " + message);
    }

    class GasContainer : Container, IHazardNotifier
    {
        public double Pressure { get; }

        public GasContainer(double weight, double pressure) : base("G", weight, 220, 280)
        {
            Pressure = pressure;
        }

        public override void Unload()
        {
            LoadMass *= 0.05;
        }

        public void NotifyDanger(string message)
        {
            Console.WriteLine($"ALERT: {message} - Container Serial Number: {Id}");
        }

        public new void Load(double mass)
        {
            if (LoadMass + mass > Weight)
                NotifyDanger($"Dangerous load operation on {Id}! Load exceeds the maximum allowed.");
            else
                base.Load(mass);
        }
    }

    class RefrigeratedContainer : Container, IHazardNotifier
    {
        public string ProductType { get; }
        public double Temperature { get; private set; }

        private static readonly Dictionary<string, double> ProductTemperatureRequirements = new Dictionary<string, double>
        {
            { "Bananas", 10 },
            { "Sausages", 4 },
            { "FrozenFood", -18 },
        };

        public RefrigeratedContainer(string productType, double temperature, double weight)
            : base("C", weight, 270, 320)
        {
            ProductType = productType;

            if (ProductTemperatureRequirements.ContainsKey(productType))
            {
                double requiredTemperature = ProductTemperatureRequirements[productType];

                if (temperature < requiredTemperature)
                {
                    NotifyDanger($"Temperature for {productType} container cannot be lower than {requiredTemperature}°C. Given: {temperature}°C.");
                }

                Temperature = temperature;
            }
            else
            {
                NotifyDanger($"No temperature requirement defined for product type {productType}.");
            }
        }

        public void NotifyDanger(string message) => Console.WriteLine($"ALERT: {message} - Container Serial Number: {Id}");

        public new void Load(double mass)
        {
            if (LoadMass + mass > Weight)
                NotifyDanger($"Dangerous load operation on {Id}! Load exceeds the maximum allowed.");
            else
                base.Load(mass);
        }
    }

    class Ship
    {
        private int maxContainers;
        private double maxWeight;
        private List<Container> containers;

        public Ship(int maxContainers, double maxWeight)
        {
            this.maxContainers = maxContainers;
            this.maxWeight = maxWeight * 1000;
            this.containers = new List<Container>();
        }

        public void LoadContainer(Container container)
        {
            if (containers.Count >= maxContainers || GetTotalWeight() + container.LoadMass > maxWeight)
                Console.WriteLine($"Cannot load {container.Id}, exceeds ship limits.");
            else
                containers.Add(container);
        }

        public void UnloadContainer(string id)
        {
            Container container = containers.Find(c => c.Id == id);
            if (container != null)
            {
                containers.Remove(container);
                Console.WriteLine($"Container {id} removed from ship.");
            }
        }

        public void TransferContainer(string id, Ship targetShip)
        {
            Container container = containers.Find(c => c.Id == id);
            if (container != null)
            {
                containers.Remove(container);
                targetShip.LoadContainer(container);
            }
        }

        public double GetTotalWeight()
        {
            double totalWeight = 0;
            foreach (var container in containers)
                totalWeight += container.LoadMass;
            return totalWeight;
        }

        public void DisplayShipInfo()
        {
            Console.WriteLine($"Ship: {containers.Count}/{maxContainers} containers, {GetTotalWeight()}/{maxWeight} kg loaded.");
            foreach (var container in containers)
                Console.WriteLine(container);
        }
    }

    class OverfillException : Exception
    {
        public OverfillException(string message) : base(message) { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Ship ship = new Ship(10, 50);

          
            RefrigeratedContainer bananaContainer = new RefrigeratedContainer("Bananas", 1, 20000);
            RefrigeratedContainer sausagesContainer = new RefrigeratedContainer("Sausages", 5,5000);
            GasContainer heliumContainer = new GasContainer(5000, 50);
            LiquidContainer fuelContainer = new LiquidContainer(14000, true);

            bananaContainer.Load(10000);
            heliumContainer.Load(2500); 
            sausagesContainer.Load(4000);
            fuelContainer.Load(7000);

            ship.LoadContainer(bananaContainer);
            ship.LoadContainer(heliumContainer);
            ship.LoadContainer(sausagesContainer);
            ship.LoadContainer(fuelContainer);
            heliumContainer.Unload();

            ship.DisplayShipInfo();
        }
    }
}
