using Microsoft.JSInterop;
using System.Text.Json;

public class LocalStorageService {
    private readonly IJSRuntime _jsRuntime; // Injected JS runtime
    
    // Constructor to inject the JS runtime
    public LocalStorageService(IJSRuntime jsRuntime) {
        _jsRuntime = jsRuntime;
    }
    
    // Set an item in local storage
    public async Task SetItemAsync<T>(string key, T value) {
        var json = JsonSerializer.Serialize(value); // Serialize the value to JSON
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json); // Store the JSON string in local storage
    }

    // Get an item from local storage
    public async Task<T?> GetItemAsync<T>(string key) {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key); // Retrieve the JSON string from local storage
        return json != null ? JsonSerializer.Deserialize<T>(json) : default; // Deserialize the JSON string back to the original type
    }

    // Remove an item from local storage
    public async Task RemoveItemAsync(string key) {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key); // Remove the item from local storage
    }
    
    // Get keys from local storage
    public async Task<List<string>> GetKeysAsync(string prefix) {
        return await _jsRuntime.InvokeAsync<List<string>>("getFilteredLocalStorageKeys", prefix); // Get keys with the specified prefix
    }
}
