using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using APS.Models;
using APS.Models.ViewModels;
using APS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace APS.Controllers
{
    public class AccountController : Controller
    {
        private readonly APSContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private static readonly List<string> RomanianCities = new List<string>
        {
            "Abrud", "Adjud", "Agnita", "Aiud", "Alba Iulia", "Alesd", "Alexandria", "Amara", "Anina", "Aninoasa", "Arad", "Ardud", "Avrig", "Azuga", "Babadag", "Babeni", "Bacau", "Baia de Arama", "Baia de Aries", "Baia Mare", "Baia Sprie", "Baicoi", "Baile Govora", "Baile Herculane", "Baile Olanesti", "Bailesti", "Baile Tusnad", "Balan", "Balcesti", "Bals", "Baneasa", "Baraolt", "Barlad", "Bechet", "Beclean", "Beius", "Berbesti", "Beresti", "Bicaz", "Bistrita", "Blaj", "Bocsa", "Boldesti-Scaeni", "Bolintin-Vale", "Borsa", "Borsec", "Botosani", "Brad", "Bragadiru", "Braila", "Brasov", "Breaza", "Brezoi", "Brosteni", "Bucecea", "Bucuresti", "Budesti", "Buftea", "Buhusi", "Bumbesti-Jiu", "Busteni", "Buzau", "Buzias", "Cajvana", "Calafat", "Calan", "Calarasi", "Calimanesti", "Campeni", "Campia Turzii", "Campina", "Campulung", "Campulung Moldovenesc", "Caracal", "Caransebes", "Carei", "Cavnic", "Cazanesti", "Cehu Silvaniei", "Cernavoda", "Chisineu-Cris", "Chitila", "Ciacova", "Cisnadie", "Cluj-Napoca", "Codlea", "Comanesti", "Comarnic", "Constanta", "Copsa Mica", "Corabia", "Costesti", "Covasna", "Craiova", "Cristuru Secuiesc", "Cugir", "Curtea de Arges", "Curtici", "Dabuleni", "Darabani", "Darmanesti", "Dej", "Deta", "Deva", "Dolhasca", "Dorohoi", "Draganesti-Olt", "Dragasani", "Dragomiresti", "Drobeta-Turnu Severin", "Dumbraveni", "Eforie", "Fagaras", "Faget", "Falticeni", "Faurei", "Fetesti", "Fieni", "Fierbinti-Targ", "Filiasi", "Flamanzi", "Focsani", "Frasin", "Fundulea", "Gaesti", "Galati", "Gataia", "Geoagiu", "Gheorgheni", "Gherla", "Ghimbav", "Giurgiu", "Gura Humorului", "Harlau", "Harsova", "Hateg", "Horezu", "Huedin", "Hunedoara", "Husi", "Ianca", "Iasi", "Iernut", "Ineu", "Insuratei", "Intorsura Buzaului", "Isaccea", "Jibou", "Jimbolia", "Lehliu Gara", "Lipova", "Liteni", "Livada", "Ludus", "Lugoj", "Lupeni", "Macin", "Magurele", "Mangalia", "Marasesti", "Marghita", "Medgidia", "Medias", "Miercurea Ciuc", "Miercurea Nirajului", "Miercurea Sibiului", "Mihailesti", "Milisauti", "Mioveni", "Mizil", "Moinesti", "Moldova Noua", "Moreni", "Motru", "Murfatlar", "Murgeni", "Nadlac", "Nasaud", "Navodari", "Negresti", "Negresti-Oas", "Negru Voda", "Nehoiu", "Novaci", "Nucet", "Ocna Mures", "Ocna Sibiului", "Ocnele Mari", "Odobesti", "Odorheiu Secuiesc", "Oltenita", "Onesti", "Oradea", "Orastie", "Oravita", "Orsova", "Otelu Rosu", "Otopeni", "Ovidiu", "Panciu", "Pancota", "Pantelimon", "Pascani", "Patarlagele", "Pecica", "Petrila", "Petrosani", "Piatra-Olt", "Piatra Neamt", "Pitesti", "Ploiesti", "Plopeni", "Podu Iloaiei", "Pogoanele", "Popesti-Leordeni", "Potcoava", "Predeal", "Pucioasa", "Racari", "Radauti", "Ramnicu Sarat", "Ramnicu Valcea", "Rasnov", "Recas", "Reghin", "Resita", "Roman", "Rosiorii de Vede", "Rovinari", "Roznov", "Rupea", "Sacele", "Sacueni", "Salcea", "Saliste", "Salistea de Sus", "Salonta", "Sangeorgiu de Padure", "Sangeorz-Bai", "Sannicolau Mare", "Santana", "Sarmasu", "Satu Mare", "Saveni", "Scornicesti", "Sebes", "Sebis", "Segarcea", "Seini", "Sfantu Gheorghe", "Sibiu", "Sighetu Marmatiei", "Sighisoara", "Simeria", "Sinaia", "Siret", "Slanic", "Slanic-Moldova", "Slatina", "Slobozia", "Solca", "Somcuta Mare", "Sovata", "Stefanesti", "Stefanesti", "Stei", "Strehaia", "Suceava", "Sulina", "Talmaciu", "Tandarei", "Targoviste", "Targu Bujor", "Targu Carbunesti", "Targu Frumos", "Targu Jiu", "Targu Lapus", "Targu Mures", "Targu Neamt", "Targu Ocna", "Targu Secuiesc", "Tarnaveni", "Tasnad", "Tautii-Magheraus", "Techirghiol", "Tecuci", "Teius", "Ticleni", "Timisoara", "Tismana", "Titu", "Toplita", "Topoloveni", "Tulcea", "Turceni", "Turda", "Turnu Magurele", "Ulmeni", "Ungheni", "Uricani", "Urlati", "Urziceni", "Valea lui Mihai", "Valenii de Munte", "Vanju Mare", "Vascau", "Vaslui", "Vatra Dornei", "Vicovu de sus", "Victoria", "Videle", "Viseu de Sus", "Vlahita", "Voluntari", "Vulcan", "Zalau", "Zarnesti", "Zimnicea", "Zlatna", "Simleu Silvaniei"
        };
        private static readonly List<string> FictionalPublishers = new List<string>
        {
            "Romanian Press Group", "Dacia Media", "Transylvania News", "Carpathian Times", "Danube Daily", "Bucharest Bulletin", "Constanta Chronicle", "Moldova Monitor", "Banat Gazette", "Dobrogea Journal"
        };

        public AccountController(APSContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && user.IsActive)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt or account not active.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                Cities = RomanianCities
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use.");
                    // Repopulate dropdowns before returning view
                    model.Cities = RomanianCities;
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    DateOfBirth = model.DateOfBirth,
                    City = model.City,
                    JournalistType = model.JournalistType,
                    Publication = model.Publication,
                    UserName = model.Email,
                    IsActive = false,
                    EmailVerificationToken = GenerateEmailVerificationToken()
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // TODO: Send email verification
                    // TODO: Send notification to admin for approval
                    return RedirectToAction("RegistrationSuccess");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Repopulate dropdowns before returning view
            model.Cities = RomanianCities;
            return View(model);
        }

        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        private string GenerateEmailVerificationToken()
        {
            return Guid.NewGuid().ToString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 