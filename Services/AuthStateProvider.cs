using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class AuthStateProvider : AuthenticationStateProvider {
    private readonly LocalStorageService _localStorageService;  // Local storage service for storing user and event data

    // Constructor to inject the local storage service
    public AuthStateProvider(LocalStorageService localStorageService) {
        _localStorageService = localStorageService;
    }

    // Method to get the current authentication state
    public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
        // Retrieve the current user from local storage
        var currentUser = await _localStorageService.GetItemAsync<UserModel>("currentUser");

        // Create a ClaimsIdentity for the current user
        var identity = currentUser is not null
            ? new ClaimsIdentity([
                new Claim(ClaimTypes.Name, currentUser.Email),
                new Claim(ClaimTypes.Role, "User")
            ], "SessionAuth")
            : new ClaimsIdentity();

        var principal = new ClaimsPrincipal(identity); // Create a ClaimsPrincipal based on the ClaimsIdentity
        return new AuthenticationState(principal); // Return the authentication state
    }

    // Method to update the authentication state
    public void UpdateAuthState() {
        // Get the current authentication state and notify the authentication state provider
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
