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

type MenuGroupItemBase = {
  readonly label: string;
  readonly icon: React.ElementType;
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

/*
  The original code used to determine if a menu item matched the current location relied on react hooks.

    import { useMatch, useResolvedPath } from 'react-router';

    // What's the path associated with the menu item (within group.items.map)
    const resolvedPath = useResolvedPath(item.href);

    // Does it match the current path?
    isMatch = !!useMatch({ path: resolvedPath.pathname, end: true });

  This cannot be used, however, because the hooks cannot be called within a callback.
*/

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

  return (
    <SidebarGroup>
      <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarMenu>
          {group.items.map((item, index) => {
            const Icon = item.icon;

            if (isOnClickLink(item)) {
              return (
                <SidebarMenuItem key={index}>
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
                <SidebarMenuItem key={index}>
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

            // This should never happen with proper typing, but TypeScript requires it
            return null;
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
