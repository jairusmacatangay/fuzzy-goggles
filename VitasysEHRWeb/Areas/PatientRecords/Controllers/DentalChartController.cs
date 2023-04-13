using Microsoft.AspNetCore.Authorization;
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
    public class DentalChartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DentalChartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(int id)
        {
            ViewData["CurrentPage"] = "dentalChart";
            ViewData["PatientId"] = id;

            ApplicationUser user = GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }
            InsertLog("View Dental Chart List", user.Id, user.ClinicId, SD.AuditView);

            return View();
        }

        [HttpPost]
        public IActionResult Create(int patientId)
        {
            try
            {
                var user = GetCurrentUser();

                if (user.ClinicId == null)
                    return View("Error");

                OralCavity oralCavity = new();

                _unitOfWork.OralCavity.Add(oralCavity);
                _unitOfWork.Save();

                DentalChart chart = new();
                chart.PatientId = patientId;
                chart.ClinidId = (int)user.ClinicId;
                chart.Status = "Draft";
                chart.OralCavityId = oralCavity.Id;
                chart.EncounterDate = DateTime.Now;

                _unitOfWork.DentalChart.Add(chart);
                _unitOfWork.Save();

                InsertLog("Create Dental Chart Draft", user.Id, user.ClinicId, SD.AuditCreate);

                ViewData["CurrentPage"] = "dentalChart";
                return RedirectToAction("View", new { dentalChartId = chart.Id });
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult View(int dentalChartId)
        {
            try
            {
                DentalChartVM vm = new();
                vm.DentalChart = _unitOfWork.DentalChart.GetFirstOrDefault(x => x.Id == dentalChartId, includeProperties: "OralCavity");

                if (vm.DentalChart.EncounterDate == null)
                    return View("Error");

                DateTime encounterDate = (DateTime)vm.DentalChart.EncounterDate;

                vm.EncounterDate = encounterDate.ToString("MM/dd/yyyy hh:mm:ss tt");
                vm.ToothList = new();

                var toothList = _unitOfWork.ToothDetail.GetAll(x => x.DentalChartId == dentalChartId);

                foreach (var item in toothList)
                {
                    vm.ToothList.Add(new ToothVM
                    {
                        Id = item.Id,
                        ToothNo = item.ToothNo,
                        Type = item.Type,
                        Condition = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Condition),
                        BuccalOrLabial = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.BuccalOrLabial),
                        Lingual = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Lingual),
                        Mesial = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Mesial),
                        Distal = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Distal),
                        Occlusal = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Occlusal)
                    });
                }

                vm.Tooth55 = vm.ToothList.Find(x => x.ToothNo == 55);
                vm.Tooth54 = vm.ToothList.Find(x => x.ToothNo == 54);
                vm.Tooth53 = vm.ToothList.Find(x => x.ToothNo == 53);
                vm.Tooth52 = vm.ToothList.Find(x => x.ToothNo == 52);
                vm.Tooth51 = vm.ToothList.Find(x => x.ToothNo == 51);
                vm.Tooth61 = vm.ToothList.Find(x => x.ToothNo == 61);
                vm.Tooth62 = vm.ToothList.Find(x => x.ToothNo == 62);
                vm.Tooth63 = vm.ToothList.Find(x => x.ToothNo == 63);
                vm.Tooth64 = vm.ToothList.Find(x => x.ToothNo == 64);
                vm.Tooth65 = vm.ToothList.Find(x => x.ToothNo == 65);
                vm.Tooth18 = vm.ToothList.Find(x => x.ToothNo == 18);
                vm.Tooth17 = vm.ToothList.Find(x => x.ToothNo == 17);
                vm.Tooth16 = vm.ToothList.Find(x => x.ToothNo == 16);
                vm.Tooth15 = vm.ToothList.Find(x => x.ToothNo == 15);
                vm.Tooth14 = vm.ToothList.Find(x => x.ToothNo == 14);
                vm.Tooth13 = vm.ToothList.Find(x => x.ToothNo == 13);
                vm.Tooth12 = vm.ToothList.Find(x => x.ToothNo == 12);
                vm.Tooth11 = vm.ToothList.Find(x => x.ToothNo == 11);
                vm.Tooth21 = vm.ToothList.Find(x => x.ToothNo == 21);
                vm.Tooth22 = vm.ToothList.Find(x => x.ToothNo == 22);
                vm.Tooth23 = vm.ToothList.Find(x => x.ToothNo == 23);
                vm.Tooth24 = vm.ToothList.Find(x => x.ToothNo == 24);
                vm.Tooth25 = vm.ToothList.Find(x => x.ToothNo == 25);
                vm.Tooth26 = vm.ToothList.Find(x => x.ToothNo == 26);
                vm.Tooth27 = vm.ToothList.Find(x => x.ToothNo == 27);
                vm.Tooth28 = vm.ToothList.Find(x => x.ToothNo == 28);
                vm.Tooth48 = vm.ToothList.Find(x => x.ToothNo == 48);
                vm.Tooth47 = vm.ToothList.Find(x => x.ToothNo == 47);
                vm.Tooth46 = vm.ToothList.Find(x => x.ToothNo == 46);
                vm.Tooth45 = vm.ToothList.Find(x => x.ToothNo == 45);
                vm.Tooth44 = vm.ToothList.Find(x => x.ToothNo == 44);
                vm.Tooth43 = vm.ToothList.Find(x => x.ToothNo == 43);
                vm.Tooth42 = vm.ToothList.Find(x => x.ToothNo == 42);
                vm.Tooth41 = vm.ToothList.Find(x => x.ToothNo == 41);
                vm.Tooth31 = vm.ToothList.Find(x => x.ToothNo == 31);
                vm.Tooth32 = vm.ToothList.Find(x => x.ToothNo == 32);
                vm.Tooth33 = vm.ToothList.Find(x => x.ToothNo == 33);
                vm.Tooth34 = vm.ToothList.Find(x => x.ToothNo == 34);
                vm.Tooth35 = vm.ToothList.Find(x => x.ToothNo == 35);
                vm.Tooth36 = vm.ToothList.Find(x => x.ToothNo == 36);
                vm.Tooth37 = vm.ToothList.Find(x => x.ToothNo == 37);
                vm.Tooth38 = vm.ToothList.Find(x => x.ToothNo == 38);
                vm.Tooth85 = vm.ToothList.Find(x => x.ToothNo == 85);
                vm.Tooth84 = vm.ToothList.Find(x => x.ToothNo == 84);
                vm.Tooth83 = vm.ToothList.Find(x => x.ToothNo == 83);
                vm.Tooth82 = vm.ToothList.Find(x => x.ToothNo == 82);
                vm.Tooth81 = vm.ToothList.Find(x => x.ToothNo == 81);
                vm.Tooth71 = vm.ToothList.Find(x => x.ToothNo == 71);
                vm.Tooth72 = vm.ToothList.Find(x => x.ToothNo == 72);
                vm.Tooth73 = vm.ToothList.Find(x => x.ToothNo == 73);
                vm.Tooth74 = vm.ToothList.Find(x => x.ToothNo == 74);
                vm.Tooth75 = vm.ToothList.Find(x => x.ToothNo == 75);

                if (vm.DentalChart == null)
                    return View("Error");

                if (vm.DentalChart.OralCavity == null)
                    return View("Error");

                ApplicationUser user = GetCurrentUser();
                InsertLog("View Dental Chart", user.Id, user.ClinicId, SD.AuditView);

                ViewData["CurrentPage"] = "dentalChart";
                return View(vm);
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult UpdateOralCavity(DentalChartVM obj)
        {
            try
            {
                if (obj.DentalChart == null)
                    return View("Error");

                var chart = _unitOfWork.DentalChart.GetFirstOrDefault(x => x.Id == obj.DentalChart.Id);

                if (chart == null)
                    return View("Error");

                if (ModelState.IsValid)
                {
                    if (obj.DentalChart.OralCavity == null)
                        return View("Error");

                    var objFromDb = _unitOfWork.OralCavity.GetFirstOrDefault(x => x.Id == obj.DentalChart.OralCavity.Id);

                    if (objFromDb == null)
                        return View("Error");

                    objFromDb.IsGingivitis = obj.DentalChart.OralCavity.IsGingivitis;
                    objFromDb.IsEarlyPerio = obj.DentalChart.OralCavity.IsEarlyPerio;
                    objFromDb.IsModPerio = obj.DentalChart.OralCavity.IsModPerio;
                    objFromDb.IsAdvPerio = obj.DentalChart.OralCavity.IsAdvPerio;
                    objFromDb.ClassType = obj.DentalChart.OralCavity.ClassType;
                    objFromDb.ClassType = obj.DentalChart.OralCavity.ClassType;
                    objFromDb.IsOverjet = obj.DentalChart.OralCavity.IsOverjet;
                    objFromDb.IsOverbite = obj.DentalChart.OralCavity.IsOverbite;
                    objFromDb.IsMidlineDeviation = obj.DentalChart.OralCavity.IsMidlineDeviation;
                    objFromDb.IsCrossbite = obj.DentalChart.OralCavity.IsCrossbite;
                    objFromDb.OrthoApplication = obj.DentalChart.OralCavity.OrthoApplication;
                    objFromDb.IsClenching = obj.DentalChart.OralCavity.IsClenching;
                    objFromDb.IsClicking = obj.DentalChart.OralCavity.IsClicking;
                    objFromDb.IsTrismus = obj.DentalChart.OralCavity.IsTrismus;
                    objFromDb.IsMusclePain = obj.DentalChart.OralCavity.IsMusclePain;

                    _unitOfWork.OralCavity.Update(objFromDb);
                    _unitOfWork.Save();

                    chart.LastModified = DateTime.Now;

                    _unitOfWork.DentalChart.Update(chart);
                    _unitOfWork.Save();

                    TempData["success"] = "Successfuly update oral cavity conditions!";
                    ViewData["CurrentPage"] = "dentalChart";

                    ApplicationUser user = GetCurrentUser();
                    InsertLog("Update Oral Cavity", user.Id, user.ClinicId, SD.AuditUpdate);

                    return RedirectToAction("View", new { dentalChartId = obj.DentalChart.Id });
                }

                ViewData["CurrentPage"] = "dentalChart";
                return RedirectToAction("View", new { dentalChartId = obj.DentalChart.Id });
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult SaveTooth(ToothFormVM vm)
        {
            try
            {
                if (vm.Tooth == null) return View("Error");

                var chart = _unitOfWork.DentalChart.GetFirstOrDefault(x => x.Id == vm.Tooth.DentalChartId);

                if (chart == null) return View("Error");

                if (vm.Tooth.Id > 0)
                {
                    if (ModelState.IsValid)
                    {
                        var tooth = _unitOfWork.ToothDetail.GetFirstOrDefault(x => x.Id == vm.Tooth.Id);
                        tooth.Condition = vm.Tooth.Condition;

                        if (vm.Tooth.Condition != 1)
                        {
                            tooth.BuccalOrLabial = 20;
                            tooth.Lingual = 20;
                            tooth.Occlusal = 20;
                            tooth.Mesial = 20;
                            tooth.Distal = 20;
                        }
                        else
                        {
                            tooth.BuccalOrLabial = vm.Tooth.BuccalOrLabial;
                            tooth.Lingual = vm.Tooth.Lingual;
                            tooth.Occlusal = vm.Tooth.Occlusal;
                            tooth.Mesial = vm.Tooth.Mesial;
                            tooth.Distal = vm.Tooth.Distal;
                        }

                        _unitOfWork.ToothDetail.Update(tooth);
                        _unitOfWork.Save();

                        chart.LastModified = DateTime.Now;

                        _unitOfWork.DentalChart.Update(chart);
                        _unitOfWork.Save();

                        TempData["success"] = "Successfully updated tooth!";
                        ViewData["CurrentPage"] = "dentalChart";

                        ApplicationUser user = GetCurrentUser();
                        InsertLog("Update Tooth", user.Id, user.ClinicId, SD.AuditUpdate);

                        return RedirectToAction("View", new { dentalChartId = vm.Tooth.DentalChartId });
                    }

                    ViewData["CurrentPage"] = "dentalChart";
                    return RedirectToAction("View", new { dentalChartId = vm.Tooth.DentalChartId });
                }

                if (ModelState.IsValid)
                {
                    Tooth tooth = new()
                    {
                        DentalChartId = vm.Tooth.DentalChartId,
                        ToothNo = vm.Tooth.ToothNo,
                        Type = vm.Tooth.Type,
                        Condition = vm.Tooth.Condition,
                        BuccalOrLabial = vm.Tooth.BuccalOrLabial,
                        Lingual = vm.Tooth.Lingual,
                        Occlusal = vm.Tooth.Occlusal,
                        Mesial = vm.Tooth.Mesial,
                        Distal = vm.Tooth.Distal
                    };

                    _unitOfWork.ToothDetail.Add(tooth);
                    _unitOfWork.Save();

                    chart.LastModified = DateTime.Now;

                    _unitOfWork.DentalChart.Update(chart);
                    _unitOfWork.Save();

                    TempData["success"] = "Successfully created tooth!";
                    ViewData["CurrentPage"] = "dentalChart";

                    ApplicationUser user = GetCurrentUser();
                    InsertLog("Create Tooth", user.Id, user.ClinicId, SD.AuditCreate);

                    return RedirectToAction("View", new { dentalChartId = vm.Tooth.DentalChartId });
                }

                ViewData["CurrentPage"] = "dentalChart";
                return RedirectToAction("View", new { dentalChartId = vm.Tooth.DentalChartId });
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Finish(int dentalChartId)
        {
            try
            {
                var chart = _unitOfWork.DentalChart.GetFirstOrDefault(x => x.Id == dentalChartId);

                if (chart == null) return View("Error");

                chart.Status = "Completed";
                chart.LastModified = DateTime.Now;

                _unitOfWork.DentalChart.Update(chart);
                _unitOfWork.Save();

                TempData["success"] = "Successfully finalized Dental Chart!";
                ViewData["CurrentPage"] = "dentalChart";

                ApplicationUser user = GetCurrentUser();
                InsertLog("Finalize Dental Chart", user.Id, user.ClinicId, SD.AuditUpdate);

                return RedirectToAction("Index", new { id = chart.PatientId });
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult LoadToothForm(int toothNo, string type, int id)
        {
            try
            {
                ApplicationUser user = GetCurrentUser();

                ToothFormVM vm = new();

                var tooth = _unitOfWork.ToothDetail.GetFirstOrDefault(x => x.ToothNo == toothNo && x.DentalChartId == id);

                if (tooth != null)
                {
                    vm.Tooth = new Tooth
                    {
                        ToothNo = toothNo,
                        Type = type,
                        DentalChartId = id,
                        Condition = tooth.Condition,
                        BuccalOrLabial = tooth.BuccalOrLabial,
                        Lingual = tooth.Lingual,
                        Occlusal = tooth.Occlusal,
                        Mesial = tooth.Mesial,
                        Distal = tooth.Distal,
                        Id = tooth.Id
                    };

                    vm.LabelWholeList = _unitOfWork.ToothLabel.GetAll(x => x.Scope == "Whole").Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Label
                    }).ToList();

                    vm.LabelPartList = _unitOfWork.ToothLabel.GetAll(x => x.Scope == "Part").Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Label
                    }).ToList();

                    InsertLog("View Tooth Form", user.Id, user.ClinicId, SD.AuditView);

                    return PartialView("_ToothForm", vm);
                }

                vm.Tooth = new Tooth
                {
                    ToothNo = toothNo,
                    Type = type,
                    DentalChartId = id
                };

                vm.LabelWholeList = _unitOfWork.ToothLabel.GetAll(x => x.Scope == "Whole").Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Label
                }).ToList();

                vm.LabelPartList = _unitOfWork.ToothLabel.GetAll(x => x.Scope == "Part").Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Label
                }).ToList();

                InsertLog("View Tooth Form", user.Id, user.ClinicId, SD.AuditView);

                return PartialView("_ToothForm", vm);
            }
            catch
            {
                return View("Error");
            }
        }

        public string GetEncounters(int id, string status)
        {
            var list = _unitOfWork.DentalChart.GetAll(x => x.PatientId == id);

            if (list == null)
                return string.Empty;

            if (status == "archived")
                list = list.Where(x => x.IsArchived == true);
            else
                list = list.Where(x => x.IsArchived == false);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(list, jsonSettings);
        }

        [HttpPost]
        public IActionResult Archive(int? id)
        {
            try
            {
                var obj = _unitOfWork.DentalChart.GetFirstOrDefault(x => x.Id == id);
                if (obj == null)
                    return Json(new { success = false, message = "Dental Chart does not exist." });

                obj.IsArchived = true;
                _unitOfWork.DentalChart.Update(obj);
                _unitOfWork.Save();

                ApplicationUser user = GetCurrentUser();
                InsertLog("Archive Tooth", user.Id, user.ClinicId, SD.AuditArchive);

                ViewData["CurrentPage"] = "dentalChart";

                return Json(new { success = true, message = "Successfully archived Dental Chart!" });
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            try
            {
                var obj = _unitOfWork.DentalChart.GetFirstOrDefault(u => u.Id == id);

                if (obj == null)
                    return Json(new { success = false, message = "Dental Chart does not exist." });

                var toothList = _unitOfWork.ToothDetail.GetAll(x => x.DentalChartId == id);

                if (toothList.Count() > 0)
                {
                    _unitOfWork.ToothDetail.RemoveRange(toothList);
                    _unitOfWork.Save();
                }

                var oralcavityId = obj.OralCavityId;

                _unitOfWork.DentalChart.Remove(obj);
                _unitOfWork.Save();

                var oralCavity = _unitOfWork.OralCavity.GetFirstOrDefault(x => x.Id == oralcavityId);
                _unitOfWork.OralCavity.Remove(oralCavity);
                _unitOfWork.Save();

                ApplicationUser user = GetCurrentUser();
                InsertLog("Delete Dental Chart", user.Id, user.ClinicId, SD.AuditDelete);

                ViewData["CurrentPage"] = "dentalChart";

                return Json(new { success = true, message = "Successfully deleted Dental Chart!" });
            }
            catch
            {
                return View("Error");
            }
        }

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
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
        }
    }
}
