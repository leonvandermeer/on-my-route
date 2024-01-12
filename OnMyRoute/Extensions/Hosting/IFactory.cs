namespace OnMyRoute.Extensions.Hosting;

internal interface IFactory<TImplementation> {
    TImplementation Create();
}
