using CLMS_PROJECT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace CLMS_PROJECT.Controllers
{
    public class MasterController : Controller
    {
        private readonly DbpracticeContext mcontext;

        public MasterController(DbpracticeContext   Mcontext)
        {
            mcontext = Mcontext;
        }


        public async Task<IActionResult> ParameterMaster(Guid? id, int page = 1, string pcode = "", string parameterName = "", DateTime? createdOn = null)
        {
            int pageSize = 5;
            var query = mcontext.ParameterMasters.AsQueryable();

            // Filter by pcode if provided
            if (!string.IsNullOrEmpty(pcode))
            {
                query = query.Where(p => p.ParameterCode.Contains(pcode));
            }

            // Filter by ParameterName if provided
            if (!string.IsNullOrEmpty(parameterName))
            {
                query = query.Where(p => p.ParameterName.Contains(parameterName));
            }

            // Filter by CreatedOn date if provided
            if (createdOn.HasValue)
            {
                query = query.Where(p => EF.Functions.DateDiffDay(p.CreatedOn, createdOn.Value) == 0);
            }

            // Pagination logic
            var pagedData = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalCount = await query.CountAsync();

            ViewBag.pList = pagedData;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pass filter values back to the view for display in form inputs
            ViewBag.SearchPcode = pcode;
            ViewBag.SearchParameterName = parameterName;
            ViewBag.SearchCreatedOn = createdOn?.ToString("yyyy-MM-dd");

            if (id.HasValue)
            {
                var model = await mcontext.ParameterMasters.FindAsync(id.Value);
                if (model == null)
                {
                    return NotFound();
                }

                // Return JSON data for form fields
                return Json(new
                {
                    id = model.Id,
                    parameterCode = model.ParameterCode,
                    parameterName = model.ParameterName,
                    parameterDesc = model.ParameterDesc,
                    CreatedOn = model.CreatedOn
                });
            }

            // Return view with a new model for the form
            return View(new ParameterMaster());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParameterMaster(ParameterMaster parameter)
        {
         
                if (parameter.Id != Guid.Empty) // Editing an existing record
                {
                    var existingParameter = await mcontext.ParameterMasters.FindAsync(parameter.Id);
                    if (existingParameter != null)
                    {
                        existingParameter.ParameterCode = parameter.ParameterCode;
                        existingParameter.ParameterName = parameter.ParameterName;
                        existingParameter.ParameterDesc = parameter.ParameterDesc;
                        // Preserve CreatedOn value
                    }
                }
                else // Adding a new record
                {
                    parameter.CreatedOn = DateTime.Now; // Set CreatedOn when adding
                    await mcontext.ParameterMasters.AddAsync(parameter);
                }

                await mcontext.SaveChangesAsync();
            

            return RedirectToAction("ParameterMaster");
        }

        // POST: Delete the record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParameterMasterDelete(Guid id)
        {
            var para = await mcontext.ParameterMasters.FindAsync(id);
            if (para == null)
            {
                return NotFound();
            }

            mcontext.ParameterMasters.Remove(para);
            await mcontext.SaveChangesAsync();

            return RedirectToAction("ParameterMaster");
        }









        public IActionResult AddVendorMaster()
        {
            var VmasterList = mcontext.AppVendorMasters.ToList();
            ViewBag.vRegList = VmasterList;
            return View();
        }




        [HttpPost]
        public async Task<IActionResult> AddVendorMaster(AppVendorMaster MasterTbl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                  
              
                    MasterTbl.Id = Guid.NewGuid();
                    await mcontext.AppVendorMasters.AddAsync(MasterTbl);
                    await mcontext.SaveChangesAsync();
                    TempData["msg"] = "Created Master Successfully";
                    return RedirectToAction(nameof(AddVendorMaster));
                }
                catch (Exception ex)
                {
                    // Capture the inner exception
                    ViewBag.error = "An error occurred while saving: " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        ViewBag.error += " | Inner Exception: " + ex.InnerException.Message;
                    }
                }
            }

            return View();
        }

    }
}
