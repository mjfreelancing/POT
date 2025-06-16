/**
 * Calculate optimal number of X-axis ticks based on screen width
 * @param screenWidth Current screen width
 * @param dataLength Total number of data points
 * @returns Optimal tick count
 */
function calculateOptimalTickCount(
  screenWidth: number,
  dataLength: number,
): number {
  // Base calculation: ~150px per tick for readability
  const baseTickCount = Math.floor(screenWidth / 150);

  // Ensure we don't exceed data length
  const maxTicks = Math.min(baseTickCount, dataLength);

  // Minimum of 3 ticks, maximum of 12 for readability
  return Math.max(3, Math.min(12, maxTicks));
}

export { calculateOptimalTickCount };
