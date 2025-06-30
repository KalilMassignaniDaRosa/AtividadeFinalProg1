using Microsoft.AspNetCore.Mvc;
using RealState.ViewModel;
using RealStateModel;
using Repository;
using RealState.Utils;
using System.Text;

namespace RealState.Controllers
{
    public class CategoryController : Controller
    {
        #region Attributes
        private readonly IWebHostEnvironment _environment;
        private readonly CategoryRepository? _categoryRepository;
        private readonly PropertyRepository _propertyRepository;
        StreamReader? reader = null;
        #endregion

        #region Constructor
        public CategoryController(IWebHostEnvironment environment)
        {
            _environment = environment;
            _categoryRepository = new CategoryRepository(_environment.WebRootPath);
            _propertyRepository = new PropertyRepository(_environment.WebRootPath);
        }
        #endregion

        #region Index
        [HttpGet]
        public IActionResult Index(string? sortOrder, string? searchString, int pageNumber = 1, int pageSize = 5)
        {
            // View bag para passar informações para a view
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";

            // Usando var
            // AsQueryable transforma em um objeto
            var categories = _categoryRepository!.RetrieveAll().AsQueryable();

            // Procura
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name!.ToLower().Contains(searchString.ToLower()));
            }

            // Ordem
            categories = sortOrder switch
            {
                "id_desc" => categories.OrderByDescending(c => c.Id),
                "name" => categories.OrderBy(c => c.Name),
                "name_desc" => categories.OrderByDescending(c => c.Name),
                _ => categories.OrderBy(c => c.Id),
            };

            int totalItems = categories.Count();
            // Math.Ceiling arredonda para cima
            // Usando cast de int e double
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Iterface para tipo genérico
            IEnumerable<Category> pagedCategories = PaginationHelper.Paginate(categories, pageNumber, pageSize);

            CategoryListViewModel  viewModel = new()
            {
                Categories = pagedCategories,
                SearchString = searchString,
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
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            // ModelState.IsValid verifica se os dados são válidos
            // É nativo do Framework
            if (ModelState.IsValid)
            {
                _categoryRepository!.Save(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }
        #endregion

        #region Update
        [HttpGet]
        public IActionResult Update(int? id)
        {
            if (id == null || id.Value <= 0)
                return NotFound();

            Category category = _categoryRepository!.Retrieve(id.Value)!;
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Update(int id, Category updated)
        {
            if (ModelState.IsValid)
            {
                updated.SetId(id);
                _categoryRepository!.Update(updated);
                return RedirectToAction("Index");
            }
            return View(updated);
        }
        #endregion

        #region Delete
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id.Value <= 0)
                return NotFound();

            Category category = _categoryRepository!.Retrieve(id.Value)!;
            if (category == null)
                return NotFound();

            // View Delete com variavel
            return View("Delete", category);
        }

        [HttpPost]
        public IActionResult ConfirmDelete(int? id)
        {
            if (id == null || id.Value <= 0)
                return NotFound();

            if (!_categoryRepository!.DeleteById(id.Value))
                return NotFound();

            return RedirectToAction("Index");
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportCategory()
        {
            string content = System.IO.File.ReadAllText(_categoryRepository!.GetFilePath());
            SaveFile(content, "Category", "Categories");

            ViewBag.File = new { Location = "Category", Name = "Categories.txt" };
            return View("Export");
        }

        private bool SaveFile(string content, string fileLocation, string fileName)
        {
            string path = Path.Combine(_environment.WebRootPath, "textFiles", fileLocation);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, fileName + ".txt");
            System.IO.File.WriteAllText(fullPath, content);

            return true;
        }
        #endregion

        #region Import
        [HttpPost]
        public IActionResult ImportCategory(IFormFile file, string importOption)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a valid TXT file.");
                return View("Import");
            }

            //  Limpa as categorias 
            if (importOption == "overwrite")
            {
                RealStateData.Categories.Clear();
            }

            // Importa categorias do arquivo
            // Abre fluxo para leitura de aqruivo
            reader = new(file.OpenReadStream());
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) 
                    continue;

                string[] parts = line.Split(';');
                // Verifica se possui pelo menos 2 elementos
                // E convete ára omt
                if (parts.Length >= 2 && int.TryParse(parts[0], out int id))
                {
                    Category? existingCategory = _categoryRepository!.Retrieve(id);

                    if (importOption == "overwrite")
                    {
                        // Recria ou atualiza no repositório
                        Category newCategory = new() { Name = parts[1] };
                        newCategory.SetId(id);
                        _categoryRepository.Save(newCategory);
                    }
                    else if (importOption == "add")
                    {
                        if (existingCategory == null)
                        {
                            Category newCategory = new() { Name = parts[1] };
                            newCategory.SetId(id);
                            _categoryRepository.Save(newCategory);
                        }
                    }
                }
            }

            // Remove todas as Properties órfãs
            if (importOption == "overwrite")
            {
                // ToHashSet não permite elementos repetidos
                HashSet<int> validCategoryIds = RealStateData.Categories
                    .Select(c => c.Id)
                    .ToHashSet();

                RealStateData.Properties.RemoveAll(p => !validCategoryIds.Contains(p.CategoryId));

                _propertyRepository.SaveToFile();
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult ShowImportForm()
        {
            return View("Import");
        }
        #endregion
    }
}
