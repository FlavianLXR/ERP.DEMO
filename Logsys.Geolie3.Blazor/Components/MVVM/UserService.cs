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
        private User? User;
        private LoginService? _login;
        private readonly IServiceProvider _provider;

        public UserService(GenericService<TestDbContext> testService,
                           LoggerService logger,
                           IServiceProvider provider)
            : base(testService, logger)
        {
            _provider = provider;
        }

        private LoginService Login => _login ??= _provider.GetRequiredService<LoginService>();

        private async Task SaveUserPreferencesAsync()
        {
            if (User == null)
                return;

            var jsonPreferences = UserPreferences.ToJson();
            User.Preferences = jsonPreferences;

            var dbContext = TestDbContext.GetDbContext();

            await TestDbContext.ExecuteInTransactionAsync(async () =>
            {
                dbContext.Update(User);
                await dbContext.SaveChangesAsync();
            });

            Login?.UpdatePreferenceUser(User.Preferences);
        }

        public User? GetUser()
        {
            return Login.GetUser();
        }

        public int GetUserId() => GetUser().Id;

        public UserPreferences GetUserPreferences()
        {
            return UserPreferences;
        }

        public async Task SetUser(User user)
        {
            User = user;

            if (!string.IsNullOrEmpty(user.Preferences))
                UserPreferences = UserPreferences.FromJson(user.Preferences);
            else
            {
                UserPreferences = new UserPreferences();
                await SaveUserPreferencesAsync();
            }

            UserPreferences.PropertyChanged += async (sender, e) => await SaveUserPreferencesAsync();
        }

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
