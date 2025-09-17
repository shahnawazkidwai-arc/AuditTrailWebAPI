using audit_trail_.NET_Core_Web_API.Models;
using audit_trail_.NET_Core_Web_API.Repositories;
using System.Text.Json;

namespace audit_trail_.NET_Core_Web_API.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repo;

        public AuditService(IAuditRepository repo)
        {
            _repo = repo;
        }

        public async Task<AuditRecord> CreateAuditAsync(AuditRequest request, CancellationToken cancellationToken = default)
        {
            var timestamp = request.Timestamp ?? DateTime.UtcNow;

            var changes = ComputeChanges(request.Before, request.After);

            var rec = new AuditRecord
            {
                EntityName = request.EntityName,
                Action = request.Action,
                UserId = request.UserId,
                Timestamp = timestamp,
                Changes = changes
            };

            await _repo.SaveAsync(rec, cancellationToken);
            return rec;
        }

        private List<AuditChange> ComputeChanges(JsonElement? before, JsonElement? after)
        {
            var result = new List<AuditChange>();

            if (before == null && after == null)
                return result;

            // If both present -> compare recursively.
            // If one null -> generate change for entire object (or top-level properties).
            if (before.HasValue && after.HasValue)
            {
                CompareJsonElements(before.Value, after.Value, "", result);
            }
            else if (!before.HasValue && after.HasValue)
            {
                // Created -> record all top-level properties as changed from null to value
                foreach (var prop in after.Value.EnumerateObject())
                {
                    result.Add(new AuditChange
                    {
                        Path = prop.Name,
                        Before = null,
                        After = prop.Value.GetRawText()
                    });
                }
            }
            else // before has value and after is null -> Deleted
            {
                foreach (var prop in before.Value.EnumerateObject())
                {
                    result.Add(new AuditChange
                    {
                        Path = prop.Name,
                        Before = prop.Value.GetRawText(),
                        After = null
                    });
                }
            }

            return result;
        }

        private void CompareJsonElements(JsonElement before, JsonElement after, string path, List<AuditChange> output)
        {
            // If kinds differ or both primitive with different raw text => record change
            if (before.ValueKind != after.ValueKind)
            {
                output.Add(new AuditChange { Path = path, Before = before.GetRawText(), After = after.GetRawText() });
                return;
            }

            switch (before.ValueKind)
            {
                case JsonValueKind.Object:
                    {
                        var beforeProps = before.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                        var afterProps = after.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                        var allKeys = new HashSet<string>(beforeProps.Keys, StringComparer.OrdinalIgnoreCase);
                        allKeys.UnionWith(afterProps.Keys);

                        foreach (var key in allKeys)
                        {
                            var propPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";

                            var hasBefore = beforeProps.TryGetValue(key, out var bVal);
                            var hasAfter = afterProps.TryGetValue(key, out var aVal);

                            if (hasBefore && hasAfter)
                            {
                                CompareJsonElements(bVal, aVal, propPath, output);
                            }
                            else if (hasBefore && !hasAfter)
                            {
                                // property removed
                                output.Add(new AuditChange { Path = propPath, Before = bVal.GetRawText(), After = null });
                            }
                            else if (!hasBefore && hasAfter)
                            {
                                // property added
                                output.Add(new AuditChange { Path = propPath, Before = null, After = aVal.GetRawText() });
                            }
                        }
                        break;
                    }

                case JsonValueKind.Array:
                    {
                        // For arrays: do simple strategy: if lengths differ or any index differs -> record whole array change
                        var beforeArray = before.EnumerateArray().ToArray();
                        var afterArray = after.EnumerateArray().ToArray();

                        if (beforeArray.Length != afterArray.Length)
                        {
                            output.Add(new AuditChange { Path = path, Before = before.GetRawText(), After = after.GetRawText() });
                            return;
                        }

                        for (int i = 0; i < beforeArray.Length; i++)
                        {
                            var b = beforeArray[i];
                            var a = afterArray[i];

                            // If primitive mismatch or different structure - treat as changed (could be more granular if needed)
                            if (!JsonElementRawTextEquals(b, a))
                            {
                                output.Add(new AuditChange { Path = path + $"[{i}]", Before = b.GetRawText(), After = a.GetRawText() });
                            }
                        }
                        break;
                    }

                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                default:
                    {
                        if (!JsonElementRawTextEquals(before, after))
                        {
                            output.Add(new AuditChange { Path = path, Before = before.GetRawText(), After = after.GetRawText() });
                        }
                        break;
                    }
            }
        }

        // Normalized raw text equality for primitive and objects: using GetRawText compare
        private bool JsonElementRawTextEquals(JsonElement a, JsonElement b)
        {
            // Using GetRawText ensures numbers/strings/booleans/null are represented consistently
            return string.Equals(a.GetRawText(), b.GetRawText(), StringComparison.Ordinal);
        }
    }
}
