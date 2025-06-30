using RealStateModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Repository
{
    public class CategoryRepository
    {
        #region Attributes
        private readonly string _dataFilePath;
        private readonly PropertyRepository _propertyRepository;
        #endregion

        #region Constructor
        public CategoryRepository(string webRootPath)
        {
            _dataFilePath = Path.Combine(webRootPath, "data", "categories.txt");
            _propertyRepository = new PropertyRepository(webRootPath);
            LoadFromFile();
        }
        #endregion

        #region Retrieve

        public Category? Retrieve(int id)
        {
            Category? category = RealStateData.Categories.Find(c => c.Id == id);
            if (category != null)
            {
                List<Property> propertyList = _propertyRepository.RetrieveAll()
                    .Where(p => p.CategoryId == id)
                    .ToList();
                category.Properties = propertyList;
            }
            return category;
        }

        public List<Category> RetrieveAll()
        {
            List<Category> categoryList = RealStateData.Categories;
            foreach (Category c in categoryList)
            {
                List<Property> propertyList = _propertyRepository.RetrieveAll()
                    .Where(p => p.CategoryId == c.Id)
                    .ToList();
                c.Properties = propertyList;
            }
            return categoryList;
        }

        public List<Category> RetrieveByName(string name)
        {
            List<Category> filteredList = new List<Category>();
            foreach (Category c in RetrieveAll())
            {
                if (c.Name != null && c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    filteredList.Add(c);
                }
            }
            return filteredList;
        }

        #endregion

        #region Save and Update

        public void Save(Category category)
        {
            if (category == null || !category.Validate())
                return;

            if (category.Id == 0)
            {
                int newId = RealStateData.Categories.Any()
                    ? RealStateData.Categories.Max(c => c.Id) + 1
                    : 1;
                category.SetId(newId);
                RealStateData.Categories.Add(category);
            }
            else
            {
                Category? existingCategory = RealStateData.Categories.Find(c => c.Id == category.Id);
                if (existingCategory == null)
                {
                    bool exists = RealStateData.Categories.Any(c => c.Id == category.Id);
                    if (exists)
                        throw new InvalidOperationException($"Category with Id {category.Id} already exists.");
                    RealStateData.Categories.Add(category);
                }
                else
                {
                    existingCategory.Name = category.Name;
                    existingCategory.Properties = category.Properties;
                }
            }
            SaveToFile();
        }

        public void Update(Category updatedCategory)
        {
            if (updatedCategory == null || !updatedCategory.Validate())
                return;

            Category? existingCategory = RealStateData.Categories.Find(c => c.Id == updatedCategory.Id);
            if (existingCategory == null)
                return;

            existingCategory.Name = updatedCategory.Name;
            existingCategory.Properties = updatedCategory.Properties;

            SaveToFile();
        }

        #endregion

        #region Delete

        public bool Delete(Category category)
        {
            if (category == null) return false;

            // Deleta todas as propriedades associadas
            var properties = _propertyRepository.RetrieveAll()
                .Where(p => p.CategoryId == category.Id)
                .ToList();

            foreach (var prop in properties)
            {
                _propertyRepository.DeleteById(prop.Id);
            }

            bool removed = RealStateData.Categories.Remove(category);
            if (removed) SaveToFile();

            return removed;
        }

        public bool DeleteById(int id)
        {
            Category? categoryToDelete = Retrieve(id);
            if (categoryToDelete == null)
                return false;
            return Delete(categoryToDelete);
        }

        #endregion

        #region Utility

        public int GetCount()
        {
            return RealStateData.Categories.Count;
        }
        public string GetFilePath()
        {
            return _dataFilePath;
        }
        #endregion

        #region File Persistence

        private void SaveToFile()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Category c in RealStateData.Categories)
            {
                sb.AppendLine(c.Id + ";" + c.Name);
            }

            string directory = Path.GetDirectoryName(_dataFilePath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_dataFilePath, sb.ToString());
        }

        private void LoadFromFile()
        {
            if (!File.Exists(_dataFilePath))
                return;

            string[] lines = File.ReadAllLines(_dataFilePath);
            RealStateData.Categories.Clear();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(';');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int id))
                {
                    Category category = new Category { Name = parts[1] };
                    category.SetId(id);
                    RealStateData.Categories.Add(category);
                }
            }
        }

        #endregion
    }
}
    

