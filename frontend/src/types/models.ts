// Domain model types mapped 1:1 from the Verdiq backend DTOs (camelCase JSON).

// ---- Cases ----
export interface CaseClient {
  id: string;
  name: string;
  phone: string;
  role: string;
}

export interface CaseProcedure {
  id: string;
  legalProcedureId: string;
  procedureTitle: string;
  stepNumber: number;
  description: string | null;
  requiredDocuments: string | null;
  recommendedTimeline: string | null;
  responsibleRole: string | null;
  isMandatory: boolean;
  isCompleted: boolean;
  completedAt: string | null;
  completedBy: string | null;
  notes: string | null;
}

export interface CaseLegalSection {
  id: string;
  legalSectionId: string;
  sectionCode: string;
  sectionTitle: string;
  lawName: string;
  procedures: CaseProcedure[];
}

export interface Case {
  id: string;
  caseNumber: string;
  title: string;
  courtName: string;
  caseType: string;
  filingDate: string;
  opponent: string | null;
  status: string;
  priority: string;
  description: string | null;
  actsAndSections: string | null;
  closingDate: string | null;
  assignedLawyerId: string | null;
  assignedLawyerName: string | null;
  teamId: string | null;
  teamName: string | null;
  clients: CaseClient[];
  hearingsCount: number;
  documentsCount: number;
  createdAt: string;
  firNumber: string | null;
  policeStation: string | null;
  gdNumber: string | null;
  judgeName: string | null;
  bench: string | null;
  prosecutor: string | null;
  opposingLawyer: string | null;
  jurisdiction: string | null;
  appealStatus: string | null;
  riskLevel: string | null;
  complexityScore: number | null;
  practiceArea: string | null;
  department: string | null;
  internalNotes: string | null;
  retainerAmount: number | null;
  billingMethod: string | null;
  fixedFee: number | null;
  hourlyRate: number | null;
  budgetLimit: number | null;
  expenseBudget: number | null;
  nextHearingDate: string | null;
  lastHearingDate: string | null;
  lastHearingResult: string | null;
  criticalDeadlines: string | null;
  limitationExpiry: string | null;
  legalSections: CaseLegalSection[];
}

export interface CaseActivity {
  id: string;
  caseId: string;
  activityType: string;
  description: string;
  createdBy: string;
  createdByName?: string;
  createdAt: string;
  isClientVisible?: boolean;
}

export interface Judgment {
  id: string;
  caseId: string;
  caption: string;
  summary: string | null;
  result: string | null;
  judgmentDate: string;
  nextHearingDate: string | null;
  keyFindings: string | null;
  fileName: string | null;
  originalFileName: string | null;
  fileType: string | null;
  fileSize: number | null;
  hasDocument: boolean;
  recordedByName: string | null;
  createdAt: string;
}

export interface CreateJudgmentInput {
  caption: string;
  summary?: string | null;
  result?: string | null;
  judgmentDate?: string | null;
  nextHearingDate?: string | null;
  keyFindings?: string | null;
}

export interface CasePhoto {
  id: string;
  caseId: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  caption: string | null;
  capturedAt: string;
  uploadedByName: string | null;
  createdAt: string;
}

export interface ConfirmCaseDeleteInput {
  email: string;
  password: string;
}

export interface CreateCaseInput {
  title: string;
  caseNumber?: string;
  courtName: string;
  caseType: string;
  filingDate: string;
  opponent?: string | null;
  priority?: string | null;
  description?: string | null;
  actsAndSections?: string | null;
  firNumber?: string | null;
  policeStation?: string | null;
  assignedLawyerId?: string | null;
  teamId?: string | null;
  clientIds?: string[];
  gdNumber?: string | null;
  judgeName?: string | null;
  bench?: string | null;
  prosecutor?: string | null;
  opposingLawyer?: string | null;
  jurisdiction?: string | null;
  appealStatus?: string | null;
  riskLevel?: string | null;
  complexityScore?: number | null;
  practiceArea?: string | null;
  department?: string | null;
  internalNotes?: string | null;
  retainerAmount?: number | null;
  billingMethod?: string | null;
  fixedFee?: number | null;
  hourlyRate?: number | null;
  budgetLimit?: number | null;
  expenseBudget?: number | null;
  nextHearingDate?: string | null;
  criticalDeadlines?: string | null;
  limitationExpiry?: string | null;
  legalSectionIds?: string[];
  clientRoles?: { clientId: string; role: string }[];
}

export type UpdateCaseInput = Partial<CreateCaseInput> & { status?: string | null };

