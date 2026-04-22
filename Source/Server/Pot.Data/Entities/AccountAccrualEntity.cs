using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pot.Data.Entities;

// Regarding [Index(nameof(AccruedIsDirty), nameof(LastAccruedDate))]
// This composite index is designed to optimize queries that filter by AccruedIsDirty and/or LastAccruedDate.
//
// Supports status-query access patterns that first constrain by dirty state and then evaluate
// date freshness. This helps for predicates such as:
// - AccruedIsDirty = true
// - AccruedIsDirty = false AND LastAccruedDate < asOfDate
//
// The current status rule is OR-based (dirty OR never-accrued OR stale-date), so this composite
// index may not be the final optimum for every workload. Keep this in place for initial rollout,
// then re-evaluate with EXPLAIN ANALYZE on production-like data. If plans favor alternatives,
// consider replacing with:
// - an index on LastAccruedDate, and/or
// - a partial index for rows where AccruedIsDirty = true.

[Index(nameof(AccountId), IsUnique = true)]
[Index(nameof(AccruedIsDirty), nameof(LastAccruedDate))]
public sealed class AccountAccrualEntity : EntityBase
{
    [ForeignKey(nameof(Account))]
    public int AccountId { get; set; }

    public bool AccruedIsDirty { get; set; } = true;
    public DateOnly? LastAccruedDate { get; set; }

    // Not marked as 'required' so FK-only insert paths can set AccountId without assigning
    // the navigation and inadvertently triggering EF graph attachment side effects.
    public AccountEntity Account { get; set; } = null!;
}