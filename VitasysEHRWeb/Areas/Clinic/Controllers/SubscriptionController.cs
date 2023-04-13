using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Security.Claims;
using System.Text;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.Paymongo;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{

    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner)]
    public class SubscriptionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var user = GetCurrentUser();

            InsertLog("View Subscription", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpGet]
        public IActionResult Upgrade()
        {
            var user = GetCurrentUser();
            InsertLog("View Upgrade Subscription", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpPost]
        public IActionResult Upgrade(string subscriptionPlan)
        {
            if (subscriptionPlan == null)
            {
                TempData["error"] = "Select a plan from the dropdown list.";
                return RedirectToAction("Upgrade");
            }

            var user = GetCurrentUser();

            if (user == null || user.ClinicId == null) return View("Error");

            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            if (subscription == null) return View("Error");

            subscription.Type = subscriptionPlan;
            subscription.PaymentMode = "Monthly";
            subscription.BillingDate = DateTime.Now;
            subscription.LastModified = DateTime.Now;

            _unitOfWork.Subscription.Update(subscription);
            _unitOfWork.Save();

            InsertLog("Upgraded Subscription", user.Id, user.ClinicId, SD.AuditUpdate);

            return RedirectToAction("PayWithGCash");
        }

        [HttpGet]
        public IActionResult Change()
        {
            var user = GetCurrentUser();
            InsertLog("View Change Subscription", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpPost]
        public IActionResult Change(string subscriptionPlan)
        {
            if (subscriptionPlan == null) 
            {
                TempData["error"] = "Select a plan from the dropdown list.";
                return RedirectToAction("Change");
            }

            var user = GetCurrentUser();

            if (user == null || user.ClinicId == null) return View("Error");

            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            if (subscription == null) return View("Error");

            if (subscription.Type == subscriptionPlan)
            {
                TempData["error"] = "Choose a plan that is different from your current one.";
                return RedirectToAction("Change");
            }

            subscription.Type = subscriptionPlan;
            subscription.BillingDate = DateTime.Now;
            subscription.LastModified = DateTime.Now;

            _unitOfWork.Subscription.Update(subscription);
            _unitOfWork.Save();

            InsertLog("Changed Subscription", user.Id, user.ClinicId, SD.AuditUpdate);

            return RedirectToAction("PayWithGCash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(string subscriptionPlan)
        {
            var user = GetCurrentUser();

            if (user == null || user.ClinicId == null) return View("Error");

            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            if (subscription == null)
            {
                Subscription obj = new();
                obj.ClinicId = (int)user.ClinicId;
                obj.Type = subscriptionPlan;
                obj.DateCreated = DateTime.Now;

                if (subscriptionPlan != "Free")
                {
                    obj.BillingDate = obj.DateCreated.AddDays(7);
                    obj.PaymentMode = "Monthly";
                }

                if (subscriptionPlan == "Free")
                    obj.PaymentMode = "None";

                _unitOfWork.Subscription.Add(obj);
                _unitOfWork.Save();

                InsertLog("Create Subscription", user.Id, user.ClinicId, SD.AuditCreate);

                TempData["success"] = "Successfully created your subscription.";

                return RedirectToAction("Index", "Account");
            }

            subscription.Type = subscriptionPlan;
            subscription.PaymentMode = "Monthly";

            _unitOfWork.Subscription.Update(subscription);
            _unitOfWork.Save();

            InsertLog("Update Subscription", user.Id, user.ClinicId, SD.AuditUpdate);

            TempData["success"] = "Successfully updated your subscription.";

            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public IActionResult Cancel()
        {
            var user = GetCurrentUser();

            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            if (subscription == null)
            {
                return Json(new { success = false, message = "Subscription does not exist." });
            }

            subscription.Type = "Free";
            subscription.PaymentMode = "None";
            subscription.LastModified = DateTime.Now;

            _unitOfWork.Subscription.Update(subscription);
            _unitOfWork.Save();

            InsertLog("Cancel Subscription", user.Id, user.ClinicId, SD.AuditUpdate);

            TempData["success"] = "Successfully cancelled your subscription";

            return Json(new { success = true });
        }

        [AllowAnonymous]
        public IActionResult Success()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Failed()
        {
            return View();
        }

        #region API CALLS

        public IActionResult PayWithGCash()
        {
            var user = GetCurrentUser();
            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            int amount;

            if (subscription.Type == "Basic")
                amount = 30000;
            else
                amount = 60000;

            string? failedUrl = Url.Action("Failed", "Subscription", new { area = "Clinic" });
            string failedCallbackUrl = $"{Request.Scheme}://{Request.Host}{failedUrl}";

            string? successUrl = Url.Action("Success", "Subscription", new { area = "Clinic" });
            string successCallbackUrl = $"{Request.Scheme}://{Request.Host}{successUrl}";

            var obj = new Source
            {
                data = new Source.Data
                {
                    attributes = new Source.Attributes
                    {
                        amount = amount,
                        currency = "PHP",
                        type = "gcash",
                        billing = new Source.Billing
                        {
                            email = user.Email,
                            name = AesOperation.DecryptString(user.FirstName) + " " + AesOperation.DecryptString(user.LastName),
                            phone = AesOperation.DecryptString(user.PhoneNumber),
                            address = new Source.Address
                            {
                                city = AesOperation.DecryptString(user.City),
                                country = "PH",
                                line1 = AesOperation.DecryptString(user.Address),
                                postal_code = AesOperation.DecryptString(user.ZipCode),
                                state = AesOperation.DecryptString(user.Province),
                            }
                        },
                        redirect = new Source.Redirect
                        {
                            success = successCallbackUrl,
                            failed = failedCallbackUrl
                        }
                    }
                }
            };

            var client = new RestClient("https://api.paymongo.com/v1/");
            var request = new RestRequest("sources", Method.Post);
            request.AddHeader("Authorization", "Basic cGtfdGVzdF9HNjlISnlaYVJZb0Rkb3Q4SFg3bjFoWks6");
            request.AddJsonBody(obj);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Source? sourceObj = JsonConvert.DeserializeObject<Source?>(response.Content);
                string checkoutUrl = sourceObj.data.attributes.redirect.checkout_url;

                InsertLog("Pay with GCash", user.Id, user.ClinicId, SD.AuditUpdate);

                return Redirect(checkoutUrl);
            }

            TempData["error"] = "An unexpected error occurred while processing your subscription.";
            return View("Index");
        }

        [AllowAnonymous]
        public async Task<OkResult> ProcessPaymentAsync()
        {
            // markb.uk/asp-net-core-read-raw-request-body-as-string.html
            if (!Request.Body.CanSeek)
            {
                Request.EnableBuffering();
            }

            Request.Body.Position = 0;

            var reader = new StreamReader(Request.Body, Encoding.UTF8);

            var rawRequestBody = await reader.ReadToEndAsync().ConfigureAwait(false);

            Request.Body.Position = 0;

            Event? eventObj = JsonConvert.DeserializeObject<Event?>(rawRequestBody);

            string eventType = eventObj.data.attributes.type;

            if (eventType == "source.chargeable")
            {
                int amount = eventObj.data.attributes.data.attributes.amount;
                string id = eventObj.data.attributes.data.id;
                string description = "VitaSys EHR subscription payment using GCash";

                var obj = new VitasysEHR.Models.Paymongo.Payment
                {
                    data = new VitasysEHR.Models.Paymongo.Payment.Data
                    {
                        attributes = new VitasysEHR.Models.Paymongo.Payment.Attributes
                        {
                            amount = amount,
                            source = new VitasysEHR.Models.Paymongo.Payment.Source
                            {
                                id = id,
                                type = "source"
                            },
                            currency = "PHP",
                            description = description
                        }
                    }
                };

                var client = new RestClient("https://api.paymongo.com/v1");
                var request = new RestRequest("payments", Method.Post);
                request.AddHeader("Authorization", "Basic c2tfdGVzdF84ajZOM3dqUXVGY2pFdXBuNlVncW1URDE6");
                request.AddJsonBody(obj);
                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    VitasysEHR.Models.Paymongo.Payment? paymentObj = JsonConvert.DeserializeObject<VitasysEHR.Models.Paymongo.Payment?>(response.Content);

                    var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Email == paymentObj.data.attributes.billing.email);

                    var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);
                    var nextBillingDate = subscription.BillingDate.AddMonths(1);

                    subscription.BillingDate = new DateTime(nextBillingDate.Year, nextBillingDate.Month, 15);
                    
                    if (subscription.IsLockout == true)
                        subscription.IsLockout = false;

                    _unitOfWork.Subscription.Update(subscription);
                    _unitOfWork.Save();

                    decimal amountPaid;

                    if (paymentObj.data.attributes.amount == 30000)
                        amountPaid = 300.00M;
                    else
                        amountPaid = 600.00M;

                    var payment = new VitasysEHR.Models.Payment()
                    {
                        SubscriptionId = subscription.Id,
                        Amount = amountPaid,
                        Status = "Paid"
                    };

                    _unitOfWork.Payment.Add(payment);
                    _unitOfWork.Save();
                }
            }

            return Ok();
        }

        public IActionResult LoadPaymentsList()
        {
            var user = GetCurrentUser();
            InsertLog("View Payment List", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_PaymentsList");
        }

        public string GetAllPayments()
        {
            ApplicationUser? user = GetCurrentUser();

            Subscription? subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            IEnumerable<VitasysEHR.Models.Payment>? list = _unitOfWork.Payment.GetAll(x => x.SubscriptionId == subscription.Id);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(list, jsonSettings);
        }

        #endregion


        #region HELPER FUNCTIONS

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
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

        #endregion

    }
}
