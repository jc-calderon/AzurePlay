using AzurePlay.Common.Models;
using AzurePlay.Common.Services;
using Core.Azure.KeyVault;
using Core.DI;
using Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzurePlay.Web
{
	public class Startup
	{
		private IAzureKeyVaultService _azureKeyVaultService;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			this.ConfigureAzureKeyVault();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddSingleton<IDataAccessService>(x => new DataAccessService(_azureKeyVaultService.GetSecret(AzureKeyVaultKeys.DbConnectionString)));
			services.RegisterModule(new LoggingModule(new LogglySettings { LogglyCustomerToken = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.LogglyCustomerToken), LogglyTag = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.LogglyTag) }));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.UseHttpsRedirection();
			app.UseMvc();
		}

		private void ConfigureAzureKeyVault()
		{
			var azureKeyVaultSettings = new AzureKeyVaultSettings
			{
				VaultBaseUrl = this.Configuration.GetValue<string>("KeyVaultBaseUrl"),
				ApplicationId = this.Configuration.GetValue<string>("KeyVaultApplicationId"),
				ClientSecret = this.Configuration.GetValue<string>("KeyVaultClientSecret"),
				CertificateThumbprint = this.Configuration.GetValue<string>("KeyVaultCertificateThumbprint")
			};

			_azureKeyVaultService = new AzureKeyVaultService(azureKeyVaultSettings);
		}
	}
}