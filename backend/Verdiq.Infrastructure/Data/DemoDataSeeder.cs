using Microsoft.EntityFrameworkCore;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Task = System.Threading.Tasks.Task;
using TaskStatus = Verdiq.Domain.Enums.TaskStatus;

namespace Verdiq.Infrastructure.Data;

public static class DemoDataSeeder
{
    private static readonly Guid DefaultChamberId = Guid.Parse("c0000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Clients.AnyAsync()) return;

        var adminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var lawyer1Id = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");

        var lawyer2Id = Guid.Parse("22220001-0000-0000-0000-000000000001");
        var lawyer3Id = Guid.Parse("22220001-0000-0000-0000-000000000002");
        var lawyer4Id = Guid.Parse("22220001-0000-0000-0000-000000000003");

        var lawyer2SubId = Guid.Parse("22220002-0000-0000-0000-000000000001");
        var lawyer3SubId = Guid.Parse("22220002-0000-0000-0000-000000000002");
        var lawyer4SubId = Guid.Parse("22220002-0000-0000-0000-000000000003");

        var now = DateTime.UtcNow;

        var lawyers = new List<User>
        {
            new()
            {
                Id = lawyer2Id,
                FullName = "Adv. Fatima Begum",
                Email = "fatima@verdiq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("lawyer123"),
                Phone = "+8801711122233",
                BarCouncilId = "BC-2024-002",
                Role = UserRole.SeniorLawyer,
                IsActive = true,
                ChamberId = DefaultChamberId,
                CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = lawyer3Id,
                FullName = "Adv. Mohammad Hossain",
                Email = "hossain@verdiq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("lawyer123"),
                Phone = "+8801814455667",
                BarCouncilId = "BC-2024-003",
                Role = UserRole.JuniorLawyer,
                IsActive = true,
                ChamberId = DefaultChamberId,
                CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = lawyer4Id,
                FullName = "Adv. Shahida Parvin",
                Email = "shahida@verdiq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("lawyer123"),
                Phone = "+8801917788990",
                BarCouncilId = "BC-2024-004",
                Role = UserRole.JuniorLawyer,
                IsActive = true,
                ChamberId = DefaultChamberId,
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        db.Users.AddRange(lawyers);

        db.Subscriptions.AddRange(
            new Subscription
            {
                Id = lawyer2SubId,
                ChamberId = DefaultChamberId,
                Plan = SubscriptionPlan.Pro,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Subscription
            {
                Id = lawyer3SubId,
                ChamberId = DefaultChamberId,
                Plan = SubscriptionPlan.Chamber,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new Subscription
            {
                Id = lawyer4SubId,
                ChamberId = DefaultChamberId,
                Plan = SubscriptionPlan.Pro,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        var allLawyerIds = new[] { lawyer1Id, lawyer2Id, lawyer3Id, lawyer4Id };

        var clientNames = new[]
        {
            "Md. Kamal Hossain", "Shirin Akhter", "Md. Rafiqul Islam", "Nasrin Sultana",
            "Md. Jahangir Alam", "Fatema Khatun", "Md. Abdul Mannan", "Rokeya Begum",
            "Md. Shafiqur Rahman", "Jahanara Parvin", "Md. Mizanur Rahman", "Salma Begum",
            "Md. Abul Kalam", "Hasina Akhter", "Md. Shahjahan Miah", "Rahima Khatun",
            "Md. Abdul Latif", "Shamima Sultana", "Md. Mokbul Hossain", "Anwara Begum",
            "Md. Nurul Islam", "Parvin Akhter", "Md. Abdul Quader", "Shahnaj Parvin",
            "Md. Sirajul Islam", "Jahanara Begum", "Md. Alauddin", "Maksuda Akhter",
            "Md. Mohiuddin", "Nilufar Yeasmin", "Md. Enamul Haque", "Taslima Begum",
            "Md. Abdul Hakim", "Sharmin Akhter", "Md. Tofazzal Hossain", "Laily Begum",
            "Md. Aminur Rahman", "Kulsum Akhter", "Md. Abdul Bari", "Sajeda Parvin",
            "Md. Zahid Hasan", "Ferdousi Begum", "Md. Mosharraf Hossain", "Rashida Akhter",
            "Md. Delwar Hossain", "Shahida Parvin", "Md. Mahbubur Rahman", "Rowshan Akhter",
            "Md. Abdul Alim", "Ayesha Khatun"
        };

        var clientEmails = new[]
        {
            "kamal.hossain@email.com", "shirin.akter@email.com", "rafiqul.islam@email.com",
            "nasrin.sultana@email.com", "jahangir.alam@email.com", "fatema.khatun@email.com",
            "abdul.mannan@email.com", "rokeya.begum@email.com", "shafiq.rahman@email.com",
            "jahanara.parvin@email.com", "mizanur.rahman@email.com", "salma.begum@email.com",
            "abul.kalam@email.com", "hasina.akter@email.com", "shahjahan.miah@email.com",
            "rahima.khatun@email.com", "abdul.latif@email.com", "shamima.sultana@email.com",
            "mokbul.hossain@email.com", "anwara.begum@email.com", "nurul.islam@email.com",
            "parvin.akter@email.com", "abdul.quader@email.com", "shahnaj.parvin@email.com",
            "sirajul.islam@email.com", "jahanara.begum@email.com", "alauddin@email.com",
            "maksuda.akter@email.com", "mohiuddin@email.com", "nilufar.yeasmin@email.com",
            "enamul.haque@email.com", "taslima.begum@email.com", "abdul.hakim@email.com",
            "sharmin.akter@email.com", "tofazzal.hossain@email.com", "laily.begum@email.com",
            "aminur.rahman@email.com", "kulsum.akter@email.com", "abdul.bari@email.com",
            "sajeda.parvin@email.com", "zahid.hasan@email.com", "ferdousi.begum@email.com",
            "mosharraf.hossain@email.com", "rashida.akter@email.com", "delwar.hossain@email.com",
            "shahida.parvin.c@email.com", "mahbubur.rahman@email.com", "rowshan.akter@email.com",
            "abdul.alim@email.com", "ayesha.khatun@email.com"
        };

        var phones = new[]
        {
            "+8801700000101", "+8801700000102", "+8801700000103", "+8801700000104",
            "+8801700000105", "+8801700000106", "+8801700000107", "+8801700000108",
            "+8801700000109", "+8801700000110", "+8801700000111", "+8801700000112",
            "+8801700000113", "+8801700000114", "+8801700000115", "+8801700000116",
            "+8801700000117", "+8801700000118", "+8801700000119", "+8801700000120",
            "+8801700000121", "+8801700000122", "+8801700000123", "+8801700000124",
            "+8801700000125", "+8801700000126", "+8801700000127", "+8801700000128",
            "+8801700000129", "+8801700000130", "+8801700000131", "+8801700000132",
            "+8801700000133", "+8801700000134", "+8801700000135", "+8801700000136",
            "+8801700000137", "+8801700000138", "+8801700000139", "+8801700000140",
            "+8801700000141", "+8801700000142", "+8801700000143", "+8801700000144",
            "+8801700000145", "+8801700000146", "+8801700000147", "+8801700000148",
            "+8801700000149", "+8801700000150"
        };

        var addresses = new[]
        {
            "42 Gulshan Avenue, Dhaka", "15 Banani Road 11, Dhaka", "28 Motijheel C/A, Dhaka",
            "7 Dhanmondi Road 2, Dhaka", "35 Mirpur Road 1, Dhaka", "12 Uttara Sector 4, Dhaka",
            "55 Lalmatia Block A, Dhaka", "8 Mohammadpur Road, Dhaka", "20 Shyamoli, Dhaka",
            "45 Bashundhara Block C, Dhaka", "18 Malibagh, Dhaka", "30 Rajshahi City Center, Rajshahi",
            "25 Khulna Sadar, Khulna", "10 Chattogram City, Chattogram", "50 Sylhet City Center, Sylhet",
            "22 Barisal Sadar, Barisal", "33 Rangpur City, Rangpur", "15 Mymensingh Sadar, Mymensingh",
            "40 Comilla City, Comilla", "28 Bogura City, Bogura", "55 Kushtia Sadar, Kushtia",
            "12 Jessore City, Jessore", "35 Dinajpur City, Dinajpur", "8 Cox's Bazar City, Cox's Bazar",
            "42 Gazipur City, Gazipur", "18 Narayanganj City, Narayanganj", "30 Tangail City, Tangail",
            "25 Faridpur City, Faridpur", "50 Pabna City, Pabna", "22 Sirajganj City, Sirajganj",
            "33 Noakhali City, Noakhali", "15 Patuakhali City, Patuakhali", "40 Habiganj City, Habiganj",
            "28 Sunamganj City, Sunamganj", "55 Maulvibazar City, Maulvibazar", "12 Netrokona City, Netrokona",
            "35 Kishoreganj City, Kishoreganj", "8 Manikganj City, Manikganj", "42 Munshiganj City, Munshiganj",
            "18 Gopalganj City, Gopalganj", "30 Chandpur City, Chandpur", "25 Laxmipur City, Laxmipur",
            "50 Shariatpur City, Shariatpur", "22 Narsingdi City, Narsingdi", "33 Brahmanbaria City, Brahmanbaria",
            "15 Feni City, Feni", "40 Rangamati City, Rangamati", "28 Khagrachari City, Khagrachari",
            "55 Bandarban City, Bandarban", "12 Chuadanga City, Chuadanga"
        };

        var nids = new[]
        {
            "19872345678901234", "19901234567890123", "19851234567890123", "19931234567890123",
            "19801234567890123", "19951234567890123", "19821234567890123", "19941234567890123",
            "19871234567890123", "19921234567890123", "19831234567890123", "19911234567890123",
            "19861234567890123", "19901234567890124", "19841234567890123", "19931234567890124",
            "19811234567890123", "19951234567890124", "19881234567890123", "19901234567890125",
            "19851234567890124", "19921234567890124", "19871234567890124", "19941234567890124",
            "19831234567890124", "19911234567890124", "19861234567890124", "19901234567890126",
            "19841234567890124", "19931234567890125", "19881234567890124", "19951234567890125",
            "19801234567890124", "19911234567890125", "19851234567890125", "19921234567890125",
            "19871234567890125", "19941234567890125", "19831234567890125", "19901234567890127",
            "19861234567890125", "19901234567890128", "19841234567890125", "19931234567890126",
            "19881234567890125", "19951234567890126", "19801234567890125", "19911234567890126",
            "19851234567890126", "19921234567890126"
        };

        var clients = new List<Client>();
        for (int i = 0; i < 50; i++)
        {
            clients.Add(new Client
            {
                Id = Guid.Parse($"B0000001-0000-0000-0000-{(i + 1):D12}"),
                Name = clientNames[i],
                Email = clientEmails[i],
                Phone = phones[i],
                Address = addresses[i],
                Nid = nids[i],
                IsActive = true,
                ChamberId = DefaultChamberId,
                Notes = (i % 5) switch
                {
                    0 => "Referred by Adv. Abdul Karim",
                    1 => "Referred by Mohammad Hossain",
                    2 => "Long-standing client, multiple cases",
                    3 => "Corporate client",
                    _ => null
                },
                CreatedAt = new DateTime(2024, 3, 1 + (i % 20), 10, 0, 0, DateTimeKind.Utc).AddMonths((i / 20) * 3)
            });
        }
        db.Clients.AddRange(clients);

        await db.SaveChangesAsync();

        var caseTypeData = new[]
        {
            ("Criminal", "CR", new[] {
                ("State vs. ", "Penal Code, 1860 — Sections 302/307/324/325/326", "Murder & Assault"),
                ("State vs. ", "Penal Code, 1860 — Sections 379/380/457/458", "Theft & Burglary"),
                ("State vs. ", "Penal Code, 1860 — Section 376/511", "Attempted Rape"),
                ("State vs. ", "Penal Code, 1860 — Sections 341/342/343", "Wrongful Restraint"),
                ("State vs. ", "Penal Code, 1860 — Sections 378/379", "Theft Case")
            }),
            ("Civil", "CV", new[] {
                (" vs. Opp. Party", "Code of Civil Procedure, 1908", "Civil Suit"),
                (" vs. Opp. Party", "Specific Relief Act, 1877 — Sections 8/9", "Specific Performance"),
                (" vs. Opp. Party", "Limitation Act, 1908 — Section 5", "Limitation Matter"),
                (" vs. Opp. Party", "Contract Act, 1872 — Sections 73/74", "Breach of Contract"),
                (" vs. Opp. Party", "Tort Law — Defamation", "Defamation Suit")
            }),
            ("Family", "FM", new[] {
                (" vs. Spouse", "Muslim Family Laws Ordinance, 1961 — Section 8", "Divorce Proceeding"),
                (" vs. Spouse", "Muslim Family Laws Ordinance, 1961 — Sections 4/5", "Maintenance Dispute"),
                (" vs. Spouse", "Guardians and Wards Act, 1890", "Child Custody"),
                (" vs. Spouse", "Muslim Family Laws Ordinance, 1961 — Section 7", "Restitution of Conjugal Rights"),
                (" vs. Spouse", "Family Court Ordinance, 1985", "Family Dispute")
            }),
            ("Property", "PR", new[] {
                (" vs. Opp. Party", "Transfer of Property Act, 1882 — Sections 53/54", "Property Dispute"),
                (" vs. Opp. Party", "Registration Act, 1908 — Section 17", "Deed Cancellation"),
                (" vs. Opp. Party", "Land Reforms Ordinance, 1984", "Land Dispute"),
                (" vs. Opp. Party", "Code of Civil Procedure, 1908 — Order 1 Rule 8", "Partition Suit"),
                (" vs. Opp. Party", "State Acquisition and Tenancy Act, 1950", "Tenancy Dispute")
            }),
            ("Cyber Crime", "CY", new[] {
                (" vs. Opp. Party", "Digital Security Act, 2018 — Sections 21/24", "Cyber Harassment"),
                (" vs. Opp. Party", "Digital Security Act, 2018 — Section 25", "Data Theft"),
                (" vs. Opp. Party", "Digital Security Act, 2018 — Sections 22/23", "Identity Theft"),
                (" vs. Opp. Party", "Digital Security Act, 2018 — Section 29", "Social Media Abuse"),
                (" vs. Opp. Party", "ICT Act, 2006 — Sections 54/57", "Hacking & Fraud")
            }),
            ("Narcotics", "NC", new[] {
                ("State vs. ", "Narcotics Control Act, 2018 — Sections 8/9", "Drug Possession"),
                ("State vs. ", "Narcotics Control Act, 2018 — Section 19", "Drug Trafficking"),
                ("State vs. ", "Narcotics Control Act, 2018 — Section 10(A)", "Yaba Trafficking"),
                ("State vs. ", "Narcotics Control Act, 2018 — Sections 12/13", "Cannabis Possession"),
                ("State vs. ", "Narcotics Control Act, 2018 — Section 21", "Conspiracy to Traffic")
            }),
            ("Financial Fraud", "FF", new[] {
                (" vs. Opp. Party", "Money Laundering Prevention Act, 2012 — Sections 4/5", "Money Laundering"),
                (" vs. Opp. Party", "Bank Companies Act, 1991 — Section 109", "Cheque Fraud"),
                (" vs. Opp. Party", "Money Loan Court Act, 2003", "Loan Default"),
                (" vs. Opp. Party", "Penal Code, 1860 — Sections 406/420", "Cheating & Embezzlement"),
                (" vs. Opp. Party", "Companies Act, 1994 — Section 228", "Corporate Fraud")
            })
        };

        var clientIds = clients.Select(c => c.Id).ToArray();

        var courts = new[]
        {
            "Dhaka District Court", "Chattogram District Court", "Rajshahi District Court",
            "Khulna District Court", "Sylhet District Court", "Barisal District Court",
            "Rangpur District Court", "Mymensingh District Court",
            "Dhaka Chief Metropolitan Magistrate Court",
            "Dhaka Metropolitan Sessions Judge Court",
            "Cyber Tribunal, Dhaka", "Family Court, Dhaka",
            "Joint District Judge Court, Dhaka",
            "Senior Assistant Judge Court, Dhaka",
            "Narcotics Tribunal, Dhaka",
            "Chattogram Metropolitan Sessions Judge Court",
            "Khulna Metropolitan Sessions Judge Court",
            "Supreme Court of Bangladesh, High Court Division",
            "Dhaka Money Loan Court",
            "Comilla District Court"
        };

        var courtRooms = new[] { "Court Room 101", "Court Room 202", "Court Room 303", "Court Room 404", "Court Room 505" };

        var policeStations = new[]
        {
            "Gulshan Police Station", "Banani Police Station", "Motijheel Police Station",
            "Lalbagh Police Station", "Ramna Police Station", "Mirpur Police Station",
            "Uttara Police Station", "Dhanmondi Police Station", "Mohammadpur Police Station",
            "Tejgaon Police Station", "Shahbag Police Station", "New Market Police Station",
            "Paltan Police Station", "Kotwali Police Station", "Savar Police Station"
        };

        var judgeNames = new[]
        {
            "Justice Md. Nuruzzaman", "Justice Syeda Kashfia Parvin",
            "Justice A. K. M. Asaduzzaman", "Justice Farah Mahbub",
            "Justice Md. Mozammel Hossain", "Judge Md. Shahidul Islam",
            "Judge Salma Akhter", "Judge K. M. Aminul Haque",
            "Judge Sharmin Rahman", "Judge Tahmina Begum",
            "Judge Md. Nazrul Islam", "Judge Rokeya Sultana",
            "Judge Abdul Latif", "Judge Shahinur Islam",
            "Judge Rezaul Karim"
        };

        var opponentNames = new[]
        {
            "Md. Abdul Gaffar", "Shamima Begum", "Mohammad Ali", "Rita Sultana",
            "Md. Helal Uddin", "Parvin Nahar", "Kazi Mahmud", "Sharmin Rani Das",
            "Md. Shamsul Islam", "Nargis Akhter", "Syed Manzur", "Fahmida Hasan",
            "Md. Jasim Uddin", "Rokeya Begum", "Harun-or-Rashid", "Runa Laila",
            "Md. Shahidullah", "Shahanara Khatun", "Rezaul Karim", "Nasima Akhter"
        };

        var descriptions = new[]
        {
            "The case involves serious allegations requiring urgent hearing.",
            "Both parties have been advised to seek mediation.",
            "Multiple witnesses have been cited by the prosecution.",
            "Evidence documents have been submitted to the court.",
            "The accused is currently on bail. Next hearing for charge framing.",
            "Case transferred from lower court for revision.",
            "Parties are attempting an out-of-court settlement."
        };

        var allStatuses = new[] { CaseStatus.Active, CaseStatus.Pending, CaseStatus.Closed, CaseStatus.Appeal };
        var allPriorities = new[] { CasePriority.Low, CasePriority.Medium, CasePriority.High };

        var allCaseTypeNames = new[] { "Criminal", "Civil", "Family", "Property", "Cyber Crime", "Narcotics", "Financial Fraud" };

        var random = new Random(42);
        var cases = new List<Case>();
        var caseNumberCounters = new Dictionary<string, int>();
        foreach (var tn in allCaseTypeNames)
            caseNumberCounters[tn] = 0;

        for (int i = 0; i < 120; i++)
        {
            var ctIdx = i switch
            {
                < 25 => 0,
                < 45 => 1,
                < 63 => 2,
                < 80 => 3,
                < 95 => 4,
                < 108 => 5,
                _ => 6
            };

            var (typeName, prefix, templates) = caseTypeData[ctIdx];
            caseNumberCounters[typeName]++;
            var seq = caseNumberCounters[typeName];
            var caseNum = $"CASE-{prefix}-{2024 + (i / 40)}-{seq:D3}";

            var (_, acts, _) = templates[seq % templates.Length];
            var clientIdx = i % 50;
            var clientName = clientNames[clientIdx].Replace("Md. ", "").Replace("Adv. ", "").Split(' ').First();
            var title = typeName == "Criminal" || typeName == "Narcotics"
                ? $"State vs. {clientNames[clientIdx]}"
                : $"{clientNames[clientIdx]} vs. Opp. Party";

            var opponent = opponentNames[random.Next(opponentNames.Length)];
            var lawyerId = allLawyerIds[clientIdx % 4];

            var status = allStatuses[random.Next(allStatuses.Length)];
            var priority = allPriorities[random.Next(allPriorities.Length)];
            var court = courts[random.Next(courts.Length)];
            var ps = policeStations[random.Next(policeStations.Length)];
            var desc = descriptions[random.Next(descriptions.Length)];

            var baseFilingDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 800));
            DateTime? closingDate = status == CaseStatus.Closed
                ? baseFilingDate.AddDays(random.Next(30, 365))
                : null;

            string? firNum = typeName is "Criminal" or "Narcotics"
                ? $"FIR-{ps.Split(' ')[0]}-2024-{100 + seq:D3}"
                : null;

            cases.Add(new Case
            {
                Id = Guid.Parse($"C0000001-0000-0000-0000-{(i + 1):D12}"),
                CaseNumber = caseNum,
                Title = title,
                CaseType = typeName,
                Status = status,
                Priority = priority,
                CourtName = court,
                Opponent = opponent,
                FirNumber = firNum,
                PoliceStation = typeName is "Criminal" or "Narcotics" ? ps : null,
                ActsAndSections = acts,
                Description = desc,
                FilingDate = baseFilingDate,
                ClosingDate = closingDate,
                AssignedLawyerId = lawyerId,
                ChamberId = DefaultChamberId,
                CreatedAt = baseFilingDate
            });
        }
        db.Cases.AddRange(cases);

        await db.SaveChangesAsync();

        var clientCases = new List<ClientCase>();
        var ccIdx = 0;
        for (int i = 0; i < 120; i++)
        {
            var clientIdx = i % 50;
            ccIdx++;
            clientCases.Add(new ClientCase
            {
                Id = Guid.Parse($"CC000001-0000-0000-0000-{ccIdx:D12}"),
                ClientId = clientIds[clientIdx],
                CaseId = cases[i].Id,
                CreatedAt = cases[i].CreatedAt
            });
        }
        db.ClientCases.AddRange(clientCases);

        await db.SaveChangesAsync();

        var caseIds = cases.Select(c => c.Id).ToArray();
        var hearingStatuses = new[] { HearingStatus.Scheduled, HearingStatus.Completed, HearingStatus.Adjourned, HearingStatus.Cancelled };

        var hearingResults = new[]
        {
            "Charge framed", "Witness examination completed", "Bail granted",
            "Bail rejected", "Adjourned for lack of prosecution witness",
            "Evidence submitted", "Cross-examination completed",
            "Final arguments heard. Judgment reserved.", "Settlement reached",
            "Case dismissed for lack of evidence"
        };

        var hearingNotes = new[]
        {
            "Witness cross-examination completed.",
            "Charge framing completed. Next date for witness examination.",
            "Defense counsel requested adjournment.",
            "Both parties present. Evidence submitted.",
            "Case adjourned due to absence of prosecution witness.",
            "Final arguments heard. Judgment reserved.",
            "Settlement talks failed. Matter proceeds to trial."
        };

        var hearings = new List<Hearing>();
        var hearingIdx = 0;
        var baseHearingDate = new DateTime(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < 120; i++)
        {
            var numHearings = i switch
            {
                < 10 => random.Next(3, 5),
                < 30 => random.Next(2, 4),
                < 60 => random.Next(1, 3),
                < 90 => random.Next(1, 2),
                _ => random.NextDouble() < 0.3 ? 1 : 0
            };

            for (int h = 0; h < numHearings && hearingIdx < 200; h++)
            {
                hearingIdx++;
                var hStatus = hearingStatuses[random.Next(hearingStatuses.Length)];
                var offset = random.Next(0, 700);
                var hDate = baseHearingDate.AddDays(offset);

                var hearingJudge = judgeNames[random.Next(judgeNames.Length)];

                DateTime? nextHearing = hStatus == HearingStatus.Adjourned
                    ? hDate.AddDays(random.Next(7, 60))
                    : (hStatus == HearingStatus.Completed && random.NextDouble() < 0.3
                        ? hDate.AddDays(random.Next(30, 90))
                        : null);

                hearings.Add(new Hearing
                {
                    Id = Guid.Parse($"D0000001-0000-0000-0000-{hearingIdx:D12}"),
                    CaseId = caseIds[i],
                    HearingDate = hDate,
                    Courtroom = courtRooms[random.Next(courtRooms.Length)],
                    JudgeName = hearingJudge,
                    Status = hStatus,
                    Result = hStatus == HearingStatus.Completed ? hearingResults[random.Next(hearingResults.Length)] : null,
                    NextHearingDate = nextHearing,
                    Notes = hearingNotes[random.Next(hearingNotes.Length)],
                    ReminderSent = hStatus == HearingStatus.Scheduled && random.NextDouble() < 0.7,
                    CreatedAt = hDate.AddDays(-random.Next(5, 30))
                });
            }
        }
        db.Hearings.AddRange(hearings);

        await db.SaveChangesAsync();

        var docCategories = new[] { "Pleadings", "Evidence", "Court Orders", "Correspondence", "Research", "Administrative" };
        var fileTypes = new[] { "pdf", "jpg", "doc", "docx" };
        var docStatuses = new[] { DocumentStatus.Draft, DocumentStatus.Final, DocumentStatus.Filed };

        var documents = new List<Document>();
        var docIdx = 0;

        for (int i = 0; i < 80; i++)
        {
            docIdx++;
            var caseIdx = i % 120;
            var clientIdx = caseIdx % 50;
            var clientName = clientNames[clientIdx];

            var firstName = clientName.Replace("Md. ", "").Split(' ').First();
            var fileType = fileTypes[random.Next(fileTypes.Length)];
            var fileName = $"{firstName}_Document_{i + 1:D2}.{fileType}";
            var fileSize = random.Next(100, 5000) * 1024L;

            documents.Add(new Document
            {
                Id = Guid.Parse($"F0000001-0000-0000-0000-{docIdx:D12}"),
                FileName = fileName,
                OriginalFileName = fileName,
                FilePath = $"uploads/cases/{caseIds[caseIdx]:N}/{fileName}",
                FileType = fileType,
                FileSize = fileSize,
                Category = docCategories[random.Next(docCategories.Length)],
                FolderPath = docCategories[random.Next(docCategories.Length)],
                Status = docStatuses[random.Next(docStatuses.Length)],
                Version = random.Next(1, 4),
                CaseId = caseIds[caseIdx],
                UploadedById = allLawyerIds[random.Next(allLawyerIds.Length)],
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 800))
            });
        }
        db.Documents.AddRange(documents);

        await db.SaveChangesAsync();

        var invoices = new List<Invoice>();
        var invIdx = 0;
        var invoiceStatuses = new[] { PaymentStatus.Pending, PaymentStatus.Processing, PaymentStatus.Completed, PaymentStatus.Failed };

        for (int i = 0; i < 30; i++)
        {
            invIdx++;
            var clientIdx = i % 50;
            var caseIdx = i % 120;
            var invStatus = invoiceStatuses[random.Next(invoiceStatuses.Length)];

            invoices.Add(new Invoice
            {
                Id = Guid.Parse($"I0000001-0000-0000-0000-{invIdx:D12}"),
                InvoiceNumber = $"INV-2024-{invIdx:D4}",
                Amount = random.Next(5000, 200001) * (decimal)0.01,
                Currency = "BDT",
                Status = invStatus,
                Description = $"Professional fees for case {cases[caseIdx].CaseNumber}",
                DueDate = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 365)),
                PaidAt = invStatus == PaymentStatus.Completed
                    ? new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 365))
                    : null,
                ClientId = clientIds[clientIdx],
                CaseId = caseIds[caseIdx],
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 365))
            });
        }
        db.Invoices.AddRange(invoices);

        await db.SaveChangesAsync();

        var expenses = new List<Expense>();
        var expIdx = 0;
        var expenseCategories = new[] { "Travel", "Filing Fees", "Stationery", "Witness Expenses", "Photocopy", "Miscellaneous" };

        for (int i = 0; i < 40; i++)
        {
            expIdx++;
            var caseIdx = i % 120;
            var userId = allLawyerIds[i % 4];

            expenses.Add(new Expense
            {
                Id = Guid.Parse($"E0000001-0000-0000-0000-{expIdx:D12}"),
                Description = $"{expenseCategories[i % expenseCategories.Length]} for case {cases[caseIdx].CaseNumber}",
                Amount = random.Next(500, 50001) * (decimal)0.01,
                Currency = "BDT",
                Category = expenseCategories[i % expenseCategories.Length],
                ExpenseDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 700)),
                ChamberId = DefaultChamberId,
                CaseId = caseIds[caseIdx],
                UserId = userId,
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 700))
            });
        }
        db.Expenses.AddRange(expenses);

        await db.SaveChangesAsync();

        var tasks = new List<Domain.Entities.Task>();
        var taskIdx = 0;
        var taskStatuses = new[] { TaskStatus.Pending, TaskStatus.InProgress, TaskStatus.Completed, TaskStatus.Cancelled };
        var taskPriorities = new[] { "Low", "Medium", "High", "Urgent" };

        for (int i = 0; i < 50; i++)
        {
            taskIdx++;
            var caseIdx = i % 120;
            var assignerId = allLawyerIds[i % 4];
            var assigneeId = allLawyerIds[(i + 1) % 4];

            tasks.Add(new Domain.Entities.Task
            {
                Id = Guid.Parse($"T0000001-0000-0000-0000-{taskIdx:D12}"),
                Title = new[] { "File review", "Draft pleading", "Collect evidence", "Client meeting", "Court appearance preparation", "Legal research" }[i % 6],
                Description = $"Task related to case {cases[caseIdx].CaseNumber}",
                DueDate = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 700)),
                Status = taskStatuses[random.Next(taskStatuses.Length)],
                Priority = taskPriorities[random.Next(taskPriorities.Length)],
                AssignedTo = assigneeId,
                AssignedBy = assignerId,
                CaseId = caseIds[caseIdx],
                ChamberId = DefaultChamberId,
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 365))
            });
        }
        db.Tasks.AddRange(tasks);

        await db.SaveChangesAsync();

        var allUserIds = new[] { adminId, lawyer1Id, lawyer2Id, lawyer3Id, lawyer4Id };
        var notificationTemplates = new[]
        {
            ("New Hearing Scheduled", "A hearing has been scheduled for case {0} on {1:dd MMM yyyy}."),
            ("Case Updated", "Case {0} status has been updated to {1}."),
            ("Document Uploaded", "A new document has been uploaded for case {0}."),
            ("Hearing Reminder", "Reminder: Hearing for case {0} is scheduled for tomorrow."),
            ("Case Assigned", "You have been assigned case {0}."),
            ("Case Closed", "Case {0} has been closed. Closing date: {1:dd MMM yyyy}."),
            ("Payment Received", "Payment of {0} BDT received for case {1}."),
            ("Client Added", "Client {0} has been added to case {1}.")
        };

        var notifications = new List<Notification>();
        var notifIdx = 0;

        for (int i = 0; i < 100; i++)
        {
            notifIdx++;
            var userId = allUserIds[i % 5];
            var caseRef = cases[i % 120];
            var (notifTitle, notifFormat) = notificationTemplates[i % notificationTemplates.Length];
            var msg = notifTitle switch
            {
                "New Hearing Scheduled" => string.Format(notifFormat, caseRef.CaseNumber, DateTime.UtcNow.AddDays(random.Next(1, 30))),
                "Case Updated" => string.Format(notifFormat, caseRef.CaseNumber, caseRef.Status),
                "Document Uploaded" => string.Format(notifFormat, caseRef.CaseNumber),
                "Hearing Reminder" => string.Format(notifFormat, caseRef.CaseNumber),
                "Case Assigned" => string.Format(notifFormat, caseRef.CaseNumber),
                "Case Closed" => string.Format(notifFormat, caseRef.CaseNumber, DateTime.UtcNow),
                "Payment Received" => string.Format(notifFormat, random.Next(5000, 50000) * 100, caseRef.CaseNumber),
                "Client Added" => string.Format(notifFormat, clientNames[i % 50], caseRef.CaseNumber),
                _ => notifFormat
            };

            var createdDate = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(random.Next(0, 780));

            notifications.Add(new Notification
            {
                Id = Guid.Parse($"N0000001-0000-0000-0000-{notifIdx:D12}"),
                UserId = userId,
                Title = notifTitle,
                Message = msg,
                Type = notifTitle switch
                {
                    "New Hearing Scheduled" => "hearing",
                    "Hearing Reminder" => "reminder",
                    "Case Updated" or "Case Assigned" or "Case Closed" => "case",
                    "Document Uploaded" => "document",
                    "Payment Received" => "payment",
                    "Client Added" => "client",
                    _ => "general"
                },
                IsRead = i % 3 == 0,
                ReferenceId = caseRef.Id.ToString(),
                CreatedAt = createdDate
            });
        }
        db.Notifications.AddRange(notifications);

        await db.SaveChangesAsync();
    }
}