// ---- Clients ----
export interface Client {
  id: string;
  name: string;
  phone: string;
  email: string;
  address: string | null;
  nid: string | null;
  companyName: string | null;
  notes: string | null;
  isActive: boolean;
  casesCount: number;
  createdAt: string;
  clientType: string | null;
  clientCode: string | null;
  passportNumber: string | null;
  dateOfBirth: string | null;
  gender: string | null;
  occupation: string | null;
  nationality: string | null;
  tradeLicense: string | null;
  registrationNumber: string | null;
  taxVatNumber: string | null;
  authorizedRepresentative: string | null;
  tags: string | null;
  riskLevel: string | null;
  clientCategory: string | null;
  billingPreference: string | null;
  paymentTerms: string | null;
  creditLimit: number | null;
  preferredContactMethod: string | null;
  whatsappNumber: string | null;
  secondaryPhone: string | null;
  emergencyContact: string | null;
  isBlacklisted: boolean;
  avatarUrl: string | null;
}

export interface CreateClientInput {
  name: string;
  phone: string;
  email: string;
  address?: string | null;
  nid?: string | null;
  companyName?: string | null;
  notes?: string | null;
  clientType?: string | null;
  passportNumber?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  occupation?: string | null;
  nationality?: string | null;
  tradeLicense?: string | null;
  registrationNumber?: string | null;
  taxVatNumber?: string | null;
  authorizedRepresentative?: string | null;
  tags?: string | null;
  riskLevel?: string | null;
  clientCategory?: string | null;
  billingPreference?: string | null;
  paymentTerms?: string | null;
  creditLimit?: number | null;
  preferredContactMethod?: string | null;
  whatsappNumber?: string | null;
  secondaryPhone?: string | null;
  emergencyContact?: string | null;
  avatarUrl?: string | null;
}

// ---- Hearings ----
export interface Hearing {
  id: string;
  caseId: string;
  caseNumber: string;
  caseTitle: string;
  hearingDate: string;
  courtroom: string | null;
  judgeName: string | null;
  result: string | null;
  nextHearingDate: string | null;
  status: string;
  notes: string | null;
  createdAt: string;
  hasIncompletePreHearingTasks: boolean;
  hasPreHearingTasks: boolean;
}

// ---- Tasks ----
export interface TaskComment {
  id: string;
  content: string;
  userId: string;
  userName: string;
  userAvatar: string | null;
  createdAt: string;
}

export interface TaskAttachment {
  id: string;
  fileName: string;
  originalFileName: string;
  fileType: string;
  fileSize: number;
  uploadedByName: string;
  createdAt: string;
}

export interface Task {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  status: string;
  priority: string | null;
  assignedTo: string;
  assignedToName: string;
  assignedByName: string;
  caseId: string | null;
  caseTitle: string | null;
  hearingId: string | null;
  isPreHearing: boolean;
  createdAt: string;
  sortOrder: number;
  isRecurring: boolean;
  recurrencePattern: string | null;
  recurrenceInterval: number | null;
  completedAt: string | null;
  estimatedHours: number | null;
  actualHours: number | null;
  commentCount: number;
  attachmentCount: number;
  comments?: TaskComment[];
  attachments?: TaskAttachment[];
  watcherIds: string[];
}

// ---- Documents ----
export interface DocumentVersion {
  id: string;
  versionNumber: number;
  fileName: string;
  originalFileName: string;
  fileType: string;
  fileSize: number;
  status: string;
  changeNotes: string | null;
  uploadedByName: string;
  createdAt: string;
}

export interface DocumentShare {
  id: string;
  sharedWithUserId: string;
  sharedWithUserName: string;
  permissions: string;
  createdAt: string;
}

export interface DocumentComment {
  id: string;
  content: string;
  userId: string;
  userName: string;
  userAvatar: string | null;
  createdAt: string;
  parentCommentId: string | null;
  replies: DocumentComment[];
}

export interface Document {
  id: string;
  fileName: string;
  originalFileName: string;
  fileType: string;
  fileSize: number;
  category: string;
  folderPath: string | null;
  status: string;
  version: number;
  caseId: string;
  caseTitle: string;
  uploadedByName: string;
  createdAt: string;
  versionCount: number;
  tags: string | null;
  description: string | null;
  expiryDate: string | null;
  viewCount: number;
  downloadCount: number;
  isFavorited: boolean;
  approvalStatus: string | null;
  approvedByName: string | null;
  approvedAt: string | null;
  commentCount: number;
  versions: DocumentVersion[];
  shares: DocumentShare[];
}

