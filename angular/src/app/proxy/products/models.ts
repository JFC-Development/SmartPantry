import type { EntityDto } from '@abp/ng.core';

export interface CreateProductDto {
  codigoBarras: string;
  nombre: string;
  marca?: string | null;
  ingredientes?: string | null;
  alergenos?: string | null;
}

export interface ProductDto extends EntityDto<string> {
  codigoBarras?: string;
  nombre?: string;
  marca?: string | null;
  ingredientes?: string | null;
  alergenos?: string | null;
}

export interface UpdateProductDto {
  codigoBarras: string;
  nombre: string;
  marca?: string | null;
  ingredientes?: string | null;
  alergenos?: string | null;
}
