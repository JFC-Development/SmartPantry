using SmartPantry.Despensa;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Applications.Despensa;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCorePantryItemAppService_Tests : PantryItemAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}