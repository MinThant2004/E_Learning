using Blazored.LocalStorage;
using ELearningManagementSystem.App;
using ELearningManagementSystem.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Authentication State
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ClientPermissionPolicyProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>(sp =>
    (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

// Register AuthHttpHandler
builder.Services.AddTransient<AuthHttpHandler>();

// Register Feature API Clients
builder.Services.AddScoped<CourseApiClient>();
builder.Services.AddScoped<CategoryApiClient>();
builder.Services.AddScoped<LessonApiClient>();
builder.Services.AddScoped<EnrollmentApiClient>();
builder.Services.AddScoped<LessonProgressApiClient>();
builder.Services.AddScoped<QuizApiClient>();
builder.Services.AddScoped<QuizAttemptApiClient>();
builder.Services.AddScoped<StudentDashboardApiClient>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";

// Unauthenticated HttpClient for AuthApiService to prevent circular dependency
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddScoped<AuthApiService>(sp => 
    new AuthApiService(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthApi"),
        sp.GetRequiredService<ILocalStorageService>(),
        sp.GetRequiredService<CustomAuthStateProvider>()
    ));

// Authenticated HttpClient for API communication
builder.Services.AddHttpClient("ELearningApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthHttpHandler>();

// Default HttpClient (scoped) uses the authenticated client
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ELearningApi"));

await builder.Build().RunAsync();
