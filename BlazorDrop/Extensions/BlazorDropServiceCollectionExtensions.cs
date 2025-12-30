using BlazorDrop.Interfaces;
using BlazorDrop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDrop.Extensions
{
	public static class BlazorDropServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazorDrop(this IServiceCollection services)
		{
			services.AddScoped<IBlazorDropInteropService, BlazorDropInteropService>();

			return services;
		}
	}
}
