using System.Globalization;
using System.Runtime.CompilerServices;

// La lingua scelta dall'utente è globale: i test che la cambiano non devono sovrapporsi agli altri
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace LaunchPlanner.Tests;

// I test confrontano i testi in italiano: non devono dipendere dalla lingua del computer che li esegue
internal static class TestCulture
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        var italian = CultureInfo.GetCultureInfo("it-IT");
        CultureInfo.DefaultThreadCurrentCulture = italian;
        CultureInfo.DefaultThreadCurrentUICulture = italian;
    }
}
