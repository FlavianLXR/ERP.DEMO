using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ERP.DEMO.Components.MVVM;
using ERP.DEMO.Components.Tools.DataGrid;
using ERP.DEMO.Models.DataAccessLayer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using MudBlazor;
using ERP.DEMO.Models.TestDb;
using Type = System.Type;

namespace ERP.DEMO.Components.MVVM
{
    public class UserService : BaseService
    {
        private UserPreferences UserPreferences { get; set; } = new();
        private readonly LoginService _login;
        private bool _initialized = false;
        private CancellationTokenSource _cts;

        public UserService(IDbContextFactory<TestDbContext> testService,
                           LoggerService logger,
                           LoginService login)
            : base(testService, logger)
        {
            _login = login;
        }

        private async Task SaveUserPreferencesAsync()
        {
            var user = _login.GetUser();
            if (user == null)
                return;

            using var db = CreateDb();

            var entity = await db.Users.FindAsync(user.Id);
            if (entity == null)
                return;

            entity.Preferences = UserPreferences.ToJson();

            await db.SaveChangesAsync();

            _login.UpdatePreferenceUser(entity.Preferences);
        }

        public User GetUser()
        {
            var user = _login.GetUser();

            if (user == null)
                throw new UnauthorizedAccessException("User not authenticated");

            if (!_initialized)
            {
                InitPreferences(user);
                _initialized = true;
            }

            return user;
        }

        public int GetUserId() => GetUser().Id;

        public UserPreferences GetUserPreferences()
        {
            return UserPreferences;
        }

        private void InitPreferences(User user)
        {
            UserPreferences.PropertyChanged -= async (sender, e) => await SaveUserPreferencesAsync();

            UserPreferences = string.IsNullOrEmpty(user.Preferences)
                ? new UserPreferences()
                : UserPreferences.FromJson(user.Preferences);

                UserPreferences.PropertyChanged += async (sender, e) => await SaveUserPreferencesAsync();
        }

        //private void OnPreferencesChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    DebouncedSave();
        //}

        //private void DebouncedSave()
        //{
        //    _cts?.Cancel();
        //    _cts = new CancellationTokenSource();

        //    _ = Task.Delay(500, _cts.Token)
        //        .ContinueWith(async t =>
        //        {
        //            if (!t.IsCanceled)
        //                await SaveUserPreferencesAsync();
        //        });
        //}

        //public async Task SetUser(User user)
        //{
        //    _user = user;

        //    if (!string.IsNullOrEmpty(user.Preferences))
        //        UserPreferences = UserPreferences.FromJson(user.Preferences);
        //    else
        //    {
        //        UserPreferences = new UserPreferences();
        //        await SaveUserPreferencesAsync();
        //    }

        //    UserPreferences.PropertyChanged += async (sender, e) => await SaveUserPreferencesAsync();
        //}

        public ObservableCollection<FilterView<object>> GetUserFilters(Type type = null)
        {
            if (type != null)
            {
                var method = typeof(Enumerable).GetMethod("OfType")?.MakeGenericMethod(type);
                if (method != null)
                {
                    var result = (IEnumerable<object>)method.Invoke(null, new object[] { UserPreferences.SavedFilterViews });
                    return new ObservableCollection<FilterView<object>>(result.OfType<FilterView<object>>());
                }
            }
            return UserPreferences.SavedFilterViews;
        }

        public void SetUserFilters(ObservableCollection<FilterView<object>> filterViews)
        {
            UserPreferences.SavedFilterViews = filterViews;
        }

        public bool GetIsDarkMode() => UserPreferences.IsDarkMode;

        public void SetIsDarkMode(bool value) => UserPreferences.IsDarkMode = value;

        public int GetDefaultPageSize() => UserPreferences.DefaultPageSize;

        public void SetDefaultPageSize(int value) => UserPreferences.DefaultPageSize = value;

        public int GetPageSizeByIndex(Type type)
        {
            return UserPreferences.PageSizeByIndex.TryGetValue(type.AssemblyQualifiedName, out int pageSize)
                ? (pageSize != 0 ? pageSize : GetDefaultPageSize())
                : GetDefaultPageSize();
        }

        public async Task SetPageSizeByIndex(Type type, int pageSize)
        {
            UserPreferences.PageSizeByIndex[type.AssemblyQualifiedName] = pageSize;
            await SaveUserPreferencesAsync();
        }

        public bool GetChartsVisibilityByIndex(Type type)
        {
            return UserPreferences.ChartsVisibilityByIndex.TryGetValue(type.AssemblyQualifiedName, out bool value)
                ? value
                : true;
        }

        public async Task SetChartsVisibilityByIndex(Type type, bool chartsVisibility)
        {
            UserPreferences.ChartsVisibilityByIndex[type.AssemblyQualifiedName] = chartsVisibility;
            await SaveUserPreferencesAsync();
        }

        public DataGridParameters GetDataGridParameters(Type type)
        {
            return UserPreferences.DataGridParametersByIndex.TryGetValue(type.AssemblyQualifiedName, out var value)
                ? value
                : new DataGridParameters();
        }

        public async Task SetDataGridParameters(Type type, DataGridParameters parameters)
        {
            UserPreferences.DataGridParametersByIndex[type.AssemblyQualifiedName] = parameters;
            await SaveUserPreferencesAsync();
        }
    }


    public partial class UserPreferences : ObservableObject
    {
        [ObservableProperty]
        private bool isDarkMode = false;

        [ObservableProperty]
        private int defaultPageSize = 10;

        [ObservableProperty]
        private Dictionary<string, int> pageSizeByIndex = new();

        [ObservableProperty]
        private Dictionary<string, bool> chartsVisibilityByIndex = new();

        [ObservableProperty]
        private Dictionary<string, DataGridParameters> dataGridParametersByIndex = new();

        [ObservableProperty]
        private ObservableCollection<FilterView<object>> savedFilterViews = new();

        public static UserPreferences FromJson(string json) =>
            JsonSerializer.Deserialize<UserPreferences>(json) ?? new UserPreferences();

        public string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
