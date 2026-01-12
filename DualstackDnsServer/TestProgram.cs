#if INTEGRATION_TESTS
namespace DualstackDnsServer
{
    // Used only for integration test host startup
    // SonarQube S2094: This class must be empty for WebApplicationFactory<T> compatibility with static Program.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarQube", "S2094", Justification = "Required for integration test host startup; see WebApplicationFactory<T> pattern.")]
    public class TestProgram { }
}
#endif