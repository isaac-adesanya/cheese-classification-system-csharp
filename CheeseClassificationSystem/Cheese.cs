namespace CheeseClassificationSystem
{

    public class Cheese 
    {
        public int Id      { get; set; }
        public int BatchNumber { get; set; }
        public string Type { get; set; }
        public DateTime CreationDate { get; set; }
        public int NumberOfCheeses { get; set; }
        public string Description { get; set; }

        public Cheese(int id, string type, string description, int batchNumber, DateTime creationDate, int numberOfCheeses) // Constructor with Id
        {
            Id = id;
            Type = type;
            Description = description;
            BatchNumber = batchNumber;
            CreationDate = creationDate;
            NumberOfCheeses = numberOfCheeses;
        }

        public Cheese(string type, string description, int batchNumber, DateTime creationDate, int numberOfCheeses) // Constructor without Id
        {
            Type = type;
            Description = description;
            BatchNumber = batchNumber;
            CreationDate = creationDate;
            NumberOfCheeses = numberOfCheeses;
        }
        public string Classification // Computed property for cheese classification
        {
            get
            {
                int months = (int)((DateTime.Now - CreationDate).TotalDays / 30); // Approximate age in months

                switch (Type) 
                {
                    case "Gouda": // Classification rules for Gouda
                        if (months < 3) return "Too young";
                        if (months >= 3 && months < 4) return "Mild";
                        if (months >= 4 && months < 6) return "Medium";
                        if (months >= 6 && months < 12) return "Aged";
                        if (months >= 12 && months < 18) return "Grizzly";
                        if (months >= 18 && months < 24) return "Old Grizzly";
                        return "Extra Old Grizzly";

                    case "Havarti": // Classification rules for Havarti
                        if (months < 3) return "Too Young";
                        if (months >= 3 && months < 4) return "Mild";
                        return "Medium";

                    case "Klondyk Gruyere": // Classification rules for Klondyk Gruyere
                        if (months < 3) return "Too Young";
                        if (months >= 3 && months < 6) return "Medium";
                        return "Cave Aged";

                    default: 
                        return "Unkown Cheese type";
                }
            }
        }

    }

}
