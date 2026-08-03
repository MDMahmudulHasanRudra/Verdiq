import {
  LayoutDashboard,
  FolderOpen,
  Users,
  CalendarClock,
  FileText,
  ListTodo,
  Receipt,
  Wallet,
  BookOpen,
  Scale,
  Sparkles,
  Bell,
  Settings,
  Cog,
  UsersRound,
  Landmark,
  PiggyBank,
  Boxes,
  Percent,
  ScrollText,
  UserPlus,
  Clock,
  ShieldCheck,
  ArrowLeftRight,
  Handshake,
  TrendingUp,
  Workflow
} from "lucide-react";

export interface NavItem {
  label: string;
  i18nKey: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  module?: string;
  group: string;
}

export interface NavGroup {
  key: string;
  i18nKey: string;
  label: string;
  items: NavItem[];
}

export const navGroups: NavGroup[] = [
  {
    key: "overview",
    i18nKey: "nav.overview",
    label: "Overview",
    items: [
      { label: "Dashboard", i18nKey: "nav.dashboard", href: "/lawyer", icon: LayoutDashboard, module: "Dashboard", group: "overview" },
      { label: "AI Assistant", i18nKey: "nav.aiAssistant", href: "/lawyer/ai-assistant", icon: Sparkles, module: "AI", group: "overview" }
    ]
  },
  {
    key: "practice",
    i18nKey: "nav.practice",
    label: "Practice",
    items: [
      { label: "Cases", i18nKey: "nav.cases", href: "/lawyer/cases", icon: FolderOpen, module: "Cases", group: "practice" },
      { label: "Hearings", i18nKey: "nav.hearings", href: "/lawyer/hearings", icon: CalendarClock, module: "Hearings", group: "practice" },
      { label: "Clients", i18nKey: "nav.clients", href: "/lawyer/clients", icon: Users, module: "Clients", group: "practice" },
      { label: "Leads", i18nKey: "nav.leads", href: "/lawyer/leads", icon: UserPlus, module: "Leads", group: "practice" },
      { label: "Bails", i18nKey: "nav.bails", href: "/lawyer/bails", icon: Handshake, module: "Bails", group: "practice" },
      { label: "Documents", i18nKey: "nav.documents", href: "/lawyer/documents", icon: FileText, module: "Documents", group: "practice" },
      { label: "Tasks", i18nKey: "nav.tasks", href: "/lawyer/tasks", icon: ListTodo, module: "Tasks", group: "practice" },
      { label: "Time Entries", i18nKey: "nav.timeEntries", href: "/lawyer/time-entries", icon: Clock, module: "TimeEntries", group: "practice" },
      { label: "Legal Database", i18nKey: "nav.legalDatabase", href: "/lawyer/legal-database", icon: BookOpen, module: "LegalDatabase", group: "practice" },
      { label: "Templates", i18nKey: "nav.templates", href: "/lawyer/templates", icon: ScrollText, module: "Templates", group: "practice" },
      { label: "Workflows", i18nKey: "nav.workflows", href: "/lawyer/workflows", icon: Workflow, module: "Cases", group: "practice" }
    ]
  },
  {
    key: "finance",
    i18nKey: "nav.finance",
    label: "Finance",
    items: [
      { label: "Invoices", i18nKey: "nav.invoices", href: "/lawyer/invoices", icon: Receipt, module: "Billing", group: "finance" },
      { label: "Expenses", i18nKey: "nav.expenses", href: "/lawyer/expenses", icon: Wallet, module: "Billing", group: "finance" },
      { label: "Accounting", i18nKey: "nav.accounting", href: "/lawyer/accounting", icon: TrendingUp, module: "Accounting", group: "finance" },
      { label: "Banking", i18nKey: "nav.banking", href: "/lawyer/banking", icon: Landmark, module: "Banking", group: "finance" },
      { label: "Budget", i18nKey: "nav.budget", href: "/lawyer/budget", icon: PiggyBank, module: "Budget", group: "finance" },
      { label: "Fixed Assets", i18nKey: "nav.fixedAssets", href: "/lawyer/fixed-assets", icon: Boxes, module: "FixedAssets", group: "finance" },
      { label: "Tax", i18nKey: "nav.tax", href: "/lawyer/tax", icon: Percent, module: "Tax", group: "finance" },
      { label: "Payroll", i18nKey: "nav.payroll", href: "/lawyer/payroll", icon: ArrowLeftRight, module: "Payroll", group: "finance" }
    ]
  },
  {
    key: "organization",
    i18nKey: "nav.organization",
    label: "Organization",
    items: [
      { label: "Teams", i18nKey: "nav.teams", href: "/lawyer/teams", icon: UsersRound, module: "Teams", group: "organization" },
      { label: "Audit Logs", i18nKey: "nav.auditLogs", href: "/lawyer/audit", icon: ShieldCheck, module: "Audit", group: "organization" },
      { label: "Notifications", i18nKey: "nav.notifications", href: "/lawyer/notifications", icon: Bell, module: "Notifications", group: "organization" },
      { label: "Configuration", i18nKey: "nav.configuration", href: "/lawyer/configuration", icon: Cog, module: "Configuration", group: "organization" },
      { label: "Settings", i18nKey: "nav.settings", href: "/lawyer/settings", icon: Settings, module: "Settings", group: "organization" }
    ]
  }
];
