import React from 'react';
import { matchPath, useLocation } from 'react-router';
import { Link } from 'react-router';

import {
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
} from '@/components/ui/sidebar';

export type MenuGroupItem = {
  readonly label: string;
  readonly icon: React.ElementType;
  readonly href: string;
};

export type MenuGroupDefinition = {
  readonly label: string;
  readonly items: MenuGroupItem[];
};

type MenuGroupProps = {
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

const MenuGroup: React.FC<MenuGroupProps> = ({ group }) => {
  const location = useLocation();

  return (
    <SidebarGroup>
      <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarGroupContent>
          {group.items.map((item, index) => {
            const isActive = isActivePath(location.pathname, item.href);
            const Icon = item.icon;

            return (
              <SidebarMenu key={index}>
                <SidebarMenuButton
                  isActive={isActive}
                  tooltip={item.label}
                  asChild
                >
                  <Link to={item.href} aria-label={item.label}>
                    <Icon />
                    <span>{item.label}</span>
                  </Link>
                </SidebarMenuButton>
              </SidebarMenu>
            );
          })}
        </SidebarGroupContent>
      </SidebarGroupContent>
    </SidebarGroup>
  );
};

export default MenuGroup;

/*
    TODO:

    Had not planned on using (item, index) as explained below (according to ChatGPT), but using item.href showed
    errors in the console for duplicate keys. To be revisited.

    ---

    Not using items.map((item, index) => { .. } );
    along with <SidebarMenu key={index}> ... </SidebarMenu>

    When using index as the key, React relies on the array's order to track each item. If the order changes
    (e.g., an item is added, removed, or moved), React may incorrectly associate existing elements with new ones,
    leading to:

    * Unnecessary re-renders – Components may be recreated when they don't need to be.
    * Loss of component state – If an input or animation exists in a list item, React might reset it when the order changes.
    * Incorrect behavior in dynamic lists – Items may not update correctly when new ones are inserted or removed.

    Using a unique, stable identifier like item.href ensures:

    * Consistent identity – Each list item is uniquely identified regardless of its position.
    * Efficient re-rendering – React can track and update only the changed elements.
    * State preservation – Inputs, animations, and interactive elements inside a list item won’t lose their state unexpectedly.

    Using index is acceptable if:

    * The list is static (never changes).
    * The list is small and performance isn't a concern.
    * The list doesn't include user input or animations.

    In this case, item.href is a natural unique identifier because each sidebar link points to a distinct page,
    making it a better choice than index.
*/
