using System.Collections.Concurrent; // For thread-safe collections

public class EventService {
    private readonly LocalStorageService _localStorageService; // Local storage service for storing user and event data
    private readonly UserService _userService; // User service for managing user data

    // Constructor to inject the local storage service and user service
    public EventService(LocalStorageService localStorageService, UserService userService) {
        _localStorageService = localStorageService;
        _userService = userService;
    }

    // Event creation method
    public async Task<bool> CreateEvent(EventModel newEvent) {
        var key = $"event_{newEvent.Name}";

        // Check if the event already exists
        if (await _localStorageService.GetItemAsync<EventModel>(key).ConfigureAwait(false) is not null) return false; // Event already exists

        // Retrieve the current user from local storage
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser is null) return false; // User not logged in

        newEvent.HostEmail = currentUser.Email; // Set the host email

        // Store event in local storage
        await _localStorageService.SetItemAsync(key, newEvent).ConfigureAwait(false);

        return true; // Successful event creation
    }

    public async Task<bool> UpdateEvent(EventModel updatedEvent) {
        var key = $"event_{updatedEvent.Name}";

        // Check if the event exists
        var existingEvent = await _localStorageService.GetItemAsync<EventModel>(key).ConfigureAwait(false);
        if (existingEvent is null) return false; // Event does not exist

        // Retrieve the current user from local storage
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser is null) return false; // User not logged in

        if (existingEvent.HostEmail != currentUser.Email) return false; // User is not the event host

        // Update the event in local storage
        await _localStorageService.SetItemAsync(key, updatedEvent).ConfigureAwait(false);

        return true; // Successful event update
    }

    public async Task<bool> DeleteEvent(string eventName) {
        var key = $"event_{eventName}";

        // Check if the event exists
        var eventToDelete = await _localStorageService.GetItemAsync<EventModel>(key).ConfigureAwait(false);
        if (eventToDelete is null) return false; // Event does not exist

        // Retrieve the current user from local storage
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser is null) return false; // User not logged in

        if (eventToDelete.HostEmail != currentUser.Email) return false; // User is not the event host

        // Remove the event from local storage
        await _localStorageService.RemoveItemAsync(key).ConfigureAwait(false);

        return true; // Successful event deletion
    }

    public async Task<bool> AddAttendeeAsync(string eventName, string attendeeEmail) {
        var key = $"event_{eventName}";

        // Check if the event exists
        var existingEvent = await _localStorageService.GetItemAsync<EventModel>(key).ConfigureAwait(false);
        if (existingEvent is null) return false; // Event does not exist

        // Retrieve the current user from local storage
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser is null) return false; // User not logged in

        // Check if the attendee is already attending the event
        if (existingEvent.Attendees.Any(a => a.Email == attendeeEmail)) return false; // Already attending

        existingEvent.Attendees.Add(currentUser); // Add the current user as an attendee
        await _localStorageService.SetItemAsync(key, existingEvent).ConfigureAwait(false); // Update the event in local storage

        return true; // Successful attendee addition
    }

    public async Task<bool> RemoveAttendeeAsync(string eventName, string attendeeEmail) {
        var key = $"event_{eventName}";

        // Check if the event exists
        var existingEvent = await _localStorageService.GetItemAsync<EventModel>(key).ConfigureAwait(false);
        if (existingEvent is null) return false; // Event does not exist

        // Retrieve the current user from local storage
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser is null) return false; // User not logged in

        // Check if the attendee is attending the event
        if (!existingEvent.Attendees.Any(a => a.Email == attendeeEmail)) return false; // Not attending

        existingEvent.Attendees.RemoveAll(a => a.Email == attendeeEmail); // Remove the attendee from the event
        await _localStorageService.SetItemAsync(key, existingEvent).ConfigureAwait(false); // Update the event in local storage

        return true; // Successful attendee removal
    }

    // Event retrieval methods
    public async Task<EventModel?> GetEventByNameAsync(string eventName) {
        return await _localStorageService.GetItemAsync<EventModel>($"event_{eventName}").ConfigureAwait(false);
    }

    public async Task<List<EventModel>> GetEventsByHostAsync(string hostEmail) {
        var events = await GetAllEventsAsync();
        return events.Where(e => e.HostEmail == hostEmail).ToList();
    }

    public async Task<List<EventModel>> GetEventsByAttendeeAsync(string attendeeEmail) {
        var events = await GetAllEventsAsync();
        return events.Where(e => e.Attendees.Any(a => a.Email == attendeeEmail)).ToList();
    }

    public async Task<List<EventModel>> GetAllEventsAsync() {
        var events = new ConcurrentBag<EventModel>(); // Thread-safe collection for storing event data
        var keys = await _localStorageService.GetKeysAsync("event_").ConfigureAwait(false); // Retrieve all keys with the prefix "event_"

        // Parallel processing of user data retrieval
        await Parallel.ForEachAsync(keys, async (key, _) => {
            var eventItem = await _localStorageService.GetItemAsync<EventModel>(key).ConfigureAwait(false); // Retrieve each event by key
            if (eventItem is not null) events.Add(eventItem);
        });

        return events.ToList(); // Return all events
    }
}