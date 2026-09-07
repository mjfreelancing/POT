import { describe, expect, test } from 'vitest';

import { budgetRemindersSchema } from '@/features/userSettings/sections/budgetReminders/budgetRemindersSchema';

import { flattenIssues } from '../../../../shared/schemaAssertions';

describe('budgetRemindersSchema', () => {
  test('accepts valid reminder settings', () => {
    const result = budgetRemindersSchema.safeParse({
      enabled: true,
      reminderDays: 5,
      localHourTrigger: 12,
    });

    expect(result.success).toBe(true);
  });

  test('accepts a disabled reminder with zeroed fields', () => {
    const result = budgetRemindersSchema.safeParse({
      enabled: false,
      reminderDays: 0,
      localHourTrigger: 0,
    });

    expect(result.success).toBe(true);
  });

  test('requires reminder days when the input is empty', () => {
    const result = budgetRemindersSchema.safeParse({
      enabled: true,
      reminderDays: '',
      localHourTrigger: 12,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'reminderDays',
      message: 'Reminder days is required',
    });
  });

  test('requires reminder days to be between 0 and 31', () => {
    const result = budgetRemindersSchema.safeParse({
      enabled: true,
      reminderDays: 32,
      localHourTrigger: 12,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'reminderDays',
      message: 'Reminder days must be between 0 and 31',
    });
  });

  test('requires reminder days to be a whole number', () => {
    const result = budgetRemindersSchema.safeParse({
      enabled: true,
      reminderDays: 1.5,
      localHourTrigger: 12,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'reminderDays',
      message: 'Reminder days must be a whole number',
    });
  });

  test('requires the hour to be between 0 and 23', () => {
    const result = budgetRemindersSchema.safeParse({
      enabled: true,
      reminderDays: 5,
      localHourTrigger: 24,
    });

    expect(flattenIssues(result)).toContainEqual({
      path: 'localHourTrigger',
      message: 'Reminder hour must be between 0 and 23',
    });
  });
});
