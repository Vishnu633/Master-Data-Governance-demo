using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
using Microsoft.EntityFrameworkCore;

namespace Hofinsoft.Mdg.Services
{
    public class NomBotService
    {
        private readonly NomcatDbContext _db;
        private readonly GeminiService _gemini;

        public NomBotService(NomcatDbContext db, GeminiService gemini)
        {
            _db = db;
            _gemini = gemini;
        }

        public async Task<NomBotResponse> GetAnswerAsync(string message)
        {
            var cleanMsg = message.Trim().ToLower();

            // ── Fast-path: NMSR ticket lookup ──
            var ticketMatch = Regex.Match(cleanMsg, @"nmsr/\d{4}/\d{2}/\d{4}");
            if (ticketMatch.Success)
            {
                var ticket = ticketMatch.Value.ToUpper();
                var req = await _db.ItemRequests.FirstOrDefaultAsync(r => r.RequestRefNo == ticket);
                if (req == null)
                {
                    return new NomBotResponse
                    {
                        Reply = $"I couldn't find any material request with ticket number **{ticket}** in our staging database."
                    };
                }

                return new NomBotResponse
                {
                    Reply = $"Here are the details for **{ticket}**:\n" +
                            $"- **Noun/Modifier**: {req.Noun} — {req.Modifier}\n" +
                            $"- **Description**: `{req.ShortDescription}`\n" +
                            $"- **Status**: `{req.ApprovalStatus}`\n" +
                            $"- **Current Stage**: Stage {req.CurrentStage} of {req.TotalStages} (Owner: *{req.CurrentOwnerRole}*)\n" +
                            $"- **Plant**: {req.Plant}\n" +
                            $"- **Created On**: {req.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC"
                    };
            }

            // ── Fast-path: MAT code lookup ──
            var matMatch = Regex.Match(cleanMsg, @"mat-\d{6}");
            if (matMatch.Success)
            {
                var matCode = matMatch.Value.ToUpper();
                var record = await _db.GoldenMasterCatalog.FirstOrDefaultAsync(g => g.MaterialNumber == matCode);
                if (record == null)
                {
                    return new NomBotResponse
                    {
                        Reply = $"I couldn't find any Golden Record with SAP code **{matCode}** in our catalog."
                    };
                }

                return new NomBotResponse
                {
                    Reply = $"Here is the Golden Master Record for **{matCode}**:\n" +
                            $"- **Noun/Modifier**: {record.Noun} — {record.Modifier}\n" +
                            $"- **Standard Nomenclature**: `{record.ShortDescription}`\n" +
                            $"- **Plant Assigned**: {record.Plant}\n" +
                            $"- **Source Request**: {record.SourceRequestRef}\n" +
                            $"- **Approved At**: {record.ApprovedAt:yyyy-MM-dd HH:mm:ss} UTC by *{record.ApprovedBy}*"
                };
            }

            // ── AI-powered chat via Gemini ──
            if (_gemini.IsConfigured)
            {
                var systemPrompt = await BuildSystemPromptAsync();
                var aiReply = await _gemini.ChatAsync(systemPrompt, message);
                return new NomBotResponse { Reply = aiReply };
            }

            // ── Fallback: keyword matching (when AI is not configured) ──
            return await KeywordFallbackAsync(cleanMsg);
        }

        private async Task<string> BuildSystemPromptAsync()
        {
            // Gather live stats
            var totalRequests = await _db.ItemRequests.CountAsync();
            var pendingRequests = await _db.ItemRequests.CountAsync(r =>
                r.ApprovalStatus == "Stage1_Validated" || r.ApprovalStatus == "In_Progress");
            var approvedRequests = await _db.ItemRequests.CountAsync(r =>
                r.ApprovalStatus == "Approved");
            var goldenRecords = await _db.GoldenMasterCatalog.CountAsync();

            // Gather available noun/modifier profiles
            var profiles = await _db.AttributeMaster
                .Select(a => new { a.Noun, a.Modifier })
                .Distinct()
                .ToListAsync();
            var profileList = string.Join(", ", profiles.Select(p => $"{p.Noun}/{p.Modifier}"));

            // Gather active Golden Catalog records details for context injection (RAG)
            var records = await _db.GoldenMasterCatalog
                .Select(g => new
                {
                    g.MaterialNumber,
                    g.Noun,
                    g.Modifier,
                    g.Plant,
                    g.ShortDescription,
                    g.JsonAttributeValues
                })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("You are NomBot, an AI assistant for the NOMCAT Master Data Governance system.");
            sb.AppendLine("You help users with material cataloging, governance workflows, and data quality.");
            sb.AppendLine();
            sb.AppendLine("=== Live System Stats ===");
            sb.AppendLine($"- Total material requests in staging: {totalRequests}");
            sb.AppendLine($"- Pending/In-Progress requests: {pendingRequests}");
            sb.AppendLine($"- Approved requests: {approvedRequests}");
            sb.AppendLine($"- Golden Master Records in catalog: {goldenRecords}");
            sb.AppendLine();
            sb.AppendLine("=== Available Noun/Modifier Profiles ===");
            sb.AppendLine(profileList);
            sb.AppendLine();
            sb.AppendLine("=== Golden Master Catalog Records ===");
            sb.AppendLine("Below is the list of all active materials in our catalog. Use this data to answer specific queries:");
            foreach (var r in records)
            {
                sb.AppendLine($"- Material: {r.MaterialNumber} | Plant: {r.Plant} | Noun: {r.Noun} | Modifier: {r.Modifier} | Standard Nomenclature: {r.ShortDescription} | Classified Attributes: {r.JsonAttributeValues}");
            }
            sb.AppendLine();
            sb.AppendLine("Format responses in markdown. Be concise but informative. If asked to count specific items, inspect the records list above and calculate the count accurately.");

            return sb.ToString();
        }

