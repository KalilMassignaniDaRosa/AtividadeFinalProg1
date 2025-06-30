using Microsoft.AspNetCore.Mvc;
using RealState.ViewModel;
using RealStateModel;
using Repository;
using RealState.Utils;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace RealState.Controllers
{
    public class PropertyController : Controller
    {
        #region Attributes
        private readonly IWebHostEnvironment _environment;
        private readonly PropertyRepository _propertyRepository;
        private readonly CategoryRepository _categoryRepository;
        #endregion

        #region Constructor
        public PropertyController(IWebHostEnvironment environment)
        {
            _environment = environment;
            _propertyRepository = new (_environment.WebRootPath);
            _categoryRepository = new (_environment.WebRootPath);
        }
        #endregion

        #region Index
        [HttpGet]
        public IActionResult Index(string? sortOrder, string? searchString, int pageNumber = 1, int pageSize = 5)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.StreetSortParam = sortOrder == "street" ? "street_desc" : "street";
            ViewBag.CitySortParam = sortOrder == "city" ? "city_desc" : "city";
            ViewBag.StateSortParam = sortOrder == "state" ? "state_desc" : "state";
            ViewBag.CountrySortParam = sortOrder == "country" ? "country_desc" : "country";

            IQueryable<Property> properties = _propertyRepository.RetrieveAll().AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string lower = searchString.ToLower();
                properties = properties.Where(p =>
                    p.Name.ToLower().Contains(lower) ||
                    p.Location.Street.ToLower().Contains(lower) ||
                    p.Location.City.ToLower().Contains(lower) ||
                    p.Location.State.ToLower().Contains(lower) ||
                    p.Location.Country.ToLower().Contains(lower)
                );
            }

            properties = sortOrder switch
            {
                "id_desc" => properties.OrderByDescending(p => p.Id),
                "name" => properties.OrderBy(p => p.Name),
                "name_desc" => properties.OrderByDescending(p => p.Name),
                "street" => properties.OrderBy(p => p.Location.Street),
                "street_desc" => properties.OrderByDescending(p => p.Location.Street),
                "city" => properties.OrderBy(p => p.Location.City),
                "city_desc" => properties.OrderByDescending(p => p.Location.City),
                "state" => properties.OrderBy(p => p.Location.State),
                "state_desc" => properties.OrderByDescending(p => p.Location.State),
                "country" => properties.OrderBy(p => p.Location.Country),
                "country_desc" => properties.OrderByDescending(p => p.Location.Country),
                _ => properties.OrderBy(p => p.Id),
            };


            int totalItems = properties.Count();
            // Math.Ceiling arredonda para cima
            // Usando cast de int e double
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            IEnumerable<Property> pagedProperties = PaginationHelper.Paginate(properties, pageNumber, pageSize);

            PropertyListViewModel viewModel = new ()
            {
                Properties = pagedProperties,
                SearchString = searchString,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            IEnumerable<Category> categories = _categoryRepository.RetrieveAll();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Property property)
        {
            if (!string.IsNullOrEmpty(property.Currency))
            {
                property.Currency = property.Currency.ToUpperInvariant();
            }

            if (property.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than 0");

            if (property.YearBuilt < 1800 || property.YearBuilt > DateTime.Now.Year)
                ModelState.AddModelError("YearBuilt", "Invalid year");

            // ModelState.IsValid verifica se os dados são válidos
            // É nativo do Framework
            if (ModelState.IsValid)
            {
                _propertyRepository.Save(property);
                return RedirectToAction("Index");
            }

            IEnumerable<Category> categories = _categoryRepository.RetrieveAll();
            ViewBag.Categories = categories;
            return View(property);
        }
        #endregion

        #region Update
        [HttpGet]
        public IActionResult Update(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            Property? property = _propertyRepository.Retrieve(id.Value);
            if (property == null)
                return NotFound();

            IEnumerable<Category> categories = _categoryRepository.RetrieveAll();
            ViewBag.Categories = categories;
            return View(property);
        }

        [HttpPost]
        public IActionResult Update(int id, Property updated)
        {
            if (ModelState.IsValid)
            {
                updated.SetId(id);
                _propertyRepository.Update(updated);
                return RedirectToAction("Index");
            }

            IEnumerable<Category> categories = _categoryRepository.RetrieveAll();
            ViewBag.Categories = categories;
            return View(updated);
        }
        #endregion

        #region Delete
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            Property? property = _propertyRepository.Retrieve(id.Value);
            if (property == null)
                return NotFound();

            // View Delete com variavel
            return View(property);
        }

        [HttpPost]
        public IActionResult ConfirmDelete(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            // Se não conseguiu deletar
            bool deleted = _propertyRepository.DeleteById(id.Value);
            if (!deleted)
                return NotFound();

            return RedirectToAction("Index");
        }
        #endregion

        #region Details
        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            Property? property = _propertyRepository.Retrieve(id.Value);
            if (property == null)
                return NotFound();

            Category? category = _categoryRepository.Retrieve(property.CategoryId);
            property.Category = category;

            return View(property);
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportProperty()
        {
            string filePath = Path.Combine(_environment.WebRootPath, "data", "properties.txt");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            string content = System.IO.File.ReadAllText(filePath);
            SaveFile(content, "Property", "Properties");

            ViewBag.File = new
            {
                Location = "Property",
                Name = "Properties.txt"
            };

            return View("Export");
        }

        private bool SaveFile(string content, string fileLocation, string fileName)
        {
            string path = Path.Combine(_environment.WebRootPath, "TextFiles", fileLocation);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, fileName + ".txt");
            System.IO.File.WriteAllText(fullPath, content);

            return true;
        }
        #endregion

        #region Import
        [HttpGet]
        public IActionResult ImportRoute()
        {
            return ShowImportForm();
        }

        [HttpGet]
        public IActionResult ShowImportForm()
        {
            return View("Import");
        }

        [HttpPost]
        public IActionResult ImportProperty(IFormFile file, string importOption)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid TXT file.";
                return View("Import");
            }

            int importedCount = 0;
            List<string> errors = new List<string>();

            if (importOption == "overwrite")
            {
                RealStateData.Properties.Clear();
            }

            using (StreamReader reader = new StreamReader(file.OpenReadStream()))
            {
                int lineNumber = 0;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    string? line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split(';');
                    if (parts.Length < 16)
                    {
                        errors.Add($"Line {lineNumber}: Invalid format, expected 16 fields.");
                        continue;
                    }

                    if (!int.TryParse(parts[0], out int id))
                    {
                        errors.Add($"Line {lineNumber}: Invalid property ID.");
                        continue;
                    }

                    if (!int.TryParse(parts[15], out int categoryId) || categoryId <= 0)
                    {
                        errors.Add($"Line {lineNumber}: Invalid category ID, property skipped.");
                        continue;
                    }

                    Category? existingCategory = _categoryRepository.Retrieve(categoryId);
                    if (existingCategory == null)
                    {
                        errors.Add($"Line {lineNumber}: Category ID {categoryId} not found, property skipped.");
                        continue;
                    }

                    Property prop = new Property
                    {
                        Name = parts[1],
                        Description = string.IsNullOrWhiteSpace(parts[2]) ? "" : parts[2],
                        Location = new Address
                        {
                            Street = parts[3],
                            City = parts[4],
                            State = parts[5],
                            Country = parts[6]
                        },
                        NumBedrooms = int.TryParse(parts[7], out int bedrooms) ? bedrooms : 0,
                        NumBathrooms = int.TryParse(parts[8], out int bathrooms) ? bathrooms : 0,
                        NumParkingSpaces = int.TryParse(parts[9], out int parking) ? parking : 0,
                        AreaSize = double.TryParse(parts[10], out double area) ? area : 0.0,
                        YearBuilt = int.TryParse(parts[11], out int year) ? year : 1800,
                        Price = decimal.TryParse(parts[12], out decimal price) ? price : 0.0m,
                        Currency = string.IsNullOrWhiteSpace(parts[13]) ? "USD" : parts[13],
                        Status = Enum.TryParse<PropertyStatus>(parts[14], out PropertyStatus status) ? status : PropertyStatus.ForSale,
                        CategoryId = categoryId
                    };
                    prop.SetId(id);

                    if (prop.Validate())
                    {
                        if (importOption == "overwrite")
                        {
                            _propertyRepository.Save(prop);
                            importedCount++;
                        }
                        else if (importOption == "add")
                        {
                            if (_propertyRepository.Retrieve(id) == null)
                            {
                                _propertyRepository.Save(prop);
                                importedCount++;
                            }
                        }
                    }
                    else
                    {
                        errors.Add($"Line {lineNumber}: Property validation failed.");
                    }
                }
            }

            if (errors.Any())
                TempData["Error"] = string.Join("<br>", errors);

            return RedirectToAction("Index");
        }

        #endregion
    }
}
