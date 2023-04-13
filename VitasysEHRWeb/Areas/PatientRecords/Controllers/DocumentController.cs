using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientRecords.Controllers
{
    [Area("PatientRecords")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist)]
    public class DocumentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly static string[] _folderPaths = new string[3];

        public DocumentController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _folderPaths[0] = @"documents\radiographs\";
            _folderPaths[1] = @"documents\photos\";
            _folderPaths[2] = @"documents\others\";

            GenerateDocumentsFolder();
        }

        public IActionResult Index()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ViewData["CurrentPage"] = "document";
            ApplicationUser user = GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }
            InsertLog("View Folders", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        public string GetAllFolders(int id)
        {
            if (AuthorizeAccess() == false) return String.Empty;

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.FolderList = _unitOfWork.Folder.GetAll(includeProperties: "FolderType").Where(x => x.PatientId == id);
            vm.Patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(vm.FolderList, jsonSettings);
        }

        public IActionResult LoadAddFolderForm()
        {
            if (AuthorizeAccess() == false) return View("Error");

            DocumentsFolderVM vm = new DocumentsFolderVM()
            {
                Folder = new(),
                FolderTypeList = _unitOfWork.FolderType.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Type,
                    Value = c.Id.ToString()
                })
            };
            return PartialView("_AddFolderForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFolder(DocumentsFolderVM vm)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string folderGuid = "-" + Guid.NewGuid().ToString("N");
            
            vm.FolderList = _unitOfWork.Folder.GetAll().Where(x => x.PatientId == vm.Folder.PatientId);
            
            if (ModelState.IsValid)
            {
                string? folderPath = Path.Combine(wwwRootPath, _folderPaths[vm.Folder.FolderTypeId - 1] + vm.Folder.Name + folderGuid);

                if (!Directory.Exists(folderPath) && !vm.FolderList.Any(x => x.Name.Substring(0, x.Name.Length - 33) == vm.Folder.Name))
                {
                    ApplicationUser user = GetCurrentUser();
                    vm.Folder.Name = vm.Folder.Name + folderGuid;
                    vm.Folder.ClinicId = (int)user.ClinicId;
                    _unitOfWork.Folder.Add(vm.Folder);
                   
                    _unitOfWork.Save();
                    
                    Directory.CreateDirectory(folderPath);
                    
                    TempData["success"] = "The folder is created successfully";
                    
                    
                    InsertLog("Create Folder", user.Id, user.ClinicId, SD.AuditCreate);
                }
                else
                {
                    TempData["error"] = "The folder is already existing! Try a different name instead.";
                }

                return RedirectToAction("Index");
            }
            return View("Index");
        }

        public IActionResult LoadEditFolderForm(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == id);
            //Remove the GUID 33 characters including the hyphen separator
            vm.Folder.Name = vm.Folder.Name.Substring(0, vm.Folder.Name.Length - 33);

            vm.FolderTypeList = _unitOfWork.FolderType.GetAll().Select(c => new SelectListItem
            {
                Text = c.Type,
                Value = c.Id.ToString()
            });
            return PartialView("_EditFolderForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateFolder(DocumentsFolderVM vm)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            vm.FolderList = _unitOfWork.Folder.GetAll().Where(x => x.PatientId == vm.Folder.PatientId);
            vm.DocumentList = _unitOfWork.Document.GetAll().Where(x => x.FolderId == vm.Folder.Id);
            vm.Document = new();

            if (ModelState.IsValid)
            {
                //get old path
                var oldFolderFromDb = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == vm.Folder.Id);
                var oldfolderPath = Path.Combine(wwwRootPath, _folderPaths[oldFolderFromDb.FolderTypeId - 1] + oldFolderFromDb.Name);

                //get guid value from oldFolder
                var guidValue = oldFolderFromDb.Name.Substring(oldFolderFromDb.Name.Length - 33, 33);

                //create new path
                string? newFolderPath = Path.Combine(wwwRootPath, _folderPaths[vm.Folder.FolderTypeId - 1] + vm.Folder.Name + guidValue);

                //if both paths are different => there's a change in the path
                if (!String.Equals(oldfolderPath, newFolderPath))
                {
                    //if new path does not exists and folder name has no duplicates
                    if (!Directory.Exists(newFolderPath) && !vm.FolderList.Any(x => x.Name.Substring(0, x.Name.Length - 33) == vm.Folder.Name))
                    {
                        //change DocumentUrl for all files depending on the new path
                        foreach (var url in vm.DocumentList)
                        {
                            var splitPath = url.DocumentUrl.Split(@"\");
                            var fileName = splitPath[4];
                            url.DocumentUrl = @"\" + _folderPaths[vm.Folder.FolderTypeId - 1] + vm.Folder.Name + guidValue + @"\" + fileName;
                            vm.Document = url;
                        }

                        //move the old directory to the new directory (automatically create a new folder and delete the old folder)
                        Directory.Move(oldfolderPath, newFolderPath);
                        //update database
                        vm.Folder.Name = vm.Folder.Name + guidValue;
                        _unitOfWork.Document.Update(vm.Document);
                        _unitOfWork.Folder.Update(vm.Folder);
                        _unitOfWork.Save();

                        ApplicationUser user = GetCurrentUser();
                        InsertLog("Update Folder", user.Id, user.ClinicId, SD.AuditUpdate);

                        TempData["success"] = "The folder is updated successfully";
                    }
                    else
                    {
                        TempData["error"] = "The folder is already existing! Try a different name instead.";
                    }
                }
                
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpDelete]
        public IActionResult DeleteFolder(int? id)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            var folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == id, includeProperties: "FolderType");

            if (folder == null)
            {
                return Json(new { success = false, message = "Folder does not exist." });
            }

            var folderPath = Path.Combine(wwwRootPath, _folderPaths[folder.FolderTypeId - 1] + folder.Name);

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            _unitOfWork.Folder.Remove(folder);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Delete Folder", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Deleted successfully!" });
        }


        public IActionResult Folder(string name)
        {
            if (AuthorizeAccess() == false) return View("Error");

            var decodeName = name.Replace("%20", " ");
            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Name == name, includeProperties: "FolderType");

            if (vm.Folder == null) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Documents in Folder", user.Id, user.ClinicId, SD.AuditView);

            ViewData["CurrentPage"] = "document";

            return View(vm);
        }

        public string GetAllDocuments(string name, string status)
        {
            if (AuthorizeAccess() == false) return String.Empty;

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Name == name, includeProperties: "FolderType");
            vm.DocumentList = _unitOfWork.Document.GetAll().Where(x => x.FolderId == vm.Folder.Id);

            switch (status)
            {
                case "archived":
                    vm.DocumentList = vm.DocumentList.Where(x => x.IsArchived == true).ToList();
                    break;
                case "active":
                    vm.DocumentList = vm.DocumentList.Where(x => x.IsArchived == false).ToList();
                    break;
                default:
                    break;
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(vm.DocumentList, jsonSettings);
        }

        public IActionResult LoadAddDocumentForm(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            DocumentsFolderVM documentsFolder = new DocumentsFolderVM()
            {
                Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == id, includeProperties: "FolderType"),
                Document = new()
            };
            return PartialView("_AddDocumentForm", documentsFolder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDocument(DocumentsFolderVM vm, IFormFile file)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var otherExtensions = new[] { ".pdf", ".doc", ".docx" };

            string wwwRootPath = _hostEnvironment.WebRootPath;
            var data = new { name = vm.Folder.Name, id = vm.Folder.Id };
            vm.DocumentList = _unitOfWork.Document.GetAll().Where(x => x.FolderId == vm.Document.FolderId);

            if (ModelState.IsValid)
            {
                if (!vm.DocumentList.Any(x => x.Name == vm.Document.Name))
                {
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var upload = Path.Combine(wwwRootPath, _folderPaths[vm.Folder.FolderTypeId - 1] + vm.Folder.Name);
                        var extension = Path.GetExtension(file.FileName).ToLower();

                        //check extension depending on folder type
                        if (vm.Folder.FolderTypeId == 1 || vm.Folder.FolderTypeId == 2)
                        {
                            if (!imageExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("DocumentUrl", "The file type is not supported in this folder!");
                                TempData["error"] = "File is not supported in this folder!";
                            }
                            else
                            {
                                using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                                {
                                    file.CopyTo(fileStreams);
                                }
                                vm.Document.DocumentUrl = @"\" + _folderPaths[vm.Folder.FolderTypeId - 1] + vm.Folder.Name + @"\" + fileName + extension;
                                _unitOfWork.Document.Add(vm.Document);
                                _unitOfWork.Save();

                                ApplicationUser user = GetCurrentUser();
                                InsertLog("Create Document", user.Id, user.ClinicId, SD.AuditCreate);

                                TempData["success"] = "File uploaded successfully";
                            }
                        }
                        else
                        {
                            if (!otherExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("DocumentUrl", "The file type is not supported in this folder!");
                                TempData["error"] = "File is not supported in this folder!";
                            }
                            else
                            {
                                using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                                {
                                    file.CopyTo(fileStreams);
                                }
                                vm.Document.DocumentUrl = @"\" + _folderPaths[vm.Folder.FolderTypeId - 1] + vm.Folder.Name + @"\" + fileName + extension;
                                _unitOfWork.Document.Add(vm.Document);
                                _unitOfWork.Save();

                                ApplicationUser user = GetCurrentUser();
                                InsertLog("Create Document", user.Id, user.ClinicId, SD.AuditCreate);

                                TempData["success"] = "File uploaded successfully";
                            }
                        }
                    }
                    else
                    {
                        TempData["error"] = "File is not uploaded! Please try again";
                    }
                }
                else
                {
                    TempData["error"] = "File name is already taken! Try a different name instead.";
                }
                return RedirectToAction("Folder", data);
            }
            return View("Folder", data);
        }

        public IActionResult LoadEditDocumentForm(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Document = _unitOfWork.Document.GetFirstOrDefault(x => x.Id == id, includeProperties:"Folder");
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Name == vm.Document.Folder.Name);
            return PartialView("_EditDocumentForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDocument(DocumentsFolderVM vm)
        {
            vm.DocumentList = _unitOfWork.Document.GetAll().Where(x => x.FolderId == vm.Document.FolderId);
            if (ModelState.IsValid)
            {
                if (vm.DocumentList.Any(x => x.Name == vm.Document.Name))
                {
                    TempData["Error"] = "Document name is already existing!";
                }
                else
                {
                    _unitOfWork.Document.Update(vm.Document);
                    _unitOfWork.Save();

                    ApplicationUser user = GetCurrentUser();
                    InsertLog("Update Document", user.Id, user.ClinicId, SD.AuditUpdate);

                    TempData["success"] = "Document name updated successfully";
                }
            }

            var data = new { name = vm.Document.Folder.Name, id = vm.Document.FolderId };
            return RedirectToAction("Folder", data);
        }

        public IActionResult ViewDocument(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Document = _unitOfWork.Document.GetFirstOrDefault(x => x.Id == id);
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == vm.Document.FolderId);

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Document", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewDocument",vm);
        }

        [HttpPost]
        public IActionResult ShareDocument(int? id)
        {
            var document = _unitOfWork.Document.GetFirstOrDefault(x => x.Id == id);

            if (document == null)
            {
                return Json(new { success = false, message = "Document does not exist." });
            }

            document.IsShared = true;
            _unitOfWork.Document.Update(document);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Share Document", user.Id, user.ClinicId, SD.AuditShare);

            return Json(new { success = true, message = "Shared successfully!" });
        }

        [HttpPost]
        public IActionResult ArchiveDocument(int? id)
        {
            var document = _unitOfWork.Document.GetFirstOrDefault(x => x.Id == id);

            if (document == null)
            {
                return Json(new { success = false, message = "Document does not exist." });
            }

            document.IsArchived = true;
            _unitOfWork.Document.Update(document);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Archive Document", user.Id, user.ClinicId, SD.AuditArchive);

            return Json(new { success = true, message = "Archived successfully!" });
        }

        [HttpDelete]
        public IActionResult DeleteDocument(int? id)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            var document = _unitOfWork.Document.GetFirstOrDefault(x => x.Id == id);

            if (document == null)
            {
                return Json(new { success = false, message = "Document does not exist." });
            }

            if (document.DocumentUrl != null)
            {
                var path = Path.Combine(wwwRootPath, document.DocumentUrl.TrimStart('\\'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            _unitOfWork.Document.Remove(document);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Delete Document", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Deleted successfully!" });
        }

        //I suggest do this when the app is initialized first to avoid calling this function repeatedly :D
        public void GenerateDocumentsFolder()
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            string folderDocument = Path.Combine(wwwRootPath, @"documents");
            string folderRadiograph = Path.Combine(wwwRootPath, _folderPaths[0]);
            string folderPhoto = Path.Combine(wwwRootPath, _folderPaths[1]);
            string folderOther = Path.Combine(wwwRootPath, _folderPaths[2]);


            if (!Directory.Exists(folderDocument))
            {
                Directory.CreateDirectory(folderDocument);
                Directory.CreateDirectory(folderRadiograph);
                Directory.CreateDirectory(folderPhoto);
                Directory.CreateDirectory(folderOther);
            }
        }

        #region HELPER FUNCTIONS

        public void InsertLog(string activityType, string userId, int? clinicId, string description)
        {
            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = activityType,
                DateAdded = DateTime.Now,
                UserId = userId,
                ClinicId = clinicId,
                Description = description,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();
        }

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string? userid = claim.Value;
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userid);
        }

        public bool AuthorizeAccess()
        {
            string? subscriptionType = HttpContext.Session.GetString(SD.SessionKeySubscriptionType);
            string? subscriptionIsLockout = HttpContext.Session.GetString(SD.SessionKeySubscriptionIsLockout);

            if (subscriptionType == "Free" || subscriptionIsLockout == "true")
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