// ---- Invoices / Finance ----
export interface Invoice {
  id: string;
  invoiceNumber: string;
  amount: number;
  currency: string;
  status: string;
  description: string | null;
  dueDate: string | null;
  paidAt: string | null;
  clientId: string;
  clientName: string;
  caseId: string | null;
  caseTitle: string | null;
  createdAt: string;
}

export interface Expense {
  id: string;
  description: string;
  amount: number;
  currency: string;
  category: string;
  expenseDate: string;
  receiptPath: string | null;
  caseId: string | null;
  caseTitle: string | null;
  createdByName: string;
  createdAt: string;
}

export interface Subscription {
  id: string;
  chamberId: string;
  plan: string;
  status: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  cancelAtPeriodEnd: boolean;
}

// ---- Templates ----
export interface Template {
  id: string;
  title: string;
  category: string;
  content: string;
  description?: string | null;
  variables: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

// ---- Reminders ----
export interface Reminder {
  id: string;
  userId: string;
  userName: string;
  type: string;
  channel: string;
  priority: string;
  title: string;
  message: string;
  relatedEntityType: string | null;
  relatedEntityId: string | null;
  scheduledAt: string | null;
  sentStatus: boolean;
  sentAt: string | null;
  status: string;
  readAt: string | null;
  completedAt: string | null;
  snoozedUntil: string | null;
  escalationLevel: string | null;
  createdAt: string;
}

// ---- Legal Documents ----
export interface LegalDocument {
  id: string;
  title: string;
  category: string;
  content: string;
  citation: string | null;
  judgeName: string | null;
  keywords: string | null;
  year: number | null;
  createdAt: string;
}

// ---- Notifications ----
export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  referenceId: string | null;
  createdAt: string;
}

// ---- Legal Sections / Workflow ----
export interface LegalSection {
  id: string;
  sectionCode: string;
  sectionTitle: string;
  lawName: string;
  country: string;
  category: string;
  description: string | null;
  severity: string | null;
  isActive: boolean;
  procedureCount: number;
  createdAt: string;
}

export interface LegalProcedure {
  id: string;
  legalSectionId: string;
  stepNumber: number;
  title: string;
  description: string | null;
  requiredDocuments: string | null;
  recommendedTimeline: string | null;
  responsibleRole: string | null;
  isMandatory: boolean;
  createdAt: string;
}

export interface WorkflowTemplate {
  id: string;
  name: string;
  description: string | null;
  isDefault: boolean;
  sections: {
    id: string;
    legalSectionId: string;
    sectionCode: string;
    sectionTitle: string;
    lawName: string;
    displayOrder: number;
  }[];
  createdAt: string;
}

// ---- Case Workflows (Process) ----
export interface WorkflowStepItem {
  id: string;
  title: string;
  description: string | null;
  orderIndex: number;
  dueInDays: number | null;
}

export interface Workflow {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  stepCount: number;
  createdByName: string | null;
  createdAt: string;
  steps: WorkflowStepItem[];
}

export interface CreateWorkflowStepInput {
  title: string;
  description?: string | null;
  orderIndex: number;
  dueInDays?: number | null;
}

export interface CreateWorkflowInput {
  name: string;
  description?: string | null;
  steps: CreateWorkflowStepInput[];
}

export type UpdateWorkflowInput = CreateWorkflowInput;

export interface CaseWorkflowStep {
  id: string;
  stepId: string | null;
  title: string;
  description: string | null;
  orderIndex: number;
  dueDate: string | null;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  completedByName: string | null;
  notes: string | null;
  isActive: boolean;
  isLocked: boolean;
  isCompleted: boolean;
  isOverdue: boolean;
}

export interface CaseWorkflow {
  id: string;
  caseId: string;
  workflowId: string;
  workflowName: string;
  workflowDescription: string | null;
  status: string;
  startedAt: string;
  completedAt: string | null;
  startedByName: string | null;
  stepCount: number;
  completedStepCount: number;
  percentComplete: number;
  isOverdue: boolean;
  nextStepTitle: string | null;
  steps: CaseWorkflowStep[];
}

// ---- Messages ----
export interface Message {
  id: string;
  senderId: string;
  senderName: string;
  senderAvatar: string | null;
  content: string;
  attachmentUrl: string | null;
  attachmentFileName: string | null;
  isRead: boolean;
  readAt: string | null;
  createdAt: string;
}

// ---- Chamber Settings ----
export interface ChamberSettings {
  id: string;
  chamberId: string;
  settings: Record<string, Record<string, unknown>>;
  updatedAt: string;
}

