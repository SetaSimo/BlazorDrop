using BlazorDrop.Interfaces;
using BlazorDrop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDrop.Extensions
{
	public static class BlazorDropServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazorDrop(this IServiceCollection services)
		{
			services.AddScoped<IBlazorDropScrollInteropService, BlazorDropScrollInteropService>();
			services.AddScoped<IBlazorDropClickOutsideService, BlazorDropClickOutsideService>();
			services.AddScoped<IBlazorDropInputInteropService, BlazorDropInputInteropService>();

			return services;
		}
	}
}
