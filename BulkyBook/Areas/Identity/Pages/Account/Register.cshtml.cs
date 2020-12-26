using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace BulkyBook.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork uow,
             IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _uow = uow;
            _mapper = mapper;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            Input = new InputModel()
            {
                CompanyList = _uow.Company.GetAll().Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                }),
                RoleList = _roleManager.Roles.Where(a => a.Name != SD.Roles.IndividualCustomer).Select(a => a.Name).Select(a => new SelectListItem
                {
                    Text = a,
                    Value = a
                }),
            };

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        ////public class InputModel
        ////{
        ////    [Required]
        ////    [EmailAddress]
        ////    [Display(Name = "Email")]
        ////    public string Email { get; set; }

        ////    [Required]
        ////    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        ////    [DataType(DataType.Password)]
        ////    [Display(Name = "Password")]
        ////    public string Password { get; set; }

        ////    [DataType(DataType.Password)]
        ////    [Display(Name = "Confirm password")]
        ////    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        ////    public string ConfirmPassword { get; set; }

        ////    [Required]
        ////    public string Name { get; set; }
        ////    public string StreetAddress { get; set; }
        ////    public string City { get; set; }
        ////    public string State { get; set; }
        ////    public string PostalCode { get; set; }
        ////    public string PhoneNumber { get; set; }
        ////    public int? CompanyId { get; set; }
        ////    public string Role { get; set; }
        ////}

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<ApplicationUser>(Input);
                ////var user = new ApplicationUser()
                ////{
                ////    UserName = Input.Email,
                ////    Email = Input.Email,
                ////    CompanyId = Input.CompanyId,
                ////    Name = Input.Name,
                ////    StreetAddress = Input.StreetAddress,
                ////    City = Input.City,
                ////    State = Input.State,
                ////    PostalCode = Input.PostalCode,
                ////    PhoneNumber = Input.PhoneNumber,
                ////    Role = Input.Role
                ////};
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await AddMissingRolesIfAny();

                    await AddUserRole(user);

                    ////var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    ////code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    ////var callbackUrl = Url.Page(
                    ////    "/Account/ConfirmEmail",
                    ////    pageHandler: null,
                    ////    values: new { area = "Identity", userId = user.Id, code = code },
                    ////    protocol: Request.Scheme);

                    ////await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    ////    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        if (user.Role == null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        {
                            //Admin is registering new user;
                            return RedirectToAction("Index", "User", new { Area = "Admin" });
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task AddUserRole(ApplicationUser user)
        {
            if (user.Role == null)
                await _userManager.AddToRoleAsync(user, SD.Roles.IndividualCustomer);
            else
            {
                if (user.CompanyId > 0)
                    await _userManager.AddToRoleAsync(user, SD.Roles.CompanyCustomer);

                await _userManager.AddToRoleAsync(user, user.Role);
            }
        }

        private async Task AddMissingRolesIfAny()
        {
            if (!await _roleManager.RoleExistsAsync(SD.Roles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Roles.Admin));
            }
            if (!await _roleManager.RoleExistsAsync(SD.Roles.Employee))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Roles.Employee));
            }
            if (!await _roleManager.RoleExistsAsync(SD.Roles.CompanyCustomer))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Roles.CompanyCustomer));
            }
            if (!await _roleManager.RoleExistsAsync(SD.Roles.IndividualCustomer))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Roles.IndividualCustomer));
            }
        }
    }
}
