using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBook.DataAccess.Data;
using MyBook.DataAccess.Repository.IRepository;
using MyBook.Models;

namespace MyBook.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUnitofWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext db,
            IUnitofWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _unitOfWork = unitOfWork;
        }

        public string Username { get; set; }

        public string Name { get; set; }
        public string City { get; set; } 
        public string State { get; set; } 
        public string PostalCode { get; set; } 
        public string StreetAddress { get; set; } 
        public string Role { get; set; } 

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(IdentityUser user) {  
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            
           

            var objDb = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == userName);
            var obj2 = _db.UserRoles.FirstOrDefault(u => u.UserId == objDb.Id);
            var obj3 = _db.Roles.FirstOrDefault(u => u.Id == obj2.RoleId);


            Username = userName;
            Name = objDb.Name;
            City = objDb.City;
            State = objDb.State;
            Role = obj3.Name;
            PostalCode = objDb.PostalCode;
            StreetAddress = objDb.StreetAddress;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber


            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(ApplicationUser applicationUser)
        {
            StatusMessage = "Nothing in your profile has changed";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            

            var objDb = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == user.UserName);
            var name = objDb.Name;
            if (applicationUser.Name != name)
            {
                if (applicationUser.Name != null)
                {
                    objDb.Name = applicationUser.Name;
                    StatusMessage = "Your profile has been updated";
                    if (!StatusMessage.Contains("Error"))
                    {
                        StatusMessage = "Your profile has been updated";
                    }
                    else
                    {
                        //continue
                    }
                }
                else
                {
                    StatusMessage = "Error, your profile has not been changed";
                }
            }

            var state = objDb.State;
            if (applicationUser.State != state)
            {
                if (applicationUser.State != null)
                {
                    
                    if (!StatusMessage.Contains("Error"))
                    {
                        objDb.State = applicationUser.State;
                        StatusMessage = "Your profile has been updated";
                    }
                    else
                    {
                        //continue
                    }
                }
                else
                {
                    StatusMessage = "Error, your profile's state name has not been changed";
                }
            }

            var city = objDb.City;
            if (applicationUser.City != city)
            {
                if (applicationUser.City != null)
                {
                    if (!StatusMessage.Contains("Error"))
                    {
                        objDb.City = applicationUser.City;
                        StatusMessage = "Your profile has been updated";
                    }
                    else
                    {
                        //continue
                    }
                }
                else
                {
                    StatusMessage = "Error, your profile's city name has not been changed";
                }
            }
           
            var postalCode = objDb.PostalCode;
            if (applicationUser.PostalCode != postalCode)
            {
                if (applicationUser.PostalCode != null)
                {
                    if (!StatusMessage.Contains("Error"))
                    {
                        objDb.PostalCode = applicationUser.PostalCode;
                        StatusMessage = "Your profile has been updated";
                    }
                    else
                    {
                        //continue
                    }
                }
                else
                {
                    StatusMessage = "Error, your profile's postal code has not been changed";
                }
            }
            var streetAddress = objDb.StreetAddress;
            if (applicationUser.StreetAddress != streetAddress)
            {
                if (applicationUser.StreetAddress != null)
                {
                    if (!StatusMessage.Contains("Error"))
                    {
                        objDb.StreetAddress = applicationUser.StreetAddress;
                        StatusMessage = "Your profile has been updated";
                    }
                    else
                    {
                        //continue
                    }
                }
                else
                {
                    StatusMessage = "Error, your profile's street address has not been changed";
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                if (Input.PhoneNumber != null)
                {
                    if (!StatusMessage.Contains("Error"))
                    {
                        var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                        StatusMessage = "Your profile has been updated";

                        
                    }
                    else
                    {
                        //continue
                    }  
                }
                else
                {
                    StatusMessage = "Error, your profile's phone number has not been changed";
                }

            }

            _unitOfWork.Save();

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }
    }
}
