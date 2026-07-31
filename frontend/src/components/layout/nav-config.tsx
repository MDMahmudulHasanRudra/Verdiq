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
  TrendingUp
} from "lucide-react";

export interface NavItem {
  label: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  module?: string;
  group: string;
}

export const navGroups: { key: string; label: string; items: NavItem[] }[] = [
  {
    key: "overview",
    label: "Overview",
    items: [
      { label: "Dashboard", href: "/lawyer", icon: LayoutDashboard, module: "Dashboard", group: "overview" },
      { label: "AI Assistant", href: "/lawyer/ai-assistant", icon: Sparkles, module: "AI", group: "overview" }
    ]
  },
  {
    key: "practice",
    label: "Practice",
    items: [
      { label: "Cases", href: "/lawyer/cases", icon: FolderOpen, module: "Cases", group: "practice" },
      { label: "Hearings", href: "/lawyer/hearings", icon: CalendarClock, module: "Hearings", group: "practice" },
      { label: "Clients", href: "/lawyer/clients", icon: Users, module: "Clients", group: "practice" },
      { label: "Leads", href: "/lawyer/leads", icon: UserPlus, module: "Leads", group: "practice" },
      { label: "Bails", href: "/lawyer/bails", icon: Handshake, module: "Bails", group: "practice" },
      { label: "Documents", href: "/lawyer/documents", icon: FileText, module: "Documents", group: "practice" },
      { label: "Tasks", href: "/lawyer/tasks", icon: ListTodo, module: "Tasks", group: "practice" },
      { label: "Time Entries", href: "/lawyer/time-entries", icon: Clock, module: "TimeEntries", group: "practice" },
      { label: "Legal Database", href: "/lawyer/legal-database", icon: BookOpen, module: "LegalDatabase", group: "practice" },
      { label: "Templates", href: "/lawyer/templates", icon: ScrollText, module: "Templates", group: "practice" }
    ]
  },
  {
    key: "finance",
    label: "Finance",
    items: [
      { label: "Invoices", href: "/lawyer/invoices", icon: Receipt, module: "Billing", group: "finance" },
      { label: "Expenses", href: "/lawyer/expenses", icon: Wallet, module: "Billing", group: "finance" },
      { label: "Accounting", href: "/lawyer/accounting", icon: TrendingUp, module: "Accounting", group: "finance" },
      { label: "Banking", href: "/lawyer/banking", icon: Landmark, module: "Banking", group: "finance" },
      { label: "Budget", href: "/lawyer/budget", icon: PiggyBank, module: "Budget", group: "finance" },
      { label: "Fixed Assets", href: "/lawyer/fixed-assets", icon: Boxes, module: "FixedAssets", group: "finance" },
      { label: "Tax", href: "/lawyer/tax", icon: Percent, module: "Tax", group: "finance" },
      { label: "Payroll", href: "/lawyer/payroll", icon: ArrowLeftRight, module: "Payroll", group: "finance" }
    ]
  },
  {
    key: "organization",
    label: "Organization",
    items: [
      { label: "Teams", href: "/lawyer/teams", icon: UsersRound, module: "Teams", group: "organization" },
      { label: "Audit Logs", href: "/lawyer/audit", icon: ShieldCheck, module: "Audit", group: "organization" },
      { label: "Notifications", href: "/lawyer/notifications", icon: Bell, module: "Notifications", group: "organization" },
      { label: "Configuration", href: "/lawyer/configuration", icon: Cog, module: "Configuration", group: "organization" },
      { label: "Settings", href: "/lawyer/settings", icon: Settings, module: "Settings", group: "organization" }
    ]
  }
];
