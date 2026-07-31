export interface ApiResponse<T = unknown> {
  success: boolean;
  message: string;
  data: T | null;
  errors?: string[] | null;
}

export interface PagedResponse<T = unknown> {
  success: boolean;
  message: string;
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PageParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  barCouncilId: string | null;
  avatarUrl: string | null;
  chamberId: string;
  chamberName: string;
  modules: string[];
}

export interface AuthResponse {
  success: boolean;
  message: string;
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
}

export interface Chamber {
  id: string;
  name: string;
  logo: string | null;
  address: string | null;
  phone: string | null;
  subscriptionPlan: string;
  usersCount: number;
  casesCount: number;
  clientsCount: number;
  createdAt: string;
}
