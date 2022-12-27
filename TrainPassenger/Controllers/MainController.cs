using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainPassenger.Models;
using TrainPassenger.ViewModels;

namespace TrainPassenger.Controllers
{
    public class MainController : Controller
    {
        TrainDbContext db;
        IWebHostEnvironment env;
        public MainController(TrainDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }
        public async Task<IActionResult> Index()
        {
            var id = 0;
            if (db.Tickets.Any())
            {
                id = db.Tickets.ToList()[0].TicketId;
            }

            DataViewModel data = new DataViewModel();
            data.SelectedTicketId = id;

            data.Trains = await db.Trains.ToListAsync();
            data.Passengers = await db.Passengers.ToListAsync();
            data.Tickets = await db.Tickets.ToListAsync();
            data.TicketItems = await db.TicketItems.Where(ti => ti.TicketId == id).ToListAsync();
            return View(data);
        }
        #region child actions
        public async Task<IActionResult> GetSelectedTicketItems(int id)
        {

            var TicketItems = await db.TicketItems.Include(x => x.Passenger).Where(oi => oi.TicketId == id).ToListAsync();
            return PartialView("_TicketItemTable", TicketItems);
        }       
        public IActionResult CreateTrain()
        {
            return PartialView("_CreateTrain");
        }
        [HttpPost]
        public async Task<IActionResult> CreateTrain(Train t)
        {
            if (ModelState.IsValid)
            {
                await db.Trains.AddAsync(t);
                await db.SaveChangesAsync();
                return Json(t);
            }
            return BadRequest("Unexpected error");
        }
        public async Task<IActionResult> EditTrain(int id)
        {
            var data = await db.Trains.FirstOrDefaultAsync(t => t.TrainId == id);
            return PartialView("_EditTrain", data);
        }
        [HttpPost]
        public async Task<IActionResult> EditTrain(Train t)
        {
            if (ModelState.IsValid)
            {
                db.Entry(t).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(t);
            }
            return BadRequest("Unexpected error");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTrain(int id)
        {
            if (!await db.Tickets.AnyAsync(t => t.TrainId == id))
            {
                var t = new Train { TrainId = id };
                db.Entry(t).State = EntityState.Deleted;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = true, message = "Data deleted" });
            }
            return Json(new { success = false, message = "Cannot delete because it has child." });
        }
        public IActionResult CreatePassenger()
        {
            return PartialView("_CreatePassenger");
        }
        [HttpPost]
        public async Task<IActionResult> CreatePassenger(PassengerInputModel p)
        {
            if (ModelState.IsValid)
            {
                var passenger = new Passenger { PassengerName = p.PassengerName, Phone = p.Phone };
                string fileName = Guid.NewGuid() + Path.GetExtension(p.Picture.FileName);
                string savePath = Path.Combine(this.env.WebRootPath, "Pictures", fileName);
                var fs = new FileStream(savePath, FileMode.Create);
                p.Picture.CopyTo(fs);
                fs.Close();
                passenger.Picture = fileName;
                await db.Passengers.AddAsync(passenger);
                await db.SaveChangesAsync();
                return Json(passenger);


            }
            return BadRequest("Falied to insert passenger");
        }

