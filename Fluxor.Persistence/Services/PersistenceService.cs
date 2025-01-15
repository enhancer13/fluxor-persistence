using Blazored.LocalStorage;

namespace Fluxor.Persistence.Services;

internal sealed class PersistenceService : IPersistenceService
{
    private readonly ILocalStorageService _LocalStorage;

    public PersistenceService(ILocalStorageService localStorage)
    {
        _LocalStorage = localStorage;
    }

    public ValueTask<string?> GetItemAsStringAsync(string storageKey) => _LocalStorage.GetItemAsStringAsync(storageKey);

    public ValueTask SetItemAsStringAsync(string storageKey, string value) => _LocalStorage.SetItemAsStringAsync(storageKey, value);
}
