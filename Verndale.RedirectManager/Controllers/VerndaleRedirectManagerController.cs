using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Logging;
using EPiServer.PlugIn;
using Verndale.RedirectManager.Models;
using Verndale.RedirectManager.Repositories;
using Verndale.RedirectManager.Util;
using Verndale.RedirectManager.ViewModels;
using Shell = EPiServer.Shell;

namespace Verndale.RedirectManager.Controllers
{
    [GuiPlugIn(Area = PlugInArea.AdminMenu, UrlFromModuleFolder = "RedirectManager", DisplayName = "Redirect Manager")]
    public class VerndaleRedirectManagerController : Controller
    {
        #region Properties

        private readonly IRedirectManagerRepository _redirectManagerRepository;
        private static readonly ILogger Logger = LogManager.GetLogger();

        #endregion

        #region Constructor

        public VerndaleRedirectManagerController(IRedirectManagerRepository redirectManagerRepository)
        {
            this._redirectManagerRepository = redirectManagerRepository;
        }

        #endregion

        public ActionResult Index()
        {
            Logger.Debug($"Begin Index()");

            var model = CreateRedirectModel(null, 1);
            return View(GetViewLocation("Index"), model);
        }

        public ActionResult Search(string term, int page)
        {
            Logger.Debug($"Begin Search({term ?? "NULL"}, {page})");

            var model = CreateRedirectModel(term, page);
            return View(GetViewLocation("Index"), model);
        }

        public ActionResult Remove(string id)
        {
            Logger.Debug($"Begin Remove({id})");

            if (int.TryParse(id, out var value))
            {
                _redirectManagerRepository.Delete(value);
            }
            else
            {
                Logger.Warning($"id is invalid. Value: {id}");
            }

            return RedirectToAction("Index");
        }

        public ActionResult RemoveAll()
        {
            Logger.Debug($"Begin RemoveAll()");

            _redirectManagerRepository.DeleteAll();
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SaveRedirect(RedirectManagerModel model)
        {
            Logger.Debug("Begin SaveRedirect()");

            var item = model;

            if (model.Id > 0)
            {
                item = _redirectManagerRepository.GetById(model.Id);
                UpdateModel(item);
            }

            _redirectManagerRepository.InsertOrUpdate(item);

            return RedirectToAction("Index");
        }

        public ActionResult Add()
        {
            Logger.Debug($"Begin Add()");

            var model = new RedirectManagerModel();
            return View(GetViewLocation("Add"), model);
        }

        public ActionResult Edit(string id)
        {
            Logger.Debug($"Begin Edit({id})");

            var model = new RedirectManagerModel();

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Add");
            }
            
            if (int.TryParse(id, out var value))
            {
                model = _redirectManagerRepository.GetById(value);
            }
            else
            {
                Logger.Warning($"id is invalid. Value: {id}");
            }

            return View(GetViewLocation("Edit"), model);
        }

        public FileContentResult Export()
        {
            Logger.Debug("Begin Export()");

            var redirectManagerRepository = new RedirectManagerRepository();
            var data = redirectManagerRepository.GetAll().ToList();

            Logger.Debug($"Total rows: {data.Count}");

            var csvData = CsvUtil.ToCsv(",", data);

            return File(new System.Text.UTF8Encoding().GetBytes(csvData), "text/csv", "Redirects.csv");
        }

        [HttpPost]
        public ActionResult Import(ImportViewModel model)
        {
            Logger.Debug("Begin Import()");

            var redirectManagerRepository = new RedirectManagerRepository();

            var resultModel = new ImportResultViewModel();

            if (model.File.ContentType != "application/vnd.ms-excel")
            {
                Logger.Warning($"File format {model.File.ContentType} is not supported");
                ModelState.AddModelError(string.Empty, "Only CSV files are accepted.");
            }
            else
            {
                var line = 1;

                try
                {
                    using (var reader = new StreamReader(model.File.InputStream))
                    {
                        string currentLine;
                        var watch = Stopwatch.StartNew();

                        var data = redirectManagerRepository.GetAll().ToList();

                        while ((currentLine = reader.ReadLine()) != null)
                        {
                            var values = currentLine.Split('\t');
                            if (values.Length == 1)
                            {
                                values = currentLine.Split(',');
                            }

                            var entry = data.Where(p => p.OldUrl == values[0]).FirstOrDefault();
                            if (entry == null)
                            {
                                entry = new RedirectManagerModel
                                {
                                    OldUrl = values[0],
                                    NewUrl = values[1],
                                    Type = Convert.ToInt32(values[2])
                                };

                                data.Add(entry);
                            }
                            else
                            {
                                entry.OldUrl = values[0];
                                entry.NewUrl = values[1];
                                entry.Type = Convert.ToInt32(values[2]);
                            }

                            var message = Validate(entry);

                            if (message != string.Empty)
                            {
                                Logger.Warning(message);
                                resultModel.AddError(line, message);
                            }
                            else
                            {
                                redirectManagerRepository.RemoveAndInsert(entry);
                                resultModel.TotalImported++;
                            }

                            line++;
                        }

                        watch.Stop();
                        Logger.Debug("Total time: " +
                                     $"{(int) watch.Elapsed.TotalMinutes}:{watch.Elapsed.Seconds:00}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Line {line}: {ex.Message}", ex);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return !ModelState.IsValid
                ? View(GetViewLocation("Index"), CreateRedirectModel(string.Empty, 1))
                : View(GetViewLocation("ImportAndValidate"), resultModel);
        }

        #region Private Methods

        private RedirectViewModel CreateRedirectModel(string term, int page)
        {

            var items = _redirectManagerRepository.GetAll(term, page, RedirectViewModel.PageSize).ToList();

            var totalItems = string.IsNullOrEmpty(term)
                ? _redirectManagerRepository.Count()
                : _redirectManagerRepository.Count(term);

            return new RedirectViewModel
            {
                Term = term,
                PageNumber = page,
                TotalItems = totalItems,
                Items = items
            };
        }

        private static string GetViewLocation(string viewName)
        {
            return $"{Shell.Paths.ProtectedRootPath}Verndale.RedirectManager/Views/RedirectManager/{viewName}.cshtml";
        }

        private string Validate(RedirectManagerModel entry)
        {
            if (string.IsNullOrEmpty(entry.OldUrl))
            {
                return "Old url cannot be empty";
            }

            if (!UrlUtil.IsValidUrl(entry.OldUrl))
            {
                return $"Old url '{entry.OldUrl}' is invalid.";
            }

            if (string.IsNullOrEmpty(entry.NewUrl))
            {
                return "New url cannot be empty";
            }

            if (!UrlUtil.IsValidUrl(entry.NewUrl))
            {
                return $"New url '{entry.NewUrl}' is invalid.";
            }

            if (entry.Type != 301 && entry.Type != 302)
            {
                return $"Type '{entry.Type}' is invalid.";
            }

            return string.Empty;
        }


        #endregion

    }
}