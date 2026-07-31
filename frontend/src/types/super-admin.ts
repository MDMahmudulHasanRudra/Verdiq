// Super Admin model types

export interface SuperAdminChamber {
  id: string;
  name: string;
  logo: string | null;
  address: string | null;
  phone: string | null;
  subscriptionPlan: string;
  subscriptionStatus: string;
  usersCount: number;
  casesCount: number;
  clientsCount: number;
  totalRevenue: number;
  createdAt: string;
  isActive: boolean;
  documentsCount: number;
  hearingsCount: number;
  invoicesCount: number;
}

export interface SuperAdminUser {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  isActive: boolean;
  chamberId: string;
  chamberName: string;
  createdAt: string;
  subscriptionPlan: string;
  subscriptionStatus: string;
  subscriptionEnd: string | null;
}

export interface SuperAdminSubscription {
  id: string;
  chamberId: string;
  chamberName: string;
  plan: string;
  status: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  cancelAtPeriodEnd: boolean;
  userFullName: string;
}

export interface SuperAdminDashboard {
  totalChambers: number;
  totalUsers: number;
  totalCases: number;
  totalClients: number;
  activeSubscriptions: number;
  monthlyRevenue: number;
  totalRevenueAllTime: number;
  newChambersThisMonth: number;
  newCasesThisMonth: number;
  newUsersThisMonth: number;
  expiredSubscriptions: number;
  totalDocuments: number;
  totalHearings: number;
  totalPayments: number;
  chambers: SuperAdminChamber[];
  alerts: string[];
}

export interface Permission {
  id: string;
  name: string;
  description: string;
  module: string;
}

export interface RolePermissions {
  role: string;
  permissions: Permission[];
}

export interface AuditLog {
  id: string;
  userId: string;
  userName: string;
  action: string;
  entity: string;
  entityId: string;
  oldValues: string | null;
  newValues: string | null;
  changes: { field: string; oldValue: string; newValue: string }[];
  ipAddress: string;
  createdAt: string;
  actionLabel: string;
}

export interface SystemConfig {
  allowSelfRegistration: boolean;
  maintenanceMode: boolean;
  trialDays: number;
  maxLoginAttempts: number;
  requireEmailVerification: boolean;
  enableAiFeatures: boolean;
  defaultCurrency: string;
}

export interface SystemHealth {
  status: string;
  databaseStatus: string;
  databaseSizeBytes: number;
  activeConnections: number;
  totalChambers: number;
  totalUsers: number;
  totalCases: number;
  activeSubscriptions: number;
  monthlyRevenue: number;
  storageUsedBytes: number;
  uptime: string;
  lastBackup: string | null;
  activeAlerts: string[];
}

export interface AdminCase {
  id: string;
  caseNumber: string;
  title: string;
  caseType: string;
  status: string;
  courtName: string;
  assignedLawyerName: string;
  filingDate: string;
  createdAt: string;
}