        public async Task<IActionResult> EditPassenger(int id)
        {
            var data = await db.Passengers.FirstAsync(x => x.PassengerId == id);
            ViewData["CurrentPic"] = data.Picture;
            return PartialView("_EditPassenger", new PassengerEditModel { PassengerId = data.PassengerId, PassengerName = data.PassengerName, Phone = data.Phone });
        }
        [HttpPost]
        public async Task<IActionResult> EditPassenger(PassengerEditModel p)
        {
            if (ModelState.IsValid)
            {
                var passenger = await db.Passengers.FirstAsync(x => x.PassengerId == p.PassengerId);
                passenger.PassengerName = p.PassengerName;
                passenger.Phone = p.Phone;
                if (p.Picture != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(p.Picture.FileName);
                    string savePath = Path.Combine(this.env.WebRootPath, "Pictures", fileName);
                    var fs = new FileStream(savePath, FileMode.Create);
                    p.Picture.CopyTo(fs);
                    fs.Close();
                    passenger.Picture = fileName;
                }


                await db.SaveChangesAsync();
                return Json(passenger);


            }
            return BadRequest();
        }
        public async Task<IActionResult> DeletePassenger(int id)
        {
            if (!await db.TicketItems.AnyAsync(o => o.PassengerId == id))
            {
                var o = new Passenger { PassengerId = id };
                db.Entry(o).State = EntityState.Deleted;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = true, message = "Data deleted" });
            }
            return Json(new { success = false, message = "Cannot delete, item has related child." });
        }
        //
        public async Task<IActionResult> CreateTicket()
        {
            ViewData["Passengers"] = await db.Passengers.ToListAsync();
            ViewData["Trains"] = await db.Trains.ToListAsync();
            return PartialView("_CreateTicket");
        }
        [HttpPost]
        public async Task<IActionResult> CreateTicket(Ticket t, int[] PassengerId, int[] Quantity)
        {
            if (ModelState.IsValid)
            {
                for (var i = 0; i < PassengerId.Length; i++)
                {
                    t.TicketItems.Add(new TicketItem { PassengerId = PassengerId[i], Quantity = Quantity[i] });
                }
                await db.Tickets.AddAsync(t);

                await db.SaveChangesAsync();


                var tkt = await GetTicket(t.TicketId);
                return Json(tkt);
            }
            return BadRequest();
        }
        //
        // new
        public async Task<IActionResult> EditTicket(int id)
        {
            ViewData["Passengers"] = await db.Passengers.ToListAsync();
            ViewData["Trains"] = await db.Trains.ToListAsync();
            var data = await db.Tickets
                .Include(x => x.TicketItems).ThenInclude(x => x.Passenger)
                .FirstOrDefaultAsync(x => x.TicketId == id);
            return PartialView("_EditTicket", data);

        }
        [HttpPost]
        public async Task<IActionResult> EditTicket(Ticket t)
        {
            if (ModelState.IsValid)
            {
                var existing = await db.Tickets.Include(x => x.Train).FirstAsync(x => x.TicketId == t.TicketId);
                existing.JourneyDate = t.JourneyDate;
                existing.Category = t.Category;
                existing.Price = t.Price;
                existing.TrainId = t.TrainId;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return Json(existing);
            }

            return BadRequest();
        }
        private async Task<Ticket> GetTicket(int id)
        {
            var o = await db.Tickets.Include(x => x.Train).FirstOrDefaultAsync(x => x.TicketId == id);
            return o;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var o = new Ticket { TicketId = id };
            db.Entry(o).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Data deleted" });
        }       
        //       
        public async Task<IActionResult> CreateItem()
        {
            ViewData["Passengers"] = await db.Passengers.ToListAsync();
            return PartialView("_CreateItem");
        }
        //
        // new
        public async Task<IActionResult> CreateTicketItem(int id)
        {
            ViewData["TicketId"] = id;
            ViewData["Passengers"] = await db.Passengers.ToListAsync();
            return PartialView("_CreateTicketItem");
        }
        [HttpPost]
        public async Task<IActionResult> CreateTicketItem(TicketItem ti)
        {
            if (ModelState.IsValid)
            {
                await db.TicketItems.AddAsync(ti);
                await db.SaveChangesAsync();
                var o = await GetTicketItem(ti.TicketId, ti.PassengerId);
                return Json(o);
            }
            return BadRequest();
        }
        public async Task<IActionResult> EditTicketItem(int tid, int pid)
        {
            ViewData["Passengers"] = await db.Passengers.ToListAsync();
            var ti = await db.TicketItems.FirstAsync(x => x.TicketId == tid && x.PassengerId == pid);
            return PartialView("_EditTicketItem", ti);
        }
        [HttpPost]
        public async Task<IActionResult> EditTicketItem(TicketItem ti)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ti).State = EntityState.Modified;
                await db.SaveChangesAsync();
                var o = await GetTicketItem(ti.TicketId, ti.PassengerId);
                return Json(o);
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTicketItem([FromQuery] int tid, [FromQuery] int pid)
        {

            var t = new TicketItem { PassengerId = pid, TicketId = tid };
            db.Entry(t).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Data deleted" });

        }
        private async Task<TicketItem> GetTicketItem(int tid, int pid)
        {
            var ti = await db.TicketItems
                .Include(t => t.Ticket)
                .Include(p => p.PassengerId)
                .FirstAsync(x => x.TicketId == tid && x.PassengerId == pid);
            return ti;
        }
        #endregion
    }
}
