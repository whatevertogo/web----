export interface ApiResponse<T> {
  data: T;
  status: number;
  message: string;
}

export interface PaginationParams {
  page: number;
  pageSize: number;
}

export interface SearchParams {
  keyword?: string;
  filters?: Record<string, any>;
  sort?: {
    field: string;
    order: 'asc' | 'desc';
  };
} 