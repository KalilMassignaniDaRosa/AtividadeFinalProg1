using RealStateModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Repository
{
    public class PropertyRepository
    {
        #region Attributes
        private readonly string _dataFilePath;
        #endregion

        #region Constructor
        public PropertyRepository(string webRootPath)
        {
            _dataFilePath = Path.Combine(webRootPath, "data", "properties.txt");
            LoadFromFile();
        }
        #endregion

        #region Retrieve

        public Property? Retrieve(int id)
        {
            return RealStateData.Properties.Find(p => p.Id == id);
        }

        public List<Property> RetrieveAll()
        {
            LoadFromFile();
            return RealStateData.Properties;
        }

        #endregion

        #region Save and Update

        public void Save(Property property)
        {
            if (property == null || !property.Validate())
                return;

            if (property.Id == 0)
            {
                int newId = RealStateData.Properties.Any()
                    ? RealStateData.Properties.Max(p => p.Id) + 1
                    : 1;

                property.SetId(newId);
                RealStateData.Properties.Add(property);
            }
            else
            {
                Property? existingProperty = Retrieve(property.Id);
                if (existingProperty == null)
                {
                    RealStateData.Properties.Add(property);
                }
                else
                {
                    existingProperty.Name = property.Name;
                    existingProperty.Description = property.Description;
                    existingProperty.Location = property.Location;
                    existingProperty.NumBedrooms = property.NumBedrooms;
                    existingProperty.NumBathrooms = property.NumBathrooms;
                    existingProperty.NumParkingSpaces = property.NumParkingSpaces;
                    existingProperty.AreaSize = property.AreaSize;
                    existingProperty.YearBuilt = property.YearBuilt;
                    existingProperty.Price = property.Price;
                    existingProperty.Currency = property.Currency;
                    existingProperty.Status = property.Status;
                    existingProperty.CategoryId = property.CategoryId;
                }
            }

            SaveToFile();
        }

        public void Update(Property updatedProperty)
        {
            if (updatedProperty == null || !updatedProperty.Validate())
                return;

            Property? existingProperty = Retrieve(updatedProperty.Id);
            if (existingProperty == null)
                return;

            existingProperty.Name = updatedProperty.Name;
            existingProperty.Description = updatedProperty.Description;
            existingProperty.Location = updatedProperty.Location;
            existingProperty.NumBedrooms = updatedProperty.NumBedrooms;
            existingProperty.NumBathrooms = updatedProperty.NumBathrooms;
            existingProperty.NumParkingSpaces = updatedProperty.NumParkingSpaces;
            existingProperty.AreaSize = updatedProperty.AreaSize;
            existingProperty.YearBuilt = updatedProperty.YearBuilt;
            existingProperty.Price = updatedProperty.Price;
            existingProperty.Currency = updatedProperty.Currency;
            existingProperty.Status = updatedProperty.Status;
            existingProperty.CategoryId = updatedProperty.CategoryId;

            SaveToFile();
        }

        #endregion

        #region Delete

        public bool DeleteById(int id)
        {
            Property? propertyToDelete = Retrieve(id);
            if (propertyToDelete == null)
                return false;

            bool removed = RealStateData.Properties.Remove(propertyToDelete);
            if (removed)
                SaveToFile();

            return removed;
        }

        #endregion

        #region File Persistence

        public void SaveToFile()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Property p in RealStateData.Properties)
            {
                sb.AppendLine(string.Join(";", new object[]
                {
                    p.Id, p.Name, p.Description,
                    p.Location.Street, p.Location.City, p.Location.State, p.Location.Country,
                    p.NumBedrooms, p.NumBathrooms, p.NumParkingSpaces,
                    p.AreaSize, p.YearBuilt, p.Price, p.Currency, p.Status, p.CategoryId
                }));
            }

            string directory = Path.GetDirectoryName(_dataFilePath)!;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_dataFilePath, sb.ToString());
        }

        private void LoadFromFile()
        {
            if (!File.Exists(_dataFilePath))
                return;

            string[] lines = File.ReadAllLines(_dataFilePath);
            RealStateData.Properties.Clear();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(';');
                if (parts.Length < 16)
                    continue;

                Property property = new Property
                {
                    Name = parts[1],
                    Description = parts[2],
                    Location = new Address
                    {
                        Street = parts[3],
                        City = parts[4],
                        State = parts[5],
                        Country = parts[6]
                    },
                    NumBedrooms = int.Parse(parts[7]),
                    NumBathrooms = int.Parse(parts[8]),
                    NumParkingSpaces = int.Parse(parts[9]),
                    AreaSize = double.Parse(parts[10]),
                    YearBuilt = int.Parse(parts[11]),
                    Price = decimal.Parse(parts[12]),
                    Currency = parts[13],
                    Status = Enum.Parse<PropertyStatus>(parts[14]),
                    CategoryId = int.Parse(parts[15])
                };
                property.SetId(int.Parse(parts[0]));
                RealStateData.Properties.Add(property);
            }
        }

        #endregion
    }
}
