using ERP.DEMO.Models.TestDb;
using ERP.DEMO.Models.DataAccessLayer;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ERP.DEMO.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ERP.DEMO.Components.MVVM
{
    public class LoginService : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly LoggerService _logger;
        private readonly IDbContextFactory<TestDbContext> _dbFactory;

        public string username;
        public string password;
        public string errorMessage;
        public bool isAuthenticated = false;

        public User? CurrentUser { get; private set; }

        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public LoginService(IDbContextFactory<TestDbContext> dbFactory, ProtectedLocalStorage localStorage, LoggerService logger)
        {
            _dbFactory = dbFactory;
            _localStorage = localStorage;
            _logger = logger;

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task SetAuthenticationStateAsync(string key)
        {
            await _localStorage.SetAsync("authUser", key);
            await _localStorage.SetAsync("authDate", DateTime.Now.ToString());

            if (int.TryParse(key, out int clientId))
            {
                using var db = _dbFactory.CreateDbContext();

                var clt = await db.Users.FirstOrDefaultAsync(x => x.Id == clientId);

                if (clt != null)
                {
                    SetClaimsPrincipal(clt.Username);
                    CurrentUser = clt;
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }
                else
                {
                    // ← Utilisateur non trouvé, on nettoie le localStorage
                    await Logout();
                }
            }
            else
            {
                // ← Utilisateur non trouvé, on nettoie le localStorage
                await Logout();
            }
        }

        public async Task Login()
        {
            if (isAuthenticated) return;

            using var db = _dbFactory.CreateDbContext();

            var user = await db.Users
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                CurrentUser = user;
                await SetAuthState(user.Username, user.Id.ToString());
                return;
            }

            errorMessage = "Nom d'utilisateur ou mot de passe incorrect !";
        }

        private async Task SetAuthState(string username, string key)
        {
            SetClaimsPrincipal(username);
            await _localStorage.SetAsync("authUser", key);
            await _localStorage.SetAsync("authDate", DateTime.Now.ToString());

            this.username = null;
            this.password = null;
            this.errorMessage = null;
            this.isAuthenticated = true;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private void SetClaimsPrincipal(string username)
        {
            var identity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, username),
        }, "authUser");

            _currentUser = new ClaimsPrincipal(identity);
        }

        public async Task Logout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            await _localStorage.DeleteAsync("authUser");
            await _localStorage.DeleteAsync("authDate");

            username = null;
            password = null;
            errorMessage = null;
            CurrentUser = null;

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public User? GetUser() => CurrentUser;

        public string? GetUserName() => CurrentUser?.Username;

        public string? GetFirstName() => CurrentUser?.FirstName;

        public void UpdatePreferenceUser(string preference)
        {
            if (CurrentUser != null)
                CurrentUser.Preferences = preference;
        }
    }

}
