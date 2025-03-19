import { useMatch, useResolvedPath } from "react-router-dom";
import { MenuGroupItem } from "../MenuGroup";

// Determines if the menu item matches the current path, or it is associated with the home path.
export function matchesCurrentPath(item: MenuGroupItem): boolean {
  // What's the path associated with the menu item
  const resolvedPath = useResolvedPath(item.href);

  // Does it match the current path?
  return !!useMatch({ path: resolvedPath.pathname, end: true });
}
