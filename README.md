## SmartPantry
Archivo inicial de README
# <SmartPantry>

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026.

## Integrantes
- Juan Manuel De Agostini - Juanmadee
- Constanzo De Zan - constanzodz
- Facundo Gentile - Fakuun


# SmartPantry

## About this solution

This is a layered startup solution based on [Domain Driven Design (DDD)](https://abp.io/docs/latest/framework/architecture/domain-driven-design) practises. All the fundamental ABP modules are already installed. Check the [Application Startup Template](https://abp.io/docs/latest/solution-templates/layered-web-application) documentation for more info.

### Pre-requirements

* [.NET10.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)

### Configurations

The solution comes with a default configuration that works out of the box. However, you may consider to change the following configuration before running your solution:

* Check the `ConnectionStrings` in `appsettings.json` files under the `SmartPantry.HttpApi.Host` and `SmartPantry.DbMigrator` projects and change it if you need.

### Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies. This step is automatically done when you create a new solution, if you didn't especially disabled it. However, you should run it yourself if you have first cloned this solution from your source control, or added a new client-side package dependency to your solution.
* Run `SmartPantry.DbMigrator` to create the initial database. This step is also automatically done when you create a new solution, if you didn't especially disabled it. This should be done in the first run. It is also needed if a new database migration is added to the solution later.

## Para correr, usar la terminal de VSCode o un powershell, parado en la carpeta de origen del repo:

dotnet run --project .\src\SmartPantry.DbMigrator 
dotnet run --project .\src\SmartPantry.HttpApi.Host 
Push-Location .\angular 
yarn start 

* En caso de no dejarte, ejecutar:

cd ./src/SmartPantry.DbMigrator
dotnet run
cd ./src/SmartPantry.HttpApi.Host
dotnet run
Push-Location .\angular
yarn install
yarn start

## Pantalla "Mi despensa" (TP10)

Pantalla Angular (RF-11, con RF-09 para datos faltantes) que lista los ítems de despensa del usuario logueado y permite editarlos, marcarlos como consumidos y eliminarlos.

- **Ruta del componente:** `angular/src/app/pantry`
- **Acceso a la API:** proxies generados por ABP (`angular/src/app/proxy/despensa`), no hay llamadas HTTP escritas a mano.
- **Edición:** formulario reactivo con validación de cantidad (no negativa), unidad (máx. 20 caracteres) y fecha de vencimiento.
- **Eliminar:** pide confirmación antes de llamar al servidor.
- **Localización:** textos en español e inglés mediante claves `::Pantry:...`.
- **Seguridad:** el BackEnd solo permite operar sobre ítems propios; un ítem ajeno responde 404.

### Cómo probarla

1. Levantar el DbMigrator, el Host y Angular como se indica arriba.
2. Entrar a `http://localhost:4200` e iniciar sesión.
3. Abrir "Mi despensa" desde el menú.

### Regenerar los proxies

Con el Host corriendo, desde la carpeta `angular`:

    abp generate-proxy -t ng -u https://localhost:44369

### Pruebas

Desde la carpeta `angular`:

    yarn test

Desde la raíz del repo, para las pruebas del BackEnd:

    dotnet test
