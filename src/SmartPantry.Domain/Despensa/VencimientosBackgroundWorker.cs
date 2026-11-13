using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.Despensa
{
    public class VencimientosBackgroundWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
{
    public VencimientosBackgroundWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
    {
        Timer.Period = 60 * 60000;
    }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            Logger.LogInformation("Iniciando verificación de vencimientos de la despensa...");

            try
            {
                var uowManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                var gestor = workerContext.ServiceProvider.GetRequiredService<GestorVencimientosDominioServicio>();

                using (var uow = uowManager.Begin(requiresNew: true, isTransactional: true))
                {
                    await gestor.ProcesarVencimientosAsync();
                    await uow.CompleteAsync();
                }

                Logger.LogInformation("Verificación de vencimientos finalizada.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Falló la verificación de vencimientos. Se reintentará en la próxima ejecución.");
            }
        }
    }
}