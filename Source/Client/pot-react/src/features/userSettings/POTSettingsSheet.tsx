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
import { Building2, KeyIcon, PiggyBank, UserIcon, XIcon } from 'lucide-react';
import type { JSX } from 'react';

import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Button } from '@/components/ui/button';
import { DialogDescription, DialogTitle } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Sheet, SheetContent } from '@/components/ui/sheet';
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

function AccountSettingsSheet(props: AccountSettingsSheetProps): JSX.Element {
  const { open, onClose } = props;
  const { hasPermission } = usePermissions();
  const canManageSite = hasPermission('site:manage');

  return (
    <Sheet open={open} modal={false}>
      <SheetContent
        side="right"
        className="w-full sm:max-w-sm p-6 overflow-y-auto [&>button:first-of-type]:hidden"
      >
        <div className="space-y-6">
          <div className="flex items-center justify-between">
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
              onClick={onClose}
            >
              <XIcon className="size-5" />
            </Button>
          </div>

          <Separator />

          <Accordion type="single" collapsible defaultValue="user-details">
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
                            Update your site name and description
                          </span>
                        </div>
                      </div>
                    </AccordionTrigger>
                    <AccordionContent className="flex flex-col gap-4 pt-2">
                      <SiteDetailsForm readonly={!canManageSite} />
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
                        Update your profile information
                      </span>
                    </div>
                  </div>
                </AccordionTrigger>
                <AccordionContent className="flex flex-col gap-4 pt-2">
                  <UserDetailsForm />
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
                        Update your account password
                      </span>
                    </div>
                  </div>
                </AccordionTrigger>
                <AccordionContent className="flex flex-col gap-4 pt-2">
                  <ChangePasswordForm />
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
                          <PiggyBank className="size-5 text-blue-400" />
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
                      <BudgetRemindersForm readonly={!canManageSite} />
                    </AccordionContent>
                  </AccordionItem>
                </div>
              </>
            )}
          </Accordion>
        </div>
      </SheetContent>
    </Sheet>
  );
}

export { AccountSettingsSheet };
