using app.Configuration;
using app.Model;
using app.Model.DBase;
using app.Model.Devices;
using app.Model.DTOs.Requests;
using app.Model.DTOs.Responses;
using app.Services;
using app.Services.Spire;
using app.Services.Titan;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VendorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private ICashWorker _cashDevice;
        private ICardWorker _cardDevice;
        private TitanGeneralPayments _titanPaymentResponse = new TitanGeneralPayments();
        private SpirePaymentResponse _spirePaymentResponse = new SpirePaymentResponse();

        public VendorController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ICashWorker cashDevice,
            ICardWorker cardDevice
            )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cashDevice = cashDevice ?? throw new ArgumentNullException(nameof(cashDevice));
            _cardDevice = cardDevice ?? throw new ArgumentNullException(nameof(cardDevice));
        }

        private async Task<ApplicationUser> getUser()
        {
            return await _userManager.FindByEmailAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        public DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        private async Task UpdateCashDeviceData(CashDevice cashDevice)
        {
            var response = await _cashDevice.LoadTranslations();
            _context.CashDeviceErrors.RemoveRange(_context.CashDeviceErrors);
            var err = JsonConvert.DeserializeObject<TitanDictionaryError>(response.ToString());
            var errorList = JsonConvert.DeserializeObject<List<CashDeviceError>>(JsonConvert.SerializeObject(err.Ru.Err.ToList()));
            _context.CashDeviceErrors.AddRange(errorList);

            cashDevice.LoadInfoDate = DateTime.Now;
            _context.CashDevices.Update(cashDevice);
            await _context.SaveChangesAsync();
        }
        private async Task CashDeviceConnection(ApplicationUser user)
        {
            var setting = await _context.UserSettings
            .Where(a => a.UserId == user.Id)
                .Include(b => b.Setting)
                    .ThenInclude(c => c.CashDevice)
                        .FirstOrDefaultAsync();

            _cashDevice.Connection(
                setting.Setting.CashDevice.HOST,
                setting.Setting.CashDevice.Login,
                setting.Setting.CashDevice.Password,
                setting.Setting.CashDevice.Port,
                setting.Setting.CashDevice.Speed);

            var response = await _cashDevice.GetState();
            TitanResponse state = JsonConvert.DeserializeObject<TitanResponse>(response.ToString());

            var foo1 = UnixTimeStampToDateTime(state.JrnTime);
            var foo2 = UnixTimeStampToDateTime(state.time);

            if (setting.Setting.CashDevice.LoadInfoDate.ToShortDateString() != DateTime.Now.ToShortDateString())
                await UpdateCashDeviceData(setting.Setting.CashDevice);
        }
        
        #region Vendor TransientDraft CRUD
        private async Task<VendoreResponse> GetTransientDraftResponse(ApplicationUser user)
        {
            var transientDraft = await _context.TransientDrafts.Where(a => a.UserId == user.Id).Include(b => b.Good).ToListAsync();
            if (transientDraft.Any(a => a.isReturn == true))
                return new VendoreResponse() { Success = true, isReturn = true, result = transientDraft, count = transientDraft.Count() };

            return new VendoreResponse() { Success = true, result = transientDraft, count = transientDraft.Count() };
        }

        [HttpGet]
        public async Task<IActionResult> GetTransientDraft()
        {
            var user = await getUser();
            if (user != null)
                return Ok(await GetTransientDraftResponse(user));

            return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { "Something wrong" } });

        }

        [HttpPost]
        [Route("search/{id}")]
        public async Task<IActionResult> PostTransientDraft(string id)
        {
            var searchGood = await _context.Goods.Where(a => a.Barcode == id).FirstOrDefaultAsync();
            if (searchGood != null)
            {
                var user = await getUser();
                if (_context.TransientDrafts.Any(a => a.UserId == user.Id && a.GoodId == searchGood.Id))
                {
                    var update = _context.TransientDrafts.Where(a => a.UserId == user.Id && a.GoodId == searchGood.Id).Include(b => b.Good).Single();
                    update.Quantity = update.Quantity + 1;
                    update.Amount = update.Good.Price * update.Quantity;
                    _context.TransientDrafts.Update(update);
                }
                else
                {
                    var newTransientDraft = new TransientDraft() { GoodId = searchGood.Id, UserId = user.Id, Quantity = 1, Amount = searchGood.Price };
                    _context.TransientDrafts.Add(newTransientDraft);
                }

                await _context.SaveChangesAsync();
                return Ok(await GetTransientDraftResponse(user));
            }

            return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Searching not result" } });
        }

        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> PutTransientDraft(TransientDraft transientDraft)
        {
            var user = await getUser();
            transientDraft.Amount = transientDraft.Good.Price * transientDraft.Quantity;
            transientDraft.Good = null; transientDraft.User = null;
            _context.TransientDrafts.Update(transientDraft);
            await _context.SaveChangesAsync();
            return Ok(await GetTransientDraftResponse(user));
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteTransientDraft(int id)
        {
            var user = await getUser();
            if (id == 0)
                _context.TransientDrafts.RemoveRange(_context.TransientDrafts.Where(a => a.UserId == user.Id));
            else
                _context.TransientDrafts.Remove(_context.TransientDrafts.Where(a => a.Id == id && a.UserId == user.Id).FirstOrDefault());
            await _context.SaveChangesAsync();

            return Ok(await GetTransientDraftResponse(user));
        }
        #endregion

        #region Vendor Payment reports
        [HttpPut]
        [Route("openworkshift")]
        public async Task<IActionResult> PutOpenWorkShift()
        {
            var user = await getUser();

            if (!_context.UserWorkShifts.Where(a => a.UserId == user.Id).Any())
            {
                var newWorkShift = new UserWorkShift() { UserId = user.Id, StartWorkShift = DateTime.Now, NumberOfWorkShift = 1, Status = true };
                _context.UserWorkShifts.Add(newWorkShift);
                await _context.SaveChangesAsync();
                return Ok(new VendoreResponse() { Success = true, Message = new List<string> { $"Смена {newWorkShift.NumberOfWorkShift} успешно открыта" } });
            }

            var lastWorkShift = _context.UserWorkShifts.Where(a => a.UserId == user.Id).AsEnumerable().LastOrDefault();
            if (lastWorkShift.Status)
                return Ok(new VendoreResponse() { Success = false, Message = new List<string> { $"Текущая рабочая смена {lastWorkShift.NumberOfWorkShift}" } });
            else
            {
                var newWorkShift = new UserWorkShift() { UserId = user.Id, StartWorkShift = DateTime.Now, NumberOfWorkShift = lastWorkShift.NumberOfWorkShift + 1, Status = true };
                _context.UserWorkShifts.Add(newWorkShift);
                await _context.SaveChangesAsync();
                return Ok(new VendoreResponse() { Success = true, Message = new List<string> { $"Смена {newWorkShift.NumberOfWorkShift} успешно открыта" } });

            }
        }

        [HttpPut]
        [Route("closeworkshift")]
        public async Task<IActionResult> PutCloseWorkShift()
        {
            var user = await getUser();
            await CashDeviceConnection(user);

            if (_context.UserWorkShifts.Where(a => a.UserId == user.Id && a.Status == true).Any())
            {
                var response = await _cashDevice.SendReport(0);
                try
                {
                    Error err = JsonConvert.DeserializeObject<Error>(response.ToString());
                    return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == err.err).Value } });
                }
                catch
                {
                    var lastWorkShift = _context.UserWorkShifts.Where(a => a.UserId == user.Id).AsEnumerable().LastOrDefault();
                    lastWorkShift.EndWorkShift = DateTime.Now;
                    lastWorkShift.Status = false;
                    lastWorkShift.Amount = _context.Drafts.Where(a => a.UserWorkShiftId == lastWorkShift.Id).AsEnumerable().Sum(b => b.Amount);
                    _context.Update(lastWorkShift);
                    await _context.SaveChangesAsync();
                    return Ok(new VendoreResponse() { Success = true, Message = new List<string> { $"Смена {lastWorkShift.NumberOfWorkShift} успешно закрыта" } });
                }
            }

            return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { "Нет смены которую можно закрыть" } });
        }

        [HttpPut]
        [Route("xreport")]
        public async Task<IActionResult> PutXReport()
        {
            var user = await getUser();
            await CashDeviceConnection(user);
            var response = await _cashDevice.SendReport(100);
            try
            {
                Error err = JsonConvert.DeserializeObject<Error>(response.ToString());
                return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == err.err).Value } });
            }
            catch
            {
                return Ok(new VendoreResponse() { Success = true, Message = new List<string> { "Отчет без гашения успешно сформирован" } });
            }
        }

        [HttpPut]
        [Route("vendorstat")]
        public async Task<IActionResult> PutVendorStat()
        {
            var user = await getUser();
            await CashDeviceConnection(user);
            var response = await _cashDevice.SendReport(201);
            try
            {
                Error err = JsonConvert.DeserializeObject<Error>(response.ToString());
                return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == err.err).Value } });
            }
            catch
            {
                return Ok(new VendoreResponse() { Success = true, Message = new List<string> { "Отчет по статистике успешно сформирован" } });
            }
        }
        #endregion

        #region Vendor Payment cash operation
        [HttpPut]
        [Route("payment")]
        public async Task<IActionResult> PutPayment([FromHeader] HeaderRequest headerRequest, Payment payment)
        {
            int sign = 1;
            if (headerRequest.isreturn)
                sign = -1;
            //TitanGeneralPayments _titanPaymentResponse = new TitanGeneralPayments();
            //SpirePaymentResponse _spirePaymentResponse = new SpirePaymentResponse();

            var user = await getUser();
            await CashDeviceConnection(user);

            if (payment.CardPayment > 0 && payment.CashPayment == 0)
            {
                //spirePaymentResponse = await _cardDevice.SendPaymentGeneral(Convert.ToUInt16(payment.CardPayment * 100), 0, 933, _settings.CardDevice.Ip, 5757);
                //if (spirePaymentResponse.UNBank == null)
                //    return new JsonResult(new Response() { response = false, title = "Ошибка", message = "Проблема с терминалом" });
            }

            if (payment.CashPayment > 0 && payment.CardPayment == 0)
            {
                var transitDraft = await _context.TransientDrafts.Where(a => a.UserId == user.Id).Include(b => b.Good).ToListAsync();
                var responseCash = await _cashDevice.SendPaymentGeneral(payment, transitDraft, headerRequest);
                TitanErrorResponse err = JsonConvert.DeserializeObject<TitanErrorResponse>(responseCash.ToString());
                if (err.err != null)
                {
                    var key = err.err.e;
                    return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == key).Value } });
                }
                else
                {
                    _titanPaymentResponse = JsonConvert.DeserializeObject<TitanGeneralPayments>(responseCash.ToString());
                }
            }

            if (payment.CashPayment > 0 && payment.CardPayment > 0)
            {
                ////spirePaymentResponse = await _cardDevice.SendPaymentGeneral(Convert.ToUInt16(payment.CardPayment * 100), 0, 933, _settings.CardTerminal_Ip, 5757);
                ////if (spirePaymentResponse.UNBank == null)
                ////    return new JsonResult(new { result = 0, title = "Ошибка", message = "Проблема с терминалом" });

                //var transitcheck = await _context.TransitCheck.Where(a => a.UserId == stringParameters.userid).ToListAsync();
                //var responseCash = await _cashDevice.SendPaymentGeneral(payment, transitcheck);
                //TitanErrorResponse err = JsonConvert.DeserializeObject<TitanErrorResponse>(responseCash.ToString());
                //if (err.err != null)
                //{
                //    var key = err.err.e;
                //    return new JsonResult(new Response() { response = false, title = "Ошибка", message = _context.CashTerminalError.Single(a => a.Key == key).Value });
                //}
                //else
                //{
                //    titanPaymentResponse = JsonConvert.DeserializeObject<TitanGeneralPayments>(responseCash.ToString());
                //}
            }

            //if (_titanPaymentResponse != null)
            if (_titanPaymentResponse != null && (payment.CashPayment > 0 || payment.CardPayment > 0))
            {
                var draft = new Draft
                {
                    UserId = user.Id,
                    DateTime = UnixTimeStampToDateTime(_titanPaymentResponse.datetime).ToString(),
                    DraftNumber = _titanPaymentResponse.no,
                    UserWorkShiftId = _context.UserWorkShifts.Where(a => a.UserId == user.Id && a.Status == true).FirstOrDefault().Id
                };

                Draft refDraft = await _context.Drafts
                    .Where(a => a.DraftNumber == _context.TransientDrafts.Where(b => b.UserId == user.Id).FirstOrDefault().refDraftNumber)
                        .Include(c => c.TableDraft)
                            .FirstOrDefaultAsync();

                if (headerRequest.isreturn)
                {
                    draft.refDraftNumber = refDraft.DraftNumber;
                    draft.Status = DraftStatuses.Returned;
                    //refDraft.Amount = refDraft.Amount - payment.Amount;
                    refDraft.Status = DraftStatuses.Returned;
                }

                draft.Amount = sign * payment.Amount;
                await _context.Drafts.AddAsync(draft);

                var tableDraft = await _context.TransientDrafts.Where(a => a.UserId == user.Id).Select(b => new TableDraft
                {
                    DraftId = draft.Id,
                    GoodId = b.GoodId,
                    Quantity = sign * b.Quantity,
                    Discount = b.Discount,
                    DiscountPercent = b.DiscountPercent,
                    DiscountPrice = b.DiscountPrice,
                    Amount = sign * b.Amount
                }).ToListAsync();

                var billingDraft = new BillingDraft
                {
                    DraftId = draft.Id,
                    Amount = sign * payment.Amount,
                    CashPayment = sign * payment.CashPayment,
                    CardPayment = sign * payment.CardPayment,
                    CashBack = payment.CashBack,
                    UNOperation = _spirePaymentResponse.UNOperation,
                    UNBankresponse = _spirePaymentResponse.UNBank,
                    CardNumber = _spirePaymentResponse.CardNumber
                };

                await _context.TableDrafts.AddRangeAsync(tableDraft);
                await _context.BillingDrafts.AddAsync(billingDraft);
                _context.TransientDrafts.RemoveRange(_context.TransientDrafts.Where(a => a.UserId == user.Id));
                await _context.SaveChangesAsync();

                return Ok(new VendoreResponse
                {
                    Success = true,
                    Message = new List<string> { $"Чек № {_titanPaymentResponse.no} успешно закрыт" }
                });
            }

            return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Somthing wrong" }});
        }

        [HttpPut]
        [Route("cashin")]
        public async Task<IActionResult> PutCashIn(Payment payment)
        {
            var user = await getUser();
            await CashDeviceConnection(user);

            var workShift = _context.UserWorkShifts.Any(a => a.UserId == user.Id && a.Status == true);

            if (workShift)
            {
                var response = await _cashDevice.SendPayment(Convert.ToSingle(payment.CashPayment));
                var cashDeviceResponse = JsonConvert.DeserializeObject<TitanGeneralPayments>(response.ToString());
                try
                {
                    Error err = JsonConvert.DeserializeObject<Error>(response.ToString());
                    return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == err.err).Value } });
                }
                catch
                {
                    var inDraft = new InOutDraft
                    {
                        UserId = user.Id,
                        UserWorkShiftId = _context.UserWorkShifts.Where(a => a.UserId == user.Id && a.Status == true).FirstOrDefault().Id,
                        DateTime = UnixTimeStampToDateTime(cashDeviceResponse.datetime).ToString(),
                        DraftNumber = cashDeviceResponse.no,
                        Amount = payment.CashPayment,
                        Type = DraftStatuses.In
                    };

                    await _context.InOutDrafts.AddAsync(inDraft);
                    await _context.SaveChangesAsync();

                    return Ok(new VendoreResponse { Success = true, Message = new List<string> { $"Чек внесения № {cashDeviceResponse.no} успешно закрыт" } });

                }

            }

            return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Workshift is not open yet" } });
        }

        [HttpPut]
        [Route("cashout")]
        public async Task<IActionResult> PutCashOut(Payment payment)
        {
            var user = await getUser();
            await CashDeviceConnection(user);

            var workShift = _context.UserWorkShifts.Any(a => a.UserId == user.Id && a.Status == true);

            if (workShift)
            {
                var response = await _cashDevice.SendPayment(Convert.ToSingle("-" + payment.CashPayment));
                var cashDeviceResponse = JsonConvert.DeserializeObject<TitanGeneralPayments>(response.ToString());
                try
                {
                    Error err = JsonConvert.DeserializeObject<Error>(response.ToString());
                    return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == err.err).Value } });
                }
                catch
                {
                    var inDraft = new InOutDraft
                    {
                        UserId = user.Id,
                        UserWorkShiftId = _context.UserWorkShifts.Where(a => a.UserId == user.Id && a.Status == true).FirstOrDefault().Id,
                        DateTime = UnixTimeStampToDateTime(cashDeviceResponse.datetime).ToString(),
                        DraftNumber = cashDeviceResponse.no,
                        Amount = payment.CashPayment,
                        Type = DraftStatuses.Out
                    };

                    await _context.InOutDrafts.AddAsync(inDraft);
                    await _context.SaveChangesAsync();

                    return Ok(new VendoreResponse { Success = true, Message = new List<string> { $"Чек изъятия № {cashDeviceResponse.no} успешно закрыт" } });

                }

            }

            return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Workshift is not open yet" } });
        }
        #endregion

        #region Draft Method

        [HttpGet]
        [Route("detail/{id}")]
        public async Task<IActionResult> GetDraftDetail(string id)
        {
            var number = await _context.Drafts.SingleAsync(a => a.Id == Guid.Parse(id));

            var tableDraft = _context.Drafts
            .Where(a => a.DraftNumber == number.DraftNumber || a.refDraftNumber == number.DraftNumber)
            .Include(b => b.TableDraft)
            .Join(_context.TableDrafts,
            c => c.Id,
            d => d.DraftId,
            (c, d) => new TableDraft
            {
                GoodId = d.GoodId,
                Amount = d.Amount,
                Quantity = d.Quantity,
            })
            .AsEnumerable()
            .GroupBy(e => e.GoodId)
            .Select(f => new {
                GoodId = f.Key,
                Amount = f.Sum(g => g.Amount),
                Quantity = f.Sum(h => h.Quantity),
            })
            .Join(_context.Goods,
            o => o.GoodId,
            i => i.Id,
            (o, i) => new TableDraft
            {
                Amount = o.Amount,
                Quantity = o.Quantity,
                Good = new Good { Price = i.Price, GoodsName = i.GoodsName, VendoreCode = i.VendoreCode, Barcode = i.Barcode }
            })
            .Where(m => m.Amount > 0)
            .ToList();

            if (tableDraft != null)
                return Ok(new VendoreResponse { Success = true, result = tableDraft, count = tableDraft.Count() });


            return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { "Something wrong" } });
        }

        #endregion

        #region Cancel Draft Method
        private async Task<VendoreResponse> GetCancelDraftResponse(ApplicationUser user, HeaderRequest headerRequest)
        {
            var draft = await _context.Drafts.Where(a => a.UserId == user.Id && a.Status == null)
                .OrderByDescending(b => b.DateTime)
                    .Include(c => c.User)
                        .Include(d => d.UserWorkShift)
                            .ToListAsync();

            var draftSkipTake = draft
                .Skip(headerRequest.skip)
                    .Take(headerRequest.take)
                        .ToList();

            if (headerRequest.search != "null" && headerRequest.search != null)
            {
                var draftSkipTakeSearch = draftSkipTake.FindAll(a =>
                a.DraftNumber.ToString().Contains(headerRequest.search)
                //a.BidDate.ToLower().Contains(search) ||
                //a.BidNumber.ToString().ToLower().Contains(search) ||
                );
                return new VendoreResponse() { Success = true, result = draftSkipTakeSearch, count = draftSkipTakeSearch.Count() };
            }

            return new VendoreResponse() { Success = true, result = draftSkipTake, count = draft.Count() };
        }

        [HttpGet]
        [Route("cancel/drafts")]
        public async Task<IActionResult> GetCancelDrafts([FromHeader] HeaderRequest headerRequest)
        {
            var user = await getUser();
            if (user != null)
                return Ok(await GetCancelDraftResponse(user, headerRequest));

            return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { "Something wrong" } });

        }

        [HttpDelete]
        [Route("cancel/draft/{id}")]
        public async Task<IActionResult> DeleteDraftCancel(string id, [FromHeader] HeaderRequest headerRequest)
        {
            int sign = -1;

            var user = await getUser();
            if (user != null)
            {
                Draft draft = await _context.Drafts.Where(a => a.Id == Guid.Parse(id))
                    .Include(b => b.TableDraft)
                        .Include(c => c.BillingDraft)
                            .SingleOrDefaultAsync();
                if (draft != null)
                {
                    var responseCash = await _cashDevice.SendPaymentGeneral(new Payment(), new List<TransientDraft>(), headerRequest, draft.DraftNumber);
                    TitanErrorResponse err = JsonConvert.DeserializeObject<TitanErrorResponse>(responseCash.ToString());
                    if (err.err != null)
                    {
                        var key = err.err.e;
                        return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { _context.CashDeviceErrors.Single(a => a.Key == key).Value } });
                    }
                    else
                    {

                        _titanPaymentResponse = JsonConvert.DeserializeObject<TitanGeneralPayments>(responseCash.ToString());
                        var draftCancel = new Draft
                        {
                            UserId = user.Id,
                            DateTime = UnixTimeStampToDateTime(_titanPaymentResponse.datetime).ToString(),
                            DraftNumber = _titanPaymentResponse.no,
                            refDraftNumber = draft.DraftNumber,
                            Amount = sign * draft.Amount,
                            //UserWorkShiftId = _context.UserWorkShifts.Where(a => a.UserId == user.Id && a.Status == true).FirstOrDefault().Id - отмену можно делать только в рамках текущей открытой смены!
                            UserWorkShiftId = draft.UserWorkShiftId,
                            Status = DraftStatuses.Cancel
                        };

                        await _context.Drafts.AddAsync(draftCancel);

                        var tableDraftCancel = draft.TableDraft.Select(a => new TableDraft
                        {
                            DraftId = draftCancel.Id,
                            GoodId = a.GoodId,
                            Quantity = sign * a.Quantity,
                            Discount = a.Discount,
                            DiscountPercent = a.DiscountPercent,
                            DiscountPrice = a.DiscountPrice,
                            Amount = sign * a.Amount
                        }).ToList();

                        var billingDraft = new BillingDraft
                        {
                            DraftId = draftCancel.Id,
                            Amount = sign * draft.BillingDraft.Amount,
                            CashPayment = sign * draft.BillingDraft.CashPayment,
                            CardPayment = sign * draft.BillingDraft.CardPayment,
                            CashBack = draft.BillingDraft.CashBack
                        };

                        await _context.TableDrafts.AddRangeAsync(tableDraftCancel);
                        await _context.BillingDrafts.AddAsync(billingDraft);
                        draft.Status = DraftStatuses.Cancel;

                        await _context.SaveChangesAsync();
                        return Ok(await GetCancelDrafts(new HeaderRequest { take = 5, skip = 0, search = "null" }));
                    }
                }
            }

            return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Bad with user cancel draft" } });
        }
        #endregion

        #region Return Draft Method
        private async Task<VendoreResponse> GetReturnDraftResponse(ApplicationUser user, HeaderRequest headerRequest)
        {
            var draft = await _context.Drafts
            .Where(a => a.UserId == user.Id && a.Status == null)
            .Include(w => w.UserWorkShift)
            .ToListAsync();

            var draftReturning = _context.Drafts
            .Where(a => a.UserId == user.Id && a.Status != DraftStatuses.Cancel)
            .Join(_context.Drafts,
            a => a.DraftNumber,
            b => b.refDraftNumber,
            (a, b) => new Draft
            {
                refDraftNumber = b.refDraftNumber,
                Amount = b.Amount
            })
            .AsEnumerable()
            .GroupBy(c => c.refDraftNumber)
            .Select(d => new
            {
                DraftNumber = d.Key,
                Amount = d.Sum(e => e.Amount)
            })
            .Join(_context.Drafts,
            f => f.DraftNumber,
            t => t.DraftNumber,
            (f, t) => new Draft
            {
                Id = t.Id,
                UserId = t.UserId,
                User = t.User,
                UserWorkShiftId = t.UserWorkShiftId,
                UserWorkShift = _context.UserWorkShifts.FirstOrDefault(m => m.Id == t.UserWorkShiftId),
                DateTime = t.DateTime,
                DraftNumber = t.DraftNumber,
                Amount = t.Amount + f.Amount
            })
            .Where(n=>n.Amount>0)
            .ToList();


            var unionDraft = draft.Union(draftReturning);

            var draftSkipTake = unionDraft
            .OrderByDescending(b => b.DateTime)
            .Skip(headerRequest.skip)
            .Take(headerRequest.take)
            .ToList();

            if (headerRequest.search != "null" && headerRequest.search != null)
            {
                var draftSkipTakeSearch = draftSkipTake.FindAll(a =>
                a.DraftNumber.ToString().Contains(headerRequest.search)
                //a.BidDate.ToLower().Contains(search) ||
                //a.BidNumber.ToString().ToLower().Contains(search) ||
                );
                return new VendoreResponse() { Success = true, result = draftSkipTakeSearch, count = draftSkipTakeSearch.Count() };
            }

            return new VendoreResponse() { Success = true, result = draftSkipTake, count = unionDraft.Count() };
        }

        [HttpGet]
        [Route("return/drafts")]
        public async Task<IActionResult> GetReturnDrafts([FromHeader] HeaderRequest headerRequest)
        {
            var user = await getUser();
            if (user != null)
                return Ok(await GetReturnDraftResponse(user, headerRequest));

            return BadRequest(new VendoreResponse() { Success = false, Error = new List<string> { "Something wrong" } });

        }

        [HttpGet]
        [Route("return/draft/{id}")]
        public async Task<IActionResult> GetReturnDraft(string id)
        {
            var user = await getUser();
            if (user != null)
            {
                var number = await _context.Drafts.SingleAsync(a => a.Id == Guid.Parse(id));

                var tableDraft = _context.Drafts
                .Where(a => a.DraftNumber == number.DraftNumber || a.refDraftNumber == number.DraftNumber)
                .Include(b => b.TableDraft)
                .Join(_context.TableDrafts,
                c => c.Id,
                d => d.DraftId,
                (c, d) => new TableDraft
                {
                    GoodId = d.GoodId,
                    Amount = d.Amount,
                    Quantity = d.Quantity,
                })
                .AsEnumerable()
                .GroupBy(e => e.GoodId)
                .Select(f => new {
                    GoodId = f.Key,
                    Amount = f.Sum(g => g.Amount),
                    Quantity = f.Sum(h => h.Quantity),
                })
                .Join(_context.Goods,
                o => o.GoodId,
                i => i.Id,
                (o, i) => new TransientDraft
                {
                    UserId = user.Id,
                    GoodId = o.GoodId,
                    Quantity = o.Quantity,
                    Amount = o.Amount,
                    isReturn = true,
                    refDraftNumber = number.DraftNumber
                    //Good = new Good { Price = i.Price, GoodsName = i.GoodsName, VendoreCode = i.VendoreCode, Barcode = i.Barcode }
                })
                .Where(m => m.Amount > 0)
                .ToList();

                if (tableDraft != null)
                {
                    await _context.TransientDrafts.AddRangeAsync(tableDraft);
                    await _context.SaveChangesAsync();

                    return Ok(new VendoreResponse { Success = true, Message = new List<string> { "Return draft is ok" }, isReturn = true });
                    
                }

                return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Bad with return draft" } });
            }

            return BadRequest(new VendoreResponse { Success = false, Error = new List<string> { "Bad with user return draft" } });
        }

        #endregion
    }
}
