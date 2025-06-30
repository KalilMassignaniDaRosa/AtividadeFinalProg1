using System;

namespace RealStateModel
{
    public enum PropertyStatus
    {
        ForSale,
        ForRent,
        Sold,
        Rented
    }

    public class Address
    {
        #region Attributes
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        #endregion

        #region Functions
        public override string ToString() => $"{Street}, {City} - {State}, {Country}";
        #endregion
    }

    public class Property
    {
        #region Attributes
        public int Id { get; private set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Address Location { get; set; } = new Address();
        public int NumBedrooms { get; set; }
        public int NumBathrooms { get; set; }
        public int NumParkingSpaces { get; set; }
        public double AreaSize { get; set; }
        public int YearBuilt { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public PropertyStatus Status { get; set; } = PropertyStatus.ForSale;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        #endregion

        #region Functions
        public string StatusDisplay => Status switch
        {
            PropertyStatus.ForSale => "For Sale",
            PropertyStatus.ForRent => "For Rent",
            PropertyStatus.Sold => "Sold",
            PropertyStatus.Rented => "Rented",
            _ => Status.ToString()
        };

        public bool Validate() =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Description) &&
            !string.IsNullOrWhiteSpace(Location.Street) &&
            !string.IsNullOrWhiteSpace(Location.City) &&
            !string.IsNullOrWhiteSpace(Location.State) &&
            !string.IsNullOrWhiteSpace(Location.Country) &&
            NumBathrooms >= 0 &&
            NumBedrooms >= 0 &&
            NumParkingSpaces >= 0 &&
            AreaSize > 0 &&
            YearBuilt > 1800 &&
            YearBuilt <= DateTime.Now.Year &&
            Price >= 0 &&
            !string.IsNullOrWhiteSpace(Currency) &&
            CategoryId > 0;

        public void SetId(int id)
        {
            Id = (id > 0) ? id : Id;
        }
        #endregion
    }
}
