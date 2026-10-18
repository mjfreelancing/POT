import { Trash2 } from 'lucide-react';

import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { usePermissions } from '@/hooks';

import DeleteAccountAction from './DeleteAccountAction';
import DeleteSiteAction from './DeleteSiteAction';

/**
 * The irreversible actions, collapsed by default at the end of the settings sheet.
 *
 * It is a separate collapsible region rather than an entry in the settings accordion, so opening it never
 * takes part in the unsaved-changes flow. The rows escalate in severity: the account action is always
 * present and always first, so it keeps the same position for every role, and the site action follows it
 * for an administrator.
 */
function DangerZone() {
  const { hasPermission } = usePermissions();
  const canManageSite = hasPermission('site:manage');

  return (
    <Accordion type="single" collapsible>
      <div className="border border-border/40 rounded-lg bg-gradient-to-br from-red-500/5 to-red-500/10 hover:from-red-500/10 hover:to-red-500/15 transition-all shadow-sm">
        <AccordionItem className="px-4" value="danger-zone">
          <AccordionTrigger className="hover:no-underline group">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-md bg-red-500/10 transition-colors group-hover:bg-red-500/15">
                <Trash2 className="size-5 text-destructive-high-contrast" />
              </div>
              <div className="flex flex-col items-start">
                <span className="text-lg font-semibold text-destructive-high-contrast">
                  Danger Zone
                </span>
                <span className="text-sm font-normal text-muted-foreground">
                  Irreversible actions
                </span>
              </div>
            </div>
          </AccordionTrigger>
          <AccordionContent className="flex flex-col gap-4 pt-2">
            <DeleteAccountAction />

            {canManageSite && (
              <div className="border-t border-border/40 pt-4">
                <DeleteSiteAction />
              </div>
            )}
          </AccordionContent>
        </AccordionItem>
      </div>
    </Accordion>
  );
}

export default DangerZone;
