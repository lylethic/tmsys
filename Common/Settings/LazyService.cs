namespace server.Common.Settings;

public class LazyService<T> : Lazy<T> where T : class
{
    public LazyService(IServiceProvider serviceProvider)
        : base(() => serviceProvider.GetRequiredService<T>())
    {
    }
}
