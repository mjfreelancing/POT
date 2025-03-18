import { useMatch, useResolvedPath } from "react-router-dom";
import { MenuGroupItem } from "../MenuGroup";

// A factory method to create a menu item where 0..1 item should only ever be created with 'IsHome: true'. This method
// effectively makes the 'isHome' property optional, without declaring it as isHome? on MenuGroupItem.
//
// Explanation
// ===========
// Omit<MenuGroupItem, "isHome">
// => Removes isHome from MenuGroupItem, ensuring that it is not required (so we can default to false if required).
//
// Partial<Pick<MenuGroupItem, "isHome">>
// => Picks only isHome from MenuGroupItem but makes it optional.
//
export function createMenuItem(
  item: Omit<MenuGroupItem, "isHome"> & Partial<Pick<MenuGroupItem, "isHome">>
): MenuGroupItem {
  return { isHome: false, ...item };
}

// Determines if the menu item matches the current path, or it is associated with the home path.
export function isHomeOrMatchesCurrentPath(item: MenuGroupItem): boolean {
  // What's the path associated with the menu item
  const resolvedPath = useResolvedPath(item.href);

  // Does it match the current path?
  const isCurrentPath = !!useMatch({ path: resolvedPath.pathname, end: true });

  // Is it a match or is the designated home route
  return isCurrentPath || (item.isHome && window.location.pathname === "/");
}
