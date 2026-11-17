import { ChangeDetectionStrategy, Component, computed, inject, Injector } from '@angular/core';
import { DatePipe } from '@angular/common';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { ListService, LocalizationPipe, PagedResultDto } from '@abp/ng.core';
import { PantryItemService, PantryItemDto } from '../proxy/despensa';

@Component({
  selector: 'app-pantry',
  templateUrl: './pantry.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, NgbPaginationModule, LocalizationPipe],
  providers: [ListService],
})
export class PantryComponent {
  public readonly list = inject(ListService);
  private readonly injector = inject(Injector);
  private readonly pantryService = inject(PantryItemService);

  private readonly query = toSignal(this.list.query$, {
    initialValue: { skipCount: 0, maxResultCount: 10, sorting: '' },
    injector: this.injector,
  });

  readonly resource = rxResource<
    PagedResultDto<PantryItemDto>,
    { skipCount: number; maxResultCount: number; sorting: string }
  >({
    params: () => this.query(),
    stream: ({ params }) => this.pantryService.getList(params as any, { skipHandleError: true }),
    defaultValue: { items: [], totalCount: 0 },
    injector: this.injector,
  });

  readonly isLoading = computed(() => {
    const status = this.resource.status();
    return status === 'loading' || status === 'reloading';
  });
  readonly hasError = computed(() => this.resource.status() === 'error');
  readonly items = computed(() => (this.resource.hasValue() ? this.resource.value().items ?? [] : []));
  readonly totalCount = computed(() => (this.resource.hasValue() ? this.resource.value().totalCount ?? 0 : 0));
  readonly isEmpty = computed(() => this.resource.status() === 'resolved' && this.items().length === 0);

  retry() {
    this.list.get();
  }

  readonly pageSize = computed(() => this.list.maxResultCount ?? 10);

  get currentPage(): number {
    return (this.list.page ?? 0) + 1;
  }

  onPageChange(page: number) {
    this.list.page = page - 1;
  }
}