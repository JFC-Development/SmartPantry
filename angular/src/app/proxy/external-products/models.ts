import type { ExternalProductStatus } from './external-product-status.enum';

export interface ExternalProductResultDto {
  status?: ExternalProductStatus;
  barcode?: string | null;
  name?: string | null;
  brand?: string | null;
  quantity?: string | null;
}

export interface GetExternalProductInput {
  barcode: string;
}
