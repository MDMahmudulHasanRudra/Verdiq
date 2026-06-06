namespace Verdiq.Domain.Enums;

public enum EmployeeStatus
{
    Active = 1,
    Inactive = 2,
    Resigned = 3,
    Terminated = 4
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    HalfDay = 4,
    Holiday = 5,
    Leave = 6
}

public enum PayrollStatus
{
    Draft = 1,
    Approved = 2,
    Paid = 3
}

public enum AssetDepreciationMethod
{
    StraightLine = 1,
    DecliningBalance = 2,
    SumOfYearsDigits = 3
}

public enum AssetStatus
{
    Active = 1,
    Disposed = 2,
    UnderMaintenance = 3
}

public enum ReconciliationStatus
{
    Unreconciled = 1,
    Reconciled = 2,
    Discrepancy = 3
}

public enum TaxType
{
    IncomeTax = 1,
    VAT = 2,
    TDS = 3,
    AIT = 4
}

public enum BudgetStatus
{
    Draft = 1,
    Approved = 2,
    Active = 3,
    Closed = 4
}
