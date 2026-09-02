/**
 * User Settings Sheet - Visual Design System
 *
 * Accordion Section Styling Pattern:
 * -------------------------------
 * 1. Container Structure
 *    - Uses rounded-lg for consistent corner radius
 *    - border-border/40 creates a subtle border (40% opacity for depth without harshness)
 *    - shadow-sm adds minimal elevation for depth
 *
 * 2. Background Effects & Interactions
 *    - bg-gradient-to-br creates a subtle bottom-right gradient
 *    - from-blue-500/5 to-blue-500/10 gives depth with very low opacity (5-10%)
 *    - hover states increase opacity (10-15%) for interactive feedback
 *    - Uses brand blue to maintain consistency with main dashboard
 *    - Elements start at 80% opacity and increase to 100% on hover
 *    - Subtle brightening of icon backgrounds on hover
 *    - Removes default accordion underline for cleaner interaction
 *    - transition-all ensures smooth state changes
 *
 * 3. Icon Styling
 *    - p-2 padding creates comfortable space around icons
 *    - rounded-md on icon container for subtle separation
 *    - bg-blue-500/10 matches gradient theme with slightly higher opacity
 *    - text-blue-400 for icon color aligns with application's primary brand color
 *
 * 4. Typography
 *    - text-lg + font-semibold for section headers
 *    - text-sm + text-muted-foreground for descriptive text
 *    - items-start ensures left alignment of multi-line text
 *
 * 5. Spacing
 *    - gap-3 between icon and text for clear visual grouping
 *    - py-3 between sections balances separation and compactness
 *    - px-4 for consistent horizontal padding
 *    - pt-2 on content adds subtle spacing after header
 *
 * Brand Integration:
 * - Uses the application's primary blue color scheme
 * - Matches the visual language of the main dashboard
 * - Creates cohesion with other UI elements like the Pay On Time logo
 * - Maintains consistent color hierarchy across the application
 *
 * Visual Goals:
 * - Create depth without heavy shadows or stark contrasts
 * - Maintain dark theme aesthetics while ensuring visibility
 * - Provide subtle but clear interactive feedback
 * - Use consistent spacing patterns for visual rhythm
 * - Ensure accessibility with sufficient text contrast
 * - Reinforce brand identity through consistent color usage
 */
import {
  Building2,
  CalendarClock,
  KeyIcon,
  UserIcon,
  XIcon,
} from 'lucide-react';
import type { JSX } from 'react';
import { useEffect, useRef, useState } from 'react';

import ErrorSheet from '@/components/feedback/sheet/ErrorSheet';
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription as ModalDescription,
  DialogTitle as ModalTitle,
} from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Sheet, SheetContent } from '@/components/ui/sheet';
import { useErrorContext } from '@/contexts';
import type { SettingsSectionFormHandle } from '@/features/userSettings/sections/settingsSectionForm';
import { usePermissions } from '@/hooks';

import BudgetRemindersForm from './sections/budgetReminders/BudgetRemindersForm';
import ChangePasswordForm from './sections/changePassword/ChangePasswordForm';
import SiteDetailsForm from './sections/siteDetails/SiteDetailsForm';
import UserDetailsForm from './sections/userDetails/UserDetailsForm';

type AccountSettingsSheetProps = {
  open: boolean;
  onClose: () => void;
  onOpenChange?: (open: boolean) => void;
};

type SettingsSectionValue =
  | ''
  | 'site-settings'
  | 'user-details'
  | 'change-password'
  | 'budget-reminders';

type PendingNavigationAction =
  | { type: 'close' }
  | { type: 'section'; target: SettingsSectionValue };

const SECTION_LABELS: Record<Exclude<SettingsSectionValue, ''>, string> = {
  'site-settings': 'Site Details',
  'user-details': 'User Details',
  'change-password': 'Change Password',
  'budget-reminders': 'Budget Reminders',
};