        private async Task<NomBotResponse> KeywordFallbackAsync(string cleanMsg)
        {
            if (cleanMsg.Contains("how many") || cleanMsg.Contains("count"))
            {
                string? targetNoun = null;
                string? targetModifier = null;

                if (cleanMsg.Contains("bearing")) targetNoun = "BEARING";
                if (cleanMsg.Contains("bolt")) targetNoun = "BOLT";
                if (cleanMsg.Contains("ball")) targetModifier = "BALL";
                if (cleanMsg.Contains("stud")) targetModifier = "STUD";

                var query = _db.GoldenMasterCatalog.AsQueryable();
                if (targetNoun != null) query = query.Where(g => g.Noun == targetNoun);
                if (targetModifier != null) query = query.Where(g => g.Modifier == targetModifier);

                var candidates = await query.ToListAsync();

                string? searchVal = null;
                string? searchAttr = null;

                // 1. Thread pattern: M12, M6, etc.
                var threadMatch = Regex.Match(cleanMsg, @"\bm\d+\b");
                if (threadMatch.Success)
                {
                    searchVal = threadMatch.Value;
                    searchAttr = "Thread";
                }

                // 2. Millimeter dimensions: 35mm, 52mm, 50mm, etc.
                var mmMatch = Regex.Match(cleanMsg, @"\b\d+mm\b");
                if (mmMatch.Success)
                {
                    searchVal = mmMatch.Value;
                    if (cleanMsg.Contains("inside") || cleanMsg.Contains("id") || cleanMsg.Contains("inner"))
                        searchAttr = "Inside_Diameter";
                    else if (cleanMsg.Contains("outside") || cleanMsg.Contains("od") || cleanMsg.Contains("outer"))
                        searchAttr = "Outside_Diameter";
                    else if (cleanMsg.Contains("length") || cleanMsg.Contains("long"))
                        searchAttr = "Length";
                    else
                    {
                        if (targetNoun == "BEARING") searchAttr = "Inside_Diameter";
                        else if (targetNoun == "BOLT") searchAttr = "Length";
                    }
                }

                // 3. Material keywords
                string[] materials = { "stainless steel", "carbon steel", "alloy steel", "chrome steel", "steel", "ceramic", "bronze", "brass" };
                foreach (var mat in materials)
                {
                    if (cleanMsg.Contains(mat))
                    {
                        searchVal = mat;
                        searchAttr = "Material";
                        break;
                    }
                }

                // 4. Plant codes
                string? plantFilter = null;
                var plantMatch = Regex.Match(cleanMsg, @"\bplt\d+\b");
                if (plantMatch.Success)
                {
                    plantFilter = plantMatch.Value.ToUpper();
                }

                var matches = new List<GoldenMasterRecord>();
                foreach (var c in candidates)
                {
                    if (plantFilter != null && c.Plant != plantFilter) continue;

                    if (searchVal != null && searchAttr != null)
                    {
                        try
                        {
                            var attrs = JsonSerializer.Deserialize<Dictionary<string, string>>(c.JsonAttributeValues);
                            if (attrs != null && attrs.TryGetValue(searchAttr, out var val))
                            {
                                if (val.Equals(searchVal, StringComparison.OrdinalIgnoreCase) || 
                                    val.Contains(searchVal, StringComparison.OrdinalIgnoreCase))
                                {
                                    matches.Add(c);
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        matches.Add(c);
                    }
                }

                string nounDisplay = targetNoun ?? "item";
                if (targetModifier != null) nounDisplay = $"{targetModifier} {nounDisplay}";
                
                string filterDesc = "";
                if (searchAttr != null && searchVal != null)
                    filterDesc = $" with **{searchAttr}** containing **'{searchVal}'**";
                if (plantFilter != null)
                    filterDesc += $" at plant **{plantFilter}**";

                var sb = new StringBuilder();
                sb.AppendLine($"I found **{matches.Count}** Golden Record(s) matching **{nounDisplay}**s{filterDesc}:");
                if (matches.Count > 0)
                {
                    foreach (var m in matches.Take(5))
                    {
                        sb.AppendLine($"- **{m.MaterialNumber}** ({m.Plant}): `{m.ShortDescription}`");
                    }
                    if (matches.Count > 5)
                    {
                        sb.AppendLine($"- ... and **{matches.Count - 5}** more records.");
                    }
                }
                else
                {
                    sb.AppendLine("No matching catalog items were found in the database.");
                }

                return new NomBotResponse { Reply = sb.ToString() };
            }

            if (cleanMsg.Contains("pending") || cleanMsg.Contains("stage") || cleanMsg.Contains("pipeline"))
            {
                var pendingCount = await _db.ItemRequests.CountAsync(r => r.ApprovalStatus == "Stage1_Validated" || r.ApprovalStatus == "In_Progress");
                var list = await _db.ItemRequests
                    .Where(r => r.ApprovalStatus == "Stage1_Validated" || r.ApprovalStatus == "In_Progress")
                    .Take(5)
                    .Select(r => $"- `{r.RequestRefNo}`: {r.Noun}/{r.Modifier} (Stage {r.CurrentStage}/{r.TotalStages}, Owner: {r.CurrentOwnerRole})")
                    .ToListAsync();

                var reply = $"There are currently **{pendingCount}** active requests in the governance pipeline.";
                if (list.Count > 0)
                {
                    reply += "\n\nHere are the most recent active requests:\n" + string.Join("\n", list);
                }
                return new NomBotResponse { Reply = reply };
            }

            if (cleanMsg.Contains("golden") || cleanMsg.Contains("master") || cleanMsg.Contains("catalog") || cleanMsg.Contains("records"))
            {
                var count = await _db.GoldenMasterCatalog.CountAsync();
                var list = await _db.GoldenMasterCatalog
                    .Take(5)
                    .Select(r => $"- **{r.MaterialNumber}** ({r.Plant}): `{r.ShortDescription}`")
                    .ToListAsync();

                var reply = $"There are **{count}** Golden Master Records in the production catalog.";
                if (list.Count > 0)
                {
                    reply += "\n\nHere are the most recent records:\n" + string.Join("\n", list);
                }
                return new NomBotResponse { Reply = reply };
            }

            if (cleanMsg.Contains("bearing") || cleanMsg.Contains("bearings"))
            {
                var stagingCount = await _db.ItemRequests.CountAsync(r => r.Noun == "BEARING");
                var goldenCount = await _db.GoldenMasterCatalog.CountAsync(r => r.Noun == "BEARING");
                return new NomBotResponse
                {
                    Reply = $"I found **{goldenCount}** Golden Record(s) and **{stagingCount}** staging request(s) categorized under Noun: **BEARING**."
                };
            }

            if (cleanMsg.Contains("bolt") || cleanMsg.Contains("bolts"))
            {
                var stagingCount = await _db.ItemRequests.CountAsync(r => r.Noun == "BOLT");
                var goldenCount = await _db.GoldenMasterCatalog.CountAsync(r => r.Noun == "BOLT");
                return new NomBotResponse
                {
                    Reply = $"I found **{goldenCount}** Golden Record(s) and **{stagingCount}** staging request(s) categorized under Noun: **BOLT**."
                };
            }

            if (cleanMsg.Contains("plant extension") || cleanMsg.Contains("plant") || cleanMsg.Contains("extension"))
            {
                return new NomBotResponse
                {
                    Reply = "💡 **Master Data Governance Knowledge Base:**\n\n" +
                            "**Plant Extension** is an SAP data management concept where a material classification (defined by its Noun, Modifier, and Attributes) is extended to another plant code. \n\n" +
                            "In NOMCAT: \n" +
                            "- The exact same attribute values **can** be used by multiple plants (they share the same SAP code).\n" +
                            "- The Duplicate Detector will block duplicate requests at the *same* plant, but *allows* them across *different* plants if they exist in the Golden Catalog. \n" +
                            "- Plant Extensions only use a compressed **3-stage pipeline** (Requester → Approver → Central Cataloger)."
                };
            }

            if (cleanMsg.Contains("hi") || cleanMsg.Contains("hello") || cleanMsg.Contains("hey") || cleanMsg.Contains("help"))
            {
                return new NomBotResponse
                {
                    Reply = "👋 Hello! I'm **NomBot**, your NOMCAT Master Data Governance Assistant.\n\n" +
                            "I can help you monitor requests and catalog status. Try asking me:\n" +
                            "- *'How many pending requests do we have?'*\n" +
                            "- *'Show me the golden records in catalog.'*\n" +
                            "- *'Explain what a plant extension is.'*\n" +
                            "- *'What is the status of NMSR/2026/06/0001?'* (replace with a real ticket)\n" +
                            "- *'Details for MAT-000001'* (replace with a real SAP code)"
                };
            }

            // General fall-back
            return new NomBotResponse
            {
                Reply = "I'm not sure how to answer that question. You can ask me about pending requests, catalog records, specific tickets (e.g., `NMSR/2026/06/0001`), SAP codes (e.g., `MAT-000001`), or plant extensions."
            };
        }
    }

    public class NomBotResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}
