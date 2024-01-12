namespace OnMyRoute.Extensions.Hosting;

internal class Factory<TService, TImplementation>(Func<TImplementation> func) :
    IFactory<TService> where TImplementation : TService {
    public TService Create() => func();
}