function AccountSettingsSheet(props: AccountSettingsSheetProps): JSX.Element {
  const { open, onClose, onOpenChange } = props;
  const { hasPermission } = usePermissions();
  const canManageSite = hasPermission('site:manage');
  const { error, setError } = useErrorContext();

  // Tracks which accordion section is currently open. Only one section is open
  // at a time; an empty string means no section is open.
  const [activeSection, setActiveSection] = useState<SettingsSectionValue>('');

  // Tracks whether the currently open section has unsaved edits. Only one
  // section can be open at a time, so a single boolean is sufficient — there
  // is no scenario where two sections are dirty simultaneously.
  const [isActiveSectionDirty, setIsActiveSectionDirty] = useState(false);

  // When the user tries to navigate away from a dirty section, the intended
  // action is stored here so it can be completed after the user resolves the
  // unsaved-changes prompt.
  const [pendingAction, setPendingAction] =
    useState<PendingNavigationAction | null>(null);
  const [isUnsavedChangesDialogOpen, setIsUnsavedChangesDialogOpen] =
    useState(false);

  // Set to true while awaiting the async save inside handleSaveChanges so the
  // dialog buttons are disabled and cannot be double-clicked.
  const [isResolvingUnsavedChanges, setIsResolvingUnsavedChanges] =
    useState(false);

  // Refs give the sheet imperative access to each form's submit() and discard()
  // without needing to lift form state into the sheet.
  const siteDetailsFormRef = useRef<SettingsSectionFormHandle | null>(null);
  const userDetailsFormRef = useRef<SettingsSectionFormHandle | null>(null);
  const changePasswordFormRef = useRef<SettingsSectionFormHandle | null>(null);
  const budgetRemindersFormRef = useRef<SettingsSectionFormHandle | null>(null);

  // Reset all tracking state when the sheet closes so that reopening the sheet
  // always starts clean — no leftover dirty flags or pending actions from the
  // previous session.
  useEffect(() => {
    if (open) {
      return;
    }

    setActiveSection('');
    setIsActiveSectionDirty(false);
    setPendingAction(null);
    setIsUnsavedChangesDialogOpen(false);
    setIsResolvingUnsavedChanges(false);
  }, [open]);

  // Called by each form's onDirtyChange callback whenever its dirty state
  // changes. The equality guard prevents a redundant state update, which would
  // otherwise cause the parent to re-render and re-trigger the child effect that
  // calls onDirtyChange — creating an infinite loop.
  function updateDirtyState(isDirty: boolean) {
    setIsActiveSectionDirty(current => {
      if (current === isDirty) {
        return current;
      }

      return isDirty;
    });
  }

  function getSectionFormHandle(
    section: SettingsSectionValue,
  ): SettingsSectionFormHandle | null {
    if (section === 'site-settings') {
      return siteDetailsFormRef.current;
    }

    if (section === 'user-details') {
      return userDetailsFormRef.current;
    }

    if (section === 'change-password') {
      return changePasswordFormRef.current;
    }

    if (section === 'budget-reminders') {
      return budgetRemindersFormRef.current;
    }

    return null;
  }

  function closeUnsavedChangesDialog() {
    setIsUnsavedChangesDialogOpen(false);
    setPendingAction(null);
  }

  function continuePendingAction(action: PendingNavigationAction) {
    if (action.type === 'close') {
      closeUnsavedChangesDialog();
      onOpenChange?.(false);
      onClose();

      return;
    }

    setActiveSection(action.target);
    closeUnsavedChangesDialog();
  }

  function promptForUnsavedChanges(action: PendingNavigationAction) {
    setPendingAction(action);
    setIsUnsavedChangesDialogOpen(true);
  }

  // Fired by the Accordion when the user opens a different section. If the
  // current section is dirty, navigation is intercepted and the unsaved-changes
  // dialog is shown. The target section is saved as the pending action and only
  // applied after the user resolves the dialog.
  function handleSectionChange(nextSection: string) {
    const targetSection = nextSection as SettingsSectionValue;

    if (targetSection === activeSection) {
      return;
    }

    if (isActiveSectionDirty) {
      promptForUnsavedChanges({ type: 'section', target: targetSection });

      return;
    }

    setActiveSection(targetSection);
  }

  // Fired by the custom close button. Applies the same dirty-state gate as
  // section navigation — closing is treated as a pending action so the same
  // dialog and resolution flow handles it.
  function requestSheetClose() {
    if (isActiveSectionDirty) {
      promptForUnsavedChanges({ type: 'close' });

      return;
    }

    onOpenChange?.(false);
    onClose();
  }

  // 'Save Changes' path in the unsaved-changes dialog.
  //
  // Calls the active form's imperative submit(), which runs the form's own
  // validation and API mutation. The possible results are:
  //   'saved'   — save succeeded; clear dirty state and continue the pending action.
  //   'blocked' — save was attempted but navigation should not proceed (e.g. the
  //               password form redirects to re-auth after a successful change).
  //   'invalid' — form validation failed; leave the dialog open so the user can
  //               correct the inputs.
  //
  // In all non-saved cases the dialog is closed and the user remains on the
  // current section.
  async function handleSaveChanges() {
    if (!pendingAction || activeSection === '') {
      closeUnsavedChangesDialog();

      return;
    }

    const activeFormHandle = getSectionFormHandle(activeSection);

    if (!activeFormHandle) {
      closeUnsavedChangesDialog();

      return;
    }

    setIsResolvingUnsavedChanges(true);

    try {
      const submitResult = await activeFormHandle.submit();

      if (submitResult === 'saved') {
        updateDirtyState(false);
        continuePendingAction(pendingAction);

        return;
      }

      closeUnsavedChangesDialog();
    } finally {
      setIsResolvingUnsavedChanges(false);
    }
  }

  // 'Discard Changes' path in the unsaved-changes dialog.
  //
  // Calls the active form's imperative discard(), which resets the form to the
  // last saved values. Dirty state is then explicitly cleared here so the sheet
  // does not prompt again, and the pending action is continued immediately.
  function handleDiscardChanges() {
    if (!pendingAction || activeSection === '') {
      closeUnsavedChangesDialog();

      return;
    }

    getSectionFormHandle(activeSection)?.discard();
    updateDirtyState(false);
    continuePendingAction(pendingAction);
  }

  function getUnsavedChangesDescription(): string {
    if (activeSection === '') {
      return 'You have unsaved changes. What would you like to do?';
    }

    const sectionName = SECTION_LABELS[activeSection];

    if (activeSection === 'change-password') {
      return 'You have unsaved password changes. Changing your password will require you to sign in again.';
    }

    if (pendingAction?.type === 'close') {
      return `You have unsaved changes in ${sectionName}. Save before closing?`;
    }

    return `You have unsaved changes in ${sectionName}. Save before leaving this section?`;
  }

  function getSaveActionLabel(): string {
    if (activeSection === 'change-password') {
      return 'Change Password';
    }

    return 'Save Changes';
  }

  return (
    <>
      <Sheet open={open} modal>
        <SheetContent
          side="right"
          className="w-full sm:max-w-sm flex flex-col [&>button:first-of-type]:hidden"
        >
          {/* Sticky header — sits outside the scroll container so it remains visible when scrolling */}
          <div className="flex items-center justify-between px-6 pt-6 pb-4 shrink-0">
            <DialogTitle className="text-lg font-semibold">
              POT Settings
            </DialogTitle>
            <DialogDescription className="sr-only">
              Manage your POT settings
            </DialogDescription>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Close POT settings"
              onClick={requestSheetClose}
            >
              <XIcon className="size-5" />
            </Button>
          </div>

          <Separator className="mx-6" />

          {error && (
            <ErrorSheet
              title={error.title}
              description={error.description}
              onDismiss={() => setError(null)}
            />
          )}

          {/* Scrollable content area */}
          <div className="flex-1 overflow-y-auto px-6 pb-6 pt-6">
            <Accordion
              type="single"
              collapsible
              value={activeSection}
              onValueChange={handleSectionChange}
            >
              {(hasPermission('site:view') || hasPermission('site:manage')) && (
                <>
                  <div className="border border-border/40 rounded-lg bg-gradient-to-br from-blue-500/5 to-blue-500/10 hover:from-blue-500/10 hover:to-blue-500/15 transition-all shadow-sm">
                    <AccordionItem className="px-4" value="site-settings">
                      <AccordionTrigger className="text-lg font-semibold text-primary [&[data-state=open]>div]:text-primary/90 hover:no-underline [&>div]:transition-all group">
                        <div className="flex items-center gap-3 opacity-80 group-hover:opacity-100">
                          <div className="p-2 rounded-md bg-blue-500/10 transition-colors group-hover:bg-blue-500/15">
                            <Building2 className="size-5 text-blue-400" />
                          </div>
                          <div className="flex flex-col items-start">
                            <span>Site Details</span>
                            <span className="text-sm font-normal text-muted-foreground group-hover:text-muted-foreground/80">
                              Customize your site name and description
                            </span>
                          </div>
                        </div>
                      </AccordionTrigger>
                      <AccordionContent className="flex flex-col gap-4 pt-2">
                        <SiteDetailsForm
                          ref={siteDetailsFormRef}
                          readonly={!canManageSite}
                          onDirtyChange={(isDirty: boolean) => {
                            updateDirtyState(isDirty);
                          }}
                        />
                      </AccordionContent>
                    </AccordionItem>
                  </div>

                  <div className="py-3" />
                </>
              )}

              <div className="border border-border/40 rounded-lg bg-gradient-to-br from-blue-500/5 to-blue-500/10 hover:from-blue-500/10 hover:to-blue-500/15 transition-all shadow-sm">
                <AccordionItem className="px-4" value="user-details">
                  <AccordionTrigger className="text-lg font-semibold text-primary [&[data-state=open]>div]:text-primary/90 hover:no-underline [&>div]:transition-all group">
                    <div className="flex items-center gap-3 opacity-80 group-hover:opacity-100">
                      <div className="p-2 rounded-md bg-blue-500/10 transition-colors group-hover:bg-blue-500/15">
                        <UserIcon className="size-5 text-blue-400" />
                      </div>
                      <div className="flex flex-col items-start">
                        <span>User Details</span>
                        <span className="text-sm font-normal text-muted-foreground group-hover:text-muted-foreground/80">
                          Update your name and contact details
                        </span>
                      </div>
                    </div>
                  </AccordionTrigger>
                  <AccordionContent className="flex flex-col gap-4 pt-2">
                    <UserDetailsForm
                      ref={userDetailsFormRef}
                      onDirtyChange={(isDirty: boolean) => {
                        updateDirtyState(isDirty);
                      }}
                    />
                  </AccordionContent>
                </AccordionItem>
              </div>

              <div className="py-3" />

              <div className="border border-border/40 rounded-lg bg-gradient-to-br from-blue-500/5 to-blue-500/10 hover:from-blue-500/10 hover:to-blue-500/15 transition-all shadow-sm">
                <AccordionItem className="px-4" value="change-password">
                  <AccordionTrigger className="text-lg font-semibold text-primary [&[data-state=open]>div]:text-primary/90 hover:no-underline [&>div]:transition-all group">
                    <div className="flex items-center gap-3 opacity-80 group-hover:opacity-100">
                      <div className="p-2 rounded-md bg-blue-500/10 transition-colors group-hover:bg-blue-500/15">
                        <KeyIcon className="size-5 text-blue-400" />
                      </div>
                      <div className="flex flex-col items-start">
                        <span>Change Password</span>
                        <span className="text-sm font-normal text-muted-foreground group-hover:text-muted-foreground/80">
                          Secure your account with a new password
                        </span>
                      </div>
                    </div>
                  </AccordionTrigger>
                  <AccordionContent className="flex flex-col gap-4 pt-2">
                    <ChangePasswordForm
                      ref={changePasswordFormRef}
                      onDirtyChange={(isDirty: boolean) => {
                        updateDirtyState(isDirty);
                      }}
                    />
                  </AccordionContent>
                </AccordionItem>
              </div>

              {(hasPermission('site:view') || hasPermission('site:manage')) && (
                <>
                  <div className="py-3" />

                  <div className="border border-border/40 rounded-lg bg-gradient-to-br from-blue-500/5 to-blue-500/10 hover:from-blue-500/10 hover:to-blue-500/15 transition-all shadow-sm">
                    <AccordionItem className="px-4" value="budget-reminders">
                      <AccordionTrigger className="text-lg font-semibold text-primary [&[data-state=open]>div]:text-primary/90 hover:no-underline [&>div]:transition-all group">
                        <div className="flex items-center gap-3 opacity-80 group-hover:opacity-100">
                          <div className="p-2 rounded-md bg-blue-500/10 transition-colors group-hover:bg-blue-500/15">
                            <CalendarClock className="size-5 text-blue-400" />
                          </div>
                          <div className="flex flex-col items-start">
                            <span>Budget Reminders</span>
                            <span className="text-sm font-normal text-muted-foreground group-hover:text-muted-foreground/80">
                              Set budget email alerts
                            </span>
                          </div>
                        </div>
                      </AccordionTrigger>
                      <AccordionContent className="flex flex-col gap-4 pt-2">
                        <BudgetRemindersForm
                          ref={budgetRemindersFormRef}
                          readonly={!canManageSite}
                          onDirtyChange={(isDirty: boolean) => {
                            updateDirtyState(isDirty);
                          }}
                        />
                      </AccordionContent>
                    </AccordionItem>
                  </div>
                </>
              )}
            </Accordion>
          </div>
        </SheetContent>
      </Sheet>

      <Dialog
        open={isUnsavedChangesDialogOpen}
        onOpenChange={nextOpen => {
          if (isResolvingUnsavedChanges) {
            return;
          }

          setIsUnsavedChangesDialogOpen(nextOpen);

          if (!nextOpen) {
            closeUnsavedChangesDialog();
          }
        }}
      >
        <DialogContent className="sm:max-w-md z-[70]" modal>
          <DialogHeader>
            <ModalTitle>Unsaved Changes</ModalTitle>
            <ModalDescription>
              {getUnsavedChangesDescription()}
            </ModalDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={closeUnsavedChangesDialog}
              disabled={isResolvingUnsavedChanges}
            >
              Keep Editing
            </Button>
            <Button
              type="button"
              variant="destructive"
              onClick={handleDiscardChanges}
              disabled={isResolvingUnsavedChanges}
            >
              Discard Changes
            </Button>
            <Button
              type="button"
              onClick={handleSaveChanges}
              disabled={isResolvingUnsavedChanges}
            >
              {isResolvingUnsavedChanges ? 'Saving...' : getSaveActionLabel()}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

export { AccountSettingsSheet };
