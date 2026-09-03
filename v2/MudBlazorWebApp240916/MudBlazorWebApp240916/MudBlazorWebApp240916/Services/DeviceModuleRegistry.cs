using System.Text.Json;
using MudBlazorWebApp240916.Shared.DataModel;

namespace MudBlazorWebApp240916.Services;

public sealed class DeviceModuleRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly object _sync = new();
    private readonly string _storagePath;
    private readonly List<DeviceModule> _modules;

    public DeviceModuleRegistry(IHostEnvironment environment, ILogger<DeviceModuleRegistry> logger)
    {
        _storagePath = Path.Combine(environment.ContentRootPath, "App_Data", "device-modules.json");
        try
        {
            _modules = File.Exists(_storagePath)
                ? JsonSerializer.Deserialize<List<DeviceModule>>(File.ReadAllText(_storagePath), JsonOptions) ?? []
                : [];
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            logger.LogWarning(exception, "Device module registry could not be loaded from {Path}", _storagePath);
            _modules = [];
        }
    }

    public IReadOnlyCollection<DeviceModule> GetAll()
    {
        lock (_sync) return _modules.OrderBy(module => module.CreatedAt).ToArray();
    }

    public DeviceModule Add(DeviceModule module)
    {
        lock (_sync)
        {
            module.Id = module.Id == Guid.Empty ? Guid.NewGuid() : module.Id;
            module.CreatedAt = DateTimeOffset.UtcNow;
            _modules.RemoveAll(existing => existing.Id == module.Id);
            _modules.Add(module);
            Save();
            return module;
        }
    }

    public bool Remove(Guid id)
    {
        lock (_sync)
        {
            var removed = _modules.RemoveAll(module => module.Id == id) > 0;
            if (removed) Save();
            return removed;
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_storagePath)!);
        var temporaryPath = _storagePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(_modules, JsonOptions));
        File.Move(temporaryPath, _storagePath, true);
    }
}
