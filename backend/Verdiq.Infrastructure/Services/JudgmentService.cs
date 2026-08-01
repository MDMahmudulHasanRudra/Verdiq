using System.Text;
using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Case;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class JudgmentService : IJudgmentService
{
    private readonly AppDbContext _context;
    private readonly ICloudStorageService _cloudStorage;

    public JudgmentService(AppDbContext context, ICloudStorageService cloudStorage)
    {
        _context = context;
        _cloudStorage = cloudStorage;
    }

    public async Task<IEnumerable<JudgmentDto>> GetByCaseIdAsync(Guid caseId)
    {
        var judgments = await _context.Judgments
            .Include(j => j.RecordedBy)
            .Where(j => j.CaseId == caseId && !j.IsDeleted)
            .OrderByDescending(j => j.JudgmentDate)
            .ThenByDescending(j => j.CreatedAt)
            .ToListAsync();

        return judgments.Select(MapToDto);
    }

    public async Task<JudgmentDto?> GetByIdAsync(Guid judgmentId)
    {
        var judgment = await _context.Judgments
            .Include(j => j.RecordedBy)
            .FirstOrDefaultAsync(j => j.Id == judgmentId && !j.IsDeleted);

        return judgment == null ? null : MapToDto(judgment);
    }

    public async Task<(bool Success, string Message, JudgmentDto? Data)> CreateAsync(Guid caseId, CreateJudgmentDto dto, Guid userId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null || caseEntity.IsDeleted)
            return (false, "Case not found", null);

        if (string.IsNullOrWhiteSpace(dto.Caption))
            return (false, "Caption is required", null);

        var judgment = new Judgment
        {
            CaseId = caseId,
            Caption = dto.Caption.Trim(),
            Summary = dto.Summary,
            Result = dto.Result,
            JudgmentDate = dto.JudgmentDate ?? DateTime.UtcNow,
            NextHearingDate = dto.NextHearingDate.HasValue
                ? DateTime.SpecifyKind(dto.NextHearingDate.Value, DateTimeKind.Utc)
                : null,
            KeyFindings = dto.KeyFindings,
            RecordedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Judgments.Add(judgment);
        await _context.SaveChangesAsync();

        _context.CaseActivities.Add(new CaseActivity
        {
            CaseId = caseId,
            ActivityType = Domain.Enums.ActivityType.Note,
            Description = $"Judgment recorded: {judgment.Caption}",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(judgment.Id);
        return (true, "Judgment recorded successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid judgmentId)
    {
        var judgment = await _context.Judgments.FindAsync(judgmentId);
        if (judgment == null || judgment.IsDeleted)
            return (false, "Judgment not found");

        if (!string.IsNullOrWhiteSpace(judgment.FilePath))
            await _cloudStorage.DeleteAsync(judgment.FilePath);

        judgment.IsDeleted = true;
        judgment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Judgment deleted successfully");
    }

    public async Task<(bool Success, string Message, JudgmentDto? Data)> UploadDocumentAsync(Guid caseId, Guid judgmentId, Guid userId, Stream fileStream, string fileName, string contentType)
    {
        var judgment = await _context.Judgments
            .Include(j => j.RecordedBy)
            .FirstOrDefaultAsync(j => j.Id == judgmentId && j.CaseId == caseId && !j.IsDeleted);
        if (judgment == null)
            return (false, "Judgment not found", null);

        var key = $"cases/{caseId}/judgments/{judgmentId}/{Guid.NewGuid():N}_{fileName}";
        var storageKey = await _cloudStorage.UploadAsync(key, fileStream, contentType);

        if (!string.IsNullOrWhiteSpace(judgment.FilePath))
            await _cloudStorage.DeleteAsync(judgment.FilePath);

        judgment.FileName = Path.GetFileName(storageKey);
        judgment.OriginalFileName = fileName;
        judgment.FilePath = storageKey;
        judgment.FileType = contentType;
        judgment.FileSize = fileStream.Length;
        judgment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(judgment.Id);
        return (true, "Judgment document uploaded", result);
    }

    public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadDocumentAsync(Guid caseId, Guid judgmentId)
    {
        var judgment = await _context.Judgments
            .FirstOrDefaultAsync(j => j.Id == judgmentId && j.CaseId == caseId && !j.IsDeleted);
        if (judgment == null || string.IsNullOrWhiteSpace(judgment.FilePath))
            return (null, null, null);

        var stream = await _cloudStorage.DownloadAsync(judgment.FilePath);
        return (stream, judgment.FileType, judgment.OriginalFileName ?? judgment.FileName);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(Guid caseId, string format)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.AssignedLawyer)
            .FirstOrDefaultAsync(c => c.Id == caseId && !c.IsDeleted);

        var caseInfo = caseEntity ?? new Case { CaseNumber = "Unknown", Title = "Unknown Case" };

        var judgments = await _context.Judgments
            .Include(j => j.RecordedBy)
            .Where(j => j.CaseId == caseId && !j.IsDeleted)
            .OrderByDescending(j => j.JudgmentDate)
            .ToListAsync();

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var fileNameBase = $"judgments-{caseInfo.CaseNumber}-{stamp}";

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            return (BuildCsv(caseInfo, judgments), "text/csv", $"{fileNameBase}.csv");

        var pdf = BuildPdf(caseInfo, judgments);
        return (pdf, "application/pdf", $"{fileNameBase}.pdf");
    }

    private static JudgmentDto MapToDto(Judgment j) => new()
    {
        Id = j.Id,
        CaseId = j.CaseId,
        Caption = j.Caption,
        Summary = j.Summary,
        Result = j.Result,
        JudgmentDate = j.JudgmentDate,
        NextHearingDate = j.NextHearingDate,
        KeyFindings = j.KeyFindings,
        FileName = j.FileName,
        OriginalFileName = j.OriginalFileName,
        FileType = j.FileType,
        FileSize = j.FileSize,
        RecordedByName = j.RecordedBy?.FullName,
        CreatedAt = j.CreatedAt
    };

    private static byte[] BuildCsv(Case caseInfo, List<Judgment> judgments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Case Number,Case Title,Court,Judgment Date,Result,Caption,Next Hearing,Recorded By,Key Findings,Summary,Attachment");
        sb.AppendLine(string.Join(",",
            Csv(caseInfo.CaseNumber), Csv(caseInfo.Title), Csv(caseInfo.CourtName),
            Csv(caseInfo.FilingDate.ToString("yyyy-MM-dd")), "", "", "", "", "", "", ""));

        foreach (var j in judgments)
        {
            sb.AppendLine(string.Join(",",
                Csv(caseInfo.CaseNumber), Csv(caseInfo.Title), Csv(caseInfo.CourtName),
                Csv(j.JudgmentDate.ToString("yyyy-MM-dd")),
                Csv(j.Result ?? ""),
                Csv(j.Caption),
                Csv(j.NextHearingDate?.ToString("yyyy-MM-dd") ?? ""),
                Csv(j.RecordedBy?.FullName ?? ""),
                Csv(j.KeyFindings ?? ""),
                Csv(j.Summary ?? ""),
                Csv(j.OriginalFileName ?? "")));
        }

        // UTF-8 BOM so Excel opens it with correct encoding
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Csv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var v = value.Replace("\"", "\"\"");
        return $"\"{v}\"";
    }

    private static byte[] BuildPdf(Case caseInfo, List<Judgment> judgments)
    {
        const float pageW = 595f, pageH = 842f, margin = 48f;
        const int leading = 15;
        const int headerLines = 3;
        int maxBody = (int)((pageH - 2 * margin) / leading) - headerLines;
        if (maxBody < 5) maxBody = 5;

        var pages = new List<List<string>> { new() };
        foreach (var line in BuildReportLines(caseInfo, judgments))
        {
            var last = pages[^1];
            if (last.Count >= maxBody)
            {
                pages.Add(new List<string>());
                last = pages[^1];
            }
            last.Add(line);
        }

        int pageCount = pages.Count;

        var objects = new List<byte[]>();
        objects.Add(Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"));

        var kids = string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{3 + i} 0 R"));
        objects.Add(Encoding.ASCII.GetBytes($"<< /Type /Pages /Kids [{kids}] /Count {pageCount} >>"));

        int contentStart = 3 + pageCount;
        int fontObj = contentStart + pageCount;

        for (int i = 0; i < pageCount; i++)
        {
            int contentObj = contentStart + i;
            objects.Add(Encoding.ASCII.GetBytes(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageW} {pageH}] /Contents {contentObj} 0 R /Resources << /Font << /F1 {fontObj} 0 R >> >> >>"));

            var stream = new StringBuilder();
            float top = pageH - margin;
            stream.AppendLine($"BT /F1 14 Tf {margin} {top:0.0} Td {PdfText($"{caseInfo.CaseNumber} - {caseInfo.Title}")} Tj ET");
            float y = top - leading;
            stream.AppendLine($"BT /F1 9 Tf {margin} {y:0.0} Td {PdfText($"Page {i + 1} of {pageCount}  |  Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC  |  Verdiq")} Tj ET");
            y -= 8;
            stream.AppendLine($"0.6 w {margin} {y:0.0} m {pageW - margin} {y:0.0} l S");
            y -= leading;
            foreach (var line in pages[i])
            {
                stream.AppendLine($"BT /F1 10 Tf {margin} {y:0.0} Td {PdfText(line)} Tj ET");
                y -= leading;
            }

            var streamBytes = Encoding.ASCII.GetBytes(stream.ToString());
            var content = new List<byte>();
            content.AddRange(Encoding.ASCII.GetBytes($"<< /Length {streamBytes.Length} >>\nstream\n"));
            content.AddRange(streamBytes);
            content.AddRange(Encoding.ASCII.GetBytes("\nendstream"));
            objects.Add(content.ToArray());
        }

        objects.Add(Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"));

        var pdf = new List<byte>();
        pdf.AddRange(Encoding.ASCII.GetBytes("%PDF-1.4\n"));
        var offsets = new int[objects.Count];
        for (int i = 0; i < objects.Count; i++)
        {
            offsets[i] = pdf.Count;
            pdf.AddRange(Encoding.ASCII.GetBytes($"{i + 1} 0 obj\n"));
            pdf.AddRange(objects[i]);
            pdf.AddRange(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        int xrefPos = pdf.Count;
        pdf.AddRange(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n"));
        foreach (var off in offsets)
            pdf.AddRange(Encoding.ASCII.GetBytes($"{off:0000000000} 00000 n \n"));

        pdf.AddRange(Encoding.ASCII.GetBytes($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF\n"));

        return pdf.ToArray();
    }

    private static List<string> BuildReportLines(Case caseInfo, List<Judgment> judgments)
    {
        var lines = new List<string>
        {
            $"Case: {caseInfo.CaseNumber} | {caseInfo.Title}",
            $"Court: {caseInfo.CourtName} | Filed: {caseInfo.FilingDate:yyyy-MM-dd}",
            $"Assigned counsel: {caseInfo.AssignedLawyer?.FullName ?? "—"}"
        };

        if (judgments.Count == 0)
        {
            lines.Add("");
            lines.Add("No judgments recorded for this case.");
            return lines;
        }

        lines.Add("");
        lines.Add($"Judgments ({judgments.Count}):");
        foreach (var j in judgments)
        {
            lines.Add("");
            lines.Add($"- {j.JudgmentDate:yyyy-MM-dd} | {j.Result ?? "No result"} | {j.Caption}");
            if (j.NextHearingDate.HasValue)
                lines.Add($"  Next hearing: {j.NextHearingDate:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(j.RecordedBy?.FullName))
                lines.Add($"  Recorded by: {j.RecordedBy.FullName}");
            if (!string.IsNullOrWhiteSpace(j.KeyFindings))
                lines.Add($"  Key findings: {j.KeyFindings}");
            if (!string.IsNullOrWhiteSpace(j.Summary))
                lines.Add($"  Summary: {j.Summary}");
            if (!string.IsNullOrWhiteSpace(j.OriginalFileName))
                lines.Add($"  Attachment: {j.OriginalFileName}");
        }

        return lines;
    }

    private static string PdfText(string value)
    {
        if (string.IsNullOrEmpty(value)) return "<FEFF>";
        var hex = Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(value));
        return $"<FEFF{hex}>";
    }
}