// ---- Admin ----
export interface AdminUser {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  isActive: boolean;
  barCouncilId: string | null;
  chamberId: string;
  chamberName: string;
  casesCount: number;
  createdAt: string;
}

// ---- Search ----
export interface SearchResult {
  id: string;
  type: string;
  title: string;
  subtitle: string;
  url: string;
  status: string;
}

export interface SearchResponse {
  results: SearchResult[];
  totalCount: number;
}

// ---- Teams ----
export interface TeamMember {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  userRole: string;
  avatarUrl: string | null;
  teamRole: string;
  joinedAt: string;
  isPending: boolean;
  invitedName: string | null;
}

export interface Team {
  id: string;
  name: string;
  description: string | null;
  memberCount: number;
  createdAt: string;
  createdByName: string;
  members: TeamMember[];
}

// ---- Accounting ----
export interface Account {
  id: string;
  code: string;
  name: string;
  type: string;
  description: string | null;
  parentId: string | null;
  parentName: string | null;
  isActive: boolean;
  openingBalance: number;
  balance: number;
  children: Account[];
}

export interface JournalLine {
  id: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  accountType: string;
  debitAmount: number;
  creditAmount: number;
  description: string | null;
}

export interface Journal {
  id: string;
  entryNumber: string;
  entryDate: string;
  description: string;
  referenceType: string | null;
  referenceId: string | null;
  createdByName: string;
  createdAt: string;
  totalDebit: number;
  totalCredit: number;
  lines: JournalLine[];
}

// ---- Payroll ----
export interface Employee {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  phone: string;
  designation: string;
  department: string;
  joinDate: string;
  baseSalary: number;
  bankName: string | null;
  bankAccountNo: string | null;
  nidNo: string | null;
  tinNo: string | null;
  status: string;
  createdAt: string;
}

export interface Payroll {
  id: string;
  payrollNumber: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  month: number;
  year: number;
  grossSalary: number;
  bonus: number;
  overtime: number;
  deductions: number;
  taxDeduction: number;
  netSalary: number;
  paidAt: string | null;
  status: string;
  createdAt: string;
}

export interface Attendance {
  id: string;
  employeeId: string;
  employeeName: string;
  date: string;
  status: string;
  checkIn: string | null;
  checkOut: string | null;
  notes: string | null;
}

// ---- Banking ----
export interface BankAccount {
  id: string;
  accountName: string;
  bankName: string;
  branchName: string;
  accountNumber: string;
  routingNumber: string | null;
  accountType: string;
  openingBalance: number;
  currentBalance: number;
  isActive: boolean;
  createdAt: string;
}

export interface BankTransaction {
  id: string;
  bankAccountId: string;
  bankAccountName: string;
  transactionDate: string;
  transactionType: string;
  amount: number;
  referenceNo: string | null;
  chequeNo: string | null;
  payee: string | null;
  description: string | null;
  reconciliationStatus: string;
  reconciledAt: string | null;
  createdAt: string;
}

// ---- Budget ----
export interface BudgetLine {
  id: string;
  accountId: string;
  accountName?: string;
  allocatedAmount: number;
}

export interface Budget {
  id: string;
  name: string;
  fiscalYear: number;
  totalAmount: number;
  totalSpent: number;
  remaining: number;
  description: string | null;
  status: string;
  createdByName: string;
  createdAt: string;
  lines: BudgetLine[];
}

// ---- Fixed Assets ----
export interface FixedAsset {
  id: string;
  assetCode: string;
  name: string;
  category: string;
  description: string | null;
  purchaseDate: string;
  purchaseCost: number;
  currentValue: number;
  depreciationMethod: string;
  usefulLifeYears: number;
  salvageValue: number;
  accumulatedDepreciation: number;
  location: string | null;
  vendor: string | null;
  status: string;
  disposalDate: string | null;
  createdAt: string;
}

