using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace SmartPantry.Despensa
{
    public class VencimientosBackgroundWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public VencimientosBackgroundWorker(
            AbpTimer timer,
            IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 60000 * 60; 
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override void DoWork(PeriodicBackgroundWorkerContext workerContext)
        {
            Logger.LogInformation("Iniciando verificación automática de vencimientos de la despensa...");

            AsyncHelper.RunSync(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var gestorVencimientos = scope.ServiceProvider.GetRequiredService<GestorVencimientosDominioServicio>();
                await gestorVencimientos.ProcesarVencimientosAsync();
            });

            Logger.LogInformation("Verificación automática de vencimientos finalizada con éxito.");
        }
    }
}