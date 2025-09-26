import React from 'react';
import { matchPath, useLocation } from 'react-router';
import { Link } from 'react-router';

import {
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from '@/components/ui/sidebar';
import usePermissions from '@/hooks/usePermissions';

type MenuGroupItemBase = {
  readonly label: string;
  readonly icon: React.ElementType;
  readonly permission?: string;
};

export type HrefLink = MenuGroupItemBase & {
  readonly type: 'href';
  readonly href: string;
};

export type OnClickLink = MenuGroupItemBase & {
  readonly type: 'onClick';
  readonly onClick: () => void;
};

export type MenuGroupItem = HrefLink | OnClickLink;

export type MenuGroupDefinition = {
  readonly label: string;
  readonly items: MenuGroupItem[];
};

export type MenuGroupProps = {
  readonly group: MenuGroupDefinition;
};

const isActivePath = (currentPath: string, href: string) => {
  const resolvedPath = new URL(href, window.location.origin).pathname;
  return !!matchPath({ path: resolvedPath, end: true }, currentPath);
};

const isOnClickLink = (item: MenuGroupItem): item is OnClickLink => {
  return item.type === 'onClick';
};

const isHrefLink = (item: MenuGroupItem): item is HrefLink => {
  return item.type === 'href';
};

const MenuGroup: React.FC<MenuGroupProps> = ({ group }) => {
  const location = useLocation();
  const { permissions } = usePermissions();

  // Pre-calculate all permission checks once for the group - not using <PermissionGuard> as there was too much flicker on a page refresh
  const permissionCache = React.useMemo(() => {
    return group.items.reduce(
      (acc, item) => {
        if (item.permission) {
          acc[item.permission] = permissions.includes(item.permission);
        }

        return acc;
      },
      {} as Record<string, boolean>,
    );
  }, [group.items, permissions]);

  // Skip rendering the group if no items have permission
  const hasAnyVisibleItems = React.useMemo(() => {
    return group.items.some(
      item => !item.permission || permissionCache[item.permission],
    );
  }, [group.items, permissionCache]);

  if (!hasAnyVisibleItems) {
    return null;
  }

  return (
    <SidebarGroup>
      <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarMenu>
          {group.items.map((item, index) => {
            const Icon = item.icon;

            const menuItem = (() => {
              if (isOnClickLink(item)) {
                return (
                  <SidebarMenuItem>
                    <SidebarMenuButton
                      tooltip={item.label}
                      onClick={item.onClick}
                      className="pl-6"
                    >
                      <Icon />
                      <span>{item.label}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              }

              if (isHrefLink(item)) {
                const isActive = isActivePath(location.pathname, item.href);

                return (
                  <SidebarMenuItem>
                    <SidebarMenuButton
                      isActive={isActive}
                      tooltip={item.label}
                      asChild
                      className="pl-6"
                    >
                      <Link to={item.href} aria-label={item.label}>
                        <Icon />
                        <span>{item.label}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              }

              return null;
            })();

            if (!menuItem) {
              return null;
            }

            // Skip items without permission
            if (item.permission && !permissionCache[item.permission]) {
              return null;
            }

            return <React.Fragment key={index}>{menuItem}</React.Fragment>;
          })}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  );
};

export default MenuGroup;

/*
  Using index as the key for menu items. This is acceptable here because:
  1. The menu items are static and don't change order dynamically
  2. The list is small and performance isn't a concern
  3. No user input or animations exist within the menu items

  If the menu becomes dynamic in the future, consider using item.href as the key.
*/