// ---- Tax ----
export interface TaxSetting {
  id: string;
  taxType: string;
  name: string;
  rate: number;
  threshold: number | null;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface TaxTransaction {
  id: string;
  referenceNumber: string;
  taxSettingId: string;
  taxTypeName: string;
  taxableAmount: number;
  taxAmount: number;
  transactionDate: string;
  month: number;
  year: number;
  challanNo: string | null;
  paidAt: string | null;
  remarks: string | null;
  createdAt: string;
}

// ---- Bails ----
export interface Bail {
  id: string;
  caseId: string;
  caseNumber: string;
  caseTitle: string;
  status: string;
  bailAmount: number | null;
  bailConditions: string | null;
  bailGrantedAt: string | null;
  bailHearingDate: string | null;
  bondNumber: string | null;
  suretyName: string | null;
  suretyAddress: string | null;
  suretyContact: string | null;
  revokedAt: string | null;
  revokedReason: string | null;
  grantedBy: string | null;
  notes: string | null;
  createdAt: string;
}

// ---- Leads ----
export interface Lead {
  id: string;
  name: string;
  phone: string;
  email: string | null;
  companyName: string | null;
  caseType: string | null;
  estimatedValue: number | null;
  leadSource: string;
  stage: string;
  assignedLawyerId: string | null;
  assignedLawyerName: string | null;
  notes: string | null;
  followUpDate: string | null;
  lastContactedAt: string | null;
  score: number | null;
  isStale: boolean;
  createdAt: string;
  convertedAt: string | null;
  lostReason: string | null;
}

// ---- Time Entries ----
export interface TimeEntry {
  id: string;
  userId: string;
  userName: string;
  clientId: string | null;
  clientName: string | null;
  caseId: string | null;
  caseTitle: string | null;
  caseNumber: string | null;
  taskId: string | null;
  taskTitle: string | null;
  invoiceId: string | null;
  invoiceNumber: string | null;
  description: string;
  category: string;
  startTime: string;
  endTime: string | null;
  durationMinutes: number;
  hourlyRate: number;
  totalAmount: number;
  billable: boolean;
  status: string;
  createdAt: string;
}

// ---- Super Admin ----
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

// ---- Client Portal ----
export interface ClientCaseSummary {
  id: string;
  caseNumber: string;
  title: string;
  caseType: string;
  status: string;
  assignedLawyerName: string;
  nextHearingDate: string | null;
  createdAt: string;
  documentsCount: number;
}

export interface ClientCaseDetail {
  id: string;
  caseNumber: string;
  title: string;
  courtName: string;
  caseType: string;
  status: string;
  opponent: string;
  assignedLawyerName: string;
  assignedLawyerPhone: string;
  assignedLawyerEmail: string;
  filingDate: string;
  createdAt: string;
  timeline: { id: string; type: string; description: string; timestamp: string }[];
}

export interface ClientHearing {
  id: string;
  caseId: string;
  caseTitle: string;
  caseNumber: string;
  hearingDate: string;
  courtroom: string | null;
  judgeName: string | null;
  status: string;
  result: string | null;
  nextHearingDate: string | null;
}

export interface ClientDocument {
  id: string;
  fileName: string;
  fileType: string;
  fileSize: number;
  category: string;
  folderPath: string | null;
  caseId: string;
  caseTitle: string;
  uploadedByName: string;
  createdAt: string;
}

export interface ClientInvoice {
  id: string;
  invoiceNumber: string;
  amount: number;
  paidAmount: number;
  balance: number;
  currency: string;
  status: string;
  description: string | null;
  dueDate: string | null;
  paidAt: string | null;
  caseTitle: string | null;
  createdAt: string;
}

export interface ClientTask {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  status: string;
  assignedByName: string;
  caseTitle: string | null;
  createdAt: string;
}

export interface ClientProfile {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: string | null;
  companyName: string | null;
  chamberId: string;
  chamberName: string;
  chamberLogo: string | null;
}

export interface ClientDashboard {
  activeCases: number;
  upcomingHearings: number;
  pendingInvoices: number;
  outstandingBalance: number;
  sharedDocuments: number;
  unreadMessages: number;
  pendingTasks: number;
  recentCases: ClientCaseSummary[];
  upcomingHearingList: ClientHearing[];
  recentInvoices: ClientInvoice[];
}

// ---- Dashboard ----
export interface DashboardStats {
  totalCases: number;
  activeCases: number;
  pendingCases: number;
  closedCases: number;
  hearingsToday: number;
  upcomingHearings: number;
  totalClients: number;
  totalLawyers: number;
  caseGrowth: number;
  clientGrowth: number;
}

export interface CaseChartPoint {
  month: string;
  active: number;
  closed: number;
  pending: number;
}

export interface RecentActivity {
  id: string;
  type: string;
  title: string;
  description: string;
  timestamp: string;
  referenceId: string;
}

export interface LawyerProductivity {
  id: string;
  name: string;
  totalCases: number;
  activeCases: number;
  closedCases: number;
  pendingTasks: number;
}

export interface WinRatio {
  id: string;
  name: string;
  totalCases: number;
  activeCases: number;
  pendingCases: number;
  closedCases: number;
}
