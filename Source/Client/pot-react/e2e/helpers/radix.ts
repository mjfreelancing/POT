import type { Locator } from '@playwright/test';
import { expect } from '@playwright/test';

/**
 * Shared helpers for Radix / shadcn overlay controls (currently `Select`).
 *
 * These controls cannot be driven with pointer events reliably: Radix selects an
 * item on pointerup only when a same-node pointerdown set its internal
 * `pointerTypeRef` to `'mouse'`, and `locator.evaluate()` has no actionability
 * retry, so a synthetic click is silently dropped while the option node is
 * mid-positioning. Both failures look like "the dropdown stayed open". The
 * helpers here drive Radix's own keyboard/AT activation path instead.
 */

/**
 * Selects an open Radix `Select` option via the AT/keyboard activation path.
 *
 * The caller opens the trigger first (an ordinary button click on the
 * `combobox`), then passes the target option — plus, when the select already has
 * a value, that value's option locator so the helper can gate on Radix's
 * open-time focus steering having settled. The steps are:
 *   1. wait for focus steering to settle — the selected option receives
 *      `data-highlighted`;
 *   2. focus the target and PROVE focus landed (`toBeFocused`), closing the
 *      window where Radix's async `focusFirst()` could steal it;
 *   3. activate with `Enter` -> `SelectItem` onKeyDown -> `handleSelect()`,
 *      Radix's own unit-tested activation path.
 *
 * Every step uses a retrying Playwright primitive, so the helper is independent
 * of machine speed and load.
 *
 * @param option The option to select.
 * @param selectedOption The option carrying the select's current value, used as
 * the focus-steering settle gate. Omit it when the select has no value yet (for
 * example a create form's placeholder), since nothing is highlighted on open.
 */
export async function selectRadixOption(
  option: Locator,
  selectedOption?: Locator,
): Promise<void> {
  await expect(option).toBeAttached();
  await expect(option).toBeVisible();

  // Gate on Radix's open-time focus steering settling (the selected option is
  // highlighted). Without this, Enter can land before the async focusFirst()
  // completes and be delivered to the wrong node.
  if (selectedOption) {
    await expect(selectedOption).toHaveAttribute('data-highlighted', '');
  }

  // Focus the target, prove it, then activate via Enter.
  await option.focus();
  await expect(option).toBeFocused();
  await option.press('Enter');

  // Fail-fast: handleSelect also closes the dropdown. If it never ran, the
  // option stays visible (dropdown still open).
  await expect(option).toBeHidden({ timeout: 5_000 });
}
