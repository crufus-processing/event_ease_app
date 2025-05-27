using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Concurrent; // For thread-safe collections

public class UserService {
    private readonly AuthenticationStateProvider _authStateProvider; // Authentication state provider for managing user authentication state
    private readonly LocalStorageService _localStorageService;  // Local storage service for storing user and event data

    // Constructor to inject the authentication state provider and local storage service
    public UserService(AuthenticationStateProvider authStateProvider, LocalStorageService localStorageService) {
        _authStateProvider = authStateProvider;
        _localStorageService = localStorageService;
    }

    // User registration method
    public async Task<bool> Signup(UserModel user) {
        // Check if the email is already registered
        if (await _localStorageService.GetItemAsync<UserModel>($"user_{user.Email}").ConfigureAwait(false) is not null) return false; // Email already exists

        // Secure password hashing
        string passwordInput = user.Password;
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, workFactor: 4);

        // Save individual user entry in local storage
        await _localStorageService.SetItemAsync($"user_{user.Email}", user).ConfigureAwait(false);

        // Store the authenticated user in local storage
        await _localStorageService.SetItemAsync("currentUser", user).ConfigureAwait(false);

        // Update the authentication state
        if (_authStateProvider is AuthStateProvider authProvider) authProvider.UpdateAuthState();
        
        return true; // Registration successful
    }

    // User login method
    public async Task<bool> LoginAsync(string email, string password) {
        // Retrieve the user
        var user = await _localStorageService.GetItemAsync<UserModel>($"user_{email}").ConfigureAwait(false);

        // Validate credentials
        if (user?.Password is null || string.IsNullOrWhiteSpace(user.Password) || !BCrypt.Net.BCrypt.Verify(password, user.Password)) return false;

        // Store the authenticated user in local storage
        await _localStorageService.SetItemAsync("currentUser", user).ConfigureAwait(false);

        // Update the authentication state
        if (_authStateProvider is AuthStateProvider authProvider) authProvider.UpdateAuthState();

        return true; // Login successful
    }

    // User logout method
    public async Task<bool> LogoutAsync() {
        // Remove authenticated user from local storage
        await _localStorageService.RemoveItemAsync("currentUser").ConfigureAwait(false);

        // Update the authentication state
        if (_authStateProvider is AuthStateProvider authProvider) authProvider.UpdateAuthState();

        return true; // Logout successful
    }

    // User retrieval methods
    public async Task<UserModel?> GetCurrentUserAsync() {
        return await _localStorageService.GetItemAsync<UserModel>("currentUser").ConfigureAwait(false);
    }
    
    public async Task<UserModel?> GetUserByEmailAsync(string email) {
        return await _localStorageService.GetItemAsync<UserModel>($"user_{email}").ConfigureAwait(false);
    }

    public async Task<List<UserModel>> GetAllUsersAsync() {
        var users = new ConcurrentBag<UserModel>(); // Thread-safe collection for storing user data
        var keys = await _localStorageService.GetKeysAsync("user_").ConfigureAwait(false); // Retrieve all keys with the prefix "user_"

        // Parallel processing of user data retrieval
        await Parallel.ForEachAsync(keys, async (key, _) => {
            var user = await _localStorageService.GetItemAsync<UserModel>(key).ConfigureAwait(false); // Retrieve each user by key
            if (user is not null) users.Add(user);
        });

        return users.ToList(); // Return all users
    }
}

